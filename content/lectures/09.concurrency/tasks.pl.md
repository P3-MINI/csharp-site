---
title: "Zadania"
weight: 40
---

# Zadania

Klasa `Task` reprezentuje operację asynchroniczną, która zostanie ukończona w przyszłości. Jest to fundamentalny element biblioteki TPL (*Task Parallel Library*) stanowiący podstawę nowoczesnego programowania asynchronicznego.

Klasa `Task` dostarcza dwa rodzaje zadań:

* `Task` - zadanie bez wartości zwracanej
* `Task<TResult>` - generyczna wersja, gdzie `TResult` jest typem wartości zwracanej

> [!WARNING]
> Zadania używają domyślnie wątków z puli, które są wątkami drugoplanowymi, co oznacza, że kończą się, gdy skończą się wszystkie wątki pierwszoplanowe.

## Tworzenie zadań

Metoda `Task.Run(Action action)` tworzy zadanie i je uruchamia. Alternatywnie można użyć konstruktora `new Task...`, ale takie zadania nie są automatycznie uruchamiane - trzeba na nich wywołać potem metodę `Start()`. Nowe zadania są uruchamiane na dowolnym wątku z puli wątków. 

```csharp
Task task = Task.Run(() =>
{
    Console.WriteLine(Thread.CurrentThread.Name);
    Console.WriteLine("Hello from the thread pool");
});
```

Metoda `Wait` pozwala blokujaco zaczekać, aż zadanie się nie zakończy.

```csharp
Console.WriteLine($"Is completed: {task.IsCompleted}");
task.Wait(); // blocks until task finishes
Console.WriteLine($"Is completed: {task.IsCompleted}");
```

## Zwracanie wartości z zadania

Metoda `Task<TResult>.Run(Func<TResult> func)` pozwala tworzyć zadania zwracające wartość. Po wykonaniu zadania możemy pobrać z takiego zadania wynik. Właściwość `Result` blokująco czeka na zakończenie zadania, i gdy zadanie jest zakończone, to pobiera z niego wartość.

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

## Wyjątki w zadaniach

W odróżnieniu od wątków nieobsłużone wyjątki nie powodują natychmiastowego zamknięcia aplikacji. Wyjątki rzucane są przechwytywane i przerzucane w momencie czekania na zadanie lub pobrania z niego wartości. Złapane wyjątki są opakowywane w `AggregateException`.

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

## Kontynuacje

Zamiast blokować wątek za pomocą `Wait()` lub `.Result`, do zadania można podpiąć kolejną operację. Służy do tego metoda `task.ContinueWith(...)`. Pozwala to tworzyć łańcuchy operacji asynchronicznych bez blokowania wątków.

```csharp
Task<int> t = Task.Run(() => CountPrimes(1000000, 1000000));

Task continuation = t.ContinueWith((Task<int> task) =>
{
    Console.WriteLine($"Primes: {task.Result}");
});

continuation.Wait();
```

## Długie zadania

Żeby nie blokować wątku z puli na zbyt długo, zadania powinny trwać maksymalnie kilkaset milisekund. Jeżeli potrzebujemy stworzyć zadanie, które będzie trwało dłużej można to zrobić za pomocą metody `Task.Factory.StartNew` z opcją `TaskCreationOptions.LongRunning`. Dla takiego zadania stworzony zostanie dedykowany wątek.

```csharp
Task<int> t = Task.Factory.StartNew(() =>
{
    Console.WriteLine($"{Thread.CurrentThread.Name}");
    return CountPrimes(2, 1_000_000_000);
}, TaskCreationOptions.LongRunning);

t.Wait();
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/concurrency/Tasks" >}}
