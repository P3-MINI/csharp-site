using MiniKV.Protocol;

namespace MiniKV.Server;

/// <summary>
/// Remembers which clients watch which keys.
/// Used by many client tasks at the same time, so every access is guarded by a lock.
/// </summary>
public sealed class WatchRegistry
{
    private readonly Dictionary<string, HashSet<ClientSession>> _watchers = [];
    private readonly Lock _lock = new();

    public void Watch(string key, ClientSession session)
    {
        lock (_lock)
        {
            if (!_watchers.TryGetValue(key, out HashSet<ClientSession>? sessions))
                _watchers[key] = sessions = [];
            sessions.Add(session);
        }
    }

    public bool Unwatch(string key, ClientSession session)
    {
        lock (_lock)
        {
            if (!_watchers.TryGetValue(key, out HashSet<ClientSession>? sessions) || !sessions.Remove(session))
                return false;
            if (sessions.Count == 0)
                _watchers.Remove(key);
            return true;
        }
    }

    public void RemoveSession(ClientSession session)
    {
        lock (_lock)
        {
            foreach ((string key, HashSet<ClientSession> sessions) in _watchers)
            {
                // Since .NET Core 3.0 Remove does not invalidate an active enumerator
                if (sessions.Remove(session) && sessions.Count == 0)
                    _watchers.Remove(key);
            }
        }
    }

    public void Notify(string key, string? value)
    {
        ClientSession[] targets;
        lock (_lock)
        {
            if (!_watchers.TryGetValue(key, out HashSet<ClientSession>? sessions))
                return;
            targets = [.. sessions]; // copy, so that we do not send while holding the lock
        }

        var notification = new KeyChanged(key, value);
        foreach (ClientSession session in targets)
            session.Send(notification);
    }
}
