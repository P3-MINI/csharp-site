---
title: "Parallel"
weight: 50
---

# Parallel

The `Parallel` class (from the `System.Threading.Tasks` namespace) is one of the elements of the TPL (*Task Parallel Library*), which allows writing concurrent code in a simple way, abstracting from thread management.

The `Parallel` class contains three basic methods:

*   `Parallel.Invoke` - allows for the concurrent execution of multiple `Action` delegates
*   `Parallel.For` - a parallel equivalent of the `for` loop
*   `Parallel.ForEach` - a parallel equivalent of the `foreach` loop

1.  For each of these methods, the work is efficiently partitioned into several tasks and run concurrently on the thread pool.
2.  These methods are blocking until all the work is completed.
3.  If an exception occurs during the execution of one of the operations, `Parallel` will collect all exceptions and rethrow them as a single `AggregateException`.

> [!NOTE]
> When using `Parallel` methods, it is always necessary to remember about thread safety. If parallel operations modify shared state (e.g., a shared variable, list, dictionary), appropriate synchronization of access to this state must be ensured.

## Parallel.Invoke

The `Parallel.Invoke` method, in its simplest signature, takes an array of delegates to execute.

```csharp
public static void Invoke([NotNull] params Action[] actions);
```

```csharp
public class Program
{
    public static void Main()
    {
        Parallel.Invoke(
            () => DownloadFile("https://pages.mini.pw.edu.pl/~hermant/Tomek.jpg", "hermant.jpg"),
            () => DownloadFile("https://pages.mini.pw.edu.pl/~aszklarp/images/me.jpg", "aszklarp.jpg"),
            () => DownloadFile("https://cadcam.mini.pw.edu.pl/static/media/kadra8.7b107dbb.jpg", "sobotkap.jpg"));
    }

    private static void DownloadFile(string url, string outputPath)
    {
        using HttpClient httpClient = new HttpClient();
        try
        {
            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(outputPath, content);

            Console.WriteLine($"{outputPath} downloaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading {url}: {ex.Message}");
        }
    }
}
```

## Parallel.For

The `Parallel.For` method, in its simplest signature, takes an execution range and a delegate that will be executed for each index.

```csharp
public static ParallelLoopResult For(
    int fromInclusive, 
    int toExclusive, 
    [NotNull] Action<int> body);
```

```csharp
public class Program
{
    public static void Main()
    {
        int from = 1_000_000, to = 1_000_100;
        Parallel.For(from, to, i =>
        {
            Console.WriteLine($"Is {i} prime: {IsPrime(i)}");
        });
    }
    
    private static bool IsPrime(int number)
    {
        if (number < 2)
            return false;

        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }
}
```

> [!NOTE]
> The iterations are not executed in order, so the lines are printed in a nondeterministic order.

### Breaking the loop

The `For` and `ForEach` methods include an overload that exposes `ParallelLoopState` in the delegate. Using it, loops can be broken early.

*   `ParallelLoopState.Break()` ensures that all currently processed iterations, and all iterations with a lower index than the one for which `Break()` was called, will be executed. Further iterations with higher indices will not be started.
*   `ParallelLoopState.Stop()` signals that the loop should end as soon as possible: no further iterations (regardless of their index) will be started. Iterations that are already running are not interrupted, but they can check the `IsStopped` property and finish early.

```csharp
public static ParallelLoopResult For(
    int fromInclusive, 
    int toExclusive, 
    [NotNull] Action<int, ParallelLoopState> body)
```

Information about the break can be read from the returned `ParallelLoopResult`.

```csharp
public class Program
{
    public static void Main()
    {
        int from = 1_000_000, to = 1_000_100;
        ParallelLoopResult result = Parallel.For(from, to, (i, loopState) =>
        {
            if (IsPrime(i))
            {
                loopState.Break();
            }
        });

        if (!result.IsCompleted)
        {
            Console.WriteLine($"There is a prime: {result.LowestBreakIteration}");
        }
        else
        {
            Console.WriteLine("There are no primes");
        }
    }
    
    private static bool IsPrime(int number)
    {
        if (number < 2)
            return false;

        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }
}
```

## Parallel.ForEach

The `Parallel.ForEach` method, in its simplest signature, takes a sequence and a delegate that will be executed for each element.

```csharp
public static ParallelLoopResult ForEach<TSource>(
    [NotNull] IEnumerable<TSource> source, 
    [NotNull] Action<TSource> body)
```

```csharp
public class Program
{
    public static void Main()
    {
        List<(string, string)> urls =
        [
            ("https://pages.mini.pw.edu.pl/~hermant/Tomek.jpg", "hermant.jpg"),
            ("https://pages.mini.pw.edu.pl/~aszklarp/images/me.jpg", "aszklarp.jpg"),
            ("https://pages.mini.pw.edu.pl/~rafalkoj/templates/mini/images/photo.jpg", "rafalkoj.jpg"),
            ("https://pages.mini.pw.edu.pl/~kaczmarskik/krzysztof.jpg", "kaczmarskik.jpg"),
            ("https://cadcam.mini.pw.edu.pl/static/media/kadra8.7b107dbb.jpg", "sobotkap.jpg")
        ];

        Parallel.ForEach(urls, ((string url, string output) tuple) =>
        {
            DownloadFile(tuple.url, tuple.output);
        });
    }

    private static void DownloadFile(string url, string outputPath)
    {
        using HttpClient httpClient = new HttpClient();
        try
        {
            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(outputPath, content);

            Console.WriteLine($"{outputPath} downloaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading {url}: {ex.Message}");
        }
    }
}
```

## Parallel.ForEachAsync and Parallel.ForAsync

`Parallel.For` and `Parallel.ForEach` block the calling thread and are intended for synchronous (CPU-bound) work. For asynchronous operations (e.g., I/O), there are asynchronous counterparts: `Parallel.ForEachAsync` (.NET 6) and `Parallel.ForAsync` (.NET 8). Their body is an asynchronous delegate that receives an element (or an index) and a `CancellationToken`, and returns a `ValueTask`. The methods return a `Task` that completes when all iterations have finished. By default, at most `Environment.ProcessorCount` iterations run concurrently; this can be changed with `ParallelOptions` (described below). The following example downloads the files from the `urls` list of the previous example:

```csharp
using HttpClient httpClient = new HttpClient();

await Parallel.ForEachAsync(urls, async ((string url, string output) tuple, CancellationToken token) =>
{
    byte[] content = await httpClient.GetByteArrayAsync(tuple.url, token);
    await File.WriteAllBytesAsync(tuple.output, content, token);
    Console.WriteLine($"{tuple.output} downloaded successfully.");
});

await Parallel.ForAsync(0, 10, async (i, token) =>
{
    await Task.Delay(1000, token); // e.g., an asynchronous I/O operation
    Console.WriteLine(i);
});
```

> [!WARNING]
> Do not pass an `async` lambda to `Parallel.For` or `Parallel.ForEach`. It would be converted to an `async void` delegate, so the loop would finish without waiting for the asynchronous operations to complete.

## ParallelOptions

An object of `ParallelOptions` can be used to configure the behavior of `Parallel` class methods.

It allows setting options such as:
*   **`MaxDegreeOfParallelism`**: Specifies the maximum number of concurrently executing operations. This is useful when we want to limit the CPU load or external resources. By default, `Parallel` attempts to utilize all available cores.
*   **`CancellationToken`**: Enables cooperative cancellation of parallel operations during their execution.

```csharp
public class Program
{
    public static void Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        ParallelOptions options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = cts.Token
        };

        int primes = 0;
        int from = 0, to = 10_000_000;
        try
        {
            Parallel.For(from, to, options, i =>
            {
                if (IsPrime(i)) Interlocked.Increment(ref primes);
            });
            Console.WriteLine($"Found exactly {primes} primes from {from}, to {to}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled");
            Console.WriteLine($"Found at least {primes} primes from {from}, to {to}");
        }
    }
    
    private static bool IsPrime(int number)
    {
        if (number < 2)
            return false;

        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }
}
```
