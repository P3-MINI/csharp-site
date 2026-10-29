---
title: "Concurrent Collections"
weight: 50
---

# Concurrent Collections

The `System.Collections.Concurrent` namespace contains collections optimized for multi-threaded access. Unlike standard collections (e.g., `List<T>`, `Dictionary<TKey, TValue>`), which require manual locking (e.g., via `lock`) for every access, concurrent collections handle synchronization internally, often using lock-free techniques or fine-grained locking.

> [!WARNING]
> Standard collections are more efficient than concurrent collections in scenarios where there is no concurrency.

## Collections

*   **`ConcurrentDictionary<TKey, TValue>`**: A thread-safe dictionary. It allows simultaneous reads and writes by multiple threads without locking the entire dictionary.
    *   Methods: `TryAdd`, `TryUpdate`, `GetOrAdd`, `AddOrUpdate`.
*   **`ConcurrentQueue<T>`**: A thread-safe FIFO queue.
*   **`ConcurrentStack<T>`**: A thread-safe LIFO stack.
*   **`ConcurrentBag<T>`**: An unordered multi-set of elements.

> [!INFO]
> Iterating over a concurrent collection while it is being modified by another thread may include both already removed and newly added values, but it will not result in an exception being thrown.

> [!INFO]
> `ConcurrentQueue`, `ConcurrentStack`, and `ConcurrentBag` internally use linked lists. In `ConcurrentBag<T>`, each thread gets its own private list, making element retrieval very fast and race-free, provided that each thread does not retrieve more elements than it has added.

## `IProducerConsumerCollection`

`IProducerConsumerCollection<T>` is an interface in the `System.Collections.Concurrent` namespace that defines a contract for thread-safe collections intended for **Producer-Consumer** scenarios.

```csharp
public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection
{
    bool TryAdd(T item);
    bool TryTake(out T item);
    T[] ToArray();
}
```

It is implemented by `ConcurrentQueue<T>`, `ConcurrentStack<T>`, and `ConcurrentBag<T>`.

## BlockingCollection

`BlockingCollection<T>` is a specialized class that implements the classic **Producer-Consumer** pattern. It wraps another concurrent collection implementing `IProducerConsumerCollection<T>` (by default, `ConcurrentQueue<T>`) and adds blocking mechanisms:

*   **Producer**: If the collection has a bounded capacity, the `Add` method will block the producer thread if the collection is full.
*   **Consumer**: The `Take` method will block the consumer thread if the collection is empty, waiting for an item to appear.

### Example

In the following example, a producer adds numbers to the collection, and a consumer retrieves them. Thanks to `GetConsumingEnumerable()`, the consumer can easily iterate over the elements as long as the producer has not signaled the end of adding (`CompleteAdding`).

```csharp
var blockingCollection = new BlockingCollection<int>(boundedCapacity: 5);

Task producer = Task.Run(() =>
{
    for (int i = 0; i < 10; i++)
    {
        Console.WriteLine($"Producing: {i}");
        blockingCollection.Add(i); // Can block, if full
        Thread.Sleep(500);
    }
    blockingCollection.CompleteAdding(); // Signal end of data
});

Task consumer = Task.Run(() =>
{
    foreach (var item in blockingCollection.GetConsumingEnumerable())
    {
        Console.WriteLine($"Consuming: {item}");
        Thread.Sleep(1000);
    }
});

Task.WaitAll(producer, consumer);
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/BlockingCollectionExample" >}}

## Channels

The `System.Threading.Channels` namespace (built into .NET since .NET Core 3.0) provides `Channel<T>` - a modern, **asynchronous** alternative to `BlockingCollection<T>`. A channel has two ends: `Writer` (`ChannelWriter<T>`), used by producers, and `Reader` (`ChannelReader<T>`), used by consumers. Instead of blocking threads, both sides wait asynchronously:

*   **Producer**: `WriteAsync` waits (`await`) if a bounded channel (`Channel.CreateBounded<T>`) is full. `Channel.CreateUnbounded<T>()` creates a channel without a capacity limit.
*   **Consumer**: `ReadAsync` waits for an item to appear. `ReadAllAsync()` returns an [asynchronous sequence]({{< ref "/lectures/09.concurrency/asynchronous-streams" >}}) (`IAsyncEnumerable<T>`) that ends when the producer calls `Writer.Complete()` and all remaining elements have been read.

### Example

The previous example rewritten using a channel:

```csharp
var channel = Channel.CreateBounded<int>(capacity: 5);

Task producer = Task.Run(async () =>
{
    for (int i = 0; i < 10; i++)
    {
        Console.WriteLine($"Producing: {i}");
        await channel.Writer.WriteAsync(i); // Waits asynchronously if full
        await Task.Delay(500);
    }
    channel.Writer.Complete(); // Signal end of data
});

Task consumer = Task.Run(async () =>
{
    await foreach (var item in channel.Reader.ReadAllAsync())
    {
        Console.WriteLine($"Consuming: {item}");
        await Task.Delay(1000);
    }
});

await Task.WhenAll(producer, consumer);
```

> [!TIP]
> In new code, especially asynchronous code, prefer `Channel<T>` over `BlockingCollection<T>` - waiting does not block thread pool threads. Bounded channels can also be configured (`BoundedChannelOptions.FullMode`) to drop the oldest or the newest element instead of waiting, and the `SingleReader`/`SingleWriter` options allow using faster implementations.

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/ChannelExample" >}}
