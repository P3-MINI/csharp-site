---
title: "Threads"
weight: 10
---

# Threads

## Introduction

A **thread** is a basic unit of code execution within a single process. Threads within the same process share its memory (heap data) and operating system resources (file handles, network connections, and others). However, each thread has its own:

*   **Call stack**: Each thread has a separate stack where its local variables, method parameters, and return addresses are stored. A variable declared inside a method is completely private to the thread executing that method.
*   **Processor context**: Each thread has its unique CPU register state, including the instruction pointer, which is saved and restored during context switching.

> [!NOTE]
> Creating a new thread is a lightweight operation compared to creating a process.

Context switching between threads by the operating system scheduler allows for achieving **concurrency** on a single CPU core. In multi-core systems, threads can execute **in parallel**, each on a separate core.

> [!NOTE]
> Operations on shared resources may require **synchronization**.

## Creating Threads

`System.Threading.Thread` is a direct representation of a native operating system thread, wrapped by the Runtime. It allows for manual creation and management of a thread's lifecycle.

A new thread is created by initializing a `Thread` object, passing a delegate in the constructor that points to the method to be executed (parameterless `delegate void ThreadStart()` or `delegate void ParameterizedThreadStart(object? obj)`). The `Start` method launches the thread.

```csharp
Thread thread = new Thread(PrintA);
thread.Start();

PrintB();

void PrintA()
{
    for (int i = 0; i < 100; i++)
        Console.Write('A');
}

void PrintB()
{
    for (int i = 0; i < 100; i++)
        Console.Write('B');
}
```

> Such a program will not yield an unambiguous result. Typically, these will be interleaved sequences of letters, e.g.:
> `BBBAAAAAAAAAAAAABAAAAAAAAAAAAAAABBBBABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABBBBBBBBBBBBBBBBBBBBBBBBBBB`


## Sleeping and Waiting for a Thread

The `Join` method allows waiting for another thread to finish.

```csharp
Thread thread = new Thread(PrintA);
thread.Start();

PrintB();
thread.Join();

Console.WriteLine("\nThread joined.");

void PrintA()
{
    for (int i = 0; i < 100; i++)
        Console.Write('A');
}

void PrintB()
{
    for (int i = 0; i < 100; i++)
        Console.Write('B');
}
```

The `Thread.Sleep()` method temporarily suspends the execution of the current thread for a specified period.

```csharp
Thread.Sleep(TimeSpan.FromMilliseconds(10));
Thread.Sleep(millisecondsTimeout: 10);
```

> [!NOTE]
> `Sleep` and `Join` are **blocking** operations that cause a thread to be suspended by the operating system and not consume CPU time until the event the thread is waiting for occurs. Other typical blocking operations include waiting for synchronization primitives (e.g., `Mutex.Wait()`) or I/O operations (e.g., `File.ReadAllText()`).

## Foreground and Background Threads

The `IsBackground` property determines whether a given thread is a foreground or background thread.
* **Foreground threads** (default): Keep the application process alive. The application will not terminate as long as at least one foreground thread is running.
* **Background threads**: Do not keep the application alive; the application terminates when all foreground threads complete their work. If any background threads are still running at this point, they are **abruptly terminated** without the possibility of completing their work. Background threads are ideal for tasks that should run "in the background" for as long as the application is running but are not critical enough for their non-completion or sudden termination to be a problem.

```csharp
using System;
using System.Threading;

public class Program
{
    public static void Main()
    {
        Thread worker = new Thread(() =>
        {
            Console.WriteLine("Background thread started");
            Console.ReadLine();
            Console.WriteLine("Background thread finished");
        })
        {
            Name = "BackgroundWorker",
            IsBackground = true
        };
        worker.Start();

        Console.WriteLine("Main thread finished.");
    }
}
```

## Thread Safety and Synchronization

Threads within the same process share resources (e.g., static fields, objects on the heap); access to them should be synchronized to avoid **race conditions**.

The following example shows a race condition problem. The `UnsafePrintOnce` method should print `Done` only once, but with concurrent execution, there is a chance it will print it twice.

```csharp
private static bool _done = false;
static void Main()
{
    Thread thread = new Thread(UnsafePrintOnce);
    thread.Start();

    UnsafePrintOnce();

    thread.Join();

    void UnsafePrintOnce()
    {
        if (_done) return;
        _done = true;
        Console.WriteLine("Done");
    }
}
```

### The `lock` Statement

In C#, the simplest synchronization primitive is the `lock` statement, which guarantees that a given block of code (critical section) will be executed by only one thread at a time. Other threads are blocked until the thread that entered the critical section exits it.

```csharp
private static bool _done = false;
private static readonly Lock LockObj = new Lock();
static void Main()
{
    Thread thread = new Thread(SafePrintOnce);
    thread.Start();

    SafePrintOnce();

    thread.Join();

    void SafePrintOnce()
    {
        lock (LockObj) // critical section
        {
            if (_done) return;
            _done = true;
        }
        
        Console.WriteLine("Done");
    }
}
```

## Exception Handling in Threads

An unhandled exception in any thread, by default, causes the **immediate termination of the entire application**.

```csharp
public class Program
{
    private static void ThrowsException()
    {
        throw new Exception("Unhandled exception");
    }

    public static void Main()
    {
        Thread thread = new Thread(ThrowsException);
        thread.Start();

        Console.WriteLine("Main thread waiting for thread...");
        thread.Join(); 
        Console.WriteLine("Main thread finished.");
    }
}
```

## Thread Pool

The CLR maintains and manages a **thread pool**, which is the preferred way to execute asynchronous tasks in .NET. Threads from the pool are reused, which reduces the overhead associated with creating and destroying threads. Pool threads are background threads by default.

The simplest method to invoke something on the thread pool is the `Task` class:

```csharp
public class Program
{
    public static void Main()
    {
        Task task = Task.Run(() =>
        {
            Console.WriteLine(Thread.CurrentThread.Name);
            Console.WriteLine("Hello from the thread pool");
        });
        
        task.Wait();
    }
}
```

> The thread pool is managed by the Runtime; initially, it contains as many threads as there are physical cores available in the system. The Runtime adds or removes threads from the pool as needed.

> [!NOTE]
> Although threads are the basis of concurrency, in modern C#, they are rarely used directly. Higher-level abstractions operating on the built-in thread pool, such as `Task`, `Parallel`, and `async/await`, are preferred.
