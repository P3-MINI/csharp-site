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
