using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MiniKV.Protocol;

namespace MiniKV.Server;

/// <summary>
/// In-memory key-value store with JSON snapshots.
/// Thread-safe: many client tasks use it at the same time.
/// </summary>
public sealed class KeyValueStore
{
    private readonly ConcurrentDictionary<string, string> _data;

    // Snapshot files are meant to be read by humans too
    private static readonly JsonSerializerOptions SnapshotOptions = new(KvProtocol.JsonOptions) { WriteIndented = true };

    public KeyValueStore() => _data = new();

    private KeyValueStore(IDictionary<string, string> entries) => _data = new(entries);

    public int Count => _data.Count;

    public bool TryGet(string key, [MaybeNullWhen(false)] out string value) => _data.TryGetValue(key, out value);

    public void Set(string key, string value) => _data[key] = value;

    public bool Delete(string key) => _data.TryRemove(key, out _);

    public IReadOnlyList<string> Keys() => _data.Keys.Order().ToList();

    public async Task SaveAsync(string path)
    {
        var snapshot = new Snapshot(DateTime.UtcNow, new SortedDictionary<string, string>(_data));

        await using FileStream file = File.Create(path);
        await JsonSerializer.SerializeAsync(file, snapshot, SnapshotOptions);
    }

    public static async Task<KeyValueStore> LoadAsync(string path)
    {
        if (!File.Exists(path))
            return new KeyValueStore();

        await using FileStream file = File.OpenRead(path);
        Snapshot? snapshot = await JsonSerializer.DeserializeAsync<Snapshot>(file, KvProtocol.JsonOptions);
        return snapshot is null ? new KeyValueStore() : new KeyValueStore(snapshot.Entries);
    }

    // Shape of the snapshot file
    private sealed record Snapshot(DateTime SavedAtUtc, SortedDictionary<string, string> Entries);
}
