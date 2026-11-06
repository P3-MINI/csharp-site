namespace Wrapped;

/// <summary>Extension members (C# 14 extension blocks).</summary>
public static class ListeningExtensions
{
    // Members available on every IEnumerable<Play>
    extension(IEnumerable<Play> plays)
    {
        /// <summary>Total time of all plays (an extension property).</summary>
        public TimeSpan TotalListeningTime
        {
            get
            {
                var total = TimeSpan.Zero;
                foreach (var play in plays)
                {
                    total += play.Duration;
                }
                return total;
            }
        }

        /// <summary>Number of plays that are not skips.</summary>
        public int StreamCount
        {
            get
            {
                int count = 0;
                foreach (var _ in plays.Filter(Filters.Not(Filters.IsSkip)))
                {
                    count++;
                }
                return count;
            }
        }
    }

    // Members available on TimeSpan
    extension(TimeSpan time)
    {
        /// <summary>Formats the time span as e.g. "12 h 05 min".</summary>
        public string ToHoursAndMinutes() => $"{(int)time.TotalHours} h {time.Minutes:00} min";
    }
}
