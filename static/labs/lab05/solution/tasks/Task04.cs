using System.Text.RegularExpressions;

namespace tasks;

public sealed class Task04 : IExecutable
{
    public void Execute(string[] args)
    {
        // testy jednostkowe oj taak :O
    }

    public static IEnumerable<IEnumerable<int>> FindSlidingWindowsWithRisingSum(IEnumerable<int> sequence)
    {
        // znajdź okna rozmiaru 5, których suma rośnie w stosunku do poprzedniego okna
        return sequence
            .SlidingWindow(5)
            .Select(w => (Window: w, Sum: w.Sum()))
            .SlidingWindow(2)
            .Where(w => w[0].Sum < w[1].Sum)
            .Select(w => w[1].Window);
    }

    public static IEnumerable<IEnumerable<int>> FindSlidingWindowsWithDuplicates(IEnumerable<int> sequence)
    {
        // znajdź okna rozmiaru 4, w których istnieje powtórzona liczba
        return sequence
            .SlidingWindow(4)
            .Where(window => window.Distinct().Count() < window.Length);
    }

    public static IEnumerable<string> FindMostCommonTrigrams(string text)
    {
        // znajdź w tekście 3 literowe ciągi kolejnych liter, które pojawiają się najczęściej
        if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
            return [];

        var trigrams = text
            .Where(char.IsLetter)
            .Select(char.ToLowerInvariant)
            .SlidingWindow(3)
            .Select(chars => new string([.. chars]))
            .GroupBy(trigram => trigram)
            .Select(group => new { Trigram = group.Key, Count = group.Count() });

        var maxCount = trigrams.Max(group => group.Count);

        return trigrams
            .Where(group => group.Count == maxCount)
            .Select(group => group.Trigram);
    }

    public static string FindWordWithMostUniqueLetters(string text)
    {
        // dla danego tekstu znajdź słowo o największej liczbie różnych liter,
        // nie uwzględniaj wielkości liter ('a' i 'A' to ta sama litera)
        // w przypadku remisu zwróć słowo najdłuższe

        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return Regex
            .Matches(text, @"[a-zA-Z]+")
            .Select(m => m.Value)
            .Select(word => new
            {
                Word = word,
                UniqueLetters = word
                    .ToLowerInvariant()
                    .Distinct()
                    .Count()
            })
            .OrderByDescending(w => w.UniqueLetters)
            .ThenByDescending(w => w.Word.Length)
            .FirstOrDefault()?.Word
                ?? string.Empty;
    }

    public static (int start, int end, int value) LongestSequence(IEnumerable<int> sequence)
    {
        return sequence.Fold(
            seed: (
                Start: 0,
                End: 0,
                Value: sequence.First(),
                CurrentStart: 0,
                CurrentEnd: 0,
                CurrentValue: sequence.First()
            ),
            func: (acc, elem) =>
            {
                if (elem == acc.CurrentValue)
                {
                    var length = acc.End - acc.Start + 1;
                    var currentLength = acc.CurrentEnd - acc.CurrentStart + 1;

                    if (currentLength > length)
                    {
                        acc.Start = acc.CurrentStart;
                        acc.End = acc.CurrentEnd;
                        acc.Value = acc.CurrentValue;
                    }
                }
                else
                {
                    acc.CurrentStart = acc.CurrentEnd;
                    acc.CurrentValue = elem;
                }

                acc.CurrentEnd++;

                return acc;
            },
            resultSelector: acc => (
                start: acc.Start,
                end: acc.End,
                value: acc.Value
            )
        );
    }

    public static (int min, int max, double average, double standardDeviation) ComputeStatistics(this IEnumerable<int> source)
    {
        if (source == null || !source.Any())
            throw new ArgumentException("Source sequence must contain at least one element.", nameof(source));

        var result = source.Fold(
            seed: (
                Min: int.MaxValue,
                Max: int.MinValue,
                Sum: 0L,
                SumOfSquares: 0L,
                Count: 0
            ),
            func: (acc, x) => (
                Min: Math.Min(acc.Min, x),
                Max: Math.Max(acc.Max, x),
                Sum: acc.Sum + x,
                SumOfSquares: acc.SumOfSquares + (long)x * x,
                Count: acc.Count + 1
            ),
            resultSelector: acc =>
            {
                var avg = (double)acc.Sum / acc.Count;
                var variance = (double)acc.SumOfSquares / acc.Count - avg * avg;
                var stdDev = Math.Sqrt(Math.Max(0, variance));
                return (acc.Min, acc.Max, avg, stdDev);
            }
        );

        return result;
    }

    public static void AnalyzeSensorData()
    {
        // dane z czujnika są wysyłane co sekundę i są określone funkcją sin(t / 10.0), gdzie t oznacza czas.
        // Oblicz średnią wartość danych wysyłanych przez czujnik w każdej minucie w ciągu pierwszej godziny od uruchomienia urządzenia.
        var sensorData = Enumerable
            .Range(0, 3600)
            .Select(t => Math.Sin(t / 10.0))
            .Batch(60)
            .Select(minute => minute.Average())
            .ToList();

        for (var i = 0; i < sensorData.Count; i++)
        {
            Console.WriteLine($"Minute {i + 1:00}: average = {sensorData[i]:F4}");
        }
    }
}

public static class EnumerableExtensions
{
    public static TResult Fold<TSource, TAccumulate, TResult>(
        this IEnumerable<TSource> source,
        TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate> func,
        Func<TAccumulate, TResult> resultSelector)
    {
        var acc = seed;

        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext())
        {
            acc = func(acc, enumerator.Current);
        }

        return resultSelector(acc);
    }

    public static IEnumerable<IEnumerable<T>> Batch<T>(
        this IEnumerable<T> collection,
        int size)
    {
        using var enumerator = collection.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var batch = new List<T>(capacity: size)
            {
                enumerator.Current
            };

            for (var i = 1; i < size && enumerator.MoveNext(); i++)
            {
                batch.Add(enumerator.Current);
            }

            yield return batch;
        }
    }

    public static IEnumerable<T[]> SlidingWindow<T>(
        this IEnumerable<T> collection,
        int size)
    {
        var window = new Queue<T>();

        using var enumerator = collection.GetEnumerator();

        while (enumerator.MoveNext())
        {
            window.Enqueue(enumerator.Current);
            if (window.Count > size)
                window.Dequeue();

            if (window.Count == size)
                yield return window.ToArray();
        }
    }
}