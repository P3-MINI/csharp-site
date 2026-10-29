---
title: "Introduction"
weight: 10
---

# Introduction

Concurrent access to resources requires the use of synchronization mechanisms to avoid *race conditions* and ensure data consistency. The .NET platform provides a set of synchronization primitives that differ in purpose, scope (intra-process or inter-process), and performance cost.

Available mechanisms can be divided into four main categories:

-   **Exclusive locking** - guarantees that only one thread has access to the resource at a given time.
    - `lock` statement
    - `Mutex`
    - `SpinLock`
-   **Non-exclusive locking** - limits concurrency
    - `Semaphore(Slim)`
    - `ReaderWriterLock(Slim)`
-   **Signaling** - allows for communication between threads and waiting for a specific event.
    - `ManualResetEvent(Slim)`, `AutoResetEvent`, `CountdownEvent`
    - `Monitor.Wait`, `Monitor.Pulse` (condition variables)
    - `Barrier`
-   **Other low-level** - do not require thread blocking.
    - `MemoryBarrier`
    - `Interlocked`

## Problem

In the example below, the `complete` variable is not protected. The JIT compiler (especially in `Release` mode) may optimize the `while` loop in such a way that the value of `complete` is fetched into the processor register only once. Even though the main thread changes the value to `true`, the background thread may never notice this, causing the program to hang.

```csharp
class Program
{
    private static void Main()
    {
        bool complete = false;
        var thread = new Thread (() =>
        {
            bool toggle = false;
            while (!complete) 
            {
                toggle = !toggle;
            }
        });
        thread.Start();
        Thread.Sleep(1000);
        complete = true;
        thread.Join();
    }
}
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/MemoryBarriers" >}}

## Memory Barrier (Fence)

A memory barrier is a processor or compiler instruction that enforces a specific ordering of memory operations (reads and writes). It is important for two reasons:

1.  **Prevents instruction reordering**:  
    Both compilers and modern processors optimize code by changing the order of instruction execution if they determine it will not affect the result within a single thread. However, in a multi-threaded environment, such reordering can lead to errors where one thread sees changes in the wrong order. A barrier "stops" these optimizations, guaranteeing that instructions before the barrier are executed before instructions after it.
2.  **Guarantees visibility**:  
    In multi-core systems, a write to a variable by one thread may first go to the core's write buffer (*store buffer*), or be kept in a register by the compiler, and therefore not be immediately visible to other cores (and threads). A memory barrier forces such pending writes to complete and prevents reading stale copies, so that other threads see the latest values. (Contrary to a popular belief, it does not "flush the cache to RAM" - the hardware itself keeps the caches of all cores coherent.)

In C#, a barrier can be invoked manually using the `Thread.MemoryBarrier` method. However, it is rarely called directly, because barriers are generated implicitly by:
1. The `lock` statement (on entry and on exit)
2. Methods from synchronization primitives (e.g., `Mutex`, `Semaphore`, `AutoResetEvent`)
3. Reads/writes of `volatile` fields (half barriers: a read has *acquire* semantics, a write has *release* semantics)
4. Methods from the `Interlocked` class
5. Starting, waiting for a thread/`Task`

## Interlocked Class

The `Interlocked` class is used to perform atomic operations on variables shared by multiple threads. It is the fastest and lightest way to synchronize simple numeric data or references without the need for heavier synchronization primitives like `lock` or `Mutex`.

> [!NOTE]
> **What does "atomic operation" mean?**
> An atomic operation is one that is indivisible. From the point of view of other threads, it executes either in its entirety or not at all - it cannot be executed halfway.

For example, the following are not atomic:
- increment, decrement
- reading/writing 64-bit values (`long`, `double`) on a 32-bit platform
- swap

### Example

The `counter++` and `counter--` operations are not atomic. They consist of three stages: read, modify, and write. When two tasks perform these operations simultaneously on the same variable, these stages overlap. The final value of `counter` will be unpredictable and almost certainly different from zero.

```csharp
int counter = 0, times = 1_000_000;

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        counter++;
});

var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        counter--;
});

await Task.WhenAll(increment, decrement);

Console.WriteLine($"Counter: {counter}");
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/InterlockedDemo" >}}

The `Interlocked.Increment` and `Interlocked.Decrement` methods force the processor to execute the entire variable modification operation as a single, indivisible unit (using hardware instructions). Additionally, they generate a full memory barrier, which guarantees correct visibility of changes for all processor cores. The code is thread-safe, and the final value of `counter` will always be exactly `0`.

```csharp
int counter = 0, times = 1_000_000;

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        Interlocked.Increment(ref counter);
});

var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        Interlocked.Decrement(ref counter);
});

await Task.WhenAll(increment, decrement);

Console.WriteLine($"Counter: {counter}");
```
