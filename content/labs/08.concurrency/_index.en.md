---
title: "Concurrency"
weight: 80
---

## Tutorial 8: Concurrency

The scope of this laboratory covers concurrent and parallel programming in C#: threads, the thread pool and tasks (`Task`, `Task<T>`), the `Parallel` class, basic synchronization (`Interlocked`, `lock`), cooperative cancellation, progress reporting, and asynchronous programming with `async`/`await`.

We will not start from scratch. In [Tutorial 3]({{< ref "/labs/03.basics" >}}) you built a ray tracer that renders an image using a single thread. Ray tracing is a perfect example of a CPU-bound task that can be parallelized: the color of every pixel is computed independently of all other pixels. In this tutorial, we will make the ray tracer **fast** (use all processor cores) and **responsive** (show progress, allow cancellation, and save images asynchronously).

Two kinds of concurrency will appear in the task:
- **Parallelism** – CPU-bound work is split among several cores, so it finishes sooner (rendering).
- **Asynchrony** – while waiting for an operation that does not need the processor (e.g. writing a file), no thread is blocked, so the program can do something else in the meantime (saving frames).

### Threads

A thread is the basic unit of execution in a process. Threads of one process share its memory (the heap, static fields), but each thread has its own call stack. The `System.Threading.Thread` class represents an operating system thread – it is the C# counterpart of `std::thread` from C++:

```csharp
Thread worker = new Thread(() => Console.WriteLine($"Hello from thread {Environment.CurrentManagedThreadId}"));
worker.Start(); // the new thread starts running the lambda
Console.WriteLine($"Hello from thread {Environment.CurrentManagedThreadId}");
worker.Join();  // wait until the thread finishes
```

The order of the two lines in the output is not determined – it depends on the operating system scheduler. Compared to C++, there are a few differences:
- A `Thread` object does not have to be joined (there is nothing like `std::terminate` for a joinable thread being destroyed). Threads are *foreground* threads by default – the process does not end until all of them finish (unless `IsBackground = true` is set).
- An unhandled exception in any thread terminates the whole process.

More details can be found in the lecture on [threads]({{< ref "/lectures/09.concurrency/threads" >}}).

### The Thread Pool and Tasks

Creating an operating system thread is relatively expensive (kernel call, separate stack). Therefore, .NET maintains a **thread pool** – a set of worker threads that execute short pieces of work from a queue and are then reused. Pool threads are background threads.

In modern C#, threads are rarely created by hand. Instead, we use the `Task` class (no result) and `Task<T>` (with a result of type `T`), which represent an operation that will complete in the future. The `Task.Run` method queues a delegate to the thread pool and returns a task. This is similar to `std::async` and `std::future<T>` from C++ – `task.Result` blocks like `future.get()`:

```csharp
Task<long> task = Task.Run(() => SumOfSquares(1_000_000)); // runs on a thread pool thread
// ... the calling thread is free to do something else here ...
long result = task.Result; // blocks until the task completes
Console.WriteLine(result); // 333333833333500000

static long SumOfSquares(int n)
{
    long sum = 0;
    for (int i = 1; i <= n; i++)
    {
        sum += (long)i * i;
    }
    return sum;
}
```

More details can be found in the lecture on [tasks]({{< ref "/lectures/09.concurrency/tasks" >}}).

### `async` and `await`

Blocking a thread while waiting (`task.Result`, `task.Wait()`, `Thread.Sleep`) wastes it – the thread cannot do anything else in the meantime. The `await` operator waits **asynchronously**: if the task has not completed yet, the method is suspended and the thread is released; when the task completes, the rest of the method (a *continuation*) is executed. A method using `await` must be marked as `async` and typically returns `Task` or `Task<T>`. The compiler transforms it into a state machine – similarly to C++20 coroutines and `co_await`.

```csharp
int count = await CountLinesAsync("data.txt");

static async Task<int> CountLinesAsync(string path)
{
    string[] lines = await File.ReadAllLinesAsync(path); // no thread waits for the disk
    return lines.Length;
}
```

Note that `async` itself does not create any threads – it only allows the method to be suspended. More details can be found in the lecture on [asynchronous programming]({{< ref "/lectures/09.concurrency/asynchronous-programming" >}}).

### Shared Data and Race Conditions

When several threads modify the same data, the result may depend on the timing of their execution – this is called a **race condition**. Even `counter++` is not safe, because it consists of three steps (read, add, write), and two threads may read the same old value:

```csharp
int unsafeCounter = 0;
int safeCounter = 0;

void Work()
{
    for (int i = 0; i < 1_000_000; i++)
    {
        unsafeCounter++;                        // read-modify-write: updates get lost
        Interlocked.Increment(ref safeCounter); // atomic increment
    }
}

Task.WaitAll(Task.Run(Work), Task.Run(Work));
Console.WriteLine($"unsafe: {unsafeCounter}, safe: {safeCounter}"); // e.g. unsafe: 1470357, safe: 2000000
```

The basic tools to avoid race conditions are:
- The `Interlocked` class – atomic operations on single variables (`Increment`, `Add`, `Exchange`, `CompareExchange`), the counterpart of `std::atomic` in C++.
- The `lock` statement – a critical section that at most one thread can execute at a time, the counterpart of `std::mutex` with `std::lock_guard`. Since C# 13 (.NET 9), the dedicated `System.Threading.Lock` type is recommended as the lock object.

The best solution, however, is to avoid shared mutable data altogether. More details can be found in the lectures on [synchronization]({{< ref "/lectures/12.synchronization/introduction" >}}) and [exclusive locking]({{< ref "/lectures/12.synchronization/exclusive-locking" >}}).

### Measuring Performance

To measure execution time, we will use the `System.Diagnostics.Stopwatch` class (`Stopwatch.StartNew()`, `Restart()`, `Elapsed`). The **speedup** of a parallel version is the ratio of the sequential time to the parallel time. When measuring performance, remember:
- Always measure the **Release** configuration: `dotnet run -c Release`. In the Debug configuration, the JIT compiler does not optimize the code – our ray tracer runs about 6 times slower.
- The first run of the code also includes JIT compilation (warm-up), so it is usually slightly slower than subsequent runs.
- `Environment.ProcessorCount` returns the number of **logical** processors. With simultaneous multithreading (SMT, Hyper-Threading), two logical processors share one physical core, so 16 logical processors on an 8-core machine do not give a 16× speedup.

## Task 1 - Fast and Responsive Ray Tracer

In this task, you will parallelize the ray tracer from [Tutorial 3]({{< ref "/labs/03.basics" >}}) in several ways and compare them: manually created threads, tasks, and the `Parallel` class. Then you will add progress reporting and cancellation, and finally, render an animation in which frames are rendered in parallel and saved asynchronously.

### Starter Code

Download the starter code. It is the final version of the ray tracer from Tutorial 3 (stage 5) with a few changes:
- The solution uses the new `.slnx` format.
- The image is no longer written to the standard output. The new `Image.Save(string path)` method writes it to a PPM file, so the console can be used for messages.
- `Camera` has a new read-only property `ImageHeight`.
- `Program.cs` builds the scene in the `CreateScene()` method, uses a smaller resolution (640x360) and 64 samples per pixel, and measures the rendering time using `Stopwatch`.

Run the program in the Release configuration:

```bash
dotnet run -c Release
```

Example output (your times will differ):

```text
Rendering 640x360, 64 samples per pixel
Sequential: 2.94 s
Saved image.ppm
```

> [!NOTE]
> #### Starter code
> {{< filetree dir="labs/w08.concurrency/Raytracer.00" >}}

Before parallelizing anything, we need to know which data is shared between threads. During rendering, the scene (`Scene`, objects, materials) and the camera parameters are **only read**. The only data that is written is the resulting image. Every pixel is written exactly once, so if every pixel is computed by exactly one thread, the threads do not interfere with each other. Our unit of work will be a single row of the image.

### Stage 1: Threads

In this stage, we will divide the image rows into equal horizontal strips and render each strip on a separate, manually created thread.

#### Refactoring: Rendering a Single Row

In `Camera.cs`:
1. Add a private method `Vector3[] RenderRow(IHittable world, int j)`, which renders row `j` of the image and returns an array of `ImageWidth` pixel colors. Move the body of the outer loop of `Render` into it (the loops over pixels and samples).
2. In `Image.cs`, add a method `void SetRow(int y, Vector3[] row)`, which copies the given colors to row `y` of the image.
3. Simplify `Render` so that for each row `j` it calls `image.SetRow(j, RenderRow(world, j))`.

Run the program – the result should not change.

#### Rendering with Threads

In `Camera`, add a method `public Image RenderWithThreads(IHittable world, int threadCount)`:
1. Create an `Image` and an array of `threadCount` threads (`Thread[]`).
2. Compute the number of rows per thread rounding up: `rowsPerThread = (_imageHeight + threadCount - 1) / threadCount`.
3. In a loop over `t`, compute the range of rows `[startRow, endRow)` of thread `t` (the last strip may be shorter – use `Math.Min`), create a thread with a lambda rendering rows from that range, and start it with `Start()`.
4. After starting all threads, wait for each of them with `Join()` and return the image.

> [!IMPORTANT]
> #### Lambdas Capture Variables, Not Values
>
> In C++, you decide whether a lambda captures by value (`[=]`) or by reference (`[&]`). In C#, a lambda always captures **variables** – it sees their current value at the moment it runs, not at the moment it was created. The loop variable of a `for` loop is a single variable shared by all iterations, so the following code is incorrect:
> ```csharp
> Thread[] threads = new Thread[4];
> for (int t = 0; t < threads.Length; t++)
> {
>     threads[t] = new Thread(() => Console.WriteLine($"Rendering strip {t}"));
>     threads[t].Start();
> }
> ```
> By the time the threads run, the loop has usually finished, and all of them print `Rendering strip 4` – a strip that does not exist. The solution is to copy the value to a variable declared **inside** the loop body – each iteration gets a fresh variable:
> ```csharp
> int strip = t; // a fresh variable in every iteration
> threads[t] = new Thread(() => Console.WriteLine($"Rendering strip {strip}"));
> ```
> In our method, compute `startRow` and `endRow` inside the loop body before creating the thread. (The variable of a `foreach` loop is fresh in every iteration, so this problem only concerns `for` loops and variables declared outside the loop.)

> [!NOTE]
> **Why don't we need a lock?** The threads read shared data (the scene, the camera), which nobody modifies during rendering, and write to disjoint rows of the image. Concurrent reads are safe, and writes to different array elements do not interfere with each other. Moreover, `Join()` guarantees that everything the thread has written is visible to the thread that called `Join()` – it is safe to use the image after all threads have been joined.

> [!IMPORTANT]
> #### Thread Safety of `Random`
>
> Rendering uses random numbers (antialiasing, scattering), and we already use `Random.Shared` for them. `Random.Shared` is a thread-safe instance that can be used from many threads at once. In contrast, a regular `Random` object (`new Random()`) is **not** thread-safe – if it were shared by many threads without synchronization, its internal state could get corrupted (the documentation warns that it may then start returning only zeros).

#### Measuring the Speedup

In `Program.cs`:
1. Store `Environment.ProcessorCount` in the `threadCount` variable and print it in the header line.
2. Measure the sequential `Render` and then `RenderWithThreads(world, threadCount)` with one `Stopwatch` (use `Restart()` between measurements).
3. Add a helper method that prints the name of the variant, its time, and the speedup relative to the sequential version:

```csharp
static void Report(string name, double seconds, double baseline)
{
    Console.WriteLine($"{name,-24}{seconds,6:F2} s   speedup {baseline / seconds,4:F1}x");
}
```

Save the image rendered by threads to `image.ppm` and check that it looks the same as before (no missing rows).

Example output on an 8-core processor with 16 logical processors:

```text
Rendering 640x360, 64 samples per pixel, 16 logical processors
Sequential                3.18 s   speedup  1.0x
Threads (16)              0.49 s   speedup  6.5x
Saved image.ppm
```

The speedup is much smaller than 16. One reason is SMT (only 8 physical cores). To find another one, temporarily measure the time of every thread – start a `Stopwatch` at the beginning of the lambda and print the time after the loop (copy `t` to a local variable `index` for the message and add `using System.Diagnostics;` to `Camera.cs`):

```csharp
Console.WriteLine($"Thread {index,2}: rows {startRow,3}-{endRow - 1,3} took {stopwatch.Elapsed.TotalSeconds:F2} s");
```

```text
Thread  2: rows  46- 68 took 0.09 s
Thread  4: rows  92-114 took 0.09 s
Thread  0: rows   0- 22 took 0.10 s
Thread  5: rows 115-137 took 0.10 s
Thread  3: rows  69- 91 took 0.10 s
Thread  1: rows  23- 45 took 0.12 s
Thread  6: rows 138-160 took 0.22 s
Thread 15: rows 345-359 took 0.21 s
Thread  7: rows 161-183 took 0.26 s
Thread 13: rows 299-321 took 0.28 s
Thread 14: rows 322-344 took 0.30 s
Thread 12: rows 276-298 took 0.33 s
Thread  8: rows 184-206 took 0.34 s
Thread 11: rows 253-275 took 0.35 s
Thread 10: rows 230-252 took 0.42 s
Thread  9: rows 207-229 took 0.50 s
Threads (16)              0.51 s   speedup  6.3x
```

The rows are not equally expensive. A ray that hits the sky ends after the first bounce, while rays that hit the spheres and the ground bounce several times. The threads rendering the sky finish after 0.1 s and then sit idle, while the whole render takes as long as the slowest thread. This problem is called **load imbalance**, and splitting the work into fixed, equal parts (*static partitioning*) suffers from it. Remove the diagnostic output before moving on.

> [!NOTE]
> #### Stage 1
> {{< filetree dir="labs/w08.concurrency/Raytracer.01" >}}

### Stage 2: Tasks and the Thread Pool

Instead of a few big strips, we will now create many small pieces of work – one task per row – and let the thread pool distribute them. A pool thread that finishes a row immediately takes the next one from the queue, so threads that got cheap rows simply render more of them (*dynamic load balancing*). Additionally, instead of writing to a shared image, each task will **return** its row as a result.

#### Rendering with Tasks

In `Camera`, add a method `public async Task<Image> RenderWithTasksAsync(IHittable world)`:
1. Create an array `Task<Vector3[]>[] tasks` with one element per row.
2. In a loop over rows, start each task with `Task.Run(() => RenderRow(world, row))`. Remember to copy the loop variable to a local variable first (see the previous stage).
3. Wait for all tasks with `Vector3[][] rows = await Task.WhenAll(tasks);`. The results are returned in the same order as the tasks in the array, regardless of the order in which they completed.
4. Create an image, fill it with `SetRow`, and return it.

> [!IMPORTANT]
> #### Asynchronous `Main`
>
> `await` can only be used in an `async` method. To call `RenderWithTasksAsync` with `await`, change the signature of `Main` to `static async Task Main(string[] args)` – the runtime then waits for the returned task before ending the program. By convention, names of methods returning tasks end with `Async`.

In `Program.cs`, change `Main` to an asynchronous method and add the measurement of `await cam.RenderWithTasksAsync(world)` (name it `Tasks (one per row)`). Save the image produced by tasks.

> [!IMPORTANT]
> #### Exceptions in Tasks
>
> An exception thrown inside a task does not crash the program – it is stored in the task and rethrown to whoever waits for it. How it is rethrown depends on the way of waiting:
> - Blocking waits (`Wait()`, `Result`, `Task.WaitAll`) throw an `AggregateException`, which contains all exceptions in the `InnerExceptions` property.
> - `await` rethrows the original exception (unwrapped). For `Task.WhenAll`, it is the first of the exceptions – all of them are available in the `Exception` property of the task returned by `WhenAll`.
> ```csharp
> Task<int>[] tasks = [Task.Run(() => 42), Task.Run(() => Fail(7)), Task.Run(() => Fail(42))];
>
> try
> {
>     Task.WaitAll(tasks); // blocking wait
> }
> catch (AggregateException e)
> {
>     foreach (Exception inner in e.InnerExceptions)
>     {
>         Console.WriteLine($"WaitAll: {inner.Message}");
>     }
> }
>
> Task<int[]> all = Task.WhenAll(tasks);
> try
> {
>     int[] results = await all; // asynchronous wait
> }
> catch (InvalidOperationException e)
> {
>     Console.WriteLine($"await: {e.Message}");
>     Console.WriteLine($"all.Exception contains {all.Exception!.InnerExceptions.Count} exceptions");
> }
>
> static int Fail(int row) => throw new InvalidOperationException($"Row {row} failed");
> ```
> ```text
> WaitAll: Row 7 failed
> WaitAll: Row 42 failed
> await: Row 7 failed
> all.Exception contains 2 exceptions
> ```

Example output:

```text
Rendering 640x360, 64 samples per pixel, 16 logical processors
Sequential                3.28 s   speedup  1.0x
Threads (16)              0.50 s   speedup  6.5x
Tasks (one per row)       0.31 s   speedup 10.6x
Saved image.ppm
```

The tasks are clearly faster – but not because tasks are faster than threads (they are executed by threads after all). The difference comes from the size of the work items: if you started only 16 tasks, each rendering one strip as in stage 1, you would get the same time as with threads. Many small work items allow the pool to balance the load dynamically.

> [!TIP]
> The granularity of work matters in both directions. Work items that are too big cause load imbalance; work items that are too small cause overhead – creating and scheduling a task costs much more than rendering one pixel, so one task per pixel (230,400 tasks) would be a bad idea. A row (several milliseconds of work) is a reasonable compromise.

> [!NOTE]
> #### Stage 2
> {{< filetree dir="labs/w08.concurrency/Raytracer.02" >}}

### Stage 3: `Parallel.For`

Parallelizing a loop whose iterations are independent is so common that .NET provides a ready-made solution: the `Parallel` class. `Parallel.For(from, to, body)` executes `body(i)` for every `i` in `[from, to)` on the thread pool – it is the counterpart of the C++17 parallel algorithms (`std::for_each(std::execution::par, ...)`). In this stage, we will also add a progress bar, which requires synchronization.

> [!IMPORTANT]
> #### `Parallel.For` and `ParallelOptions`
>
> - `Parallel.For` is **blocking** – it returns after all iterations have finished. The calling thread also takes part in the work.
> - It partitions the work dynamically: each worker repeatedly takes a small batch of consecutive indices (the batches grow as the loop progresses), and workers that run out of work take over the remaining parts of the range.
> - An exception thrown by any iteration stops the loop; all exceptions are rethrown wrapped in an `AggregateException`.
> - The overload with `ParallelOptions` allows setting `MaxDegreeOfParallelism` (the maximum number of iterations executed at the same time; the default `-1` means no limit) and `CancellationToken` (used in the next stage).
>
> More details can be found in the lecture on [the `Parallel` class]({{< ref "/lectures/09.concurrency/parallel" >}}).

#### Rendering with `Parallel.For`

In `Camera`, add a method `public Image RenderParallel(IHittable world, ParallelOptions options, ProgressBar? progressBar = null)`, which renders all rows using `Parallel.For(0, _imageHeight, options, j => ...)`. After rendering each row, increase the counter of finished rows and pass its new value to the progress bar:

```csharp
int rowsDone = 0;

Parallel.For(0, _imageHeight, options, j =>
{
    image.SetRow(j, RenderRow(world, j));

    // rowsDone++ would not be atomic - many threads update the counter at once
    int done = Interlocked.Increment(ref rowsDone);
    progressBar?.Update(done);
});
```

> [!IMPORTANT]
> #### `Interlocked`
>
> `Interlocked.Increment(ref x)` atomically increments the variable and **returns the new value**. Always use the returned value: reading `rowsDone` again a moment later could already give a value increased by another thread. `Interlocked` operations are cheaper than a lock, but they only work on single variables.

#### Progress Bar

Create a file `ProgressBar.cs` with a class `ProgressBar` drawing a text progress bar, for example `[##########------------------------------]  25%`. The class has a primary constructor (C# 12) with the total number of steps and the width of the bar: `ProgressBar(int total, int width = 40)`, and a method `void Update(int done)`, which will be called by many threads at the same time.

Drawing consists of several operations: computing the percentage, checking whether it has changed, writing the bar. They must not be interleaved between threads, so put them in a critical section guarded by a `lock`:

```csharp
public class ProgressBar(int total, int width = 40)
{
    private readonly Lock _lock = new();
    private int _lastPercent = -1;

    public void Update(int done)
    {
        int percent = done * 100 / total;

        lock (_lock)
        {
            // Another thread may have already drawn a newer state
            if (percent <= _lastPercent)
            {
                return;
            }
            _lastPercent = percent;

            // TODO: draw the bar
        }
    }
}
```

1. To draw the bar, write it with `Console.Write` starting with the `\r` character (carriage return), which moves the cursor to the beginning of the line – the next update overwrites the previous one. Strings of repeated characters can be created with `new string('#', count)`.
2. When the progress reaches 100%, move to the next line (`Console.WriteLine()`).

> [!IMPORTANT]
> #### `lock` and `System.Threading.Lock`
>
> The `lock (obj) { ... }` statement guarantees that at most one thread at a time executes the block; other threads wait at the entrance. It is the counterpart of `std::lock_guard<std::mutex>` in C++ – the lock is released when leaving the block, even by an exception. In older code you will see any private object used as the lock (`private readonly object _lock = new();`); since C# 13 (.NET 9), the dedicated `System.Threading.Lock` type is preferred – it expresses the intent and can be more efficient.
>
> Why do we need `_lastPercent`? `Interlocked.Increment` assigns consecutive values atomically, but it does not determine the order in which the threads then enter the lock. A thread holding the value 50 can draw after a thread holding 51 – without the check, the bar would occasionally go backwards.

#### Comparing the Variants

In `Program.cs`:
1. Measure `RenderParallel` for `MaxDegreeOfParallelism` equal to 1, 2, 4, 8, and `threadCount` (e.g. iterate over an array `int[] degrees = [1, 2, 4, 8, threadCount];`).
2. Finally, measure `RenderParallel` with default options (`new ParallelOptions()`) and the progress bar `new ProgressBar(cam.ImageHeight)`, and save this image.

Example output on an 8-core processor with 16 logical processors:

```text
Rendering 640x360, 64 samples per pixel, 16 logical processors
Sequential                3.26 s   speedup  1.0x
Threads (16)              0.51 s   speedup  6.5x
Tasks (one per row)       0.31 s   speedup 10.4x
Parallel.For (1)          2.95 s   speedup  1.1x
Parallel.For (2)          1.45 s   speedup  2.2x
Parallel.For (4)          0.79 s   speedup  4.1x
Parallel.For (8)          0.45 s   speedup  7.2x
Parallel.For (16)         0.46 s   speedup  7.1x
[########################################] 100%
Parallel.For + progress   0.44 s   speedup  7.5x
Saved image.ppm
```

Observations:
- Up to the number of physical cores, the time decreases almost linearly. Above it, the gain is small – logical processors of one core share its execution units.
- `Parallel.For (1)` is slightly faster than the sequential run, although it does the same work – the first measurement also includes JIT warm-up.
- `Parallel.For` is a single line of code and is clearly better than static strips. Here it is a bit slower than one task per row: the batches of consecutive rows it takes can be expensive, and at the end of the loop some cores wait for the last batches. With more work (higher resolution) the difference becomes smaller, and the overhead of `Parallel.For` per iteration is much lower than that of a task.

Measurements are noisy – run the program several times and close other applications. Your numbers will depend on the processor.

> [!TIP]
> If you need control over the size of the batches, `Parallel.ForEach` can iterate over ranges created by `Partitioner.Create(0, _imageHeight, rangeSize)` (namespace `System.Collections.Concurrent`) – each iteration receives a tuple `(start, end)`.

> [!NOTE]
> #### Stage 3
> {{< filetree dir="labs/w08.concurrency/Raytracer.03" >}}

### Stage 4: Cancellation and Progress Reporting

Rendering is now fast enough that we can go back to the quality from Tutorial 3: 1280x720 and 100 samples per pixel (about 20 s sequentially, 2–3 s in parallel). For longer renders, the user should be able to stop the rendering with Ctrl+C and still get the part of the image rendered so far. We will also decouple the camera from the console: the camera should only report progress, and the program should decide how to display it.

> [!IMPORTANT]
> #### Cooperative Cancellation
>
> In .NET, an operation cannot be stopped from the outside – it has to cooperate. The mechanism consists of two types (similar to `std::stop_source` and `std::stop_token` from C++20):
> - `CancellationTokenSource` – the object that requests the cancellation (`Cancel()`, or automatically after a time limit with `CancelAfter(TimeSpan)`).
> - `CancellationToken` – a struct passed to the operation, which checks it: `token.IsCancellationRequested` or `token.ThrowIfCancellationRequested()`, which throws `OperationCanceledException`.
>
> `Parallel.For` checks the token from `ParallelOptions.CancellationToken` before starting new iterations. After cancellation, it does not start new iterations, waits for the ones in progress, and throws `OperationCanceledException`. More details can be found in the lecture on [asynchronous programming]({{< ref "/lectures/09.concurrency/asynchronous-programming" >}}).

> [!IMPORTANT]
> #### `IProgress<T>` and `Progress<T>`
>
> `IProgress<T>` is a standard interface with a single method `void Report(T value)`. The code doing the work accepts an `IProgress<T>?` and reports progress, without knowing anything about how it is displayed.
>
> The standard implementation `Progress<T>` invokes a callback (passed to the constructor or attached to the `ProgressChanged` event) through the `SynchronizationContext` captured when it was created. In GUI applications (WPF, WinForms, Avalonia), it is exactly what you need: progress is reported from worker threads, but the callback runs on the UI thread, where controls can be updated. A console application has no synchronization context, so `Progress<T>` queues every callback to the thread pool – the callbacks run concurrently, in random order, and may even run after the rendering has finished. In a console application, it is therefore simpler to implement `IProgress<T>` directly – then `Report` runs synchronously on the reporting thread.

#### Progress through `IProgress<int>`

1. Make `ProgressBar` implement `IProgress<int>` – rename `Update` to `Report`. The lock is still needed, because `Report` is called by many threads.
2. In `Camera`, change the type of the last parameter of `RenderParallel` from `ProgressBar?` to `IProgress<int>? progress`.

#### Cancellable Rendering

To keep the partial image after cancellation, the image must be created by the caller:
1. Add an overload `public void RenderParallel(IHittable world, Image image, ParallelOptions options, IProgress<int>? progress = null)`, which renders into the given image. Move the `Parallel.For` loop into it.
2. The existing `RenderParallel(world, options, progress)` creates an image, calls the new overload, and returns the image.

In `Program.cs`:
1. Move the code measuring all variants from `Main` to the method `static async Task BenchmarkAsync(Scene world)`. It will be executed when the program is started with the `benchmark` argument: `dotnet run -c Release -- benchmark` (check it with the list pattern `args is ["benchmark"]`).
2. Otherwise, `Main` creates a `CancellationTokenSource` (in a `using` declaration) and handles Ctrl+C:
   ```csharp
   Console.CancelKeyPress += (sender, e) =>
   {
       e.Cancel = true; // do not terminate the process, stop the rendering gracefully
       cts.Cancel();
   };
   ```
   By default, Ctrl+C immediately terminates the process; setting `e.Cancel = true` keeps it running, so that it can finish in an orderly way.
3. Then `Main` calls the new method `static void RenderImage(Scene world, CancellationToken token)`, which:
   - creates a camera with `ImageWidth = 1280` and `SamplesPerPixel = 100`, and prints a message that Ctrl+C cancels the rendering,
   - creates an image of the size of the camera and `ParallelOptions` with the `CancellationToken` set,
   - calls `RenderParallel` with a `ProgressBar` in a `try` block; it prints the rendering time when it succeeds and catches `OperationCanceledException` otherwise,
   - in both cases saves the image to `image.ppm`.

Example output when the rendering completes:

```text
Rendering 1280x720, 100 samples per pixel. Press Ctrl+C to cancel.
[########################################] 100%
Rendered in 2.30 s
Saved image.ppm
```

And when Ctrl+C is pressed during rendering:

```text
Rendering 1280x720, 100 samples per pixel. Press Ctrl+C to cancel.
[##############################----------]  77%
Cancelled after 1.40 s, saving the partial image
Saved image.ppm
```

The partial image shows how `Parallel.For` distributed the rows: the cheap sky rows were finished first, and the rows below were taken by different threads in batches. The black rows have not been started yet.

> [!NOTE]
> #### Stage 4
> ![Stage 4 Render]({{< relurl "/labs/w08.concurrency/Raytracer.04/image.jpg" >}})
> {{< filetree dir="labs/w08.concurrency/Raytracer.04" >}}

### Stage 5: Asynchronous Animation Rendering

In the last stage, we will render a short animation: the camera orbits around the scene, and every frame is saved to a separate file. This time we will not parallelize inside a frame – frames are independent, so we will render many frames at the same time, each of them sequentially on a thread pool thread. Every frame should be saved as soon as it is ready, without waiting for the others, and saving should not block any thread.

> [!IMPORTANT]
> #### `Task.WhenEach` and `await foreach`
>
> `Task.WhenAll` waits for all tasks, `Task.WhenAny` for the first one. `Task.WhenEach` (.NET 9) returns an asynchronous sequence (`IAsyncEnumerable<Task<T>>`) that yields the tasks **in the order in which they complete**. Such a sequence is iterated with `await foreach` – between the elements, the loop waits asynchronously:
> ```csharp
> await foreach (Task<Frame> finished in Task.WhenEach(renderTasks))
> {
>     Frame frame = await finished; // already completed: gives the result or rethrows
>     // ...
> }
> ```
> More details can be found in the lecture on [asynchronous sequences]({{< ref "/lectures/09.concurrency/asynchronous-streams" >}}).

#### Preparing the Classes

1. In `Image`, add a method `public async Task SaveAsync(string path, CancellationToken token = default)`, which writes the image using `File.WriteAllTextAsync`.
2. In `Camera`, add an optional parameter `CancellationToken token = default` to the sequential `Render` method and call `token.ThrowIfCancellationRequested()` before rendering each row.
3. Create a file `Frame.cs` with a record `public record Frame(int Index, Image Image);` – a rendered frame together with its number.

#### Rendering Frames

In `Program.cs`, add a method `static Frame RenderFrame(Scene world, int index, int frameCount, CancellationToken token)`, which creates a camera for frame number `index` and renders the frame with `Render(world, token)`. Use `ImageWidth = 480`, `SamplesPerPixel = 32`, `Fov = 60`, the target `(0, 0, -1)`, and the camera position on a circle of radius 2.5 around the target, 0.5 above it:

```csharp
float angle = 2 * MathF.PI * index / frameCount;
Vector3 position = target + new Vector3(2.5f * MathF.Sin(angle), 0.5f, 2.5f * MathF.Cos(angle));
```

> [!WARNING]
> `Camera` is a mutable class – changing `Position` recalculates its internal fields, which are read during rendering. If all frames shared one camera and changed its position, they would interfere with each other (a race condition). Each frame must have its own `Camera` object. The scene is shared safely, because it is only read.

#### Saving Frames as Soon as They Are Ready

Add a method `static async Task RenderAnimationAsync(Scene world, CancellationToken token)`:
1. Create the `frames` directory (`Directory.CreateDirectory`) and start a `Stopwatch`.
2. For each of the `frameCount = 36` frames, start a task `Task.Run(() => RenderFrame(world, index, frameCount, token), token)` and store it in an array `Task<Frame>[]`. Passing the token to `Task.Run` causes tasks that have not started yet to be cancelled without running.
3. In a `try` block, iterate over `Task.WhenEach(renderTasks)` with `await foreach`. Save each frame with `await frame.Image.SaveAsync(path)` to `frames/frame_NNN.ppm` (use `Path.Combine` and the format `{frame.Index:D3}`) and print the elapsed time, the file name, and the number of frames saved so far.
4. Catch `OperationCanceledException` and print how many frames were saved.

In `Main`, call `await RenderAnimationAsync(world, cts.Token)` when the program is started with the `animation` argument:

```bash
dotnet run -c Release -- animation
```

Example output (shortened) – note that the frames are saved in the order in which they finish, not in the order of their numbers:

```text
Rendering 36 frames. Press Ctrl+C to cancel.
[ 1.62 s] saved frames/frame_009.ppm (1/36)
[ 1.70 s] saved frames/frame_007.ppm (2/36)
[ 1.72 s] saved frames/frame_011.ppm (3/36)
...
[ 4.12 s] saved frames/frame_035.ppm (35/36)
[ 4.15 s] saved frames/frame_034.ppm (36/36)
```

> [!NOTE]
> We do not pass the token to `SaveAsync` on purpose: after Ctrl+C, the file currently being written is finished, so no truncated file is left on the disk. Rendering stops quickly anyway, because `Render` checks the token before every row.

> [!IMPORTANT]
> #### `SynchronizationContext` and `ConfigureAwait`
>
> After `await`, the method continues in the captured `SynchronizationContext`, if there is one. In GUI applications, it is the UI thread. In a console application, `SynchronizationContext.Current` is `null`, so continuations run on any thread pool thread:
> ```csharp
> Console.WriteLine($"SynchronizationContext: {SynchronizationContext.Current?.ToString() ?? "none"}");
> Console.WriteLine($"Before await: thread {Environment.CurrentManagedThreadId}");
> await Task.Delay(100);
> Console.WriteLine($"After await:  thread {Environment.CurrentManagedThreadId}");
> ```
> ```text
> SynchronizationContext: none
> Before await: thread 1
> After await:  thread 5
> ```
> `await task.ConfigureAwait(false)` tells the continuation not to return to the captured context. In a console application, there is nothing to return to, so `ConfigureAwait(false)` changes nothing. It matters in library code that may be called from GUI applications – there it avoids unnecessarily returning to the UI thread (and deadlocks when the caller blocks the UI thread with `.Result`). See the lecture on [synchronization context]({{< ref "/lectures/09.concurrency/synchronization-context" >}}).

> [!TIP]
> The frames can be combined into an animation, e.g. with *ffmpeg* (`ffmpeg -framerate 12 -i frames/frame_%03d.ppm orbit.gif`) or *ImageMagick* (`magick -delay 8 frames/*.ppm orbit.gif`).

> [!NOTE]
> #### Stage 5
> ![Stage 5 Frames]({{< relurl "/labs/w08.concurrency/Raytracer.05/image.jpg" >}})
> {{< filetree dir="labs/w08.concurrency/Raytracer.05" >}}

### Next Steps and Further Enhancements

The ray tracer now uses all cores and stays responsive. Below are ideas for further development:

- **Producer–consumer pipeline with `Channel<T>`:** In stage 5, all render tasks are started at once, and finished frames wait in memory until they are saved. `System.Threading.Channels` provides asynchronous queues: render tasks write frames to a bounded channel (`Channel.CreateBounded<Frame>(capacity)`), and one writer task reads them with `await foreach (Frame frame in channel.Reader.ReadAllAsync())`. When the channel is full, producers wait (*backpressure*), which limits memory usage. Compare with `BlockingCollection<T>` from the lecture on [concurrent collections]({{< ref "/lectures/12.synchronization/concurrent-collections" >}}).
- **`Parallel.ForEachAsync`:** Runs an asynchronous body for every element with a limited degree of parallelism – e.g. render and save frames with at most N frames in progress at a time.
- **Tiles and progressive rendering:** Render square tiles instead of rows (better cache locality), or render the whole image with 1 sample per pixel first and refine it in subsequent passes, so that a preview is available almost immediately.
- **Per-thread state:** An overload of `Parallel.For` with `localInit` and `localFinally` allows accumulating statistics (e.g. the number of traced rays) in a local variable of each worker and merging them only once at the end, instead of calling `Interlocked` for every ray.
- **SIMD:** Parallelism inside a single core – the `System.Numerics.Vector3` type uses vector instructions of the processor. Replacing our own `Vector3` with it can speed up even the sequential version.
- **Reliable benchmarks:** The *BenchmarkDotNet* library performs warm-up, repeats measurements, and computes statistics, which makes comparisons much more reliable than a single `Stopwatch` measurement.
- **GUI with a live preview:** In a desktop application (e.g. Avalonia), `Progress<T>` created on the UI thread can deliver finished rows to the UI thread and display the image while it is being rendered.

## Example Tasks

Complete an example task from previous years. If you are able to finish it within 90 minutes, you are well-prepared for class.

- [Task 1]({{< ref "/labs/08.concurrency/example1" >}})
- [Task 2]({{< ref "/labs/08.concurrency/example2" >}})
