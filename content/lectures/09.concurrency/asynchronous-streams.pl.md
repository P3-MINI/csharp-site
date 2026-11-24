---
title: "Asynchroniczne sekwencje"
weight: 60
---

# Asynchroniczne sekwencje

Asynchroniczne sekwencje łączą ze sobą dwa koncepty: **iteratory** (instrukcja *yield*) i **metody asynchroniczne** (*async/await*). Pozwalają one na pisanie metod iterujących, które zwracają kolejne elementy asynchronicznie. Jest to szczególnie **przydatne, gdy trzeba czekać na każdy z elementów sekwencji**. (Jeżeli trzeba czekać na sekwencję jako całość, ale nie na pojedyncze elementy, lepiej rozważyć użycie `Task<IEnumerable<T>>`).

```csharp
public interface IAsyncEnumerable<out T>
{
    IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);
}

public interface IAsyncEnumerator<out T> : IAsyncDisposable
{
    T Current { get; }
    ValueTask<bool> MoveNextAsync();
}
```

> [!IMPORTANT]
> Właściwość `Current` zwraca bieżący element. Dostęp do tej właściwości jest dozwolony tylko po tym, jak `MoveNextAsync()` zakończy się z wynikiem `true`.

> `ValueTask<T>` to struktura, analogiczna do klasy `Task<T>`, która pozwala na bardziej wydajne wykonanie przez ograniczenie liczby alokacji pamięci, gdzie w przypadku sekwencji może ich być całkiem sporo.

> `IAsyncDisposable` jest asynchroniczną wersją interfejsu `IDisposable` z pojedynczą metodą `ValueTask DisposeAsync()`.

Składnia języka dostarcza specjalnej instrukcji `await foreach`, która pozwala iterować po kolejnych elementach sekwencji z oczekiwaniem asynchronicznym na kolejne elementy.

```csharp
IAsyncEnumerable<int> asyncSequence = RangeAsync(from: 0, to: 100, delay: 50);
await foreach(var item in asyncSequence)
{
    Console.WriteLine(item);
}
```

> [!NOTE]
> Żeby przekazać `CancellationToken` do sekwencji, można użyć metody rozszerzającej `WithCancellation(CancellationToken token)`, np.: `await foreach(var item in asyncSequence.WithCancellation(token))`.

Podobnie jak `foreach`, `await foreach` jest cukierkiem składniowym. Kompilator rozwija tę instrukcję następująco:

```csharp
IAsyncEnumerable<int> asyncSequence = RangeAsync(from: 0, to: 100, delay: 50);
IAsyncEnumerator<int> asyncEnumerator = asyncSequence.GetAsyncEnumerator();
try
{
    while(await asyncEnumerator.MoveNextAsync())
    {
        var item = asyncEnumerator.Current;
        Console.WriteLine(item);
    }
}
finally
{
    if (asyncEnumerator != null)
    {
        await asyncEnumerator.DisposeAsync();
    }
}
```

## Asynchroniczne metody iterujące

Podobnie jak dla metod iterujących, typem zwracanym musi być `IAsyncEnumerable<T>` lub `IAsyncEnumerator<T>`. Podobnie jak metoda asynchroniczna, musi być ona oznaczona `async`. W takiej metodzie możemy używać zarówno instrukcji `yield`, jak i `await`.

```csharp
public class Program
{
    public static async Task Main()
    {
        await foreach (int i in RangeAsync(0, 100, 50))
        {
            Console.WriteLine(i);
        }
    }
    
    private static async IAsyncEnumerable<int> RangeAsync(int from, int to, int delay)
    {
        for (int i = from; i < to; i++)
        {
            await Task.Delay(delay);
            yield return i;
        }
    }
}
```

Kompilator podobnie wygeneruje nam maszynę stanów odpowiadającą asynchronicznej metodzie iterującej.

### Anulowanie

Asynchroniczne metody iterujące mogą wspierać anulowanie. Należy dodać do takiej metody parametr typu `CancellationToken` i oznaczyć go atrybutem `EnumeratorCancellation` (z przestrzeni nazw `System.Runtime.CompilerServices`). Dzięki temu kompilator będzie wiedział, do którego parametru przekazać token w metodzie `WithCancellation`.

```csharp
using System.Runtime.CompilerServices;

public class Program
{
    public static async Task Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource(2500);
        try
        {
            await foreach (int i in RangeAsync(0, 100, 50).WithCancellation(cts.Token))
            {
                Console.WriteLine(i);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Enumeration cancelled");
        }
    }
    
    private static async IAsyncEnumerable<int> RangeAsync(int from, int to, int delay, 
        [EnumeratorCancellation] CancellationToken token = default)
    {
        for (int i = from; i < to; i++)
        {
            await Task.Delay(delay, token);
            yield return i;
        }
    }
}
```

## LINQ

Od .NET 10 operatory LINQ dla asynchronicznych sekwencji są częścią biblioteki standardowej: klasa `System.Linq.AsyncEnumerable` w przestrzeni nazw `System.Linq`. We wcześniejszych wersjach .NET dostarczał je rozwijany przez społeczność pakiet NuGet `System.Linq.Async` (w .NET 10 należy usunąć referencję do tego pakietu lub zaktualizować go do wersji 7, w przeciwnym razie mogą wystąpić błędy niejednoznacznych wywołań).

```csharp
IAsyncEnumerable<int> asyncSequence = RangeAsync(from: 0, to: 100, delay: 50)
    .Where(x => x % 2 == 0)
    .Select(x => x * x);
    
await foreach(var item in asyncSequence)
{
    Console.WriteLine(item);
}
```

Operatory zwracające pojedynczą wartość lub kolekcję (np. `ToListAsync`, `CountAsync`, `FirstAsync`) są asynchroniczne i trzeba na nie czekać (`await`). Wiele operatorów ma również przeciążenia z asynchronicznym delegatem, który przyjmuje `CancellationToken` i zwraca `ValueTask<T>`:

```csharp
List<int> squares = await RangeAsync(from: 0, to: 10, delay: 50)
    .Where(x => x % 2 == 0)
    .Select(async (x, token) =>
    {
        await Task.Delay(10, token);
        return x * x;
    })
    .ToListAsync();

Console.WriteLine(string.Join(", ", squares)); // 0, 4, 16, 36, 64
```
