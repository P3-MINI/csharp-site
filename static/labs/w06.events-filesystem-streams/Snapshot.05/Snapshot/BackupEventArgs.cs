using System.ComponentModel;

namespace Snapshot;

/// <summary>
/// Data of the FileFound event. A subscriber can set <see cref="CancelEventArgs.Cancel"/>
/// to true to exclude the file from the backup.
/// </summary>
public class FileFoundEventArgs(FileEntry file) : CancelEventArgs
{
    public FileEntry File { get; } = file;
}

/// <summary>
/// Data of the FileProcessed event: a file was hashed and compared with the previous backup.
/// </summary>
public class FileProcessedEventArgs(FileEntry file, ChangeKind kind) : EventArgs
{
    public FileEntry File { get; } = file;
    public ChangeKind Kind { get; } = kind;
}

/// <summary>
/// Data of the ProgressChanged event.
/// </summary>
public class BackupProgressEventArgs(int processedFiles, int totalFiles, long processedBytes, long totalBytes)
    : EventArgs
{
    public int ProcessedFiles { get; } = processedFiles;
    public int TotalFiles { get; } = totalFiles;
    public long ProcessedBytes { get; } = processedBytes;
    public long TotalBytes { get; } = totalBytes;

    /// <summary>
    /// Progress as a number from 0 to 1, based on the number of processed bytes.
    /// </summary>
    public double Fraction => TotalBytes == 0 ? 1.0 : (double)ProcessedBytes / TotalBytes;
}

/// <summary>
/// Data of the Completed event.
/// </summary>
public class BackupCompletedEventArgs(BackupSummary summary) : EventArgs
{
    public BackupSummary Summary { get; } = summary;
}
