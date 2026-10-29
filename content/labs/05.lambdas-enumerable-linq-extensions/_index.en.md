---
title: "Lambda expressions, IEnumerable, LINQ, extension methods"
weight: 50
---

## Tutorial 5: Lambda Expressions, IEnumerable, LINQ, Extension Methods

The scope of this laboratory covers delegates and lambda expressions, extension methods, iterators (`IEnumerable<T>` and the `yield` statement) and LINQ (*Language Integrated Query*) – the standard library of operators for querying sequences of data.

These features are closely connected: LINQ operators are *extension methods* defined for `IEnumerable<T>`, most of them are implemented as *iterators*, and their behavior is specified with *lambda expressions*. That is why in the task we will first build each of these pieces ourselves, and only then switch to the ready-made LINQ operators.

### Delegates

A delegate is a type-safe reference to a method – the C# counterpart of a function pointer or `std::function` from C++. You rarely need to declare your own delegate types, because the standard library provides generic ones:

- `Func<T1, ..., TResult>` – a function taking up to 16 arguments and returning a `TResult` value,
- `Action<T1, ...>` – a function that does not return a value (`void`),
- `Predicate<T>` – a function `T → bool`, used by some older APIs (e.g. `List<T>.RemoveAll`).

```csharp
Func<int, int, int> add = Add;          // method group
Action<string> log = Console.WriteLine; // another method group
Predicate<int> isEven = n => n % 2 == 0;

log($"2 + 3 = {add(2, 3)}, 4 is even: {isEven(4)}"); // Output: 2 + 3 = 5, 4 is even: True

static int Add(int a, int b) => a + b;
```

### Lambda Expressions

A lambda expression is an anonymous function written as `(parameters) => expression` or `(parameters) => { statements }`. The types of the parameters are usually inferred from the delegate type the lambda is assigned to:

```csharp
Func<double, double> square = x => x * x;
Func<int, int, bool> divides = (d, n) => n % d == 0;
Action greet = () => Console.WriteLine("Hello!");
var cube = (double x) => x * x * x; // inferred type: Func<double, double>
```

Unlike in C++, a lambda has no capture list – it can use any local variable or parameter that is in scope. The compiler captures **the variable itself** (not a copy of its value) and moves it to a hidden object on the heap. As a result, the variable lives as long as the lambda does (a lambda never refers to a dangling variable), and the lambda always sees its current value – similarly to capturing by reference (`[&]`) in C++:

```csharp
int counter = 0;
Action increment = () => counter++;
increment();
increment();
Console.WriteLine(counter); // Output: 2
```

You can read more in the lecture on [delegates and lambda expressions]({{< ref "/lectures/05.csharp-features/delegates" >}}).

### Extension Methods

Extension methods let you add methods to an existing type – also to an interface or a type whose code you cannot modify – without inheritance. They are static methods defined in a static class, whose first parameter is marked with the `this` keyword. They are called as if they were instance methods of the extended type. Since C# 14, *extension blocks* additionally allow defining extension properties, static members and operators:

```csharp
public static class StringExtensions
{
    // Classic extension method
    public static string Shout(this string text) => text.ToUpper() + "!";

    // C# 14 extension block - all members share the receiver 'text'
    extension(string text)
    {
        public bool IsBlank => string.IsNullOrWhiteSpace(text); // extension property
    }
}
```

```csharp
Console.WriteLine("hello".Shout()); // Output: HELLO!
Console.WriteLine("   ".IsBlank);   // Output: True
```

You can read more in the lecture on [extension methods]({{< ref "/lectures/05.csharp-features/extension-methods" >}}).

### Iterators and `IEnumerable<T>`

`IEnumerable<T>` is the basic abstraction of a sequence in C# – everything you can iterate over with `foreach` (similar to a range in C++). The simplest way to create your own sequence is an *iterator method*: a method returning `IEnumerable<T>` that produces elements with the `yield return` statement. The compiler turns such a method into a state machine – its code is executed lazily, piece by piece, whenever the consumer asks for the next element (just like C++20 coroutines, e.g. `std::generator` from C++23):

```csharp
var numbers = Countdown(3); // nothing is printed yet
foreach (int n in numbers)  // prints "start", then 3, 2, 1
{
    Console.WriteLine(n);
}

static IEnumerable<int> Countdown(int from)
{
    Console.WriteLine("start");
    for (int i = from; i > 0; i--)
    {
        yield return i;
    }
}
```

You can read more in the lecture on [enumerators]({{< ref "/lectures/06.collections/enumerators" >}}).

### LINQ

LINQ is a set of extension methods for `IEnumerable<T>` defined in the `System.Linq` namespace (imported automatically thanks to `ImplicitUsings`): filtering (`Where`), projection (`Select`), sorting (`OrderBy`), grouping (`GroupBy`), joining (`Join`), aggregation (`Count`, `Sum`, `Max`) and many more. Queries can be written in two equivalent ways – the compiler translates the query syntax into calls of the same methods:

```csharp
int[] values = [5, 3, 8, 1, 4];

// Method syntax
var methodSyntax = values.Where(n => n > 2).OrderBy(n => n).Select(n => n * 10);

// Query syntax
var querySyntax = from n in values
                  where n > 2
                  orderby n
                  select n * 10;

// Both queries produce: 30, 40, 50, 80
```

Most LINQ operators use **deferred execution**: building a query does not execute it – it is executed only when the result is enumerated. Operators returning a single value (`Count`, `Sum`, `First`, …) or a collection (`ToList`, `ToArray`, `ToDictionary`, …) execute the query immediately.

You can read more in the lecture on [LINQ]({{< ref "/lectures/06.collections/linq" >}}).

## Task 1 - Wrapped: Listening History Analyzer

Every December, music streaming services present their users with a personalized summary of the year: minutes listened, favorite artists, tracks and genres. In this task, you will build such a summary (we will call it *Wrapped*) for a listening history stored in CSV files. The application will consist of a single project – a console application.

We start with code that only loads the data. Then, stage by stage, you will write filters based on delegates, your own iterators and extension methods, LINQ queries, and finally combine them into the yearly report.

#### Starting Code

> [!NOTE]
> #### Starting code
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.00" >}}

The `Data` directory contains a generated listening history of one (fictional) user in 2025:

- `artists.csv` – 40 artists: `id,name,genres` (genres are separated with `;`),
- `tracks.csv` – 268 tracks from the user's library: `id,title,artist_id,length_s`,
- `plays.csv` – 3351 plays sorted by time: `timestamp,track_id,ms_played`.

```text
timestamp,track_id,ms_played
2025-01-02T07:37:43,247,211000
2025-01-02T07:41:16,103,1819
```

The starting code contains:

- The records `Artist`, `Track` and `Play`. `Play` has an additional computed property `Duration` (the time the track was played, as a `TimeSpan`).
- The `MusicLibrary` class with read-only lists `Artists`, `Tracks` and `Plays`, and the static method `Load(string directory)` loading the data. Note the private method `ReadCsv<T>(string path, Func<string[], T> parse)` – it already uses a delegate: the caller decides how a row is converted into an object by passing a lambda expression.
- `Program.cs`, which loads the data and prints the number of loaded objects and the first play. The line `CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;` makes numbers and dates formatted identically on every machine, so that your output can be compared with the expected outputs below.

Data files are copied to the output directory during the build (see `Wrapped.csproj`). Run the program (`dotnet run` in the `Wrapped` directory):

```text
Loaded 40 artists, 268 tracks and 3351 plays.
First play: Play { Timestamp = 01/02/2025 07:37:43, TrackId = 247, MsPlayed = 211000, Duration = 00:03:31 }
```

> [!TIP]
> The compiler generates a `ToString()` method for records, which prints the values of all public properties. That is why `Console.WriteLine` prints a readable description of a `Play` object.

### Stage 1: Filters – Delegates and Lambdas

In this stage, we will create a set of reusable *filters* – functions of type `Func<Play, bool>` that decide whether a play satisfies a given condition – and learn the different ways of creating delegates. Do not use LINQ yet.

> [!IMPORTANT]
> #### Method Groups, Lambdas and Closures
>
> A delegate can be created from:
> - a **method group** – the name of an existing method, e.g. `Func<Play, bool> f = Filters.IsSkip;`,
> - a **lambda expression**, e.g. `Func<Play, bool> f = p => p.MsPlayed < 1000;`.
>
> A lambda that uses variables or parameters from the enclosing scope is called a **closure**. Methods returning closures act as *delegate factories* – each call creates a new function "configured" with the given arguments:
> ```csharp
> public static Func<Play, bool> InMonth(int month) =>
>     play => play.Timestamp.Month == month; // captures the parameter 'month'
> ```
> If a lambda should not capture anything, mark it with the `static` modifier. The compiler then reports an error (CS8820 or CS8821) whenever the lambda tries to use a local variable, a parameter or `this`, which protects you against accidental captures (and the hidden heap allocations that come with them):
> ```csharp
> public static readonly Func<Play, bool> AtNight =
>     static play => play.Timestamp.Hour is >= 22 or < 6;
> ```

#### The Filters Class

Create a file named `Filters.cs` with a static class `Filters` containing:

1. A field `public static readonly TimeSpan SkipThreshold` equal to 30 seconds. Streaming services count a play as a *stream* only if it lasted at least 30 seconds; shorter plays are *skips*.
2. Ordinary static methods `bool IsSkip(Play play)` (play shorter than `SkipThreshold`) and `bool IsWeekend(Play play)` (play on Saturday or Sunday).
3. The `AtNight` field shown above – a `static` lambda stored in a `Func<Play, bool>` field (plays between 22:00 and 6:00).
4. Factory methods returning closures:
   - `Func<Play, bool> InMonth(int month)` (shown above),
   - `Func<Play, bool> LongerThan(TimeSpan duration)` – plays longer than `duration`,
   - `Func<Play, bool> ByArtist(MusicLibrary library, string artistName)` – plays of the artist's tracks. Before returning the lambda, collect the identifiers of the artist's tracks into a `HashSet<int>` using loops. The returned lambda captures the set, so it is computed only once, and each check is just `trackIds.Contains(play.TrackId)`.
5. Combinators creating new filters from existing ones:
   - `Func<Play, bool> Not(Func<Play, bool> filter)` – negation,
   - `Func<Play, bool> All(params Func<Play, bool>[] filters)` – true if all filters are satisfied,
   - `Func<Play, bool> Any(params Func<Play, bool>[] filters)` – true if at least one filter is satisfied.

#### Counting Plays

In `Program.cs`:

1. Add a static local function `int Count(IEnumerable<Play> plays, Func<Play, bool> predicate)` which counts the plays satisfying the predicate with a `foreach` loop. (In Stage 3 we will replace it with the LINQ `Count` method.)
2. Create a lambda `Action<string, int> print` printing a label and a value in aligned columns, e.g. `$"{label,-30}{value,6}"`.
3. Using `Count`, `print` and the filters, print the number of: all plays, skips, weekend plays, night plays, weekend nights (`All`), night streams (`All` + `Not`), plays longer than 6 minutes, plays of *Ada & The Lambdas*, and plays at 7 a.m. (pass an inline lambda to `Count`).

> [!TIP]
> #### `Func<Play, bool>` vs `Predicate<Play>`
>
> `List<T>.RemoveAll` expects a `Predicate<T>`. Although `Predicate<Play>` and `Func<Play, bool>` have identical signatures, they are **different, incompatible types**. A method group or a lambda can be converted to any delegate type with a compatible signature, but an existing delegate object cannot:
> ```csharp
> List<Play> streams = [.. plays];
> int removed = streams.RemoveAll(Filters.IsSkip);         // OK - method group
> // streams.RemoveAll(Filters.AtNight);                   // error CS1503: cannot convert Func<Play, bool> to Predicate<Play>
> streams.RemoveAll(new Predicate<Play>(Filters.AtNight)); // OK - explicit conversion
> ```

4. Copy the plays into a `List<Play>` (as shown above), remove skips and then night plays from it using `RemoveAll`, and print the number of removed skips and the number of remaining (day) streams.

#### The Captured Loop Variable

Finally, let us count the plays in each month. Create a list of 12 filters in a `for` loop and print the result of `Count` for each of them in one line (use the `{date:MMM}` format to get abbreviated month names):

```csharp
var monthFilters = new List<Func<Play, bool>>();
for (int month = 1; month <= 12; month++)
{
    monthFilters.Add(p => p.Timestamp.Month == month);
}
```

All counts are equal to 0. Why?

> [!WARNING]
> #### Capturing a Loop Variable
>
> All twelve lambdas capture **the same** variable `month` – the variable itself, not its value at the moment the lambda was created. The lambdas are executed after the loop has finished, when `month` is already equal to 13. In a `for` loop, there is only one loop variable for the whole loop. (A `foreach` loop creates a new variable in every iteration, so it does not have this problem since C# 5.)
>
> The fix is to capture a variable that is created anew in each iteration: copy the loop variable to a local variable declared inside the loop body (`int m = month;`), or – even better – use our factory `Filters.InMonth(month)`, whose parameter is a separate variable for every call.

Fix the loop with `Filters.InMonth` and print the counts again. Expected output:

```text
All plays                       3351
Skips                            446
Weekend plays                    955
Night plays                      875
Weekend nights                   324
Night streams                    760
Longer than 6 minutes            155
Ada & The Lambdas                201
Plays at 7 a.m.                  368
Removed skips                    446
Day streams                     2145
Plays per month (bug):
 Jan 0 Feb 0 Mar 0 Apr 0 May 0 Jun 0 Jul 0 Aug 0 Sep 0 Oct 0 Nov 0 Dec 0
Plays per month (fixed):
 Jan 273 Feb 298 Mar 299 Apr 277 May 233 Jun 301 Jul 274 Aug 262 Sep 285 Oct 294 Nov 326 Dec 229
```

> [!NOTE]
> #### Stage 1
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.01" >}}

### Stage 2: Custom Iterators and Extension Members

In this stage, we will write our own LINQ-like operators as extension methods implemented with iterators, observe how deferred execution works, and add extension members using the new C# 14 syntax.

#### The EnumerableExtensions Class

Create a file named `EnumerableExtensions.cs` with a static class `EnumerableExtensions`. All methods are generic extension methods for `IEnumerable<T>`. The first one – our own counterpart of `Where` – looks as follows:

```csharp
public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    foreach (var item in source)
    {
        if (predicate(item))
        {
            yield return item;
        }
    }
}
```

Implement analogously:

1. `IEnumerable<TResult> Map<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)` – our counterpart of `Select`, returning `selector(item)` for each element.
2. `IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)` – calls `action` for each element and passes the element on unchanged. It will let us observe (and count) the elements flowing through a query.
3. `IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)` – returns pairs of consecutive elements: for `a, b, c, d` it returns `(a, b), (b, c), (c, d)`. For a sequence with fewer than two elements, it returns an empty sequence.

> [!IMPORTANT]
> #### Using an Enumerator Directly
>
> A `foreach` loop is syntactic sugar: it calls `GetEnumerator()`, and then calls `MoveNext()` and reads `Current` until `MoveNext()` returns `false`. Sometimes it is more convenient to drive the enumerator manually, e.g. to handle the first element differently:
> ```csharp
> using var enumerator = source.GetEnumerator(); // IEnumerator<T> is IDisposable
> if (!enumerator.MoveNext())
> {
>     yield break; // empty sequence - end the iterator
> }
> T previous = enumerator.Current;
> // ... continue with a while (enumerator.MoveNext()) loop
> ```
> `yield break` ends the iteration (`MoveNext()` of the generated enumerator returns `false`).

Implement `Pairwise` using an enumerator directly, as shown above.

> [!TIP]
> #### Argument Validation in Iterators
>
> The whole body of an iterator method – including argument checks at its beginning – runs only when the enumeration starts, not when the method is called. If an error should be reported immediately, check the arguments in an ordinary method and move the `yield` part to a local iterator function. For example, a sliding window (a generalization of `Pairwise`):
> ```csharp
> public static IEnumerable<T[]> Window<T>(this IEnumerable<T> source, int size)
> {
>     ArgumentOutOfRangeException.ThrowIfLessThan(size, 1); // checked immediately
>     return Iterator();
>
>     IEnumerable<T[]> Iterator() // a local iterator function
>     {
>         var window = new Queue<T>(size);
>         foreach (var item in source)
>         {
>             window.Enqueue(item);
>             if (window.Count > size)
>             {
>                 window.Dequeue();
>             }
>             if (window.Count == size)
>             {
>                 yield return window.ToArray();
>             }
>         }
>     }
> }
> ```

#### Extension Members

> [!IMPORTANT]
> #### Extension Blocks (C# 14)
>
> An extension block `extension(Receiver receiver) { ... }`, placed inside a static class, groups extension members for one receiver type. Besides methods, it can contain extension **properties**, static members and operators – things that cannot be expressed with the classic `this` parameter syntax. The receiver can be any type, including a constructed generic interface such as `IEnumerable<Play>`:
> ```csharp
> public static class ListeningExtensions
> {
>     extension(IEnumerable<Play> plays)
>     {
>         // Extension property
>         public TimeSpan TotalListeningTime
>         {
>             get
>             {
>                 var total = TimeSpan.Zero;
>                 foreach (var play in plays)
>                 {
>                     total += play.Duration;
>                 }
>                 return total;
>             }
>         }
>     }
> }
> ```
> Both syntaxes produce ordinary static methods and can be freely mixed – the classic syntax is still the most common one in existing code.

Create a file named `ListeningExtensions.cs` with a static class `ListeningExtensions` containing two extension blocks:

1. For `IEnumerable<Play>`:
   - the property `TimeSpan TotalListeningTime` – the sum of `Duration` of all plays (shown above),
   - the property `int StreamCount` – the number of plays which are not skips (use your `Filter` method and the filters from Stage 1).
2. For `TimeSpan`: the method `string ToHoursAndMinutes()`, which formats the time span as e.g. `"12 h 05 min"` (whole hours from `TotalHours` and minutes from `Minutes`).

#### Deferred Execution in Practice

Replace the contents of `Program.cs` (after loading the library) with three experiments.

**1. Lazy evaluation.** Build a query and enumerate only its first two elements:

```csharp
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
```

Notice in the output that nothing happens while the query is being built, that the elements flow through `Tap`, `Filter` and `Map` one by one, and that only three out of 3351 plays are ever read.

**2. Multiple enumeration.** Count how many elements are pulled from the source by a query which is used twice:

```csharp
int pulled = 0;
var nightPlays = library.Plays.Tap(_ => pulled++).Filter(Filters.AtNight);
```

Print `nightPlays.TotalListeningTime.ToHoursAndMinutes()` and `nightPlays.StreamCount`, and then the value of `pulled`. Then reset `pulled` to 0, store the results of the query in a list (`new List<Play>(nightPlays)`), print both properties for the list and the value of `pulled` again.

> [!WARNING]
> #### Every Enumeration Runs the Query Again
>
> A variable holding a query does not hold its results – it holds a *recipe* for computing them. Each `foreach` (and each operator that consumes the sequence, such as our extension properties) runs the whole pipeline from the beginning. In our example, every play is pulled from the source twice. If the source is expensive (reading a file, a database query, a complex join) or non-deterministic (e.g. random numbers), enumerate the query once and store its results in a collection.

**3. Pairwise.** Using `Pairwise` (combined with `Filter`), count how many times the same track was played twice in a row. Then find the longest break in listening – the largest gap between the end of a play (`Timestamp + Duration`) and the start of the next one – and print it together with its start and end. Deconstruct the pairs directly in the loop: `foreach (var (previous, current) in library.Plays.Pairwise())`.

Expected output:

```text
Building the query...
Enumerating...
  pulled 01-02 07:37 (3:31)
  got    01-02 07:37
  pulled 01-02 07:41 (0:01)
  pulled 01-02 07:41 (3:29)
  got    01-02 07:41

Night listening: 49 h 51 min, 760 streams
Plays pulled from the source: 6702
Night listening: 49 h 51 min, 760 streams
Plays pulled from the source: 3351

Same track twice in a row: 276 times
Longest break: 252 h 11 min (2025-08-03 19:45 - 2025-08-14 07:57)
```

> [!NOTE]
> #### Stage 2
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.02" >}}

### Stage 3: First LINQ Queries

`Filter` and `Map` from the previous stage are simplified versions of LINQ's `Where` and `Select` (the real implementations only add optimizations, e.g. for arrays and lists). From now on, we will use LINQ. Replace the contents of `Program.cs` (after loading the library) with the queries described below.

#### Simple Aggregates

Our filters are ordinary `Func<Play, bool>` delegates, so they can be passed directly to LINQ operators. Define the sequence of streams (it is a deferred query, which we will reuse several times):

```csharp
var plays = library.Plays;
var streams = plays.Where(Filters.Not(Filters.IsSkip));
```

Print, using one LINQ expression for each value:

1. The number of plays, streams and skips (`Count()`, `Count(predicate)`).
2. Minutes listened – the sum of `MsPlayed` divided by 60 000 (`Sum`). Cast `MsPlayed` to `long` in the selector – `Sum` uses checked arithmetic and would throw `OverflowException` for larger data sets.
3. The average play duration in seconds (`Average`).
4. The number of distinct tracks streamed (`Select` + `Distinct` + `Count`) and the number of listening days (distinct `DateOnly.FromDateTime(p.Timestamp)`).
5. The time of the first and the last play (`Min`, `Max`).
6. Whether there was any play at 4 a.m. (`Any`) and whether all plays are from 2025 (`All`).
7. The longest play (`MaxBy`) with the title of its track (`library.Tracks.First(...)`).

> [!TIP]
> `MaxBy`/`MinBy` return the whole element with the largest/smallest key, while `Max`/`Min` return only the key. `First` throws an exception if no element matches, while `FirstOrDefault` returns `default` (`null` for reference types) in that case.

#### Top 10 Tracks

> [!IMPORTANT]
> #### Query Syntax
>
> The query syntax is translated by the compiler into calls of LINQ methods: `where` → `Where`, `select` → `Select`, `orderby a descending, b` → `OrderByDescending(...).ThenBy(...)`. The `let` clause introduces a helper variable – under the hood, the compiler projects each element into an anonymous object holding the element and the computed value. Operators without a keyword (e.g. `Take`, `Count`, `MaxBy`) must be called as methods:
> ```csharp
> var topTracksQuery =
>     (from t in library.Tracks
>      let count = streams.Count(p => p.TrackId == t.Id)
>      orderby count descending, t.Title
>      select new { Track = t, Streams = count })
>     .Take(10);
> ```
> `new { Track = t, Streams = count }` creates an object of an **anonymous type** – an immutable class generated by the compiler, with read-only properties and value-based equality.

Write the same query in method syntax: project each track into an anonymous object with the number of streams (`Select`), sort it descending by the number of streams and then by title (`OrderByDescending`, `ThenBy`), and take the first 10 elements (`Take`). Print the result with positions and artist names (find the artist with `library.Artists.First(...)`). To get the position of each element, use the `Index()` method (.NET 9), which returns `(int Index, T Item)` tuples:

```csharp
foreach (var (i, x) in topTracks.Index())
{
    // ...
}
```

Then check that both queries give the same result with `SequenceEqual` (it works thanks to the value equality of anonymous types).

#### Top Artist

Find the artist with the most streams: for each artist, sum (`Sum`) the numbers of streams of their tracks (`Where`), and choose the maximum with `MaxBy`.

> [!WARNING]
> #### Hidden Quadratic Complexity
>
> The queries above are concise, but `streams.Count(...)` inside `Select` runs over all plays **for every track** – 268 × 3351 ≈ 900 000 predicate calls, and each of them also re-runs the deferred `Where`. For our data, it still takes a fraction of a second, but such queries become a problem for larger data. In Stage 5 we will compute the same results in a single pass using grouping.

Expected output:

```text
Plays:            3351 (2905 streams, 446 skips)
Minutes listened: 11465
Average play:     205.3 s
Distinct tracks:  239
Listening days:   270
First and last:   2025-01-02 07:37, 2025-12-30 23:08
Any play at 4 am: False
All in 2025:      True
Longest play:     Little Hours (8:00) on 2025-01-14

Top 10 tracks:
  1. Winter Letters - Static Bloom                 134
  2. Sleepless Roads - Static Bloom                 89
  3. Lazy Satellites - Pixel Monsoon                83
  4. Analog Memories - Lena Marlowe                 73
  5. Burning Horizons - Concrete Gardens            63
  6. Crystal River - Static Bloom                   59
  7. Cosmic Cities - Pixel Monsoon                  57
  8. Lazy Evaluation - Ada & The Lambdas            54
  9. Digital Hours - Mira Volta                     53
 10. Faded Horizons - Static Bloom                  52
Query syntax gives the same result: True
Top artist: Static Bloom (430 streams)
```

> [!NOTE]
> #### Stage 3
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.03" >}}

### Stage 4: Joining Data

Plays contain only track identifiers, and tracks contain only artist identifiers. So far, we have looked up related objects with `First`, which scans the whole list for every element. In this stage, we will join the three collections, as in a relational database.

> [!IMPORTANT]
> #### Join
>
> `Join` combines elements of two sequences with equal keys (like `INNER JOIN` in SQL). It takes the inner sequence, two key selectors and a function creating the result from a matching pair:
> ```csharp
> var playsWithTracks = library.Plays.Join(
>     library.Tracks,               // inner sequence
>     play => play.TrackId,         // key of the outer element
>     track => track.Id,            // key of the inner element
>     (play, track) => (play, track));
> ```
> In query syntax, the same join is written as `join track in library.Tracks on play.TrackId equals track.Id`. `Join` first builds a hash table of the inner sequence, so the whole join takes linear time instead of the quadratic time of nested searches. Elements without a match are skipped.

#### Joined Plays

1. Create a file named `PlayInfo.cs` with the record `PlayInfo(Play Play, Track Track, Artist Artist)`.
2. In `MusicLibrary`, add the method `IEnumerable<PlayInfo> GetPlayInfos()`, which joins `Plays` with `Tracks` and then with `Artists`. Write it in query syntax (with two `join` clauses) – for multiple joins, it is much more readable than method syntax.
3. In `Program.cs`, store the joined plays in a variable (`var infos = library.GetPlayInfos();`) and print the first three of them (`Take`) in the format shown in the expected output.
4. Write the same join in method syntax (two chained `Join` calls) and check with `SequenceEqual` that both versions give the same result. Records, like anonymous types, have value-based equality.

#### Genres

> [!IMPORTANT]
> #### SelectMany
>
> `SelectMany` flattens a sequence of sequences: for each element it gets a sequence and concatenates all of them into one (similarly to `flatMap` in other languages). In query syntax, it corresponds to a second `from` clause:
> ```csharp
> var artistGenres =
>     from artist in library.Artists
>     from genre in artist.Genres
>     select (artist.Name, genre); // one element for every (artist, genre) pair
> ```

1. Print all genres in the library in alphabetical order (`SelectMany` + `Distinct` + `Order`) as a comma-separated list (`string.Join`).
2. Print the names of jazz artists – use query syntax with two `from` clauses and a `where` clause.
3. Print the total time of jazz listening: filter the joined plays by the artist's genres (`Contains`), take the plays (`Select(i => i.Play)`) and use the `TotalListeningTime` extension property from Stage 2.

#### What Was Never Played?

> [!IMPORTANT]
> #### GroupJoin and LeftJoin
>
> `GroupJoin` pairs every element of the outer sequence with a sequence of *all* matching inner elements – possibly empty. It is the basis of a left outer join (in query syntax: `join ... into group`).
>
> .NET 10 introduced `LeftJoin` (and `RightJoin`), which directly implements a `LEFT JOIN`: every outer element appears in the result, and if it has no match, the inner element passed to the result selector is `null` (`default`):
> ```csharp
> var result = library.Artists.LeftJoin(
>     infos,
>     artist => artist.Id,
>     info => info.Artist.Id,
>     (artist, info) => new { artist.Name, Info = info }); // info is null for artists without plays
> ```
> Before .NET 10, the same effect required `GroupJoin` + `SelectMany` + `DefaultIfEmpty`.

1. Using `GroupJoin` of `library.Tracks` with `library.Plays`, compute the number of plays of every track, select the titles of tracks which were never played, and store them in a list (`ToList`). Print their number and the first three titles.
2. Using `LeftJoin` of `library.Artists` with the joined plays, find the artists that were never played.

> [!TIP]
> The tracks that were never played can also be found with a set operation: `library.Tracks.ExceptBy(library.Plays.Select(p => p.TrackId), t => t.Id)`.

Expected output:

```text
First plays:
  2025-01-02 07:37  Paper Satellites - Thunder Index (3:31)
  2025-01-02 07:41  Sleepless Streets - Ruby Fieldnotes (0:01)
  2025-01-02 07:41  Lunar Hours - Kasia Wren (3:29)
Method syntax gives the same result: True

Genres: ambient, classical, electronic, folk, hip-hop, indie, jazz, metal, pop, r&b, rock, synthwave
Jazz artists: Northern Lights Quartet, Blue Hour Trio, Moss & Marble
Jazz listening time: 5 h 26 min

Tracks never played: 26 (Sweet Dreams, Velvet Heart, Distant Oceans, ...)
Artists never played: The Null References
```

> [!NOTE]
> #### Stage 4
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.04" >}}

### Stage 5: Grouping and Aggregation

In this stage, we will write the core statistics of our summary. To make them reusable (in the next stage we will combine them into a report), we will define them as extension members of `IEnumerable<PlayInfo>`.

> [!IMPORTANT]
> #### GroupBy
>
> `GroupBy` splits a sequence into groups of elements with equal keys. Each group is an `IGrouping<TKey, TElement>` – a sequence of elements with an additional `Key` property, so all LINQ operators (and our own extension methods) can be applied to it:
> ```csharp
> var streamsPerArtist = infos
>     .GroupBy(i => i.Artist)
>     .Select(g => (Artist: g.Key, Streams: g.Count()));
> ```
> Records can be used as keys thanks to their value-based equality. In query syntax, grouping is written as `group info by key into g`, after which the query continues with the groups.

> [!IMPORTANT]
> #### CountBy and AggregateBy (.NET 9)
>
> When you only need one number per key, the groups do not have to be created at all. `CountBy` returns the number of elements for each key, and `AggregateBy` computes any aggregate for each key, starting from a seed value. Both return a sequence of `KeyValuePair<TKey, TValue>`:
> ```csharp
> var streamsPerTrack = infos.CountBy(i => i.Track);     // KeyValuePair<Track, int>
> var timePerMonth = infos.AggregateBy(
>     i => i.Play.Timestamp.Month,                       // key
>     TimeSpan.Zero,                                     // seed
>     (total, i) => total + i.Play.Duration);            // KeyValuePair<int, TimeSpan>
> ```

#### The Stats Class

Create a file named `Stats.cs` with a static class `Stats` containing an extension block `extension(IEnumerable<PlayInfo> plays)` with the following members:

1. The property `IEnumerable<PlayInfo> Streams` – the plays that are not skips (`Where`).
2. `IEnumerable<(Artist Artist, int Streams, TimeSpan Time)> TopArtists(int count)` – the artists with the most streams: group `Streams` by artist (`GroupBy`), compute the number of streams and their total time (reuse `TotalListeningTime`), sort descending by the number of streams and then by name, and take `count` elements.
3. `IEnumerable<(Track Track, Artist Artist, int Streams)> TopTracks(int count)` – the tracks with the most streams. Use `CountBy` with a tuple key `(i.Track, i.Artist)`; sort descending by the count and then by title.
4. `IEnumerable<(string Genre, int Streams)> TopGenres(int count)` – the genres with the most streams (`SelectMany` + `CountBy`); sort descending by the count and then by name. A stream of an artist with two genres counts for both of them.
5. `Dictionary<int, TimeSpan> TimeByMonth()` – the listening time in every month, computed with `AggregateBy` (see above) and converted to a dictionary with `ToDictionary()`.
6. `IEnumerable<(int Month, Artist Artist, int Streams)> TopArtistByMonth()` – the most streamed artist of each month, sorted by month. Group the streams by month and reuse `TopArtists` on each group – this is where defining the queries as extension members pays off:
   ```csharp
   from info in plays.Streams
   group info by info.Play.Timestamp.Month into month
   orderby month.Key
   let top = month.TopArtists(1).First()
   select (month.Key, top.Artist, top.Streams);
   ```
7. `ILookup<int, PlayInfo> ByHour()` – the plays grouped by the hour of the day (`ToLookup`).

> [!IMPORTANT]
> #### ToDictionary and ToLookup
>
> Both methods execute the query immediately and build a collection indexed by a key. `ToDictionary` requires unique keys (a duplicate key throws an exception), and the indexer throws `KeyNotFoundException` for a missing key (use `TryGetValue` or `GetValueOrDefault` instead). `ToLookup` allows many elements per key: its indexer returns a sequence of all elements with the given key – **an empty sequence** if there are none. Unlike `GroupBy`, a lookup is computed only once and supports fast access by key.

#### Printing the Statistics

In `Program.cs`, get the joined plays (`var infos = library.GetPlayInfos();`) and print:

1. The top 5 artists (with the number of streams and listening time), the top 5 tracks and the top 3 genres. Positions can be obtained with `Index()`, and the tuples can be deconstructed directly in the loop: `foreach (var (i, (artist, streams, time)) in infos.TopArtists(5).Index())`. Compare the top tracks with the result of Stage 3 – they should be identical.
2. A table of months: the listening time, a bar of `#` characters proportional to the time (the longest bar has 20 characters; dividing a `TimeSpan` by a `TimeSpan` gives a `double`), and the top artist of the month. Convert the result of `TopArtistByMonth()` into a dictionary indexed by month (`ToDictionary(x => x.Month)`).
3. A histogram of plays by the hour of the day for hours 0–23 (the longest bar has 40 characters). Thanks to `ToLookup`, `byHour[hour].Count()` simply returns 0 for hours without plays.

Expected output:

```text
Top 5 artists:
  1. Static Bloom                              430   25 h 36 min
  2. Pixel Monsoon                             232   14 h 33 min
  3. Ada & The Lambdas                         177   10 h 18 min
  4. Lena Marlowe                              174   11 h 18 min
  5. Mira Volta                                154    9 h 55 min
Top 5 tracks:
  1. Winter Letters - Static Bloom             134
  2. Sleepless Roads - Static Bloom             89
  3. Lazy Satellites - Pixel Monsoon            83
  4. Analog Memories - Lena Marlowe             73
  5. Burning Horizons - Concrete Gardens        63
Top genres: electronic (1135), pop (952), indie (821)

Jan  16 h 26 min #################    Static Bloom (38)
Feb  17 h 49 min ###################  Lena Marlowe (41)
Mar  17 h 11 min ##################   Kasia Wren (45)
Apr  15 h 38 min ################     Static Bloom (32)
May  12 h 39 min #############        Concrete Gardens (41)
Jun  17 h 01 min ##################   Static Bloom (42)
Jul  15 h 42 min ################     Lena Marlowe (46)
Aug  14 h 55 min ################     Static Bloom (43)
Sep  16 h 03 min #################    Static Bloom (38)
Oct  16 h 07 min #################    Pixel Monsoon (68)
Nov  18 h 31 min #################### Mira Volta (46)
Dec  12 h 59 min ##############       Static Bloom (30)

00:00 #####                                     66
01:00                                            8
02:00                                            0
03:00                                            0
04:00                                            0
05:00                                            0
06:00                                            0
07:00 ############################             368
08:00 #########################                332
09:00                                            0
10:00 ####                                      61
11:00 ###########                              146
12:00 #####                                     72
13:00 #####                                     75
14:00 #######                                   91
15:00 ###                                       50
16:00 ####                                      61
17:00                                           10
18:00 ######                                    90
19:00 ###########################              356
20:00 ###########################              359
21:00 ###############################          405
22:00 ######################                   286
23:00 ######################################## 515
```

> [!NOTE]
> #### Stage 5
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.05" >}}

### Stage 6: Your Wrapped

In the last stage, we will compose the yearly report out of the pieces written so far and see why it matters *when* a query is executed.

#### One More Iterator: SplitWhen

Listening streaks (consecutive days with music) and listening sessions (plays separated by short gaps) have one thing in common: we need to split a sorted sequence into groups of consecutive elements. In `EnumerableExtensions`, add the method:

```csharp
public static IEnumerable<List<T>> SplitWhen<T>(this IEnumerable<T> source, Func<T, T, bool> startsNewGroup)
```

It collects the elements into a list; before adding an element, it calls `startsNewGroup(last element of the current group, element)`, and if the result is `true`, it returns the current group (`yield return`) and starts a new one. Do not forget to return the last group after the loop (unless the sequence was empty). For example, for `1, 2, 3, 7, 8, 10` and `(a, b) => b - a > 1` it returns `[1, 2, 3]`, `[7, 8]`, `[10]`.

#### More Statistics

Add to the extension block in `Stats`:

1. `(DateOnly First, DateOnly Last) LongestStreak()` – the longest run of consecutive days with at least one play. Get the distinct days of plays, sort them (`Order`), split them with `SplitWhen` whenever the difference of `DayNumber` of neighboring days is greater than 1, and choose the longest group (`MaxBy`).
2. `(DateTime Start, DateTime End, int Plays) LongestSession(TimeSpan maxGap)` – the longest listening session: sort the plays by time and split them whenever the gap between the end of a play and the start of the next one exceeds `maxGap`. The length of a session is the time between the start of its first play and the end of its last play.
3. `(DateTime From, DateTime To) LongestBreak()` – the longest break in listening. This time, implement it as a single LINQ expression: sort the plays, use `Pairwise` from Stage 2, project the pairs into `(From, To)` tuples and choose the longest one with `MaxBy`.
4. `(Track Track, Artist Artist, DateOnly Day, int Streams) OnRepeat()` – the track streamed most times during a single day (`CountBy` with a tuple key `(i.Track, i.Artist, Day)`, then `MaxBy`).

#### The Report

Create a file named `WrappedReport.cs` with a static class `WrappedReport` and the method `string Build(IEnumerable<PlayInfo> history, int year)`. It filters the history to the given year and builds the report text with a `StringBuilder`. The report contains (see the expected output below):

- minutes listened (whole minutes, formatted with `N0`) and the total time,
- the number of streams, and the number of distinct tracks and artists streamed (`DistinctBy`),
- the top 5 artists and the top 5 tracks,
- the top 3 genres,
- the busiest month (the maximum of `TimeByMonth()`; format the month with `{date:MMMM}`),
- the peak hour (the largest group of `ByHour()`) and the listener type, determined with a `switch` expression with relational patterns: 5–11 – *an early bird*, 12–17 – *an afternoon listener*, 18–21 – *an evening listener*, otherwise *a night owl*,
- the longest streak, the longest session (with `maxGap` equal to 15 minutes), the longest break (in days, `TotalDays`) and the track on repeat.

> [!IMPORTANT]
> #### Materialization
>
> *Materialization* means executing a query and storing its results in a collection: `ToList()`, `ToArray()`, `ToDictionary()`, `ToLookup()`, `ToHashSet()`. A materialized collection is a snapshot – enumerating it many times is cheap and always gives the same elements. On the other hand, materializing too early wastes memory, prevents processing infinite sequences and loses the benefits of lazy evaluation (e.g. `First` stopping after the first match). A good rule of thumb: keep queries deferred while composing them, and materialize when the result is going to be enumerated many times.

#### Deferred vs Materialized

Each statistic in the report enumerates its input once, so `Build` enumerates the history about a dozen times. Let us check what this means for a deferred input. In `Program.cs`:

```csharp
// Deferred: every query in the report runs the join again
int created = 0;
IEnumerable<PlayInfo> deferred = library.GetPlayInfos().Tap(_ => created++);
string report = WrappedReport.Build(deferred, 2025);
int createdDeferred = created;

// Materialized: the join runs once, the report reads a list
created = 0;
List<PlayInfo> materialized = library.GetPlayInfos().Tap(_ => created++).ToList();
string sameReport = WrappedReport.Build(materialized, 2025);
```

Print the report, the number of `PlayInfo` objects created by the join in both cases, and whether both reports are identical. With a deferred input, the join – including building its hash tables – is executed again by every statistic. For our data set, it is still fast, but it is easy to imagine a source for which every enumeration means reading a file or querying a database. If you want, measure the time of both variants with `System.Diagnostics.Stopwatch`.

> [!TIP]
> A method that has to enumerate its input many times can protect itself: materialize the argument at the beginning, or declare the parameter as `IReadOnlyCollection<T>`/`IReadOnlyList<T>`, so that the caller has to pass a ready collection. IDEs (e.g. Rider) warn about *possible multiple enumeration* of an `IEnumerable<T>`.

Expected output:

```text
============================================================
                     YOUR 2025 WRAPPED
============================================================
Minutes listened   11,465 (191 h 05 min)
Streams            2,905 streams of 239 tracks by 39 artists

Top artists
  1. Static Bloom                                  430 streams
  2. Pixel Monsoon                                 232 streams
  3. Ada & The Lambdas                             177 streams
  4. Lena Marlowe                                  174 streams
  5. Mira Volta                                    154 streams
Top tracks
  1. Winter Letters - Static Bloom                 134 streams
  2. Sleepless Roads - Static Bloom                 89 streams
  3. Lazy Satellites - Pixel Monsoon                83 streams
  4. Analog Memories - Lena Marlowe                 73 streams
  5. Burning Horizons - Concrete Gardens            63 streams

Top genres         electronic, pop, indie
Busiest month      November (18 h 31 min)
Listening clock    peak at 23:00 - you are a night owl
Longest streak     17 days (2025-09-13 - 2025-09-29)
Longest session    2 h 07 min on 2025-10-11 (34 plays)
Longest break      10.5 days (2025-08-03 - 2025-08-14)
On repeat          Paper Bridges - Lena Marlowe, 5 times on 2025-02-09
============================================================

PlayInfo objects created: deferred 43,563, materialized 3,351
Identical reports: True
```

> [!NOTE]
> #### Stage 6
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.06" >}}

### Next Steps and Further Enhancements

The application built in this tutorial can be extended in many directions. Below are some ideas worth exploring:

- **More statistics:** The completion rate of tracks (`Play.Duration` compared with `Track.Length`) and the most skipped track, the first play of each artist (*discoveries* of the year), or the most intense 7-day period computed with a sliding `Window` iterator.
- **Real data:** Streaming services allow you to download your own listening history (usually as JSON files). Load it with `System.Text.Json` and generate your own Wrapped.
- **Streaming large files:** Instead of loading all data into memory, read the file lazily with an iterator (`File.ReadLines` + `yield return`) or asynchronously with `IAsyncEnumerable<T>` – since .NET 10, LINQ operators for asynchronous sequences are built into the framework (`System.Linq.AsyncEnumerable`).
- **Unit tests:** Cover the iterators with tests (see [Tutorial 2]({{< ref "/labs/02.msbuild" >}})), especially the edge cases: empty sequences, sequences with one element, and argument validation.
- **PLINQ:** Parallelize the queries with `AsParallel()` and measure whether it pays off for a data set of this size.
- **`IQueryable<T>` and expression trees:** The same LINQ queries written against a database with Entity Framework Core are not executed in memory – they are translated into SQL. Lambdas are then compiled into expression trees (`Expression<Func<T, bool>>`) instead of delegates.

## Example Tasks

Complete an example task from previous years. If you are able to finish it within 90 minutes, you are well-prepared for class.

- [Task 1]({{< ref "/labs/05.lambdas-enumerable-linq-extensions/example1" >}})
- [Task 2]({{< ref "/labs/05.lambdas-enumerable-linq-extensions/example2" >}})
