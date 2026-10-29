namespace Snapshot;

/// <summary>
/// Performs incremental backups of a source directory into a repository directory.
/// It does not print anything - it only reports what is happening by raising events.
/// </summary>
public class BackupEngine(string sourceDirectory, string repositoryDirectory, IReadOnlyList<string> ignorePatterns)
{
    public const string ManifestFileName = "latest.manifest";

    /// <summary>Raised for every file found in the source directory, before it is processed.</summary>
    public event EventHandler<FileFoundEventArgs>? FileFound;

    /// <summary>Raised after a file was hashed and compared with the previous backup.</summary>
    public event EventHandler<FileProcessedEventArgs>? FileProcessed;

    /// <summary>Raised after every processed file.</summary>
    public event EventHandler<BackupProgressEventArgs>? ProgressChanged;

    /// <summary>Raised when the backup is finished.</summary>
    public event EventHandler<BackupCompletedEventArgs>? Completed;

    public string SourceDirectory { get; } = sourceDirectory;
    public string RepositoryDirectory { get; } = repositoryDirectory;

    public BackupSummary Run()
    {
        // 1. Find the files to back up; FileFound subscribers may exclude some of them.
        //    The list is materialized, because reporting progress requires the total size.
        var scanner = new DirectoryScanner(SourceDirectory, ignorePatterns);
        List<FileEntry> files = [];
        foreach (FileEntry file in scanner.Scan().OrderBy(f => f.RelativePath, StringComparer.Ordinal))
        {
            var args = new FileFoundEventArgs(file);
            OnFileFound(args);
            if (!args.Cancel)
            {
                files.Add(file);
            }
        }

        long totalBytes = files.Sum(f => f.Size);

        // 2. Hash all files and compare them with the previous manifest
        string manifestPath = Path.Combine(RepositoryDirectory, ManifestFileName);
        Manifest? previous = File.Exists(manifestPath) ? Manifest.Load(manifestPath) : null;
        var current = new Manifest();
        var changes = new List<FileChange>();
        long processedBytes = 0;

        foreach (FileEntry file in files)
        {
            var entry = new ManifestEntry(file.RelativePath, file.Size, FileHasher.ComputeSha256(file.FullPath));
            current.Add(entry);

            ChangeKind kind = previous?.Classify(entry) ?? ChangeKind.New;
            changes.Add(new FileChange(entry.Path, kind));
            OnFileProcessed(new FileProcessedEventArgs(file, kind));

            processedBytes += file.Size;
            OnProgressChanged(new BackupProgressEventArgs(changes.Count, files.Count, processedBytes, totalBytes));
        }

        // 3. Files present in the previous manifest, but not found now, were deleted
        foreach (ManifestEntry old in previous?.Entries ?? [])
        {
            if (!current.Contains(old.Path))
            {
                changes.Add(new FileChange(old.Path, ChangeKind.Deleted));
            }
        }

        // 4. The current manifest becomes the base for the next backup
        Directory.CreateDirectory(RepositoryDirectory);
        current.Save(manifestPath);

        var summary = new BackupSummary(changes);
        OnCompleted(new BackupCompletedEventArgs(summary));
        return summary;
    }

    // Events are raised only through these methods - derived classes can override them
    // to react to an event without subscribing to it, or to suppress it.
    protected virtual void OnFileFound(FileFoundEventArgs e) => FileFound?.Invoke(this, e);

    protected virtual void OnFileProcessed(FileProcessedEventArgs e) => FileProcessed?.Invoke(this, e);

    protected virtual void OnProgressChanged(BackupProgressEventArgs e) => ProgressChanged?.Invoke(this, e);

    protected virtual void OnCompleted(BackupCompletedEventArgs e) => Completed?.Invoke(this, e);
}
