---
title: "Asynchronous Programming"
weight: 50
---

# Asynchronous Programming

Asynchronous programming is a programming model that allows for the execution of long-running operations without blocking the application's main thread. This is important for ensuring the responsiveness of the user interface and the scalability of server applications.

In modern C#, asynchronous programming is primarily achieved using the `async` and `await` keywords. They work with the `Task` and `Task<TResult>` types, which represent asynchronous operations. This model allows writing asynchronous code that closely resembles synchronous code in its structure and readability.

## Awaiting - `await`

The `await` keyword is syntactic sugar that significantly simplifies working with asynchronous operations. The compiler translates this notation into a much more complex structure based on a state machine.

```csharp
var result = await expression;
statement(s);
```

> [!NOTICE]
> The *expression* is most often of type `Task` or `Task<TResult>`. However, any object with a `GetAwaiter` method that returns an *awaiter* will satisfy the compiler. See: [await-anything](https://devblogs.microsoft.com/pfxteam/await-anything/).

This transformation involves several steps:

1.  `GetAwaiter()`: An `awaiter` object, which manages the waiting process, is obtained from the expression (usually a Task).
2.  `IsCompleted`: An optimization is performed – if the operation is already complete, the rest of the code executes synchronously without switching threads.
3.  Context Capture: If the operation is not complete, `await` captures the current `SynchronizationContext`. This is crucial in UI applications to return to the main thread.
4.  `OnCompleted`: A "continuation" – the rest of the method – is registered. This code will be called in the future when the task completes.
5.  Return to Context: Within the continuation, the captured context is checked. If it exists (e.g., in a desktop application), the rest of the code is posted (`Post`) to be executed in the location indicated by the synchronization context. Otherwise, the code is executed on a thread from the thread pool.
6.  `GetResult()`: At the very end, the `GetResult()` method is called, which returns the result of the operation or throws an exception if the task ended in a faulted state.

```csharp
var awaiter = expression.GetAwaiter();

if (!awaiter.IsCompleted)
{
    var context = SynchronizationContext.Current;
    awaiter.OnCompleted(() =>
    {
        if (context != null)
        {
            context.Post(_ => 
            {
                var result = awaiter.GetResult();
                statement(s);
            }, null);
        }
        else
        {
            var result = awaiter.GetResult();
            statement(s);
        }
    });
}
else
{
    var result = awaiter.GetResult();
    statement(s);
}
```

> [!NOTE]
> The default behavior of `await` capturing the context can be disabled by using `await expression.ConfigureAwait(false)`.

> [!INFO]
> An example illustrating thread switching:
> {{< filetree dir="lectures/concurrency/AsyncAwaitThreads" >}}


## Asynchronous Methods

Asynchronous methods in C# are implemented as a form of coroutines. An asynchronous method in C# is a method marked with the `async` keyword. Marking a method as `async` has two main purposes:

1.  It allows the use of the `await` operator inside the method to wait for asynchronous operations (e.g., Tasks) to complete.
2.  It instructs the compiler to transform the method into a state machine that can manage suspending and resuming its execution.

*   The `async` keyword itself does not create a new thread. Simply marking a method as `async` does not make it run in the background. The method begins execution synchronously on the current thread. Only when it encounters an `await` on an operation that has not yet completed does the method suspend, and the thread is released.
*   Return Types: An `async` method must return one of three types:
    *   `Task`: For asynchronous operations that do not return a value.
    *   `Task<TResult>`: For operations that return a value of type TResult upon completion.
    *   `void`: **Recommended only for event handlers** (e.g., `async void Button_Click(...)`). Using `async void` elsewhere is bad practice because it makes exception handling and tracking the operation's completion difficult.

The following example shows a **synchronous** operation. The `GetPrimesCount()` method uses `Thread.Sleep(1000)`, which means the thread that called it is blocked for one second. If this were a UI thread, the application would become unresponsive for that time.
  
```csharp
void PrintPrimesCount()
{
    int primes = GetPrimesCount();
    Console.WriteLine($"Primes: {primes}");
}

int GetPrimesCount()
{
    Thread.Sleep(1000);
    return 42;
}
```

Below, the same goal is achieved **asynchronously**:

```csharp
async Task PrintPrimesCountAsync()
{
    Task<int> primesTask = GetPrimesCountAsync();
    Console.WriteLine($"Primes: {await primesTask}");
}

async Task<int> GetPrimesCountAsync()
{
    await Task.Delay(1000);
    return 42;
}
```

> [!NOTE] 
> Lambda expressions can also be asynchronous. The same rules apply to them as to regular async methods.

> [!NOTE]
> C# also supports asynchronous streams through the `IAsyncEnumerable` interface and the `await foreach` construct. Although `yield return` within an iterator block is always synchronous, the entire iterator can fetch data asynchronously, suspending its execution between elements.

> [!INFO]
> An example of a windowed application with an asynchronous call:
> {{< filetree dir="lectures/concurrency/AsyncAwaitUI" >}}

## Parallelism

Asynchronous programming can be used to achieve parallel code execution.

```csharp
class Program
{
    private static async Task Main()
    {
        Task<int> task1 = CountPrimesAsync(0, 1000);
        Task<int> task2 = CountPrimesAsync(1000, 2000);
        Console.WriteLine($"Primes(0-1000): {await task1}");
        Console.WriteLine($"Primes(1000-2000): {await task2}");
    }

    static async Task<int> CountPrimesAsync(int start, int end)
    {
        int count = 0;

        for (int i = start; i < end; i++)
        {
            if (await IsPrime(i))
            {
                count++;
            }
        }

        return count;
    }

    static async Task<bool> IsPrime(int number)
    {
        if (number < 2)
            return false;

        return await Task.Run(() => 
        {
            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0)
                    return false;
            }
            return true;
        });
    }
}
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/concurrency/AsyncAwaitPrimes" >}}

## Waiting for Multiple Tasks

### `Task.WhenAll`

`Task.WhenAll` creates a task that will complete only when all of the tasks in the provided collection have completed.

```csharp
Task<int> task1 = CountPrimesAsync(1, 100);
Task<int> task2 = CountPrimesAsync(101, 200);
int[] primesCounts = await Task.WhenAll(task1, task2);
```

*   If `Task.WhenAll` receives a collection of `Task` objects, it returns a `Task`.
*   If it receives a collection of `Task<TResult>` objects, it returns a `Task<TResult[]>`, which is a task whose result is an array of results from all the completed tasks (in the same order as the input tasks).

### `Task.WhenAny`

`Task.WhenAny` creates a task that will complete as soon as any of the tasks in the provided collection completes.

```csharp
Task<int> task1 = CountPrimesAsync(0, 100);
Task<int> task2 = CountPrimesAsync(100, 200);
Task<int> completedTask = await Task.WhenAny(task1, task2);
Console.WriteLine($"Primes: {await completedTask}");
```

`Task.WhenAny` returns a `Task<Task>` (or `Task<Task<TResult>>`).
*   The outer `Task` completes when any task from the collection completes.
*   The result of the outer `Task` is the inner `Task` that just completed. You need to unwrap it (e.g., with another `await`) to get its result.


## Cancelling Tasks

The mechanism for canceling tasks is based on two related types: `CancellationTokenSource` and `CancellationToken`.

1.  `CancellationTokenSource` – An object that creates `CancellationToken`s and signals cancellation.
2.  `CancellationToken` – A struct passed to a task. The task uses it to check if cancellation has been requested.

How it works:

1.  You create an instance of `CancellationTokenSource`. The constructor accepts an optional `int timeout` parameter; if present, cancellation will be automatically signaled after the specified time.
2.  From this source, you get a `CancellationToken` using the `Token` property.
3.  You pass this token to the task you want to be able to cancel.
4.  Inside the task, you must periodically check the token's state. The simplest way is to call the `token.ThrowIfCancellationRequested()` method. It throws an `OperationCanceledException` if cancellation has been signaled. Alternatively, you can check the token's state with the `token.IsCancellationRequested` property.
5.  To initiate cancellation, you call the `Cancel()` method on the `CancellationTokenSource` object.
6.  When a task is canceled, awaiting it (e.g., via `Wait()` or `await`) will result in an `OperationCanceledException`.

```csharp
class Program
{
    private static async Task Main()
    {
        var cancellationSource = new CancellationTokenSource(5000);
        try
        {
            List<int> primes = await GetPrimesAsync(2, cancellationSource.Token);
            Console.WriteLine($"Number of primes: {primes.Count}");
            if (primes.Count > 0)
            {
                Console.WriteLine($"Last prime: {primes[^1]}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Canceled");
        }
    }
    
    static async Task<List<int>> GetPrimesAsync(int start, CancellationToken token)
    {
        List<int> primes = [];

        for (int i = start; i < int.MaxValue; i++)
        {
            // if (token.IsCancellationRequested) break;
            token.ThrowIfCancellationRequested();
            if (await IsPrime(i, token))
            {
                primes.Add(i);
            }
        }

        return primes;
    }

    static async Task<bool> IsPrime(int number, CancellationToken token)
    {
        if (number < 2)
            return false;
        
        return await Task.Run(() =>
        {
            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                // if (token.IsCancellationRequested) break;
                token.ThrowIfCancellationRequested();
                if (number % i == 0)
                    return false;
            }

            return true;
        });
    }
}
```

> [!NOTE]
> Most built-in asynchronous methods include an overload that accepts a `CancellationToken`.

> [!INFO]
> Source code:
> {{< filetree dir="lectures/concurrency/AsyncAwaitCancellation" >}}
