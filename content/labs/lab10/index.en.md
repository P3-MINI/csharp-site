---
title: "Lab10"
weight: 10
---

# Lab 10: Parallel and Asynchronous Programming

## Starter Code

> [!NOTE] 
> **Student** 
> {{< filetree dir="labs/lab10/student" >}}

## Generating Fractals

{{% hint info %}}
**What is parallel programming?**

Parallel programming is a programming paradigm that utilizes the architecture of modern, multi-core processors (CPUs) to perform multiple computations simultaneously.

Instead of processing data sequentially in a single thread, we divide the work among multiple threads that execute their tasks at the same time.

{{% /hint %}}

### Task Description

The goal of the task is to implement and compare the execution time of different methods for parallelizing the computations performed while generating the [Mandelbrot Set](https://en.wikipedia.org/wiki/Mandelbrot_set).

The starter code contains an abstract class `MandelbrotSetGenerator` that manages the entire process of generating the fractal and saving it to a `.png` file.

The `SingleThreadGenerator` class is a concrete single-threaded implementation of the generator. It uses a simple, nested `for` loop to iterate over all pixels of the generated image. It will serve as a baseline for performance measurement.

You must implement the following computation parallelization methods:

- `MultiThreadGenerator`: A multi-threaded method that manually creates and manages `Thread` objects.
- `TasksGenerator`: A method that parallelizes the work using the thread pool (`ThreadPool`) via `Task` objects.
- `ParallelGenerator`: A method that uses the TPL (Task Parallel Library).

`Program.cs` contains the logic for measuring time and running each generator sequentially.

A correctly generated fractal should look as follows:

<div style="text-align: center;">
  <img src="/labs/lab10/mandelbrotset.png" width="400px" alt="mandelbrotset.png" />
</div>

{{% hint warning %}}
**Implementation Notes**

- **Number of threads:**
  - In the `MultiThreadGenerator` and `TasksGenerator` implementations, you should create `N` units of work (threads/tasks), where `N` is equal to the number of processor cores. This is the optimal number for 100% CPU-bound tasks.
- **Work division:**
  - When generating the fractal, the simplest strategy is to divide the image into `N` equal, horizontal strips.
  - Calculate how many rows fall to one thread, and then in a loop, pass the appropriate range to each thread/task for processing.

{{% /hint %}}

{{% hint info %}}
**Helpful Materials:**

- [Microsoft Learn: Threads and threading](https://learn.microsoft.com/en-us/dotnet/standard/threading/threads-and-threading)
- [Microsoft Learn: Task Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-9.0)
- [Microsoft Learn: Write a Simple Parallel.For Loop](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-simple-parallel-for-loop)

{{% /hint %}}

### Example Solution

> [!TIP] 
> **Solution** 
> {{< filetree dir="labs/lab10/solution/FractalsGenerator" >}}

## Offer Aggregator

{{% hint info %}}
**What is asynchronous programming?**

Asynchronous programming is used to avoid blocking the execution thread while waiting for operations that are outside our control.
We use it for tasks such as waiting for a response from a network (API), sending queries to a database, or reading the contents of a large file from disk.

In this case, the goal is not faster computation, but better management of waiting time and application responsiveness.

{{% /hint %}}

### Task Description

The goal of the task is to build an asynchronous console client (in the `FlightScanner.Client` project) that will aggregate flight offers from multiple services simultaneously. The client will communicate with a locally running REST API (the `FlightScanner.API` project).

The API, running at `http://localhost:5222` (the address can be found in the [launchSettings.json](/labs/lab10/solution/FlightScanner.API/Properties/launchSettings.json) file), is fully implemented and does not require modification. It must be run before running the console application.

The client application should:

- Query the "central" API endpoint (`/api/providers`) once to get the list of available airlines.
- For each airline from the retrieved list, the application must simultaneously (concurrently) send a request to its dedicated endpoint (e.g., `/api/flights/reliable-air`) to fetch specific flight offers.
- All fetched offers from different airlines must be collected, filtered for errors, and aggregated into a single, "flat" list.
- Finally, the application must display the 10 cheapest offers found.

#### Data Models

The API operates on the following DTOs (Data Transfer Objects). They are located in the `FlightScanner.Common` shared class library.

```csharp
// Received from /api/providers
public record PartnerAirlineDto(
    string Id,
    string Name,
    string Endpoint
);

// Received from airline providers
public record ProviderResponseDto(
    string ProviderName,
    List<FlightOfferDto> Flights
);

// Part of ProviderResponseDto
public record FlightOfferDto(
    string FlightId,
    string Origin,
    string Destination,
    decimal Price
);
```

{{% hint warning %}}

{{% hint warning %}}

**Implementation Notes**

- **Concurrency:**
  - Querying all airlines must happen at the same time, not sequentially.
- **Global timeout:**
  - The entire offer collection operation must have a global time limit (e.g., 3 seconds), imposed by a `CancellationTokenSource`.
  - If any provider does not respond within this time, its offer is skipped.
- **Progress Reporting:**
  - The application must report progress in real-time using the `IProgress<T>` interface.
- **Fault Tolerance:**
  - If a provider's endpoint returns an HTTP error (e.g., `500`, `404`) or exceeds the time limit, the application must not stop.
  - It should ignore that provider and continue working with the others.

{{% /hint %}}

{{% hint info %}}
**Helpful Materials:**

- [Microsoft Learn: Cancellation in Managed Threads](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Microsoft Learn: Make HTTP requests in a .NET](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient)
- [Microsoft Learn: IProgress<T> Interface](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1?view=net-9.0)
- [Microsoft Learn: Task.WhenAll Method](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall?view=net-9.0)
- [Microsoft Learn: Task Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-9.0)

{{% /hint %}}

### Example Solution

> [!TIP] 
> **Solution** 
> {{< filetree dir="labs/lab10/solution/FlightScanner.Client" >}}
>
> **Output:** 
> [FlightScanner.Client.txt](/labs/lab10/outputs/FlightScanner.Client.txt) 
> [FlightScanner.API.txt](/labs/lab10/outputs/FlightScanner.API.txt)
