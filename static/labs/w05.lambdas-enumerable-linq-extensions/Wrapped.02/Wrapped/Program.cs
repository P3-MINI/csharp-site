using System.Globalization;
using Wrapped;

// Format numbers and dates the same way on every machine
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var library = MusicLibrary.Load(Path.Combine(AppContext.BaseDirectory, "Data"));

// 1. Deferred execution: nothing happens until the query is enumerated
Console.WriteLine("Building the query...");
var longPlays = library.Plays
    .Tap(p => Console.WriteLine($"  pulled {p.Timestamp:MM-dd HH:mm} ({p.Duration:m\\:ss})"))
    .Filter(Filters.LongerThan(TimeSpan.FromMinutes(3)))
    .Map(p => $"{p.Timestamp:MM-dd HH:mm}");

Console.WriteLine("Enumerating...");
int taken = 0;
foreach (string time in longPlays)
{
    Console.WriteLine($"  got    {time}");
    if (++taken == 2)
    {
        break;
    }
}

// 2. Every enumeration runs the whole pipeline again
int pulled = 0;
var nightPlays = library.Plays.Tap(_ => pulled++).Filter(Filters.AtNight);
Console.WriteLine();
Console.WriteLine($"Night listening: {nightPlays.TotalListeningTime.ToHoursAndMinutes()}, {nightPlays.StreamCount} streams");
Console.WriteLine($"Plays pulled from the source: {pulled}");

pulled = 0;
var nightList = new List<Play>(nightPlays); // enumerates once and stores the results
Console.WriteLine($"Night listening: {nightList.TotalListeningTime.ToHoursAndMinutes()}, {nightList.StreamCount} streams");
Console.WriteLine($"Plays pulled from the source: {pulled}");

// 3. Pairwise: consecutive plays
int repeats = 0;
foreach (var _ in library.Plays.Pairwise().Filter(pair => pair.Previous.TrackId == pair.Current.TrackId))
{
    repeats++;
}
Console.WriteLine();
Console.WriteLine($"Same track twice in a row: {repeats} times");

DateTime breakStart = default, breakEnd = default;
foreach (var (previous, current) in library.Plays.Pairwise())
{
    var end = previous.Timestamp + previous.Duration;
    if (current.Timestamp - end > breakEnd - breakStart)
    {
        (breakStart, breakEnd) = (end, current.Timestamp);
    }
}
Console.WriteLine($"Longest break: {(breakEnd - breakStart).ToHoursAndMinutes()} ({breakStart:yyyy-MM-dd HH:mm} - {breakEnd:yyyy-MM-dd HH:mm})");
