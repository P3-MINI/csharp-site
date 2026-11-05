namespace Snapshot;

/// <summary>
/// The result of a single backup run.
/// </summary>
public sealed class BackupSummary(IReadOnlyList<FileChange> changes, string? snapshotPath)
{
    public IReadOnlyList<FileChange> Changes { get; } = changes;

    /// <summary>Path of the created snapshot, or null if nothing has changed.</summary>
    public string? SnapshotPath { get; } = snapshotPath;

    public int New => Count(ChangeKind.New);
    public int Modified => Count(ChangeKind.Modified);
    public int Deleted => Count(ChangeKind.Deleted);
    public int Unchanged => Count(ChangeKind.Unchanged);

    public override string ToString() =>
        $"New: {New}, modified: {Modified}, deleted: {Deleted}, unchanged: {Unchanged}";

    private int Count(ChangeKind kind) => Changes.Count(c => c.Kind == kind);
}
