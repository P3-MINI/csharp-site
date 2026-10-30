namespace Snapshot;

public enum ChangeKind
{
    Unchanged,
    New,
    Modified,
    Deleted,
}

/// <summary>
/// The state of a single file compared with the previous backup.
/// </summary>
public sealed record FileChange(string Path, ChangeKind Kind);

public static class ChangeKindExtensions
{
    /// <summary>
    /// A one-character marker used when printing changes: + new, ~ modified, - deleted.
    /// </summary>
    public static char ToSymbol(this ChangeKind kind) => kind switch
    {
        ChangeKind.New => '+',
        ChangeKind.Modified => '~',
        ChangeKind.Deleted => '-',
        _ => ' ',
    };
}
