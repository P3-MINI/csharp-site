---
title: "Wstęp"
weight: 10
---

# Wstęp

Współbieżny dostęp do zasobów wymaga stosowania mechanizmów synchronizacji, aby uniknąć wyścigów (*race conditions*) i zapewnić spójność danych. Platforma .NET udostępnia zestaw prymitywów synchronizacji, które różnią się przeznaczeniem, zasięgiem (wewnątrzprocesowe lub międzyprocesowe) oraz kosztem wydajnościowym.

Dostępne mechanizmy można podzielić na cztery główne kategorie:

-   **Blokowanie wyłączne** (*Exclusive locking*) - gwarantuje, że tylko jeden wątek ma dostęp do zasobu w danym momencie.
    - instrukcja `lock`
    - `Mutex`
    - `SpinLock`
-   **Blokowanie współdzielone** (*Non-exclusive locking*) - ogranicza równoległość
    - `Semaphore(Slim)`
    - `ReaderWriterLock(Slim)`
-   **Sygnalizacja** (*Signaling*) - pozwala na komunikację między wątkami i oczekiwanie na określone zdarzenie.
    - `(ManualReset|AutoReset|Countdown)Event(Slim)`
    - `Monitor.Wait`, `Monitor.Pulse` (zmienne warunkowe)
    - `Barrier`
-   **Inne niskopoziomowe** - nie wymagają blokowania wątków.
    - `MemoryBarrier`
    - `Interlocked`

## Problem

W poniższym przykładzie zmienna `complete` nie jest zabezpieczona. Kompilator (szczególnie w trybie `Release`) może zoptymalizować pętlę `while` w taki sposób, że wartość `complete` zostanie pobrana do rejestru procesora tylko raz. Mimo że główny wątek zmienia wartość na `true`, wątek poboczny może tego nigdy nie zauważyć, powodując zawieszenie programu.

```csharp
class Program
{
    private static void Main()
    {
        bool complete = false;
        var thread = new Thread (() =>
        {
            bool toggle = false;
            while (!complete) 
            {
                toggle = !toggle;
            }
        });
        thread.Start();
        Thread.Sleep(1000);
        complete = true;
        thread.Join();
    }
}
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/MemoryBarriers" >}}

## Bariera pamięci (*Memory Barrier/Fence*)

Bariera pamięci to instrukcja procesora lub kompilatora, która wymusza określony porządek wykonywania operacji na pamięci (odczytów i zapisów). Jest ona ważna z dwóch powodów:

1.  **Zapobiega zmianie kolejności instrukcji**:  
    Zarówno kompilatory, jak i nowoczesne procesory optymalizują kod, zmieniając kolejność wykonywania instrukcji, jeśli uznają, że nie wpłynie to na wynik w ramach pojedynczego wątku. W środowisku wielowątkowym taka zmiana może jednak prowadzić do błędów, gdzie jeden wątek widzi zmiany w niewłaściwej kolejności. Bariera "powstrzymuje" te optymalizacje, gwarantując, że instrukcje przed barierą wykonają się przed instrukcjami za nią.
2.  **Gwarantuje widoczność**:  
    W systemach wielordzeniowych każdy rdzeń ma własną pamięć podręczną (cache). Zapis do zmiennej przez jeden wątek może trafić tylko do cache'u lokalnego i nie być od razu widoczny dla innych rdzeni (i wątków). Bariera pamięci wymusza synchronizację cache'u z pamięcią główną (RAM), dzięki czemu inne wątki widzą najnowsze wartości.

W C# barierę można wywołać ręcznie, korzystając z metody `Thread.MemoryBarrier`. Rzadko jednak używa się ich bezpośrednio. Dzieje się ona niejawnie przy:
1. Instrukcji `lock` (przed i po wyjściu)
2. Metodach z prymitywów synchronizacji (np. `Mutex`, `Semaphore`, `AutoResetEvent`)
3. Odczycie/zapisie wartości `volatile`
4. Metodach z klasy `Interlocked`
5. Uruchomieniu, czekaniu na wątek/`Task`

## Klasa `Interlocked`

Klasa `Interlocked` służy do wykonywania atomowych operacji na zmiennych współdzielonych przez wiele wątków. Jest to najszybszy i najlżejszy sposób synchronizacji prostych danych liczbowych lub referencji, bez konieczności używania cięższych prymitywów synchronizacji typu `lock`, `Mutex`.

> [!NOTE]
> **Co to znaczy "atomowa operacja"?**
> Operacja atomowa to taka, która jest niepodzielna. Z punktu widzenia innych wątków wykonuje się ona w całości albo wcale - nie można jej wykonać w połowie.

Na przykład atomowe nie są:
- inkrementacja, dekrementacja
- dodawanie 64-bitowych liczb na 32-bitowym procesorze
- swap

### Przykład

Operacje `counter++` oraz `counter--` nie są atomowe. Składają się z trzech etapów: odczytu, modyfikacji i zapisu. Gdy dwa zadania wykonują te operacje jednocześnie na tej samej zmiennej, dochodzi do nakładania się tych etapów. Wartość końcowa `counter` będzie nieprzewidywalna i niemal na pewno różna od zera.

```csharp
int counter = 0, times = 1_000_000;

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        counter++;
});

var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        counter--;
});

await Task.WhenAll(increment, decrement);

Console.WriteLine($"Counter: {counter}");
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/InterlockedDemo" >}}

Metody `Interlocked.Increment` i `Interlocked.Decrement` wymuszają na procesorze wykonanie całej operacji modyfikacji zmiennej jako jednej, niepodzielnej jednostki (używając sprzętowych instrukcji). Dodatkowo generują one pełną barierę pamięci, co gwarantuje poprawną widoczność zmian dla wszystkich rdzeni procesora. Kod jest bezpieczny wątkowo, a wartość końcowa `counter` zawsze będzie wynosić dokładnie `0`.

```csharp
int counter = 0, times = 1_000_000;

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        Interlocked.Increment(ref counter);
});

var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        Interlocked.Decrement(ref counter);
});

await Task.WhenAll(increment, decrement);

Console.WriteLine($"Counter: {counter}");
```
