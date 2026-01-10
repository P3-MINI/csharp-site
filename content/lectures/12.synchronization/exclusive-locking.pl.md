---
title: "Blokowanie wyłączne"
weight: 20
---

# Blokowanie wyłączne (*Exclusive Locking*)

Blokowanie wyłączne to mechanizm synchronizacji gwarantujący, że w danej chwili tylko jeden wątek ma dostęp do chronionego zasobu (sekcji krytycznej). Inne wątki próbujące uzyskać dostęp są wstrzymywane do momentu zwolnienia blokady.

## Instrukcja `lock`

Słowo kluczowe `lock` to najprostszy i najczęściej stosowany sposób synchronizacji w C#. Jest to cukierek składniowy na klasę `System.Threading.Monitor`. Obiektem blokady może być dowolny typ referencyjny.

```csharp
lock(lockObject)
{
    statement(s);
}
```

Instrukcja `lock` jest tłumaczona następująco:

```csharp
Monitor.Enter(lockObject);
try
{
    statement(s);
}
finally { Monitor.Exit(lockObject); }
```

Jest to bardzo szybki mechanizm, jeśli nie dochodzi do rywalizacji. W przypadku rywalizacji wątek jest blokowany przez system operacyjny. Instrukcja `lock` działa tylko w obrębie jednego procesu. Ponadto tak jak było wspomniane na wstępie przed i po bloku instrukcji `lock` wstawiana jest bariera pamięci.

```csharp
int counter = 0, times = 1_000_000;
object locker = new object();

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        lock(locker) counter++;
});

var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        lock(locker) counter--;
});

await Task.WhenAll(increment, decrement);
Console.WriteLine($"Counter Value: {counter}");
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/LocksDemo" >}}

> [!NOTE] 
> Od C# 13 zaleca się używać obiektu typu `Lock` w instrukcji `lock`, lub bezpośrednio typem `Lock`.
> Jeżeli obiektem blokady jest obiekt typu `Lock`, to Instrukcja `lock` tłumaczy się następująco:
> ```csharp
> Lock.Scope scope = lockObject.EnterScope();
> try
> {
>     statement(s);
> }
> finally { scope.Dispose(); }
> ```
> Obiektu `Lock` można używać też bezpośrednio:
> ```csharp
> int counter = 0, times = 1_000_000;
> var locker = new Lock();
> 
> var increment = Task.Run(() =>
> {
>     for (int i = 0; i < times; i++)
>         using(locker.EnterScope()) counter++;
> });
> 
> var decrement = Task.Run(() =>
> {
>     for (int i = 0; i < times; i++)
>         using(locker.EnterScope()) counter--;
> });
> 
> await Task.WhenAll(increment, decrement);
> Console.WriteLine($"Counter Value: {counter}");
> ```

## Mutex

Klasa `System.Threading.Mutex` to prymityw synchronizacji działający na poziomie jądra systemu operacyjnego.

Może być nazwany, co pozwala na synchronizację wątków pomiędzy różnymi procesami (np. zapobieganie uruchomieniu drugiej instancji aplikacji). Jest znacznie wolniejszy od instrukcji `lock` (nawet 50x wolniejszy) z powodu konieczności przełączania kontekstu do jądra.

```csharp
int counter = 0, times = 1_000_000;
using var mutex = new Mutex();

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
    {
        mutex.WaitOne();
        try
        {
            counter++;
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
});
var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
    {
        mutex.WaitOne();
        try
        {
            counter--;
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
});

await Task.WhenAll(increment, decrement);
Console.WriteLine($"Counter Value: {counter}");
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/MutexDemo" >}}

## SpinLock

`System.Threading.SpinLock` to lekka struktura, która realizuje blokowanie poprzez aktywne oczekiwanie (*busy waiting*). Zamiast blokować wątek (co kosztuje czas na przełączenie kontekstu), wątek aktywnie czeka, sprawdzając dostępność blokady.

`SpinLock` jest idealny dla bardzo krótkich sekcji krytycznych, gdzie czas oczekiwania jest krótszy niż czas przełączenia kontekstu wątku. **Aktywne oczekiwanie zużywa 100% czasu procesora na danym rdzeniu podczas czekania.**

> [!WARNING]
> Niewłaściwe użycie `SpinLock` (np. w długich sekcjach krytycznych) może drastycznie obniżyć wydajność programu.

```csharp
int counter = 0, times = 1_000_000;
SpinLock spinLock = new SpinLock();

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
    {
        bool lockTaken = false;
        try
        {
            spinLock.Enter(ref lockTaken);
            counter++;
        }
        finally
        {
            if (lockTaken) spinLock.Exit();
        }
    }
});

var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
    {
        bool lockTaken = false;
        try
        {
            spinLock.Enter(ref lockTaken);
            counter--;
        }
        finally
        {
            if (lockTaken) spinLock.Exit();
        }
    }
});

await Task.WhenAll(increment, decrement);
Console.WriteLine($"Counter Value: {counter}");
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/SpinLockDemo" >}}
