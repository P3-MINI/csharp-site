namespace Wrapped;

/// <summary>Our own LINQ-like operators, written as iterator methods.</summary>
public static class EnumerableExtensions
{
    /// <summary>Returns the elements satisfying the predicate (our own <c>Where</c>).</summary>
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

    /// <summary>Transforms every element (our own <c>Select</c>).</summary>
    public static IEnumerable<TResult> Map<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
    {
        foreach (var item in source)
        {
            yield return selector(item);
        }
    }

    /// <summary>Calls the action for every element passing through - handy for logging.</summary>
    public static IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>Returns pairs of consecutive elements: (a, b), (b, c), (c, d), ...</summary>
    public static IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            yield break;
        }

        T previous = enumerator.Current;
        while (enumerator.MoveNext())
        {
            yield return (previous, enumerator.Current);
            previous = enumerator.Current;
        }
    }

    /// <summary>
    /// Splits the sequence into consecutive groups. A new group starts whenever
    /// <paramref name="startsNewGroup"/> returns true for a pair of neighbouring elements.
    /// </summary>
    public static IEnumerable<List<T>> SplitWhen<T>(this IEnumerable<T> source, Func<T, T, bool> startsNewGroup)
    {
        List<T> group = [];
        foreach (var item in source)
        {
            if (group.Count > 0 && startsNewGroup(group[^1], item))
            {
                yield return group;
                group = [];
            }
            group.Add(item);
        }

        if (group.Count > 0)
        {
            yield return group;
        }
    }
}
