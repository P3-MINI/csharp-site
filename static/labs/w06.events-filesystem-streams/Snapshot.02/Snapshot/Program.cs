namespace Snapshot;

public static class Program
{
    // Names (or simple patterns with * and ?) of files and directories that are never backed up
    private static readonly string[] IgnorePatterns = ["*.tmp", "*.bak", "bin", "obj", "cache"];

    private const string ManifestFileName = "latest.manifest";

    public static int Main(string[] args)
    {
        try
        {
            switch (args)
            {
                case ["scan", var source]:
                    return Scan(source);
                case ["backup", var source, var repository]:
                    return Backup(source, repository);
                default:
                    PrintUsage();
                    return 1;
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
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

    private static int Backup(string source, string repository)
    {
        if (!CheckDirectories(source, repository))
        {
            return 1;
        }

        string manifestPath = Path.Combine(repository, ManifestFileName);
        Manifest? previous = File.Exists(manifestPath) ? Manifest.Load(manifestPath) : null;
        Console.WriteLine(previous is null
            ? "No previous manifest - all files are new."
            : $"Previous manifest: {previous.Count} files, {previous.TotalSize.ToHumanSize()}");

        var scanner = new DirectoryScanner(source, IgnorePatterns);
        var current = new Manifest();
        var changes = new List<FileChange>();

        foreach (FileEntry file in scanner.Scan().OrderBy(f => f.RelativePath, StringComparer.Ordinal))
        {
            var entry = new ManifestEntry(file.RelativePath, file.Size, FileHasher.ComputeSha256(file.FullPath));
            current.Add(entry);
            changes.Add(new FileChange(entry.Path, previous?.Classify(entry) ?? ChangeKind.New));
        }

        // Files present in the previous manifest, but not found now, were deleted
        foreach (ManifestEntry old in previous?.Entries ?? [])
        {
            if (!current.Contains(old.Path))
            {
                changes.Add(new FileChange(old.Path, ChangeKind.Deleted));
            }
        }

        foreach (FileChange change in changes.Where(c => c.Kind != ChangeKind.Unchanged))
        {
            Console.WriteLine($"{change.Kind.ToSymbol()} {change.Path}");
        }

        Console.WriteLine($"New: {Count(ChangeKind.New)}, modified: {Count(ChangeKind.Modified)}, " +
                          $"deleted: {Count(ChangeKind.Deleted)}, unchanged: {Count(ChangeKind.Unchanged)}");

        Directory.CreateDirectory(repository); // does nothing if the directory already exists
        current.Save(manifestPath);
        Console.WriteLine($"Manifest saved to {manifestPath}");
        return 0;

        int Count(ChangeKind kind) => changes.Count(c => c.Kind == kind);
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
        Console.WriteLine("  Snapshot backup <source> <repository>");
    }
}
