using System.Globalization;
using Wrapped;

// Format numbers and dates the same way on every machine
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var library = MusicLibrary.Load(Path.Combine(AppContext.BaseDirectory, "Data"));
var plays = library.Plays;

// Our Func<Play, bool> filters plug directly into Where
var streams = plays.Where(Filters.Not(Filters.IsSkip));

// 1. Simple aggregates
Console.WriteLine($"Plays:            {plays.Count()} ({streams.Count()} streams, {plays.Count(Filters.IsSkip)} skips)");
Console.WriteLine($"Minutes listened: {plays.Sum(p => (long)p.MsPlayed) / 60_000}");
Console.WriteLine($"Average play:     {plays.Average(p => p.Duration.TotalSeconds):F1} s");
Console.WriteLine($"Distinct tracks:  {streams.Select(p => p.TrackId).Distinct().Count()}");
Console.WriteLine($"Listening days:   {plays.Select(p => DateOnly.FromDateTime(p.Timestamp)).Distinct().Count()}");
Console.WriteLine($"First and last:   {plays.Min(p => p.Timestamp):yyyy-MM-dd HH:mm}, {plays.Max(p => p.Timestamp):yyyy-MM-dd HH:mm}");
Console.WriteLine($"Any play at 4 am: {plays.Any(p => p.Timestamp.Hour == 4)}");
Console.WriteLine($"All in 2025:      {plays.All(p => p.Timestamp.Year == 2025)}");

var longest = plays.MaxBy(p => p.Duration)!;
var longestTrack = library.Tracks.First(t => t.Id == longest.TrackId);
Console.WriteLine($"Longest play:     {longestTrack.Title} ({longest.Duration:m\\:ss}) on {longest.Timestamp:yyyy-MM-dd}");

// 2. Top 10 tracks - method syntax
var topTracks = library.Tracks
    .Select(t => new { Track = t, Streams = streams.Count(p => p.TrackId == t.Id) })
    .OrderByDescending(x => x.Streams)
    .ThenBy(x => x.Track.Title)
    .Take(10);

Console.WriteLine();
Console.WriteLine("Top 10 tracks:");
foreach (var (i, x) in topTracks.Index())
{
    var artist = library.Artists.First(a => a.Id == x.Track.ArtistId);
    Console.WriteLine($"{i + 1,3}. {x.Track.Title + " - " + artist.Name,-45}{x.Streams,4}");
}

// 3. The same query - query syntax
var topTracksQuery =
    (from t in library.Tracks
     let count = streams.Count(p => p.TrackId == t.Id)
     orderby count descending, t.Title
     select new { Track = t, Streams = count })
    .Take(10);
Console.WriteLine($"Query syntax gives the same result: {topTracks.SequenceEqual(topTracksQuery)}");

// 4. Most played artist
var topArtist = library.Artists
    .Select(a => new
    {
        Artist = a,
        Streams = library.Tracks
            .Where(t => t.ArtistId == a.Id)
            .Sum(t => streams.Count(p => p.TrackId == t.Id))
    })
    .MaxBy(x => x.Streams)!;
Console.WriteLine($"Top artist: {topArtist.Artist.Name} ({topArtist.Streams} streams)");
