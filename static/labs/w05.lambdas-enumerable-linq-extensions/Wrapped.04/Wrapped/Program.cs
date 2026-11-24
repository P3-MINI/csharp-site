using System.Globalization;
using Wrapped;

// Format numbers and dates the same way on every machine
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var library = MusicLibrary.Load(Path.Combine(AppContext.BaseDirectory, "Data"));

// 1. Join: plays + tracks + artists
var infos = library.GetPlayInfos();
Console.WriteLine("First plays:");
foreach (var info in infos.Take(3))
{
    Console.WriteLine($"  {info.Play.Timestamp:yyyy-MM-dd HH:mm}  {info.Track.Title} - {info.Artist.Name} ({info.Play.Duration:m\\:ss})");
}

// The same join in method syntax
var infosMethodSyntax = library.Plays
    .Join(library.Tracks, play => play.TrackId, track => track.Id, (play, track) => (play, track))
    .Join(library.Artists, x => x.track.ArtistId, artist => artist.Id,
        (x, artist) => new PlayInfo(x.play, x.track, artist));
Console.WriteLine($"Method syntax gives the same result: {infos.SequenceEqual(infosMethodSyntax)}");

// 2. SelectMany: flatten the genres of all artists
var genres = library.Artists.SelectMany(a => a.Genres).Distinct().Order();
Console.WriteLine();
Console.WriteLine($"Genres: {string.Join(", ", genres)}");

var jazzArtists =
    from artist in library.Artists
    from genre in artist.Genres
    where genre == "jazz"
    select artist.Name;
Console.WriteLine($"Jazz artists: {string.Join(", ", jazzArtists)}");

var jazzTime = infos.Where(i => i.Artist.Genres.Contains("jazz")).Select(i => i.Play).TotalListeningTime;
Console.WriteLine($"Jazz listening time: {jazzTime.ToHoursAndMinutes()}");

// 3. GroupJoin: every track with all of its plays (possibly none)
var neverPlayed = library.Tracks
    .GroupJoin(library.Plays, track => track.Id, play => play.TrackId,
        (track, trackPlays) => new { Track = track, Plays = trackPlays.Count() })
    .Where(x => x.Plays == 0)
    .Select(x => x.Track.Title)
    .ToList();
Console.WriteLine();
Console.WriteLine($"Tracks never played: {neverPlayed.Count} ({string.Join(", ", neverPlayed.Take(3))}, ...)");

// 4. LeftJoin (.NET 10): artists without any play get null
var silentArtists = library.Artists
    .LeftJoin(infos, artist => artist.Id, info => info.Artist.Id, (artist, info) => new { artist.Name, Info = info })
    .Where(x => x.Info is null)
    .Select(x => x.Name);
Console.WriteLine($"Artists never played: {string.Join(", ", silentArtists)}");
