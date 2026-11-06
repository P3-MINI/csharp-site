using System.Globalization;

namespace Wrapped;

/// <summary>The whole data set: artists, tracks and the listening history (sorted by time).</summary>
public class MusicLibrary(IReadOnlyList<Artist> artists, IReadOnlyList<Track> tracks, IReadOnlyList<Play> plays)
{
    public IReadOnlyList<Artist> Artists { get; } = artists;
    public IReadOnlyList<Track> Tracks { get; } = tracks;
    public IReadOnlyList<Play> Plays { get; } = plays;

    /// <summary>Loads artists.csv, tracks.csv and plays.csv from the given directory.</summary>
    public static MusicLibrary Load(string directory)
    {
        var artists = ReadCsv(Path.Combine(directory, "artists.csv"),
            fields => new Artist(int.Parse(fields[0]), fields[1], fields[2].Split(';')));

        var tracks = ReadCsv(Path.Combine(directory, "tracks.csv"),
            fields => new Track(int.Parse(fields[0]), fields[1], int.Parse(fields[2]),
                TimeSpan.FromSeconds(int.Parse(fields[3]))));

        var plays = ReadCsv(Path.Combine(directory, "plays.csv"),
            fields => new Play(
                DateTime.ParseExact(fields[0], "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                int.Parse(fields[1]), int.Parse(fields[2])));

        return new MusicLibrary(artists, tracks, plays);
    }

    /// <summary>
    /// Reads a simple CSV file (a header row, comma-separated values, no quoting)
    /// and converts every data row into an object using the <paramref name="parse"/> function.
    /// </summary>
    private static List<T> ReadCsv<T>(string path, Func<string[], T> parse)
    {
        var result = new List<T>();
        bool isHeader = true;
        foreach (string line in File.ReadLines(path))
        {
            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            if (line.Length > 0)
            {
                result.Add(parse(line.Split(',')));
            }
        }
        return result;
    }
}
