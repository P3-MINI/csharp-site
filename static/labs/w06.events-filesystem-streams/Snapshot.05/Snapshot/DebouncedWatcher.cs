namespace Snapshot;

/// <summary>
/// Watches a directory tree and raises <see cref="ChangesSettled"/> once,
/// after a burst of changes, when nothing has changed for the quiet period (debouncing).
/// </summary>
public sealed class DebouncedWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly Timer _timer;
    private readonly TimeSpan _quietPeriod;
    private readonly Func<string, bool> _isIgnored;
    private readonly Lock _lock = new();
    private bool _disposed;

    /// <summary>Raised for every relevant change. Raised on a background thread!</summary>
    public event EventHandler<FileSystemEventArgs>? ChangeDetected;

    /// <summary>Raised when no change was detected for the quiet period. Raised on a thread-pool thread!</summary>
    public event EventHandler? ChangesSettled;

    public DebouncedWatcher(string directory, TimeSpan quietPeriod, Func<string, bool> isIgnored)
    {
        _quietPeriod = quietPeriod;
        _isIgnored = isIgnored;

        // A one-shot timer, initially stopped (infinite due time)
        _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

        _watcher = new FileSystemWatcher(directory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                           | NotifyFilters.LastWrite | NotifyFilters.Size,
        };
        _watcher.Created += OnFileSystemChanged;
        _watcher.Changed += OnFileSystemChanged;
        _watcher.Deleted += OnFileSystemChanged;
        _watcher.Renamed += OnFileSystemChanged; // RenamedEventArgs derives from FileSystemEventArgs
        _watcher.Error += OnWatcherError;
    }

    public void Start() => _watcher.EnableRaisingEvents = true;

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        // e.Name is the path relative to the watched directory
        string relativePath = (e.Name ?? "").Replace(Path.DirectorySeparatorChar, '/');
        if (_isIgnored(relativePath))
        {
            return;
        }

        ChangeDetected?.Invoke(this, e);
        RestartCountdown();
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        // The internal buffer overflowed and some events were lost. The backup scans
        // the whole directory anyway, so it is enough to schedule it.
        RestartCountdown();
    }

    private void RestartCountdown()
    {
        lock (_lock)
        {
            if (!_disposed)
            {
                // Every change moves the moment the timer fires - it fires only after
                // the quiet period passes without any change
                _timer.Change(_quietPeriod, Timeout.InfiniteTimeSpan);
            }
        }
    }

    private void OnTimerElapsed(object? state)
    {
        // Runs on a thread-pool thread. The lock guarantees that handlers (backups)
        // never run concurrently and that Dispose waits for a running handler.
        lock (_lock)
        {
            if (!_disposed)
            {
                ChangesSettled?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _watcher.Dispose();
            _timer.Dispose();
        }
    }
}
