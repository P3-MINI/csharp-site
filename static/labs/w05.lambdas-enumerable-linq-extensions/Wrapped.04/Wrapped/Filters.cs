namespace Wrapped;

/// <summary>Reusable filters (predicates) over the listening history.</summary>
public static class Filters
{
    /// <summary>A play shorter than this is a skip, not a stream.</summary>
    public static readonly TimeSpan SkipThreshold = TimeSpan.FromSeconds(30);

    // Ordinary static methods - they can be passed around as method groups
    public static bool IsSkip(Play play) => play.Duration < SkipThreshold;

    public static bool IsWeekend(Play play) =>
        play.Timestamp.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    // A lambda stored in a delegate field; 'static' guarantees that it captures nothing
    public static readonly Func<Play, bool> AtNight =
        static play => play.Timestamp.Hour is >= 22 or < 6;

    // Factory methods returning closures - the lambdas capture the method parameters
    public static Func<Play, bool> InMonth(int month) =>
        play => play.Timestamp.Month == month;

    public static Func<Play, bool> LongerThan(TimeSpan duration) =>
        play => play.Duration > duration;

    public static Func<Play, bool> ByArtist(MusicLibrary library, string artistName)
    {
        // Computed once, then captured by the returned lambda
        var trackIds = new HashSet<int>();
        foreach (var artist in library.Artists)
        {
            if (artist.Name != artistName)
            {
                continue;
            }
            foreach (var track in library.Tracks)
            {
                if (track.ArtistId == artist.Id)
                {
                    trackIds.Add(track.Id);
                }
            }
        }
        return play => trackIds.Contains(play.TrackId);
    }

    // Combinators - build new filters out of existing ones
    public static Func<Play, bool> Not(Func<Play, bool> filter) => play => !filter(play);

    public static Func<Play, bool> All(params Func<Play, bool>[] filters) => play =>
    {
        foreach (var filter in filters)
        {
            if (!filter(play))
            {
                return false;
            }
        }
        return true;
    };

    public static Func<Play, bool> Any(params Func<Play, bool>[] filters) => play =>
    {
        foreach (var filter in filters)
        {
            if (filter(play))
            {
                return true;
            }
        }
        return false;
    };
}
