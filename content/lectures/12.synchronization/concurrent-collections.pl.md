---
title: "Kolekcje współbieżne"
weight: 50
---

# Kolekcje współbieżne

Przestrzeń nazw `System.Collections.Concurrent` zawiera kolekcje zoptymalizowane pod kątem dostępu wielowątkowego. W przeciwieństwie do standardowych kolekcji (np. `List<T>`, `Dictionary<TKey, TValue>`), które wymagają ręcznego blokowania (np. przez `lock`) przy każdym dostępie, kolekcje współbieżne radzą sobie z synchronizacją wewnętrznie, często używając technik *lock-free* lub precyzyjnego blokowania (*fine-grained locking*).

> [!WARNING]
> Standardowe kolekcje są wydajniejsze od kolekcji współbieżnych w sytuacjach, gdzie nie ma współbieżności.

## Kolekcje

*   **`ConcurrentDictionary<TKey, TValue>`**: Bezpieczny wątkowo słownik. Pozwala na jednoczesny odczyt i zapis przez wiele wątków bez blokowania całego słownika.
    *   Metody: `TryAdd`, `TryUpdate`, `GetOrAdd`, `AddOrUpdate`.
*   **`ConcurrentQueue<T>`**: Bezpieczna kolejka FIFO.
*   **`ConcurrentStack<T>`**: Bezpieczny stos LIFO.
*   **`ConcurrentBag<T>`**: Nieuporządkowany multizbiór elementów.

> [!INFO]
> Iteracja po współbieżnej kolekcji podczas równoczesnej modyfikacji przez inny wątek, może zawierać zarówno już usunięte, jak i nowo dodane wartości, ale nie skończy się rzuceniem wyjątku.

> [!INFO]
> `ConcurrentQueue`, `ConcurrentStack`, `ConcurrentBag` używają wewnętrznie list dwukierunkowych. W `ConcurrentBag<T>`, każdy wątek otrzymuje swoją własną prywatną listę, dzięki czemu pobieranie elementu jest bardzo szybkie i bez wyścigów, pod warunkiem, że każdy wątek nie pobiera więcej elementów niż włożył.

## `IProducerConsumerCollection`

`IProducerConsumerCollection<T>` to interfejs w przestrzeni nazw `System.Collections.Concurrent`, który definiuje kontrakt dla kolekcji bezpiecznych wątkowo, przeznaczonych do scenariuszy typu **Producent-Konsument**.

```csharp
public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection
{
    bool TryAdd(T item);
    bool TryTake(out T item);
    T[] ToArray();
}
```

Jest on implementowany przez `ConcurrentQueue<T>`, `ConcurrentStack<T>` oraz `ConcurrentBag<T>`.

## BlockingCollection

`BlockingCollection<T>` to specjalna klasa, która implementuje klasyczny wzorzec **Producent-Konsument**. Opakowuje ona inną kolekcję współbieżną implementującą `IProducerConsumerCollection<T>` (domyślnie `ConcurrentQueue<T>`) i dodaje mechanizmy blokowania:

*   **Producent**: Jeśli kolekcja ma ograniczoną pojemność, metoda `Add` zablokuje wątek producenta, jeśli kolekcja jest pełna.
*   **Konsument**: Metoda `Take` zablokuje wątek konsumenta, jeśli kolekcja jest pusta, czekając na pojawienie się elementu.

### Przykład

W poniższym przykładzie producent dodaje liczby do kolekcji, a konsument je pobiera. Dzięki `GetConsumingEnumerable()`, konsument może łatwo iterować po elementach tak długo, jak producent nie zgłosi zakończenia (`CompleteAdding`).

```csharp
var blockingCollection = new BlockingCollection<int>(boundedCapacity: 5);

Task producer = Task.Run(() =>
{
    for (int i = 0; i < 10; i++)
    {
        Console.WriteLine($"Producing: {i}");
        blockingCollection.Add(i); // Can block, if full
        Thread.Sleep(500);
    }
    blockingCollection.CompleteAdding(); // Signal end of data
});

Task consumer = Task.Run(() =>
{
    foreach (var item in blockingCollection.GetConsumingEnumerable())
    {
        Console.WriteLine($"Consuming: {item}");
        Thread.Sleep(1000);
    }
});

Task.WaitAll(producer, consumer);
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/BlockingCollectionExample" >}}
