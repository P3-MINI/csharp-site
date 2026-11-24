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

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Snapshot scan <source>");
    }
}
