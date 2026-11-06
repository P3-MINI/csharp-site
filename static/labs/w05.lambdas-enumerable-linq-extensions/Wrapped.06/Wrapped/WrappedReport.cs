using System.Text;

namespace Wrapped;

/// <summary>Composes the yearly summary out of the reusable queries.</summary>
public static class WrappedReport
{
    public static string Build(IEnumerable<PlayInfo> history, int year)
    {
        var plays = history.Where(i => i.Play.Timestamp.Year == year);
        var streams = plays.Streams;
        var line = new string('=', 60);
        var report = new StringBuilder();

        report.AppendLine(line);
        report.AppendLine($"YOUR {year} WRAPPED".PadLeft(38));
        report.AppendLine(line);

        var time = plays.Select(i => i.Play).TotalListeningTime;
        report.AppendLine($"Minutes listened   {(int)time.TotalMinutes:N0} ({time.ToHoursAndMinutes()})");
        report.AppendLine($"Streams            {streams.Count():N0} streams of {streams.DistinctBy(i => i.Track).Count()} tracks " +
                          $"by {streams.DistinctBy(i => i.Artist).Count()} artists");

        report.AppendLine();
        report.AppendLine("Top artists");
        foreach (var (i, (artist, count, _)) in plays.TopArtists(5).Index())
        {
            report.AppendLine($"{i + 1,3}. {artist.Name,-45}{count,4} streams");
        }

        report.AppendLine("Top tracks");
        foreach (var (i, (track, artist, count)) in plays.TopTracks(5).Index())
        {
            report.AppendLine($"{i + 1,3}. {track.Title + " - " + artist.Name,-45}{count,4} streams");
        }

        report.AppendLine();
        report.AppendLine($"Top genres         {string.Join(", ", plays.TopGenres(3).Select(g => g.Genre))}");

        var (busiestMonth, monthTime) = plays.TimeByMonth().MaxBy(pair => pair.Value);
        report.AppendLine($"Busiest month      {new DateTime(year, busiestMonth, 1):MMMM} ({monthTime.ToHoursAndMinutes()})");

        int peakHour = plays.ByHour().MaxBy(g => g.Count())!.Key;
        string listener = peakHour switch
        {
            >= 5 and < 12 => "an early bird",
            >= 12 and < 18 => "an afternoon listener",
            >= 18 and < 22 => "an evening listener",
            _ => "a night owl",
        };
        report.AppendLine($"Listening clock    peak at {peakHour:00}:00 - you are {listener}");

        var (first, last) = plays.LongestStreak();
        report.AppendLine($"Longest streak     {last.DayNumber - first.DayNumber + 1} days ({first:yyyy-MM-dd} - {last:yyyy-MM-dd})");

        var session = plays.LongestSession(TimeSpan.FromMinutes(15));
        report.AppendLine($"Longest session    {(session.End - session.Start).ToHoursAndMinutes()} on {session.Start:yyyy-MM-dd} ({session.Plays} plays)");

        var (from, to) = plays.LongestBreak();
        report.AppendLine($"Longest break      {(to - from).TotalDays:F1} days ({from:yyyy-MM-dd} - {to:yyyy-MM-dd})");

        var repeat = plays.OnRepeat();
        report.AppendLine($"On repeat          {repeat.Track.Title} - {repeat.Artist.Name}, {repeat.Streams} times on {repeat.Day:yyyy-MM-dd}");
        report.Append(line);

        return report.ToString();
    }
}
