---
title: "Blokowanie współdzielone"
weight: 30
---

# Blokowanie współdzielone (*Non-exclusive Locking*)

Blokowanie współdzielone to mechanizm, który pozwala na dostęp do zasobu więcej niż jednemu wątkowi jednocześnie, zazwyczaj pod pewnymi warunkami (np. limit liczby wątków lub dostęp tylko do odczytu).

## Semaphore(Slim)

Semafory pozwalają **ograniczyć** liczbę wątków, które mogą jednocześnie uzyskać dostęp do zasobu lub sekcji kodu. W .NET występują w dwóch wersjach: `Semaphore` to opakowanie na uchwyt do semafora oferowanego przez system operacyjny (może być nazwany, co pozwala na synchronizację międzyprocesową; nazwane semafory są jednak obsługiwane tylko w systemie Windows). `SemaphoreSlim` jest natomiast implementowany w całości przez środowisko uruchomieniowe, dzięki czemu jest szybszy (około 10x - nie wymaga przejść do trybu jądra), ale nie może być za to użyty do synchronizacji międzyprocesowej. Wersja "Slim" wspiera także konstrukcje asynchroniczne.

Semafory są inicjalizowane z określoną liczbą "pozwoleń" (*permits*). `Wait()`/`WaitAsync()` zmniejsza licznik, a `Release()` zwiększa. Zazwyczaj używa się ich do ograniczania przepustowości (np. max 5 jednoczesnych pobierań plików, max 10 połączeń do bazy danych).

> [!NOTE]
> Jeżeli `SemaphoreSlim` ma wolne zasoby (licznik > 0), metoda `WaitAsync` kończy się synchronicznie (bez przełączania kontekstu), co jest bardzo wydajne.

### Przykład

W poniższym przykładzie `Downloader` ogranicza liczbę jednoczesnych pobierań do wartości `concurrency`.

```csharp
public class Downloader(int concurrency)
{
    private readonly SemaphoreSlim _semaphore = new(concurrency);

    public async Task DownloadAsync(string url, string fileName, CancellationToken token = default)
    {
        Console.WriteLine($"[{fileName}] Waiting to start download...");

        await _semaphore.WaitAsync(token);
        
        Console.WriteLine($"[{fileName}] Downloading...");

        try
        {
            using HttpClient client = new HttpClient();

            byte[] data = await client.GetByteArrayAsync(url, token);

            Console.WriteLine($"[{fileName}] Downloaded {data.Length / 1024.0:0.00} KB");

            await File.WriteAllBytesAsync(fileName, data, token);
            Console.WriteLine($"[{fileName}] Download finished.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{fileName}] Error: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/synchronization/SemaphoreExample" >}}

## ReaderWriterLock(Slim)

`ReaderWriterLock(Slim)` to prymityw zoptymalizowany pod scenariusze, w których operacje odczytu są znacznie częstsze niż operacje zapisu. Wspiera dwa tryby blokowania. Dowolna liczba wątków może być w trybie odczytu jednocześnie (`EnterReadLock` / `ExitReadLock`). Tylko jeden wątek może być w trybie zapisu (`EnterWriteLock` / `ExitWriteLock`). Gdy wątek jest w trybie zapisu, żaden inny wątek nie może czytać ani pisać.

Najczęściej używa się go przy współdzielonych strukturach danych, konfiguracji aplikacji lub do implementacji pamięci podręcznej - wszędzie tam, gdzie odczyty są częste, a zapisy bardzo rzadkie. W prostych przypadkach zwykły `lock` jest zazwyczaj wystarczający.

> [!WARNING]
> Należy używać nowszej klasy `ReaderWriterLockSlim` zamiast starszej `ReaderWriterLock`. Wersja "Slim" jest znacznie wydajniejsza, ma prostsze zasady rekurencji oraz podnoszenia/obniżania poziomu blokady i pozwala uniknąć wielu sytuacji grożących zakleszczeniem.
