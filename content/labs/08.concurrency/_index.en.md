---
title: "Concurrency"
weight: 80
---

**# Concurrency**

So far, all the programs we have written have been single-threaded. Successive program instructions were executed sequentially, one after another. This way of executing code is sufficient in most simple cases, but in more complex programs it can lead to inefficient use of available resources.

**Concurrent programming** is an approach that allows a program to handle multiple tasks at the same time by interleaving their execution. These tasks do not have to be executed at precisely the same moment; actual simultaneous execution depends, among other things, on the number of available processor cores. Concurrency therefore allows a program to organize its work so that multiple independent tasks can make progress independently of one another.

Concurrency can be implemented, among other methods, using **multiple threads**. If several processor cores are available, individual tasks can additionally be executed **in parallel**, meaning that they actually run at the same time.

**## Useful Functions, Classes, and Keywords**

**### async, await**

A pair of keywords used to create asynchronous methods, which allow time-consuming operations to be performed without blocking a thread while waiting for them to complete.

Asynchrony does not mean that a program can only execute one task at a time. For example, we can start several asynchronous operations and then wait for them to complete using `Task.WhenAll`. This allows the operations to run concurrently.

Imagine a program that needs to download data from an HTTP server. This operation can take a significant amount of time because a connection must be established with the server and the data must be transferred. Some of this time is due to network latency, which is beyond the direct control of the user's computer.

In this case, we can use the `await` keyword.

```
await DownloadDataFromTheInternet();
```

`await` waits for the specified task to complete. If the task has not yet completed, the asynchronous method is suspended at this point, and the thread can continue working or be used to perform other tasks. Once the awaited task completes, execution of the method resumes from the point where `await` was used.

If the awaited task returns a result, `await` also allows us to retrieve that result.

```
string data = await DownloadDataFromTheInternet();
```

The `async` keyword is placed before the declaration of a method that uses `await`. A method marked with `async` typically returns `Task` or `Task<T>`. `Task` represents an asynchronous operation that does not return a value, whereas `Task<T>` represents an operation whose result will be a value of type `T`.

```
async Task Download()
{
    string data = await DownloadDataFromTheInternet();
}
```

The method does not return the result directly; instead, it returns a `Task` representing the operation being performed.

If the method returns a result, we define it as follows:

```
async Task<string> DownloadData()
{
    return await DownloadDataFromTheInternet();
}
```

The method itself returns an object of type `Task<string>`. Only by using `await` can we obtain the actual result of type `string`.

```
string data = await DownloadData();
```

`async` does not, by itself, start a new thread. It indicates that the method can perform asynchronous operations and can be suspended at points marked with the `await` keyword.

**### Task Parallel Library (TPL)**

This is a library that provides mechanisms for creating concurrent and parallel programs. One of its most important components is the `Task` class, which represents an *\*asynchronous operation\**. A `Task` object allows us to track the state of an operation, wait for it to complete, and, in the case of `Task<T>`, obtain its result.

There are many ways to create and start tasks. One of them is the `Task.Run()` method. It is primarily used to schedule a synchronous but computationally expensive operation for execution.

```
Task<int> task = Task.Run(CalculateResults);
```

Calling `Task.Run()` returns a `Task<int>` object representing the operation that has been started. The operation executes on a thread from the thread pool, allowing the calling thread to perform other tasks in the meantime.

We can wait for the operation to finish using `await`.

```
int result = await task;
```

If the task has not yet completed, `await` suspends the method without blocking the thread. Execution of the method resumes once the task has finished.

However, `Task.Run()` is not the only way to create `Task` objects. In particular, I/O operations, such as network communication or reading files, may provide their own asynchronous methods that return `Task` or `Task<T>`, without requiring their execution to be moved to a separate thread.

Example:

```
static int CalculateResults()
{
    // time-consuming calculations
    return result;
}
```

If `CalculateResults()` performs an expensive computation:

```
static async Task Main(string[] args)
{
    Task<int> task = Task.Run(CalculateResults);

    // Perform other operations

    // At some point, we can retrieve the calculation result
    int result = await task;
}
```

`Task.Run` starts the calculation on a thread from the thread pool, while `await task` allows us to wait for it to complete later and retrieve the result.

A common practice when starting tasks is to use **lambda expressions** as arguments to `Task.Run()`. This allows local variables to be passed as arguments.

```
Task task = Task.Run(() => CalculateResults(localVariable1, localVariable2));
```

**### Parallel**

A class containing a set of methods that allow operations to be parallelized. They are primarily used to improve performance by making fuller use of the processor's capabilities through the simultaneous execution of multiple smaller parts of a task.

**Parallel.For** - used to parallelize traditional index-based `for` loops.

```
int[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
int[] results = new int[data.Length];

Parallel.For(0, data.Length, i =>
{
    results[i] = data[i] * data[i];

    Console.WriteLine($"Index {i} processed by thread {Environment.CurrentManagedThreadId}");
});
```

**Parallel.ForEach** - processes the elements of a collection (an `IEnumerable`, e.g. a list) concurrently across multiple threads.

```
var numbers = Enumerable.Range(1, 10000);

Parallel.ForEach(numbers, number =>
{
    ProcessNumber(number);
});

```

**### Lock**

When different threads access shared resources, serious errors can occur in the program's behavior, creating a need for synchronization.

Let's illustrate this with an example of a program simulating a warehouse that supplies goods to couriers. A courier thread returning to the warehouse first checks whether there are any goods available for collection, then increments its own counter of carried goods and decrements the warehouse inventory counter. If we do not use any synchronization mechanisms, a situation may arise in which one courier checks the warehouse inventory and finds one package. Then, before that courier decrements the inventory counter, a second courier may arrive and check the inventory as well. In this case, both couriers may execute the package collection operation, causing the warehouse inventory to drop to -1.

The simplest solution to this problem in C# is the **lock** statement. It provides mutual exclusion: two threads cannot enter a code block protected by **lock** at the same time. To use a lock, we first need to define an object:

```
private readonly System.Threading.Lock _lockObj = new();
```

Each object defined in this way represents a separate lock. It is used as follows:

```
lock(_lockObj)
{
    // operation requiring synchronization
}
```

If one thread enters the protected block, any other thread attempting to enter it must wait until the lock becomes available.

**## Starter Code**

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab10/student" >}}

**## Fractal Generation**

{{% hint info %}}
**What Is Parallel Programming?**

Parallel programming is a programming paradigm that takes advantage of the architecture of modern multi-core CPUs to perform multiple computations simultaneously.

Instead of processing data sequentially on a single thread, we divide the work among multiple threads that execute their parts of the task at the same time.

{{% /hint %}}

**### Task Description**

The goal of this task is to implement and compare the execution times of different methods for parallelizing the computations performed when generating the [Mandelbrot set](https://en.wikipedia.org/wiki/Mandelbrot_set).

The starter code contains the abstract class `MandelbrotSetGenerator`, which manages the entire process of generating the fractal and saving it to a `.png` file.

The `SingleThreadGenerator` class is a concrete single-threaded implementation of the generator. It uses a simple nested `for` loop to iterate over all pixels of the generated image. It will serve as the baseline for performance measurements.

Implement the following methods for parallelizing the computations:

- `MultiThreadGenerator`: A multithreaded approach that manually creates and manages `Thread` objects.
- `TasksGenerator`: An approach that uses the `Task` class from the TPL (*Task Parallel Library*) to manage parallel work using the thread pool (`ThreadPool`).
- `ParallelGenerator`: An approach that uses the high-level `Parallel` class from the TPL.

`Program.cs` contains the logic for measuring execution time and running each generator in sequence.

The correctly generated fractal should look as follows:

<div style="text-align: center;">
  <img src="/labs/lab10/mandelbrotset.png" width="400px" alt="mandelbrotset.png" />
</div>

{{% hint warning %}}
**Implementation Notes**

- **Number of threads:**
  - In the `MultiThreadGenerator` and `TasksGenerator` implementations, create `N` units of work (threads/tasks), where `N` equals the number of processor cores. This is the optimal number for tasks that fully utilize the CPU.
- **Dividing the work:**
  - For fractal generation, the simplest strategy is to divide the image into `N` equal horizontal strips.
  - Calculate how many rows should be assigned to each thread, then use a loop to pass each thread/task the appropriate range of rows to process.

{{% /hint %}}

{{% hint info %}}
**Additional Resources:**

- [Microsoft Learn: Threads and threading](https://learn.microsoft.com/en-us/dotnet/standard/threading/threads-and-threading)
- [Microsoft Learn: Task Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-9.0)
- [Microsoft Learn: Write a Simple Parallel.For Loop](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-simple-parallel-for-loop)

{{% /hint %}}

**### Example Solution**

> [!TIP]
> **Solution**
> {{< filetree dir="labs/lab10/solution/FractalsGenerator" >}}

**## RayTracer**

{{% hint info %}}
**What Is Parallel Programming?**

Parallel programming is a programming paradigm that takes advantage of the architecture of modern multi-core CPUs to perform multiple computations simultaneously.

Instead of processing data sequentially on a single thread, we divide the work among multiple threads that execute their parts of the task at the same time.

{{% /hint %}}

**### Task Description**

The task is to transform a synchronous ray tracing program into an asynchronous application that does not block the graphical user interface during rendering.

Move the rendering operation to a background task using `Task.Run()`, then use `async` and `await` to asynchronously wait for the calculations to complete. During rendering, report progress using `IProgress`.

1. Run the application in its synchronous version and observe how the graphical user interface behaves during rendering. Pay attention to the window becoming unresponsive and the inability to update interface elements while rendering is in progress.

2. Modify the rendering button's event handler so that rendering is performed in the background using `Task.Run()`.

3. Apply the `async` and `await` keywords to allow the program to wait asynchronously for rendering to complete without blocking the UI thread.

4. Add progress reporting using `IProgress`.

**### Example Solution**

> [!TIP]
> **Solution**
> {{< filetree dir="labs/lab10/solution/RayTracer" >}}