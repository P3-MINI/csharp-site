---
title: "Shared Locking"
weight: 30
---

# Non-exclusive Locking

Non-exclusive locking is a mechanism that allows more than one thread to access a resource simultaneously, usually under certain conditions (e.g., a limit on the number of threads or read-only access).

## Semaphore(Slim)

Semaphores allow **limiting** the number of threads that can simultaneously access a resource or a section of code. In .NET, they come in two versions: `Semaphore` is a wrapper around a semaphore handle provided by the operating system. `SemaphoreSlim`, on the other hand, is implemented entirely by the runtime, making it faster (approximately 10x - it does not require context switching to the kernel), but it cannot be used for cross-process synchronization. The "Slim" version also supports asynchronous constructs.

Semaphores are initialized with a specific number of "permits". `Wait()`/`WaitAsync()` decrements the counter, and `Release()` increments it. They are typically used for throttling (e.g., max 5 simultaneous file downloads, max 10 database connections).

> [!NOTE]
> If `SemaphoreSlim` has available resources (counter > 0), the `WaitAsync` method completes synchronously (without a context switch), which is very efficient.

### Example

In the following example, the `Downloader` limits the number of simultaneous downloads to the value of `concurrency`.

```csharp
public class Downloader(int concurrency)
{
    private readonly SemaphoreSlim _semaphore = new(concurrency);

    public async Task DownloadAsync(string url, string fileName, CancellationToken token = default)
    {
        Console.WriteLine($"[{fileName}] Waiting to start download...");

        await _semaphore.WaitAsync(token);
        
        Console.WriteLine($"[{fileName}] Downloading...");

        try
        {
            using HttpClient client = new HttpClient();

            byte[] data = await client.GetByteArrayAsync(url, token);

            Console.WriteLine($"[{fileName}] Downloaded {data.Length / 1024.0:0.00} KB");

            await File.WriteAllBytesAsync(fileName, data, token);
            Console.WriteLine($"[{fileName}] Download finished.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{fileName}] Error: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/SemaphoreExample" >}}

## ReaderWriterLock(Slim)

`ReaderWriterLock(Slim)` is a primitive optimized for scenarios where read operations are significantly more frequent than write operations. It supports two locking modes. Any number of threads can be in read mode simultaneously (`EnterReadLock` / `ExitReadLock`). Only one thread can be in write mode (`EnterWriteLock` / `ExitWriteLock`). When a thread is in write mode, no other thread can read or write.

It is most commonly used with shared data structures, application configurations, or for implementing a cache. In simple cases, a standard `lock` is usually sufficient.

> [!WARNING]
> You should use the newer `ReaderWriterLockSlim` class instead of the older `ReaderWriterLock`. The "Slim" version is implemented entirely by the runtime, which provides significantly better performance than the older version based on operating system resources.
