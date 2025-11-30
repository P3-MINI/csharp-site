---
title: "Tasks"
weight: 40
---

# Tasks

The `Task` class represents an asynchronous operation that will complete in the future. It is a fundamental component of the TPL (*Task Parallel Library*) and the foundation of modern asynchronous programming.

The `Task` class provides two types of tasks:

* `Task` - a task with no return value
* `Task<TResult>` - a generic version where `TResult` is the return value's type

> [!WARNING]
> By default, tasks use threads from the thread pool, which are background threads. This means they will terminate when all foreground threads have finished.

## Creating tasks

The `Task.Run(Action action)` method creates and starts a task. Alternatively, you can use the `new Task(...)` constructor, but such tasks are not started automatically—you must call the `Start()` method on them. New tasks are started on an available thread from the thread pool.

```csharp
Task task = Task.Run(() =>
{
    Console.WriteLine(Thread.CurrentThread.Name);
    Console.WriteLine("Hello from the thread pool");
});
```

The `Wait` method allows you to block the current thread and wait until the task completes.

```csharp
Console.WriteLine($"Is completed: {task.IsCompleted}");
task.Wait(); // blocks until task finishes
Console.WriteLine($"Is completed: {task.IsCompleted}");
```

## Returning values from a task

The `Task<TResult>.Run(Func<TResult> func)` method lets you create tasks that return a value. The `Result` property blocks until the task completes and then retrieves its value.

```csharp
Task<int> task1 = Task.Run(() => CountPrimes(1_000_000, 1_000_000));
Task<int> task2 = Task.Run(() => CountPrimes(2_000_000, 1_000_000));
Console.WriteLine("Started two tasks counting primes");
Console.WriteLine($"Primes(1'000'000, 2'000'000): {task1.Result}");
Console.WriteLine($"Primes(2'000'000, 3'000'000): {task2.Result}");

private static int CountPrimes(int from, int count)
{
    return Enumerable.Range(from, count).Count(n =>
    {
        if (n <= 1) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;

        var boundary = (int)Math.Floor(Math.Sqrt(n));
        
        for (int i = 3; i <= boundary; i += 2)
            if (n % i == 0)
                return false;

        return true; 
    });
}
```

## Exceptions in tasks

Unlike with threads, unhandled exceptions in a task do not cause the application to terminate immediately. Exceptions are captured and re-thrown when you wait for the task or access its result. Captured exceptions are wrapped in an `AggregateException`.

```csharp
class Program
{
    private static void Main()
    {
        Task task = Task.Run(ThrowsException);

        try
        {
            task.Wait();
        }
        catch (AggregateException e)
        {
            Console.WriteLine($"Handled: {e.InnerException?.Message}");
        }
    }

    private static void ThrowsException()
    {
        throw new Exception("Unhandled exception");
    }
}
```

## Continuations

Instead of blocking a thread with `Wait()` or `.Result`, you can attach a subsequent operation to the task. The `task.ContinueWith(...)` method is used for this purpose. It allows you to create chains of asynchronous operations without blocking threads.

```csharp
Task<int> t = Task.Run(() => CountPrimes(1000000, 1000000));

Task continuation = t.ContinueWith((Task<int> task) =>
{
    Console.WriteLine($"Primes: {task.Result}");
});

continuation.Wait();
```

## Long-running tasks

To avoid blocking a thread pool thread for too long, tasks should ideally last no more than a few hundred milliseconds. If you need to create a task that will run for a longer period, you can do so using the `Task.Factory.StartNew` method with the `TaskCreationOptions.LongRunning` option. A dedicated thread will be created for such a task.

```csharp
Task<int> t = Task.Factory.StartNew(() =>
{
    Console.WriteLine($"{Thread.CurrentThread.Name}");
    return CountPrimes(2, 1_000_000_000);
}, TaskCreationOptions.LongRunning);

t.Wait();
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/concurrency/Tasks" >}}
