---
title: "Asynchronous Sequences"
weight: 60
---

# Asynchronous Sequences

Asynchronous sequences combine two concepts: **iterators** (*yield* statement) with **asynchronous methods** (*async/await*). They allow writing iterating methods that return subsequent elements asynchronously. This is particularly **useful when each element of the sequence needs to be awaited**. (If the entire sequence needs to be awaited, but not individual elements, it is better to consider using `Task<IEnumerable<T>>`).

```csharp
public interface IAsyncEnumerable<out T>
{
    IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken = default);
}

public interface IAsyncEnumerator<out T> : IAsyncDisposable
{
    T Current { get; }
    ValueTask<bool> MoveNextAsync();
}
```

> [!IMPORTANT]
> The `Current` property returns the current element. Access to this property is only allowed after `MoveNextAsync()` completes with a `true` result.

> `ValueTask<T>` is a struct, analogous to the `Task<T>` class, which allows for more efficient execution by limiting the number of memory allocations, which can be quite numerous in the case of sequences.

> `IAsyncDisposable` is the asynchronous version of the `IDisposable` interface with a single method `ValueTask DisposeAsync()`.

The language syntax provides a special `await foreach` statement, which allows iterating over subsequent elements of a sequence with asynchronous waiting for subsequent elements.

```csharp
IAsyncEnumerable<int> asyncSequence = RangeAsync(from: 0, to: 100, delay: 50);
await foreach(var item in asyncSequence)
{
    Console.WriteLine(item);
}
```

> [!NOTE]
> To pass a `CancellationToken` to a sequence, you can use the `WithCancellation(CancellationToken token)` extension method, e.g.: `await foreach(var item in asyncSequence.WithCancellation(token))`.

Similar to `foreach`, `async foreach` is syntactic sugar. The compiler expands this statement as follows:

```csharp
IAsyncEnumerable<int> asyncSequence = RangeAsync(from: 0, to: 100, delay: 50);
IAsyncEnumerator<int> asyncEnumerator = asyncSequence.GetAsyncEnumerator();
try
{
    while(await asyncEnumerator.MoveNextAsync())
    {
        var item = asyncEnumerator.Current;
        Console.WriteLine(item);
    }
}
finally
{
    if (asyncEnumerator != null)
    {
        await asyncEnumerator.DisposeAsync();
    }
}
```

## Asynchronous Iterator Methods

Similar to iterator methods, the return type must be `IAsyncEnumerable<T>` or `IAsyncEnumerator<T>`. Similar to asynchronous methods, it must be marked `async`. In such a method, we can use both the `yield` and `await` statements.

```csharp
public class Program
{
    public static async Task Main()
    {
        await foreach (int i in RangeAsync(0, 100, 50))
        {
            Console.WriteLine(i);
        }
    }
    
    private static async IAsyncEnumerable<int> RangeAsync(int from, int to, int delay)
    {
        for (int i = from; i < to; i++)
        {
            await Task.Delay(delay);
            yield return i;
        }
    }
}
```

The compiler will similarly generate a state machine corresponding to the asynchronous iterator method.

## LINQ

Asynchronous sequences work with LINQ through the `System.Linq.Async` namespace (available via NuGet package).

```csharp
IAsyncEnumerable<int> asyncSequence = RangeAsync(from: 0, to: 100, delay: 50)
    .Where(x => x % 2 == 0)
    .Select(x => x * x);
    
await foreach(var item in asyncSequence)
{
    Console.WriteLine(item);
}
```
