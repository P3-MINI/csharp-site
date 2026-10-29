namespace Snapshot;

/// <summary>
/// Shows the progress of a backup in the console.
/// Subscribes to the engine's events when created and unsubscribes when disposed.
/// </summary>
public sealed class ConsoleReporter : IDisposable
{
    private const int BarWidth = 30;
    private const int LineWidth = 70;

    private readonly BackupEngine _engine;
    private bool _disposed;

    public ConsoleReporter(BackupEngine engine)
    {
        _engine = engine;
        _engine.FileProcessed += OnFileProcessed;
        _engine.ProgressChanged += OnProgressChanged;
        _engine.Completed += OnCompleted;
    }

    private void OnFileProcessed(object? sender, FileProcessedEventArgs e)
    {
        if (e.Kind == ChangeKind.Unchanged)
        {
            return;
        }

        // '\r' moves the cursor to the beginning of the line - the message overwrites
        // the progress bar, which is drawn again on the next ProgressChanged event
        string message = $"{e.Kind.ToSymbol()} {e.File.RelativePath}";
        Console.WriteLine("\r" + message.PadRight(LineWidth));
    }

    private void OnProgressChanged(object? sender, BackupProgressEventArgs e)
    {
        int filled = (int)(e.Fraction * BarWidth);
        int percent = (int)(e.Fraction * 100);
        string bar = new string('#', filled) + new string('.', BarWidth - filled);
        string line = $"[{bar}] {percent,3}% ({e.ProcessedFiles}/{e.TotalFiles} files, {e.ProcessedBytes.ToHumanSize()})";
        Console.Write("\r" + line.PadRight(LineWidth));
    }

    private void OnCompleted(object? sender, BackupCompletedEventArgs e)
    {
        Console.WriteLine(); // end the progress bar line
        foreach (FileChange change in e.Summary.Changes.Where(c => c.Kind == ChangeKind.Deleted))
        {
            Console.WriteLine($"{change.Kind.ToSymbol()} {change.Path}");
        }

        Console.WriteLine(e.Summary);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _engine.FileProcessed -= OnFileProcessed;
        _engine.ProgressChanged -= OnProgressChanged;
        _engine.Completed -= OnCompleted;
        _disposed = true;
    }
}
