---
title: "Synchronization Context"
weight: 30
---

# Synchronization Context

`SynchronizationContext` represents a "location" where code can be executed. This "location" is usually a thread or a thread pool. In general, it can be anything: a specific CPU core, or even another computer on the network.

Using the `SynchronizationContext.Send` and `SynchronizationContext.Post` methods, code can be invoked in this specific "location" (`Post` is the non-blocking equivalent of `Send`).

Every thread can have an instance of `SynchronizationContext` associated with it. A running thread can be associated with a synchronization context by calling the static `SynchronizationContext.SetSynchronizationContext` method, and the current context of the running thread can be queried via the `SynchronizationContext.Current` property.

Synchronization contexts are important in GUI application programming. GUI applications set the synchronization context to the so-called "UI thread". This type of synchronization context invokes the delegates passed to it precisely on that thread. This is very useful because GUI applications allow manipulating controls only on the same thread on which they were created. Thanks to synchronization contexts, we can execute code from any thread and safely return execution to the UI thread.

A typical implementation of a synchronization context, where the "location" is a single thread, looks like this:

```csharp
using System.Collections.Concurrent;

public class MySynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _workItems = new();

    private readonly Thread _processingThread;

    public MySynchronizationContext()
    {
        _processingThread = new Thread(ProcessQueue)
        {
            IsBackground = true,
            Name = "SynchronizationContext processing thread"
        };
        _processingThread.Start();
    }

    public override void Post(SendOrPostCallback callback, object? state)
    {
        if (callback == null) throw new ArgumentNullException(nameof(callback));

        _workItems.Add((callback, state));
    }

    private void ProcessQueue()
    {
        foreach (var (callback, state) in _workItems.GetConsumingEnumerable())
        {
            try
            {
                callback(state);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception in synchronization context: {ex}");
            }
        }
    }

    public void Dispose()
    {
        _workItems.CompleteAdding();
        _processingThread.Join();
    }
}
```

Such a synchronization context allows code to be executed on this thread from anywhere:

```csharp
class Program
{
    private static MySynchronizationContext? _context;

    static void Main(string[] args)
    {
        _context = new MySynchronizationContext();
        Thread worker = new Thread(() =>
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} thread started");
            _context?.Post(_ =>
            {
                Console.WriteLine($"Worker message from: {Thread.CurrentThread.Name}");
            }, null);
        }){Name = "Worker"};
        worker.Start();

        _context?.Post(_ =>
        {
            Console.WriteLine($"Main thread message from: {Thread.CurrentThread.Name}");
        }, null);
        worker.Join();
        _context?.Dispose();        
    }
}
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/concurrency/SynchronizationContext" >}}
