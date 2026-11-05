using System.IO.Enumeration;

namespace Snapshot;

/// <summary>
/// Lazily enumerates all files in a directory tree, skipping ignored ones.
/// </summary>
public sealed class DirectoryScanner(string rootDirectory, IReadOnlyList<string> ignorePatterns)
{
    public string RootDirectory { get; } = Path.GetFullPath(rootDirectory);

    public IEnumerable<FileEntry> Scan()
    {
        var root = new DirectoryInfo(RootDirectory);
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true, // skip files and directories we are not allowed to read
        };

        // EnumerateFiles is lazy: files are found one by one, while we iterate
        foreach (FileInfo file in root.EnumerateFiles("*", options))
        {
            string relativePath = ToRelativePath(file.FullName);
            if (IsIgnored(relativePath))
            {
                continue;
            }

            yield return new FileEntry(relativePath, file.FullName, file.Length, file.LastWriteTime);
        }
    }

    /// <summary>
    /// Converts a full path to a path relative to the root directory,
    /// with '/' as the separator on every operating system.
    /// </summary>
    public string ToRelativePath(string fullPath) =>
        Path.GetRelativePath(RootDirectory, fullPath).Replace(Path.DirectorySeparatorChar, '/');

    /// <summary>
    /// A path is ignored if any of its segments (directory or file name) matches any ignore pattern,
    /// e.g. "cache" ignores "cache/thumbnails.db" and "*.tmp" ignores "docs/draft.tmp".
    /// </summary>
    public bool IsIgnored(string relativePath)
    {
        foreach (string segment in relativePath.Split('/'))
        {
            foreach (string pattern in ignorePatterns)
            {
                if (FileSystemName.MatchesSimpleExpression(pattern, segment))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
