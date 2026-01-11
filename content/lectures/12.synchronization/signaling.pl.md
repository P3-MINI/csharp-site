---
title: "Sygnalizacja"
weight: 40
---

# Sygnalizacja (*Signaling*)

Mechanizmy sygnalizacji służą do komunikacji między wątkami. Pozwalają jednemu wątkowi wstrzymać działanie do momentu otrzymania powiadomienia (sygnału) od innego wątku.

## EventWaitHandle i CountdownEvent

Klasy pochodne od `EventWaitHandle` (`ManualResetEvent`, `AutoResetEvent`) działają jak bramki, które mogą być otwarte (sygnalizowane) lub zamknięte (niesygnalizowane). `CountdownEvent` działa na innej zasadzie (odwrócony licznik) i nie dziedziczy po `EventWaitHandle`.

*   **`Set()`**: Otwiera bramkę (ustawia stan na zasygnalizowany).
*   **`Reset()`**: Zamyka bramkę (ustawia stan na niezasygnalizowany).
*   **`WaitOne()`**: Czeka przy bramce. Jeśli jest otwarta, przechodzi natychmiast. Jeśli zamknięta, blokuje wątek do momentu otwarcia.

> [!NOTE]
> `CountdownEvent` używa metod `Signal()` (dekrementacja) i `Wait()` (oczekiwanie na zero).

### Rodzaje zdarzeń

1.  **`ManualResetEvent(Slim)`**: Po otwarciu (`Set`) pozostaje otwarta dla dowolnej liczby wątków, dopóki nie zostanie ręcznie zamknięta (`Reset`). Działa jak klasyczna brama.
2.  **`AutoResetEvent`**: Po otwarciu przepuszcza **tylko jeden** czekający wątek i natychmiast automatycznie się zamyka. Działa jak bramka w metrze.
3.  **`CountdownEvent`**: Zostaje zasygnalizowany dopiero, gdy jego wewnętrzny licznik spadnie do zera. Każde wywołanie `Signal()` zmniejsza ten licznik. Jest przydatny w sytuacjach, gdy jeden wątek musi czekać na zakończenie pracy przez określoną liczbę innych wątków.

### Przykład: Kolejka Producent-Konsument

Poniższy przykład implementuje kolejkę o ograniczonej pojemności, używając `ManualResetEvent` do sygnalizowania, czy kolejka jest pełna, czy pusta.

> [!NOTE]
> Zauważ pętlę `while(true)` w metodach `Enqueue` i `Dequeue`. Jest ona konieczna, ponieważ pomiędzy wywołaniem `WaitOne()` (sygnał: jest miejsce) a wejściem w `lock` inny wątek mógł zająć to miejsce. Wymaga to ponownego sprawdzenia warunku.

```csharp
public class Queue<T> : IDisposable
{
    private readonly T?[] _array;
    private int _head;
    private int _tail;
    private int _count;

    private readonly ManualResetEvent _notEmpty;
    private readonly ManualResetEvent _notFull;
    private readonly Lock _lock;

    public Queue(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _array = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;

        _notEmpty = new ManualResetEvent(false);
        _notFull = new ManualResetEvent(true);
        _lock = new Lock();
    }

    public void Enqueue(T item)
    {
        while (true)
        {
            _notFull.WaitOne();

            lock (_lock)
            {
                if (_count < _array.Length)
                {
                    _array[_tail] = item;
                    _tail = (_tail + 1) % _array.Length;
                    _count++;

                    if (_count == _array.Length)
                    {
                        _notFull.Reset();
                    }

                    _notEmpty.Set();
                    return;
                }
            }
        }
    }

    public T Dequeue()
    {
        while (true)
        {
            _notEmpty.WaitOne();

            lock (_lock)
            {
                if (_count > 0)
                {
                    T item = _array[_head]!;
                    _array[_head] = default;
                    _head = (_head + 1) % _array.Length;
                    _count--;

                    if (_count == 0)
                    {
                        _notEmpty.Reset();
                    }

                    _notFull.Set();
                    return item;
                }
            }
        }
    }

    public void Dispose()
    {
        _notEmpty.Dispose();
        _notFull.Dispose();
    }
}
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/EventWaitHandlesExample" >}}

## Barrier

Klasa `Barrier` służy do synchronizacji grupy wątków, które muszą pracować w fazach. Wątki dochodzą do bariery (`SignalAndWait`) i czekają, aż wszystkie pozostałe z grupy również tam dotrą. Dopiero gdy komplet wątków się zamelduje, wszystkie są zwalniane do kolejnej fazy.

### Przykład: Gra w kości

W tym przykładzie 3 wątki rzucają kośćmi. Bariera zapewnia, że wszystkie wątki wykonają rzut w tej samej rundzie, zanim którakolwiek przejdzie do następnej, a wyniki zostaną wypisane w jednej linii.

```csharp
class Program
{
    static void Main(string[] args)
    {
        var barrier = new Barrier(3, _ => Console.WriteLine());
        new Thread(RollDice).Start();
        new Thread(RollDice).Start();
        new Thread(RollDice).Start();
        void RollDice()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.Write($"{D6()} ");
                barrier.SignalAndWait();
            }
        }
    }

    static int D6() => 1 + Random.Shared.Next(6);
}
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/BarrierExample" >}}

## Monitor.Wait / Monitor.Pulse

Jest to mechanizm zmiennych warunkowych (powiązany z `lock`).

*   `Monitor.Wait(obj)`: Zwalnia blokadę na obiekcie `obj` i usypia wątek do momentu otrzymania powiadomienia.
*   `Monitor.Pulse(obj)`: Budzi jeden wątek czekający na obiekcie `obj`.
*   `Monitor.PulseAll(obj)`: Budzi wszystkie czekające wątki.

Jest to mechanizm bardziej niskopoziomowy niż `EventWaitHandle` i wymaga przebywania wewnątrz sekcji `lock`. Zazwyczaj użycie `EventWaitHandle` jest wystarczające i prostsze.
