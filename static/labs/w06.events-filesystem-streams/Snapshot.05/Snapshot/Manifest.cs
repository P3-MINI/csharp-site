namespace Snapshot;

/// <summary>
/// A single line of the manifest: a file, its size and the hash of its content.
/// </summary>
public sealed record ManifestEntry(string Path, long Size, string Hash);

/// <summary>
/// The list of all files in a backup, sorted by path.
/// </summary>
public sealed class Manifest
{
    private const string Header = "# Snapshot manifest v1";

    private readonly SortedDictionary<string, ManifestEntry> _entries = new(StringComparer.Ordinal);

    public IReadOnlyCollection<ManifestEntry> Entries => _entries.Values;

    public int Count => _entries.Count;

    public long TotalSize => _entries.Values.Sum(e => e.Size);

    public void Add(ManifestEntry entry) => _entries.Add(entry.Path, entry);

    public bool Contains(string path) => _entries.ContainsKey(path);

    /// <summary>
    /// Compares a file from the current state of the directory with this (previous) manifest.
    /// </summary>
    public ChangeKind Classify(ManifestEntry current) =>
        _entries.TryGetValue(current.Path, out ManifestEntry? previous)
            ? previous.Hash == current.Hash ? ChangeKind.Unchanged : ChangeKind.Modified
            : ChangeKind.New;

    /// <summary>
    /// Writes the manifest as text: a header line and then one "hash size path" line per file.
    /// </summary>
    public void WriteTo(Stream stream)
    {
        // The stream belongs to the caller - leaveOpen prevents the writer from closing it
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.WriteLine(Header);
        foreach (ManifestEntry entry in _entries.Values)
        {
            writer.WriteLine($"{entry.Hash} {entry.Size} {entry.Path}");
        }
    }

    public static Manifest ReadFrom(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var manifest = new Manifest();

        while (reader.ReadLine() is { } line)
        {
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue; // skip empty lines and comments
            }

            // The path is the last field, so it may contain spaces
            string[] parts = line.Split(' ', 3);
            if (parts.Length != 3 || !long.TryParse(parts[1], out long size))
            {
                throw new InvalidDataException($"Invalid manifest line: '{line}'");
            }

            manifest.Add(new ManifestEntry(parts[2], size, parts[0]));
        }

        return manifest;
    }

    public void Save(string path)
    {
        using FileStream stream = File.Create(path);
        WriteTo(stream);
    }

    public static Manifest Load(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return ReadFrom(stream);
    }
}
