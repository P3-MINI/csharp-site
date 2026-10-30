namespace Snapshot;

public static class Program
{
    // Names (or simple patterns with * and ?) of files and directories that are never backed up
    private static readonly string[] IgnorePatterns = ["*.tmp", "*.bak", "bin", "obj", "cache"];

    public static int Main(string[] args)
    {
        try
        {
            switch (args)
            {
                case ["scan", var source]:
                    return Scan(source);
                case ["backup", var source, var repository]:
                    return Backup(source, repository, maxFileSizeKb: null);
                case ["backup", var source, var repository, var maxSize] when long.TryParse(maxSize, out long maxSizeKb):
                    return Backup(source, repository, maxSizeKb);
                case ["restore", var repository, var target]:
                    return Restore(repository, target, snapshotName: null);
                case ["restore", var repository, var target, var snapshotName]:
                    return Restore(repository, target, snapshotName);
                case ["watch", var source, var repository]:
                    return Watch(source, repository);
                default:
                    PrintUsage();
                    return 1;
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Console.Error.WriteLine($"Error: {e.Message}");
            return 1;
        }
    }

    private static int Scan(string source)
    {
        if (!Directory.Exists(source))
        {
            Console.Error.WriteLine($"Directory '{source}' does not exist.");
            return 1;
        }

        Console.WriteLine($"Scanning {source}");
        var scanner = new DirectoryScanner(source, IgnorePatterns);

        // Enumeration order depends on the file system - sort to get a stable output
        List<FileEntry> files = scanner.Scan()
            .OrderBy(f => f.RelativePath, StringComparer.Ordinal)
            .ToList();

        foreach (FileEntry file in files)
        {
            Console.WriteLine($"{file.Size.ToHumanSize(),10}  {file.RelativePath}");
        }

        long totalSize = files.Sum(f => f.Size);
        Console.WriteLine($"{files.Count} files, {totalSize.ToHumanSize()} in total");

        Console.WriteLine("Largest files:");
        foreach (FileEntry file in files.OrderByDescending(f => f.Size).Take(3))
        {
            Console.WriteLine($"{file.Size.ToHumanSize(),10}  {file.RelativePath}");
        }

        return 0;
    }

    private static int Backup(string source, string repository, long? maxFileSizeKb)
    {
        if (!CheckDirectories(source, repository))
        {
            return 1;
        }

        var engine = new BackupEngine(source, repository, IgnorePatterns);

        // The reporter subscribes to the engine's events; 'using' unsubscribes it at the end
        using var reporter = new ConsoleReporter(engine);

        if (maxFileSizeKb is { } limitKb)
        {
            // A subscriber can exclude a file by setting e.Cancel
            engine.FileFound += (_, e) =>
            {
                if (e.File.Size > limitKb * 1024)
                {
                    e.Cancel = true;
                    Console.WriteLine($"Skipping {e.File.RelativePath} ({e.File.Size.ToHumanSize()} > {limitKb} KB)");
                }
            };
        }

        engine.Run();
        return 0;
    }

    private static int Restore(string repository, string target, string? snapshotName)
    {
        // A simple callback is enough here - there is exactly one "listener"
        RestoreResult result = SnapshotArchive.Restore(repository, target, snapshotName,
            message => Console.WriteLine($"  {message}"));

        if (result.ChecksumErrors > 0)
        {
            Console.WriteLine($"Restored {result.SnapshotName} with {result.ChecksumErrors} checksum errors!");
            return 1;
        }

        Console.WriteLine($"Restored {result.SnapshotName}: {result.FileCount} files, " +
                          $"{result.TotalSize.ToHumanSize()}, all checksums OK.");
        return 0;
    }

    private static int Watch(string source, string repository)
    {
        if (!CheckDirectories(source, repository))
        {
            return 1;
        }

        var engine = new BackupEngine(source, repository, IgnorePatterns);
        using var reporter = new ConsoleReporter(engine);

        // Bring the repository up to date before watching
        engine.Run();

        var scanner = new DirectoryScanner(source, IgnorePatterns);
        using var watcher = new DebouncedWatcher(source, TimeSpan.FromSeconds(2), scanner.IsIgnored);

        watcher.ChangeDetected += (_, e) => Console.WriteLine(e is RenamedEventArgs r
            ? $"Renamed: {r.OldName} -> {r.Name}"
            : $"{e.ChangeType}: {e.Name}");

        watcher.ChangesSettled += (_, _) =>
        {
            // This handler runs on a thread-pool thread: an unhandled exception here
            // would terminate the whole process, so errors are caught and reported
            try
            {
                Console.WriteLine("Changes settled - starting backup");
                engine.Run();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                Console.Error.WriteLine($"Backup failed: {e.Message}");
            }
        };

        watcher.Start();
        Console.WriteLine($"Watching {source} - press Enter to stop.");
        Console.ReadLine();
        return 0;
    }

    private static bool CheckDirectories(string source, string repository)
    {
        if (!Directory.Exists(source))
        {
            Console.Error.WriteLine($"Directory '{source}' does not exist.");
            return false;
        }

        if (PathUtils.IsInside(repository, source))
        {
            Console.Error.WriteLine("The repository must not be located inside the source directory.");
            return false;
        }

        return true;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Snapshot scan <source>");
        Console.WriteLine("  Snapshot backup <source> <repository> [max-file-size-KB]");
        Console.WriteLine("  Snapshot restore <repository> <target> [snapshot-name]");
        Console.WriteLine("  Snapshot watch <source> <repository>");
    }
}
