namespace Snapshot;

/// <summary>
/// The result of a single backup run.
/// </summary>
public sealed class BackupSummary(IReadOnlyList<FileChange> changes)
{
    public IReadOnlyList<FileChange> Changes { get; } = changes;

    public int New => Count(ChangeKind.New);
    public int Modified => Count(ChangeKind.Modified);
    public int Deleted => Count(ChangeKind.Deleted);
    public int Unchanged => Count(ChangeKind.Unchanged);

    public override string ToString() =>
        $"New: {New}, modified: {Modified}, deleted: {Deleted}, unchanged: {Unchanged}";

    private int Count(ChangeKind kind) => Changes.Count(c => c.Kind == kind);
}
