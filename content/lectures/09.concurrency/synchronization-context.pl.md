---
title: "Kontekst synchronizacji"
weight: 30
---

# Kontekst synchronizacji

`SynchronizationContext` reprezentuje "miejsce", w którym może być wykonany kod. Tym "miejscem" zazwyczaj jest wątek lub pula wątków. W ogólności może to być cokolwiek: konkretny rdzeń CPU, albo inny komputer w sieci.

Za pomocą metod `SynchronizationContext.Send` i `SynchronizationContext.Post` można wywołać kod w tym konkretnym "miejscu" (`Post` to nieblokujący odpowiednik `Send`).

Każdy wątek może mieć powiązaną z nim instancję `SynchronizationContext`. Uruchomiony wątek może zostać skojarzony z kontekstem synchronizacji poprzez wywołanie statycznej metody `SynchronizationContext.SetSynchronizationContext`, a bieżący kontekst uruchomionego wątku można odpytać za pomocą właściwości `SynchronizationContext.Current`.

Konteksty synchronizacji są ważne w programowaniu aplikacji okienkowych. Aplikacje okienkowe ustawiają kontekst synchronizacji na tak zwany "wątek UI". Ten typ kontekstu synchronizacji wywołuje przekazane do niego delegaty dokładnie na tym wątku. Jest to bardzo przydatne, ponieważ aplikacje okienkowe zezwalają na manipulowanie kontrolkami tylko na tym samym wątku, na którym zostały utworzone. Dzięki kontekstom synchronizacji możemy wykonać kod z dowolnego wątku i bezpiecznie wrócić z wykonaniem na wątek UI.

Typowa implementacja kontekstu synchronizacji, gdzie "miejscem" jest pojedynczy wątek wygląda następująco:

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

Taki kontekst synchronizacji pozwala na wykonaniu kodu na tym wątku z dowolnego miejsca:

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
> Kod źródłowy:
> {{< filetree dir="lectures/concurrency/SynchronizationContext" >}}
