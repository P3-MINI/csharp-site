---
title: "Wątki"
weight: 10
---

# Wątki

## Wprowadzenie

**Wątek** (ang. *thread*) to podstawowa jednostka wykonania kodu, w obrębie jednego procesu. Wątki w ramach tego samego procesu współdzielą jego pamięć (dane na stercie) i zasoby systemu operacyjnego (uchwyty do plików, połączenia sieciowe, i inne). Jednakże, każdy wątek posiada swój własny:

*   **Stos wywołań** (*call stack*): Każdy wątek ma oddzielny stos, na którym przechowywane są jego zmienne lokalne, parametry metod i adresy powrotu. Zmienna zadeklarowana wewnątrz metody jest w pełni prywatna dla wątku wykonującego tę metodę.
*   **Kontekst procesora**: Każdy wątek ma swój unikalny stan rejestrów CPU, w tym wskaźnik instrukcji (*instruction pointer*), który jest zapisywany i odtwarzany podczas przełączania kontekstu.

> [!NOTE]
> Stworzenie nowego wątku jest lekką operacją w porównaniu do stworzenia procesu. 

Przełączanie kontekstu (*context switching*) między wątkami przez scheduler systemu operacyjnego pozwala na osiągnięcie **współbieżności** na pojedynczym rdzeniu CPU. W systemach wielordzeniowych, wątki mogą być wykonywane **równolegle**, każdy na osobnym rdzeniu.

> [!NOTE]
> Operacje na współdzielonych zasobach mogą wymagać **synchronizacji**.

## Tworzenie wątków

`System.Threading.Thread` to bezpośrednia reprezentacja natywnego wątku systemu operacyjnego, opakowana przez *Runtime*. Pozwala na manualne tworzenie i zarządzanie cyklem życia wątku.

Nowy wątek tworzy się inicjalizując obiekt `Thread`, przekazując w konstruktorze delegat, który wskazuje metodę do wykonania (bezparametrowy `delegate void ThreadStart()` lub `delegate void ParameterizedThreadStart(object? obj)`). Metoda `Start` uruchamia wątek.

```csharp
Thread thread = new Thread(PrintA);
thread.Start();

PrintB();

void PrintA()
{
    for (int i = 0; i < 100; i++)
        Console.Write('A');
}

void PrintB()
{
    for (int i = 0; i < 100; i++)
        Console.Write('B');
}
```

> Taki program nie będzie dawał jednoznacznego wyniku. Typowo będą to poprzeplatane sekwencje liter, np.:
> `BBBAAAAAAAAAAAAABAAAAAAAAAAAAAAABBBBABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABBBBBBBBBBBBBBBBBBBBBBBBBBB`


## Usypianie i czekanie na wątek

Metoda `Join` pozwala na poczekanie na inny wątku, aż ten się zakończy.

```csharp
Thread thread = new Thread(PrintA);
thread.Start();

PrintB();
thread.Join();

Console.WriteLine("\nThread joined.");

void PrintA()
{
    for (int i = 0; i < 100; i++)
        Console.Write('A');
}

void PrintB()
{
    for (int i = 0; i < 100; i++)
        Console.Write('B');
}
```

Metoda `Thread.Sleep()` tymczasowo wstrzymuje wykonanie bieżącego wątku na określony czas.

```csharp
Thread.Sleep(TimeSpan.FromMilliseconds(10));
Thread.Sleep(millisecondsTimeout: 10);
```

> [!NOTE]
> Operacje `Sleep` i `Join` to operacje **blokujące**, które powodują, że wątek jest wstrzymywany przez system operacyjny i nie zużywa czasu procesora, dopóki nie zajdzie zdarzenie, na które wątek czeka. Inne typowe operacje blokujące to czekanie na prymitywy synchronizacji (np. `Mutex.Wait()`), albo oberacje I/O (np. `File.ReadAllText()`).

## Wątki pierwszo- i drugoplanowe (Background Threads)

Właściwość `IsBackground` decyduje o tym, czy dany wątek jest pierwszo- czy drugoplanowy.
* **Wątki pierwszoplanowe** (domyślnie): Utrzymują proces aplikacji przy życiu. Aplikacja nie zakończy się, dopóki działa choćby jeden wątek pierwszoplanowy.
* **Wątki drugoplanowe**: nie podtrzymują aplikacji przy życiu, aplikacja kończy się, gdy wszystkie wątki pierwszoplanowe zakończą pracę. Jeśli w tym momencie wciąż działają jakieś wątki drugoplanowe, są one **gwałtownie przerywane** bez możliwości dokończenia swojej pracy. Wątki drugoplanowe są idealne do zadań, które powinny działać "w tle" tak długo, jak działa aplikacja, ale nie są na tyle krytyczne, aby ich niedokończenie lub nagłe przerwanie było problemem.

```csharp
using System;
using System.Threading;

public class Program
{
    public static void Main()
    {
        Thread worker = new Thread(() =>
        {
            Console.WriteLine("Background thread started");
            Console.ReadLine();
            Console.WriteLine("Background thread finished");
        })
        {
            Name = "BackgroundWorker",
            IsBackground = true
        };
        worker.Start();

        Console.WriteLine("Main thread finished.");
    }
}
```

## Bezpieczeństwo wątków i synchronizacja

Wątki w ramach tego samego procesu współdzielą zasoby (np. pola statyczne, obiekty na stercie), dostęp do nich powinien być synchronizowany, aby uniknąć wyścigów (*race conditions*).

Poniższy przykład pokazuje problem wyścigu. Metoda `UnsafePrintOnce` powinna wypisać `Done` tylko raz, ale przy współbieżnym wywołaniu istnieje szansa, że wypisze je dwukrotnie.

```csharp
private static bool _done = false;
static void Main()
{
    Thread thread = new Thread(UnsafePrintOnce);
    thread.Start();

    UnsafePrintOnce();

    thread.Join();

    void UnsafePrintOnce()
    {
        if (_done) return;
        _done = true;
        Console.WriteLine("Done");
    }
}
```

### Instrukcja `lock`

W C# najprostszym prymitywem synchronizacji jest instrukcja `lock`, która gwarantuje, że dany blok kodu (sekcja krytyczna) zostanie wykonany przez tylko jeden wątek w danym momencie. Pozostałe wątki są blokowane aż wątek, który wszedł do sekcji krytycznej z niej nie wyjdzie.

```csharp
private static bool _done = false;
private static readonly Lock LockObj = new Lock();
static void Main()
{
    Thread thread = new Thread(SafePrintOnce);
    thread.Start();

    SafePrintOnce();

    thread.Join();

    void SafePrintOnce()
    {
        lock (LockObj) // critical section
        {
            if (_done) return;
            _done = true;
        }
        
        Console.WriteLine("Done");
    }
}
```

## Obsługa wyjątków w wątkach

Niewyłapany wyjątek w jakimkolwiek wątku, powoduje domyślnie **natychmiastowe zakończenie całej aplikacji**.

```csharp
public class Program
{
    private static void ThrowsException()
    {
        throw new Exception("Unhandled exception");
    }

    public static void Main()
    {
        Thread thread = new Thread(ThrowsException);
        thread.Start();

        Console.WriteLine("Main thread waiting for thread...");
        thread.Join(); 
        Console.WriteLine("Main thread finished.");
    }
}
```

## Pula Wątków (*Thread Pool*)

CLR utrzymuje i zarządza **pulą wątków** (*Thread Pool*), co jest preferowanym sposobem wykonywania asynchronicznych zadań w .NET. Wątki z puli są ponownie wykorzystywane, co redukuje narzut związany z tworzeniem i niszczeniem wątków. Wątki puli są domyślnie drugoplanowe.

Najprostszą metodą, żeby wywołać coś na puli wątków jest klasa `Task`:

```csharp
public class Program
{
    public static void Main()
    {
        Task task = Task.Run(() =>
        {
            Console.WriteLine(Thread.CurrentThread.Name);
            Console.WriteLine("Hello from the thread pool");
        });
        
        task.Wait();
    }
}
```

> Pula wątków jest zarządzana przez *Runtime*, początkowo znajduje się tyle wątków, ile jest w systemie dostępnych fizycznych rdzeni. *Runtime* w zależności od potrzeb dokłada lub usuwa wątki z puli.

> [!NOTE]
> Chociaż wątki są podstawą współbieżności, to w nowoczesnym C# bardzo rzadko korzysta się z nich bezpośrednio. Preferowane są abstrakcje wyższego poziomu operujące na wbudowanej puli wątków, takie jak `Task`, `Parallel` i `async/await`.
