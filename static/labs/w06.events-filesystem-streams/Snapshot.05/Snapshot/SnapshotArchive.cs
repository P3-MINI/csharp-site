using System.IO.Compression;

namespace Snapshot;

/// <summary>
/// The result of restoring a snapshot.
/// </summary>
public sealed record RestoreResult(string SnapshotName, int FileCount, long TotalSize, int ChecksumErrors);

/// <summary>
/// Reads and writes snapshot archives. A snapshot is a ZIP file containing:
/// - "manifest.txt" - the list of ALL files at the time of the backup,
/// - "files/..." - only the files that were new or modified since the previous snapshot.
/// </summary>
public static class SnapshotArchive
{
    private const string ManifestEntryName = "manifest.txt";
    private const string FilesPrefix = "files/";

    /// <summary>
    /// Returns paths of all snapshots in the repository, from the oldest to the newest.
    /// </summary>
    public static List<string> List(string repository)
    {
        if (!Directory.Exists(repository))
        {
            return [];
        }

        // Names contain the UTC time of the backup, so sorting by name sorts chronologically
        return Directory.EnumerateFiles(repository, "snapshot-*.zip")
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    public static string Create(string repository, Manifest manifest, IEnumerable<FileEntry> filesToStore)
    {
        Directory.CreateDirectory(repository);
        string path = Path.Combine(repository, $"snapshot-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip");

        // ZipArchiveMode.Create throws if the file already exists - snapshots are never overwritten
        using ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create);

        foreach (FileEntry file in filesToStore)
        {
            ZipArchiveEntry entry = archive.CreateEntry(FilesPrefix + file.RelativePath, CompressionLevel.Optimal);
            entry.LastWriteTime = file.LastWriteTime;

            // Only one entry can be open at a time, so the entry stream is disposed
            // at the end of each iteration, before the next entry is created
            using Stream entryStream = entry.Open();
            using FileStream source = File.OpenRead(file.FullPath);
            source.CopyTo(entryStream);
        }

        // The entry stream compresses the data written to it - Manifest.WriteTo does not need to know that
        using Stream manifestStream = archive.CreateEntry(ManifestEntryName).Open();
        manifest.WriteTo(manifestStream);

        return path;
    }

    public static Manifest ReadManifest(string snapshotPath)
    {
        using ZipArchive archive = ZipFile.OpenRead(snapshotPath);
        ZipArchiveEntry entry = archive.GetEntry(ManifestEntryName)
                                ?? throw new InvalidDataException($"'{snapshotPath}' does not contain a manifest.");

        // The entry stream decompresses the data on the fly
        using Stream stream = entry.Open();
        return Manifest.ReadFrom(stream);
    }

    /// <summary>
    /// Restores the state of the source directory from the moment the given snapshot was made
    /// (the newest one if <paramref name="snapshotName"/> is null) into an empty target directory.
    /// </summary>
    public static RestoreResult Restore(string repository, string target, string? snapshotName,
        Action<string>? log = null)
    {
        List<string> snapshots = List(repository);
        if (snapshots.Count == 0)
        {
            throw new FileNotFoundException($"No snapshots found in '{repository}'.");
        }

        int last = snapshotName is null
            ? snapshots.Count - 1
            : snapshots.FindIndex(s => Path.GetFileNameWithoutExtension(s) == snapshotName);
        if (last < 0)
        {
            throw new FileNotFoundException($"Snapshot '{snapshotName}' not found in '{repository}'.");
        }

        if (Directory.Exists(target) && Directory.EnumerateFileSystemEntries(target).Any())
        {
            throw new IOException($"Target directory '{target}' is not empty.");
        }

        string targetRoot = Path.GetFullPath(target);
        Directory.CreateDirectory(targetRoot);

        // 1. Apply the snapshots from the oldest to the chosen one - newer versions overwrite older ones
        for (int i = 0; i <= last; i++)
        {
            using ZipArchive archive = ZipFile.OpenRead(snapshots[i]);
            int extracted = 0;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (!entry.FullName.StartsWith(FilesPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string destination = Path.GetFullPath(Path.Combine(targetRoot, entry.FullName[FilesPrefix.Length..]));

                // Never trust paths stored in an archive: "files/../../somewhere" must not escape the target
                if (!PathUtils.IsInside(destination, targetRoot))
                {
                    throw new InvalidDataException($"Entry '{entry.FullName}' points outside the target directory.");
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                entry.ExtractToFile(destination, overwrite: true);
                extracted++;
            }

            log?.Invoke($"Applied {Path.GetFileName(snapshots[i])} ({extracted} files)");
        }

        // 2. Remove files deleted before the chosen snapshot was made.
        //    GetFiles (not EnumerateFiles) - we modify the directory while iterating.
        Manifest manifest = ReadManifest(snapshots[last]);
        foreach (string file in Directory.GetFiles(targetRoot, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(targetRoot, file).Replace(Path.DirectorySeparatorChar, '/');
            if (!manifest.Contains(relativePath))
            {
                File.Delete(file);
                log?.Invoke($"Removed {relativePath}");
            }
        }

        // 3. Verify the restored files
        int checksumErrors = 0;
        foreach (ManifestEntry entry in manifest.Entries)
        {
            string path = Path.Combine(targetRoot, entry.Path);
            if (!File.Exists(path) || FileHasher.ComputeSha256(path) != entry.Hash)
            {
                checksumErrors++;
                log?.Invoke($"Checksum mismatch: {entry.Path}");
            }
        }

        string name = Path.GetFileNameWithoutExtension(snapshots[last]);
        return new RestoreResult(name, manifest.Count, manifest.TotalSize, checksumErrors);
    }
}
