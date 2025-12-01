---
title: "Programowanie asynchroniczne"
weight: 40
---

# Programowanie asynchroniczne

Programowanie asynchroniczne to model programowania, który pozwala na wykonywanie długotrwałych operacji bez blokowania głównego wątku aplikacji. Jest to ważne dla zapewnienia responsywności interfejsu użytkownika oraz skalowalności aplikacji serwerowych.

W nowoczesnym C# programowanie asynchroniczne jest realizowane głównie za pomocą słów kluczowych `async` i `await`. Współpracują one z typami `Task` i `Task<TResult>`, reprezentującymi asynchroniczne zadania. Model ten pozwala pisać kod asynchroniczny, który w swojej strukturze i czytelności bardzo przypomina kod synchroniczny.

## Oczekiwanie - `await`

Słowo kluczowe `await` jest cukierkiem składniowym, który znacznie upraszcza pracę z operacjami asynchronicznymi. Kompilator tłumaczy ten zapis na znacznie bardziej złożoną strukturę opartą na maszynie stanów.

```csharp
var result = await expression;
statement(s);
```

> [!NOTICE]
> Wyrażenie (*expression*) jest najczęściej typu `Task` lub `Task<TResult>`. Jednakże każdy obiekt z metodą GetAwaiter zwracający *awaiter* zadowoli kompilator. Patrz: [await-anything](https://devblogs.microsoft.com/pfxteam/await-anything/).

Transformacja ta obejmuje kilka kroków:

1. `GetAwaiter()`: Z wyrażenia (najczęściej z Taska) pobierany jest obiekt `awaiter`, który zarządza procesem oczekiwania.
2. `IsCompleted`: Wykonywana jest optymalizacja – jeśli operacja jest już zakończona, reszta kodu wykonuje się synchronicznie, bez przełączania wątków.
3. Przechwycenie kontekstu: Jeśli operacja nie jest zakończona, `await` przechwytuje bieżący `SynchronizationContext`. Jest to kluczowe w aplikacjach UI, aby móc wrócić na główny wątek.
4. `OnCompleted`: Rejestrowana jest "kontynuacja" – czyli reszta metody. Ten kod zostanie wywołany w przyszłości, gdy zadanie się zakończy.
5. Powrót do kontekstu: Wewnątrz kontynuacji sprawdzany jest przechwycony kontekst. Jeśli istnieje (np. w aplikacji okienkowej), reszta kodu jest wywoływana (`Post`) do wykonania w miejscu wskazywanym przez kontekst synchronizacji. W przeciwnym razie kod jest wykonywany na wątku z puli.
6. `GetResult()`: Na samym końcu wywoływana jest metoda `GetResult()`, która zwraca wynik operacji lub rzuca wyjątek, jeśli zadanie zakończyło się błędem.

```csharp
var awaiter = expression.GetAwaiter();

if (!awaiter.IsCompleted)
{
    var context = SynchronizationContext.Current;
    awaiter.OnCompleted(() =>
    {
        if (context != null)
        {
            context.Post(_ => 
            {
                var result = awaiter.GetResult();
                statement(s);
            }, null);
        }
        else
        {
            var result = awaiter.GetResult();
            statement(s);
        }
    });
}
else
{
    var result = awaiter.GetResult();
    statement(s);
}
```

> [!NOTE]
> Zachowanie domyślnego przechwytywania kontekstu przez `await` można wyłączyć, używając `await expression.ConfigureAwait(false)`.

> [!INFO]
> Przykład ilustrujący przełączanie wątków:
> {{< filetree dir="lectures/concurrency/AsyncAwaitThreads" >}}


## Metody asynchroniczne

Metody asynchroniczne w C# są realizowane jako forma korutyn. Metoda asynchroniczna w C# to metoda, która jest oznaczona słowem kluczowym `async`. Oznaczenie metody jako `async` ma dwa główne cele:

1. Umożliwia użycie operatora `await` wewnątrz tej metody do oczekiwania na zakończenie operacji asynchronicznych (np. Tasków).
2. Instruuje kompilator, aby przekształcił metodę w maszynę stanów, która potrafi zarządzać zawieszaniem i wznawianiem swojego działania.

* Słowo `async` samo w sobie nie tworzy nowego wątku. Samo oznaczenie metody jako `async` nie sprawia, że wykonuje się ona w tle. Metoda rozpoczyna swoje działanie synchronicznie na bieżącym wątku. Dopiero napotkanie `await` na operacji, która jeszcze się nie zakończyła, powoduje zawieszenie metody i zwolnienie wątku.
* Typy zwracane: Metoda `async` musi zwracać jeden z trzech typów:
  * `Task`: Dla operacji asynchronicznych, które nie zwracają wartości.
  * `Task<TResult>`: Dla operacji, które po zakończeniu zwracają wartość typu TResult.
  * `void`: **Zalecane tylko dla obsługi zdarzeń** (np. `async void Button_Click(...)`). Użycie `async void` w innych miejscach jest złą praktyką, ponieważ utrudnia obsługę wyjątków i śledzenie zakończenia operacji.

Poniższy przykład przedstawia **synchroniczne** wykonanie operacji. Metoda `GetPrimesCount()` używa `Thread.Sleep(1000)`, czyli wątek, który ją wywołał, zostaje zablokowany na jedną sekundę. Jeśli byłby to wątek UI, aplikacja przestałaby odpowiadać na ten czas.
  
```csharp
void PrintPrimesCount()
{
    int primes = GetPrimesCount();
    Console.WriteLine($"Primes: {primes}");
}

int GetPrimesCount()
{
    Thread.Sleep(1000);
    return 42;
}
```

Poniżej ten sam cel osiągnięty w sposób **asynchroniczny**:

```csharp
async Task PrintPrimesCountAsync()
{
    Task<int> primesTask = GetPrimesCountAsync();
    Console.WriteLine($"Primes: {await primesTask}");
}

async Task<int> GetPrimesCountAsync()
{
    await Task.Delay(1000);
    return 42;
}
```

> [!NOTE] 
> Wyrażenia lambda również mogą być asynchroniczne. Obowiązują w nich te same zasady, co dla zwykłych metod asynchronicznych.

> [!NOTE]
> C# wspiera również asynchroniczne sekwencje poprzez interfejs `IAsyncEnumerable` i konstrukcję `await foreach`. Chociaż sam `yield return` w bloku iteratora jest zawsze synchroniczny, to cały iterator może pobierać dane asynchronicznie, zawieszając swoje działanie między kolejnymi elementami.

> [!INFO]
> Przykład aplikacji okienkowej z asynchronicznym wywołaniem:
> {{< filetree dir="lectures/concurrency/AsyncAwaitUI" >}}

## Równoległość

Programowanie asynchroniczne może być wykorzystane żeby osiągnąć równoległe wykonanie kodu.

```csharp
class Program
{
    private static async Task Main()
    {
        Task<int> task1 = CountPrimesAsync(0, 1000);
        Task<int> task2 = CountPrimesAsync(1000, 2000);
        Console.WriteLine($"Primes(0-1000): {await task1}");
        Console.WriteLine($"Primes(1000-2000): {await task2}");
    }

    private static async Task<int> CountPrimesAsync(int start, int end)
    {
        return await Task.Run(() =>
        {
            int count = 0;
            
            for (int i = start; i < end; i++)
            {
                if (IsPrime(i))
                {
                    count++;
                }
            }
            
            return count;
        });
    }
    
    static bool IsPrime(int number)
    {
        if (number < 2)
            return false;
        
        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0)
                return false;
        }
        
        return true;
    }
}
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/concurrency/AsyncAwaitPrimes" >}}

## Czekanie na wiele zadań

### `Task.WhenAll`

`Task.WhenAll` tworzy zadanie, które zakończy się dopiero wtedy, gdy wszystkie zadania z podanej kolekcji zostaną ukończone.

```csharp
Task<int> task1 = CountPrimesAsync(1, 100);
Task<int> task2 = CountPrimesAsync(101, 200);
int[] primesCounts = await Task.WhenAll(task1, task2);
```

* Jeśli `Task.WhenAll` dostaje kolekcję `Task`, zwraca `Task`.
* Jeśli dostaje kolekcję `Task<TResult>`, zwraca `Task<TResult[]>`, czyli zadanie, którego wynikiem jest tablica wyników ze wszystkich ukończonych zadań (w tej samej kolejności, w jakiej były zadania wejściowe).

### `Task.WhenAny`

`Task.WhenAny` tworzy zadanie, które zakończy się, gdy tylko którekolwiek zadanie z podanej kolekcji zostanie ukończone.

```csharp
Task<int> task1 = CountPrimesAsync(0, 100);
Task<int> task2 = CountPrimesAsync(100, 200);
Task<int> task = await Task.WhenAny(task1, task2);
Console.WriteLine($"Primes: {await task}");
```

`Task.WhenAny` zwraca `Task<Task>` (lub `Task<Task<TResult>>`).
* Zewnętrzny `Task` kończy się, gdy dowolne zadanie z kolekcji się zakończy.
* Wynikiem zewnętrznego `Task`a jest ten wewnętrzny `Task,` który właśnie się zakończył. Trzeba go odpakować, aby dostać się do jego wyniku.


## Anulowanie zadań

Do anulowania zadań w służy mechanizm oparty na dwóch powiązanych ze sobą typach: `CancellationTokenSource` i `CancellationToken`.

1. `CancellationTokenSource` – to obiekt, który tworzy tokeny `CancellationToken` i sygnalizuje anulowanie.
2. `CancellationToken` – to struktura przekazywana do zadania. Zadanie używa go do sprawdzania, czy zażądano anulowania.

Jak to działa?

1. Tworzysz instancję `CancellationTokenSource`. Konstruktor przyjmuje opcjonalny parametr `int timeout`, jeżeli obecny, to anulowanie zostanie automatycznie zasygnalizowane po określonym czasie.
2. Z tego źródła pobierasz `CancellationToken` za pomocą właściwości `Token`.
3. Przekazujesz ten token do zadania, które chcesz móc anulować.
4. Wewnątrz zadania musisz okresowo sprawdzać stan tokenu. Najprostszym sposobem jest wywołanie metody `token.ThrowIfCancellationRequested()`. Rzuca ona wyjątek `OperationCanceledException`, jeśli anulowanie zostało zasygnalizowane. Alternatywnie można sprawdzić stan tokenu właściwością `token.IsCancellationRequested`.
5. Aby zainicjować anulowanie, wywołujesz metodę `Cancel()` na obiekcie `CancellationTokenSource`.
6. Gdy zadanie jest anulowane, oczekiwanie na nie (np. przez `Wait()` lub `await`) zakończy się wyjątkiem `OperationCanceledException`.

```csharp
class Program
{
    private static async Task Main()
    {
        var cancellationSource = new CancellationTokenSource(5000);
        try
        {
            List<int> primes = await GetPrimesAsync(2, cancellationSource.Token);
            Console.WriteLine($"Number of primes: {primes.Count}");
            Console.WriteLine($"Last prime: {primes[^1]}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Canceled");
        }
    }
    
    static async Task<List<int>> GetPrimesAsync(int start, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            List<int> primes = [];
            
            for (int i = start; i < int.MaxValue; i++)
            {
                // if (token.IsCancellationRequested) break;
                token.ThrowIfCancellationRequested();
                if (IsPrime(i))
                {
                    primes.Add(i);
                }
            }

            return primes;
        });
    }

    static bool IsPrime(int number)
    {
        if (number < 2)
            return false;
        
        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }
}
```

> [!NOTE]
> Większość systemowych metod asynchronicznych zawiera przeciążenie akceptujące `CancellationToken`.

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/concurrency/AsyncAwaitCancellation" >}}
