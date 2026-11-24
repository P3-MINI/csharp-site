using System.Globalization;
using Wrapped;

// Format numbers and dates the same way on every machine
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var library = MusicLibrary.Load(Path.Combine(AppContext.BaseDirectory, "Data"));
var plays = library.Plays;

// A lambda assigned to an Action - a small reusable "printer"
Action<string, int> print = (label, value) => Console.WriteLine($"{label,-30}{value,6}");

print("All plays", plays.Count);
print("Skips", Count(plays, Filters.IsSkip));                                   // method group
print("Weekend plays", Count(plays, Filters.IsWeekend));
print("Night plays", Count(plays, Filters.AtNight));                            // delegate field
print("Weekend nights", Count(plays, Filters.All(Filters.IsWeekend, Filters.AtNight)));
print("Night streams", Count(plays, Filters.All(Filters.AtNight, Filters.Not(Filters.IsSkip))));
print("Longer than 6 minutes", Count(plays, Filters.LongerThan(TimeSpan.FromMinutes(6))));
print("Ada & The Lambdas", Count(plays, Filters.ByArtist(library, "Ada & The Lambdas")));
print("Plays at 7 a.m.", Count(plays, p => p.Timestamp.Hour == 7));             // inline lambda

// List<T>.RemoveAll expects a Predicate<Play>, not a Func<Play, bool>
List<Play> streams = [.. plays];
int removed = streams.RemoveAll(Filters.IsSkip);         // a method group converts to any compatible delegate type
// streams.RemoveAll(Filters.AtNight);                   // error CS1503: cannot convert Func<Play, bool> to Predicate<Play>
streams.RemoveAll(new Predicate<Play>(Filters.AtNight)); // explicit conversion works
print("Removed skips", removed);
print("Day streams", streams.Count);

// Pitfall: all lambdas capture the same loop variable
var monthFilters = new List<Func<Play, bool>>();
for (int month = 1; month <= 12; month++)
{
    monthFilters.Add(p => p.Timestamp.Month == month);
}
PrintMonths("Plays per month (bug):", monthFilters);

// Fix: a fresh variable in every iteration (here: the parameter of InMonth)
monthFilters.Clear();
for (int month = 1; month <= 12; month++)
{
    monthFilters.Add(Filters.InMonth(month));
}
PrintMonths("Plays per month (fixed):", monthFilters);

void PrintMonths(string title, List<Func<Play, bool>> filters)
{
    Console.WriteLine(title);
    for (int i = 0; i < filters.Count; i++)
    {
        Console.Write($" {new DateTime(2025, i + 1, 1):MMM} {Count(plays, filters[i])}");
    }
    Console.WriteLine();
}

static int Count(IEnumerable<Play> plays, Func<Play, bool> predicate)
{
    int count = 0;
    foreach (var play in plays)
    {
        if (predicate(play))
        {
            count++;
        }
    }
    return count;
}
