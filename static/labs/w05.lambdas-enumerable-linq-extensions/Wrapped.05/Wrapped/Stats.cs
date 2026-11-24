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
    }
}
