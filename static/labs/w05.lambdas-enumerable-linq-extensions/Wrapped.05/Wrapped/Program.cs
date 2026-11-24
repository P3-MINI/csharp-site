using System.Globalization;
using Wrapped;

// Format numbers and dates the same way on every machine
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var library = MusicLibrary.Load(Path.Combine(AppContext.BaseDirectory, "Data"));
var infos = library.GetPlayInfos();

// 1. Top lists
Console.WriteLine("Top 5 artists:");
foreach (var (i, (artist, streams, time)) in infos.TopArtists(5).Index())
{
    Console.WriteLine($"{i + 1,3}. {artist.Name,-40}{streams,5}  {time.ToHoursAndMinutes(),12}");
}

Console.WriteLine("Top 5 tracks:");
foreach (var (i, (track, artist, streams)) in infos.TopTracks(5).Index())
{
    Console.WriteLine($"{i + 1,3}. {track.Title + " - " + artist.Name,-40}{streams,5}");
}

var genres = infos.TopGenres(3).Select(g => $"{g.Genre} ({g.Streams})");
Console.WriteLine($"Top genres: {string.Join(", ", genres)}");

// 2. Months: listening time + top artist
var timeByMonth = infos.TimeByMonth();
var topByMonth = infos.TopArtistByMonth().ToDictionary(x => x.Month);
var maxTime = timeByMonth.Values.Max();

Console.WriteLine();
for (int month = 1; month <= 12; month++)
{
    var time = timeByMonth.GetValueOrDefault(month);
    var bar = new string('#', (int)(20 * (time / maxTime)));
    var top = topByMonth[month];
    Console.WriteLine($"{new DateTime(2025, month, 1):MMM} {time.ToHoursAndMinutes(),12} {bar,-20} {top.Artist.Name} ({top.Streams})");
}

// 3. Listening clock: a text histogram
var byHour = infos.ByHour();
int maxCount = byHour.Max(g => g.Count());

Console.WriteLine();
for (int hour = 0; hour < 24; hour++)
{
    int count = byHour[hour].Count(); // an empty sequence for hours without any plays
    Console.WriteLine($"{hour:00}:00 {new string('#', 40 * count / maxCount),-40} {count,3}");
}
