namespace Wrapped;

/// <summary>Reusable queries over the joined listening history.</summary>
public static class Stats
{
    extension(IEnumerable<PlayInfo> plays)
    {
        /// <summary>Plays that are not skips.</summary>
        public IEnumerable<PlayInfo> Streams => plays.Where(i => !Filters.IsSkip(i.Play));

        /// <summary>Artists with the most streams (GroupBy).</summary>
        public IEnumerable<(Artist Artist, int Streams, TimeSpan Time)> TopArtists(int count) =>
            plays.Streams
                .GroupBy(i => i.Artist)
                .Select(g => (Artist: g.Key, Streams: g.Count(), Time: g.Select(i => i.Play).TotalListeningTime))
                .OrderByDescending(x => x.Streams)
                .ThenBy(x => x.Artist.Name)
                .Take(count);

        /// <summary>Tracks with the most streams (CountBy).</summary>
        public IEnumerable<(Track Track, Artist Artist, int Streams)> TopTracks(int count) =>
            plays.Streams
                .CountBy(i => (i.Track, i.Artist))
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key.Track.Title)
                .Take(count)
                .Select(pair => (pair.Key.Track, pair.Key.Artist, pair.Value));

        /// <summary>Genres with the most streams (SelectMany + CountBy).</summary>
        public IEnumerable<(string Genre, int Streams)> TopGenres(int count) =>
            plays.Streams
                .SelectMany(i => i.Artist.Genres)
                .CountBy(genre => genre)
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key)
                .Take(count)
                .Select(pair => (pair.Key, pair.Value));

        /// <summary>Listening time in every month (AggregateBy).</summary>
        public Dictionary<int, TimeSpan> TimeByMonth() =>
            plays.AggregateBy(i => i.Play.Timestamp.Month, TimeSpan.Zero, (total, i) => total + i.Play.Duration)
                .ToDictionary();

        /// <summary>The most streamed artist of every month (grouping + reuse of TopArtists).</summary>
        public IEnumerable<(int Month, Artist Artist, int Streams)> TopArtistByMonth() =>
            from info in plays.Streams
            group info by info.Play.Timestamp.Month into month
            orderby month.Key
            let top = month.TopArtists(1).First()
            select (month.Key, top.Artist, top.Streams);

        /// <summary>Plays grouped by the hour of the day (ToLookup).</summary>
        public ILookup<int, PlayInfo> ByHour() => plays.ToLookup(i => i.Play.Timestamp.Hour);

        /// <summary>The longest run of consecutive days with at least one play.</summary>
        public (DateOnly First, DateOnly Last) LongestStreak()
        {
            var streak = plays
                .Select(i => DateOnly.FromDateTime(i.Play.Timestamp))
                .Distinct()
                .Order()
                .SplitWhen((previous, next) => next.DayNumber - previous.DayNumber > 1)
                .MaxBy(days => days.Count)!;
            return (streak[0], streak[^1]);
        }

        /// <summary>The longest listening session: plays separated by gaps of at most <paramref name="maxGap"/>.</summary>
        public (DateTime Start, DateTime End, int Plays) LongestSession(TimeSpan maxGap)
        {
            var session = plays
                .Select(i => i.Play)
                .OrderBy(p => p.Timestamp)
                .SplitWhen((previous, next) => next.Timestamp - (previous.Timestamp + previous.Duration) > maxGap)
                .MaxBy(s => s[^1].Timestamp + s[^1].Duration - s[0].Timestamp)!;
            return (session[0].Timestamp, session[^1].Timestamp + session[^1].Duration, session.Count);
        }

        /// <summary>The longest time without any music.</summary>
        public (DateTime From, DateTime To) LongestBreak() =>
            plays
                .Select(i => i.Play)
                .OrderBy(p => p.Timestamp)
                .Pairwise()
                .Select(pair => (From: pair.Previous.Timestamp + pair.Previous.Duration, To: pair.Current.Timestamp))
                .MaxBy(gap => gap.To - gap.From);

        /// <summary>The track streamed most times during a single day.</summary>
        public (Track Track, Artist Artist, DateOnly Day, int Streams) OnRepeat()
        {
            var (key, streams) = plays.Streams
                .CountBy(i => (i.Track, i.Artist, Day: DateOnly.FromDateTime(i.Play.Timestamp)))
                .MaxBy(pair => pair.Value);
            return (key.Track, key.Artist, key.Day, streams);
        }
    }
}
