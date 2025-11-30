---
title: "Parallel"
weight: 50
---

# Parallel

Klasa `Parallel` (z przestrzeni nazw `System.Threading.Tasks`) jest jednym z elementów biblioteki TPL (*Task Parallel Library*), która pozwala na pisanie kodu współbieżnego w prosty sposób, abstrahując od zarządzania wątkami.

Klasa `Parallel` zawiera trzy metody:

* `Parallel.Invoke` - pozwala na współbieżne wykonanie wielu delegatów typu `Action`
* `Parallel.For` - równoległy odpowiednik pętli `for`
* `Parallel.ForEach` - równoległy odpowiednik pętli `foreach`

1. W przypadku każdej z tych metod, praca jest efektywnie partycjonowana na kilka zadań i uruchamiana współbieżnie na puli wątków.
2. Metody z klasy `Parallel` są blokujące, aż do momentu wykonania całej pracy.
3. Jeżeli w czasie wykonania jednej z operacji wystąpił wyjątek, to `Parallel` zbierze wszystkie wyjątki i zgłosi je w postaci jednego `AggregateException`.

> [!NOTE]
> Przy korzystaniu z metod `Parallel` należy zawsze pamiętać o bezpieczeństwie wątkowym. Jeśli operacje równoległe modyfikują wspólny stan (np. współdzieloną zmienną, listę, słownik), konieczne jest zapewnienie odpowiedniej synchronizacji dostępu do tego stanu.

## Parallel.Invoke

Metoda `Parallel.Invoke` w najprostszej swojej sygnaturze przyjmuje tablicę delegatów do wykonania.

```csharp
public static void Invoke([NotNull] params Action[] actions);
```

```csharp
public class Program
{
    public static void Main()
    {
        Parallel.Invoke(
            () => DownloadFile("https://pages.mini.pw.edu.pl/~hermant/Tomek.jpg", "hermant.jpg"),
            () => DownloadFile("https://pages.mini.pw.edu.pl/~aszklarp/images/me.jpg", "aszklarp.jpg"),
            () => DownloadFile("https://cadcam.mini.pw.edu.pl/static/media/kadra8.7b107dbb.jpg", "sobotkap.jpg"));
    }

    private static void DownloadFile(string url, string outputPath)
    {
        using HttpClient httpClient = new HttpClient();
        try
        {
            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(outputPath, content);

            Console.WriteLine($"{outputPath} downloaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading {url}: {ex.Message}");
        }
    }
}
```

## Parallel.For

Metoda `Parallel.For` w najprostszej swojej sygnaturze przyjmuje zakres wykonania i delegat, który będzie wykonany dla każdego indeksu.

```csharp
public static ParallelLoopResult For(
    int fromInclusive, 
    int toExclusive, 
    [NotNull] Action<int> body);
```

```csharp
public class Program
{
    public static void Main()
    {
        int from = 1_000_000, to = 1_000_100;
        Parallel.For(from, to, i =>
        {
            Console.WriteLine($"Is {i} prime: {IsPrime(i)}");
        });
    }
    
    private static bool IsPrime(int number)
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

### Przerwanie pętli

Metody `For` i `Foreach` zawierają przeciążenie, które wystawia `ParallelLoopState` w delegacie. Za jego pomocą pętle można przerwać wcześnie.

* `ParallelLoopState.Break()` zapewnia, że zostaną wykonane wszystkie iteracje, które są aktualnie przetwarzane, oraz wszystkie iteracje o niższym indeksie niż ta, dla której wywołano `Break()`. Dalsze iteracje o wyższych indeksach nie będą rozpoczynane.
* `ParallelLoopState.Stop()` natychmiast wstrzymuje pracę wszystkich wątków

```csharp
public static ParallelLoopResult For(
    int fromInclusive, 
    int toExclusive, 
    [NotNull] Action<int, ParallelLoopState> body)
```

Informację o przerwaniu można odczytać ze zwracanego `ParallelLoopResult`.

```csharp
public class Program
{
    public static void Main()
    {
        int from = 1_000_000, to = 1_000_100;
        ParallelLoopResult result = Parallel.For(from, to, (i, loopState) =>
        {
            if (IsPrime(i))
            {
                loopState.Break();
            }
        });

        if (!result.IsCompleted)
        {
            Console.WriteLine($"There is a prime: {result.LowestBreakIteration}");
        }
        else
        {
            Console.WriteLine("There are no primes");
        }
    }
    
    private static bool IsPrime(int number)
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

## Parallel.ForEach

Metoda `Parallel.ForEach` w najprostszej swojej sygnaturze przyjmuje sekwencję i delegat, który będzie wykonany dla każdego elementu.

```csharp
public static ParallelLoopResult ForEach<TSource>(
    [NotNull] IEnumerable<TSource> source, 
    [NotNull] Action<TSource> body)
```

```csharp
public class Program
{
    public static void Main()
    {
        List<(string, string)> urls =
        [
            ("https://pages.mini.pw.edu.pl/~hermant/Tomek.jpg", "hermant.jpg"),
            ("https://pages.mini.pw.edu.pl/~aszklarp/images/me.jpg", "aszklarp.jpg"),
            ("https://pages.mini.pw.edu.pl/~rafalkoj/templates/mini/images/photo.jpg", "rafalkoj.jpg"),
            ("https://pages.mini.pw.edu.pl/~kaczmarskik/krzysztof.jpg", "kaczmarskik.jpg"),
            ("https://cadcam.mini.pw.edu.pl/static/media/kadra8.7b107dbb.jpg", "sobotkap.jpg")
        ];

        Parallel.ForEach(urls, ((string url, string output) tuple) =>
        {
            DownloadFile(tuple.url, tuple.output);
        });
    }

    private static void DownloadFile(string url, string outputPath)
    {
        using HttpClient httpClient = new HttpClient();
        try
        {
            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(outputPath, content);

            Console.WriteLine($"{outputPath} downloaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading {url}: {ex.Message}");
        }
    }
}
```

## ParallelOptions

Do konfiguracji zachowania metod z klasy `Parallel` można użyć obiektu `ParallelOptions`.

Pozwala on na ustawienie takich opcji jak:
* **`MaxDegreeOfParallelism`**: Określa maksymalną liczbę jednocześnie wykonywanych operacji. Jest to przydatne, gdy chcemy ograniczyć obciążenie procesora lub zasobów zewnętrznych. Domyślnie `Parallel` próbuje wykorzystać wszystkie dostępne rdzenie.
* **`CancellationToken`**: Umożliwia kooperacyjne anulowanie operacji równoległych w trakcie ich wykonywania.

```csharp
public class Program
{
    public static void Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        ParallelOptions options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = cts.Token
        };

        int primes = 0;
        int from = 0, to = 10_000_000;
        try
        {
            Parallel.For(from, to, options, i =>
            {
                if (IsPrime(i)) Interlocked.Increment(ref primes);
            });
            Console.WriteLine($"Found exactly {primes} primes from {from}, to {to}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled");
            Console.WriteLine($"Found at least {primes} primes from {from}, to {to}");
        }
    }
    
    private static bool IsPrime(int number)
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
