---
title: "Współbieżność"
weight: 80
---

## Tutorial 8: Współbieżność

Zakres tego laboratorium obejmuje programowanie współbieżne i równoległe w C#: wątki, pulę wątków i zadania (`Task`, `Task<T>`), klasę `Parallel`, podstawy synchronizacji (`Interlocked`, `lock`), kooperacyjne anulowanie, raportowanie postępu oraz programowanie asynchroniczne z użyciem `async`/`await`.

Nie zaczniemy od zera. W [Tutorialu 3]({{< ref "/labs/03.basics" >}}) napisałeś raytracer, który renderuje obraz przy użyciu jednego wątku. Śledzenie promieni to doskonały przykład obliczeń obciążających procesor (*CPU-bound*), które da się zrównoleglić: kolor każdego piksela jest obliczany niezależnie od pozostałych pikseli. W tym tutorialu sprawimy, że raytracer będzie **szybki** (wykorzysta wszystkie rdzenie procesora) i **responsywny** (pokaże postęp, pozwoli przerwać renderowanie i będzie zapisywał obrazy asynchronicznie).

W zadaniu pojawią się dwa rodzaje współbieżności:
- **Równoległość** – obliczenia obciążające procesor są dzielone między kilka rdzeni, więc kończą się szybciej (renderowanie).
- **Asynchroniczność** – podczas oczekiwania na operację, która nie potrzebuje procesora (np. zapis pliku), żaden wątek nie jest blokowany, więc program może w tym czasie robić coś innego (zapisywanie klatek).

### Wątki

Wątek to podstawowa jednostka wykonania w procesie. Wątki jednego procesu współdzielą jego pamięć (stertę, pola statyczne), ale każdy wątek ma własny stos wywołań. Klasa `System.Threading.Thread` reprezentuje wątek systemu operacyjnego – jest odpowiednikiem `std::thread` z C++:

```csharp
Thread worker = new Thread(() => Console.WriteLine($"Hello from thread {Environment.CurrentManagedThreadId}"));
worker.Start(); // the new thread starts running the lambda
Console.WriteLine($"Hello from thread {Environment.CurrentManagedThreadId}");
worker.Join();  // wait until the thread finishes
```

Kolejność dwóch wypisanych linii nie jest określona – zależy od planisty systemu operacyjnego. W porównaniu z C++ jest kilka różnic:
- Obiektu `Thread` nie trzeba dołączać przez `Join()` (nie ma tu odpowiednika `std::terminate` przy niszczeniu niedołączonego wątku). Wątki są domyślnie *pierwszoplanowe* – proces nie zakończy się, dopóki wszystkie one nie skończą pracy (chyba że ustawimy `IsBackground = true`).
- Nieobsłużony wyjątek w dowolnym wątku kończy cały proces.

Więcej szczegółów znajdziesz w wykładzie o [wątkach]({{< ref "/lectures/09.concurrency/threads" >}}).

### Pula wątków i zadania

Utworzenie wątku systemu operacyjnego jest stosunkowo kosztowne (wywołanie jądra, osobny stos). Dlatego .NET utrzymuje **pulę wątków** – zbiór wątków roboczych, które wykonują krótkie fragmenty pracy z kolejki, a następnie są ponownie wykorzystywane. Wątki z puli są wątkami drugoplanowymi.

We współczesnym C# rzadko tworzy się wątki ręcznie. Zamiast tego używamy klas `Task` (bez wyniku) i `Task<T>` (z wynikiem typu `T`), które reprezentują operację, która zakończy się w przyszłości. Metoda `Task.Run` umieszcza delegat w kolejce puli wątków i zwraca zadanie. Przypomina to `std::async` i `std::future<T>` z C++ – `task.Result` blokuje tak jak `future.get()`:

```csharp
Task<long> task = Task.Run(() => SumOfSquares(1_000_000)); // runs on a thread pool thread
// ... the calling thread is free to do something else here ...
long result = task.Result; // blocks until the task completes
Console.WriteLine(result); // 333333833333500000

static long SumOfSquares(int n)
{
    long sum = 0;
    for (int i = 1; i <= n; i++)
    {
        sum += (long)i * i;
    }
    return sum;
}
```

Więcej szczegółów znajdziesz w wykładzie o [zadaniach]({{< ref "/lectures/09.concurrency/tasks" >}}).

### `async` i `await`

Blokowanie wątku podczas oczekiwania (`task.Result`, `task.Wait()`, `Thread.Sleep`) marnuje go – wątek nie może w tym czasie robić nic innego. Operator `await` czeka **asynchronicznie**: jeśli zadanie jeszcze się nie zakończyło, metoda zostaje zawieszona, a wątek zwolniony; gdy zadanie się zakończy, wykonywana jest reszta metody (tzw. *kontynuacja*). Metoda używająca `await` musi być oznaczona jako `async` i zazwyczaj zwraca `Task` lub `Task<T>`. Kompilator przekształca ją w maszynę stanów – podobnie jak w przypadku korutyn z C++20 i `co_await`.

```csharp
int count = await CountLinesAsync("data.txt");

static async Task<int> CountLinesAsync(string path)
{
    string[] lines = await File.ReadAllLinesAsync(path); // no thread waits for the disk
    return lines.Length;
}
```

Zwróć uwagę, że samo `async` nie tworzy żadnych wątków – pozwala jedynie zawiesić wykonanie metody. Więcej szczegółów znajdziesz w wykładzie o [programowaniu asynchronicznym]({{< ref "/lectures/09.concurrency/asynchronous-programming" >}}).

### Współdzielone dane i wyścigi

Gdy kilka wątków modyfikuje te same dane, wynik może zależeć od tego, jak przeplotą się ich wykonania – taką sytuację nazywamy **wyścigiem** (*race condition*). Nawet `counter++` nie jest bezpieczne, ponieważ składa się z trzech kroków (odczyt, dodanie, zapis), a dwa wątki mogą odczytać tę samą, starą wartość:

```csharp
int unsafeCounter = 0;
int safeCounter = 0;

void Work()
{
    for (int i = 0; i < 1_000_000; i++)
    {
        unsafeCounter++;                        // read-modify-write: updates get lost
        Interlocked.Increment(ref safeCounter); // atomic increment
    }
}

Task.WaitAll(Task.Run(Work), Task.Run(Work));
Console.WriteLine($"unsafe: {unsafeCounter}, safe: {safeCounter}"); // e.g. unsafe: 1470357, safe: 2000000
```

Podstawowe narzędzia pozwalające uniknąć wyścigów to:
- Klasa `Interlocked` – operacje atomowe na pojedynczych zmiennych (`Increment`, `Add`, `Exchange`, `CompareExchange`), odpowiednik `std::atomic` z C++.
- Instrukcja `lock` – sekcja krytyczna, którą w danej chwili może wykonywać co najwyżej jeden wątek, odpowiednik `std::mutex` z `std::lock_guard`. Od C# 13 (.NET 9) jako obiekt blokady zalecany jest dedykowany typ `System.Threading.Lock`.

Najlepszym rozwiązaniem jest jednak całkowite unikanie współdzielonych, modyfikowalnych danych. Więcej szczegółów znajdziesz w wykładach o [synchronizacji]({{< ref "/lectures/12.synchronization/introduction" >}}) i [blokowaniu wyłącznym]({{< ref "/lectures/12.synchronization/exclusive-locking" >}}).

### Pomiar wydajności

Do pomiaru czasu wykonania użyjemy klasy `System.Diagnostics.Stopwatch` (`Stopwatch.StartNew()`, `Restart()`, `Elapsed`). **Przyspieszenie** (*speedup*) wersji równoległej to stosunek czasu wersji sekwencyjnej do czasu wersji równoległej. Mierząc wydajność, pamiętaj, że:
- Zawsze należy mierzyć konfigurację **Release**: `dotnet run -c Release`. W konfiguracji Debug kompilator JIT nie optymalizuje kodu – nasz raytracer działa wtedy około 6 razy wolniej.
- Pierwsze uruchomienie kodu obejmuje również kompilację JIT (rozgrzewkę), więc zwykle jest nieco wolniejsze od kolejnych.
- `Environment.ProcessorCount` zwraca liczbę procesorów **logicznych**. Przy współbieżnej wielowątkowości (SMT, Hyper-Threading) dwa procesory logiczne współdzielą jeden fizyczny rdzeń, więc 16 procesorów logicznych na 8-rdzeniowej maszynie nie daje 16-krotnego przyspieszenia.

## Zadanie 1 - Szybki i responsywny raytracer

W tym zadaniu zrównoleglisz raytracer z [Tutorialu 3]({{< ref "/labs/03.basics" >}}) na kilka sposobów i porównasz je ze sobą: ręcznie tworzone wątki, zadania oraz klasę `Parallel`. Następnie dodasz raportowanie postępu i anulowanie, a na koniec wyrenderujesz animację, w której klatki są renderowane równolegle i zapisywane asynchronicznie.

### Kod początkowy

Pobierz kod początkowy. Jest to ostateczna wersja raytracera z Tutorialu 3 (etap 5) z kilkoma zmianami:
- Rozwiązanie używa nowego formatu `.slnx`.
- Obraz nie jest już wypisywany na standardowe wyjście. Nowa metoda `Image.Save(string path)` zapisuje go do pliku PPM, dzięki czemu konsola może służyć do wypisywania komunikatów.
- `Camera` ma nową właściwość tylko do odczytu `ImageHeight`.
- `Program.cs` tworzy scenę w metodzie `CreateScene()`, używa mniejszej rozdzielczości (640x360) i 64 próbek na piksel oraz mierzy czas renderowania za pomocą `Stopwatch`.

Uruchom program w konfiguracji Release:

```bash
dotnet run -c Release
```

Przykładowe wyjście (Twoje czasy będą inne):

```text
Rendering 640x360, 64 samples per pixel
Sequential: 2.94 s
Saved image.ppm
```

> [!NOTE]
> #### Kod początkowy
> {{< filetree dir="labs/w08.concurrency/Raytracer.00" >}}

Zanim cokolwiek zrównoleglimy, musimy wiedzieć, które dane są współdzielone przez wątki. Podczas renderowania scena (`Scene`, obiekty, materiały) i parametry kamery są **tylko odczytywane**. Jedyne zapisywane dane to wynikowy obraz. Każdy piksel jest zapisywany dokładnie raz, więc jeśli każdy piksel zostanie obliczony przez dokładnie jeden wątek, wątki nie będą sobie przeszkadzać. Jednostką pracy będzie u nas pojedynczy wiersz obrazu.

### Etap 1: Wątki

W tym etapie podzielimy wiersze obrazu na równe, poziome pasy i wyrenderujemy każdy pas na osobnym, ręcznie utworzonym wątku.

#### Refaktoryzacja: renderowanie pojedynczego wiersza

W pliku `Camera.cs`:
1. Dodaj prywatną metodę `Vector3[] RenderRow(IHittable world, int j)`, która renderuje wiersz `j` obrazu i zwraca tablicę `ImageWidth` kolorów pikseli. Przenieś do niej ciało zewnętrznej pętli metody `Render` (pętle po pikselach i próbkach).
2. W pliku `Image.cs` dodaj metodę `void SetRow(int y, Vector3[] row)`, która kopiuje podane kolory do wiersza `y` obrazu.
3. Uprość metodę `Render` tak, aby dla każdego wiersza `j` wywoływała `image.SetRow(j, RenderRow(world, j))`.

Uruchom program – wynik nie powinien się zmienić.

#### Renderowanie z użyciem wątków

W klasie `Camera` dodaj metodę `public Image RenderWithThreads(IHittable world, int threadCount)`:
1. Utwórz obiekt `Image` oraz tablicę `threadCount` wątków (`Thread[]`).
2. Oblicz liczbę wierszy przypadających na wątek, zaokrąglając w górę: `rowsPerThread = (_imageHeight + threadCount - 1) / threadCount`.
3. W pętli po `t` wyznacz zakres wierszy `[startRow, endRow)` wątku `t` (ostatni pas może być krótszy – użyj `Math.Min`), utwórz wątek z lambdą renderującą wiersze z tego zakresu i uruchom go metodą `Start()`.
4. Po uruchomieniu wszystkich wątków poczekaj na każdy z nich metodą `Join()` i zwróć obraz.

> [!IMPORTANT]
> #### Lambdy przechwytują zmienne, a nie wartości
>
> W C++ decydujesz, czy lambda przechwytuje przez wartość (`[=]`), czy przez referencję (`[&]`). W C# lambda zawsze przechwytuje **zmienne** – widzi ich bieżącą wartość w momencie wykonania, a nie w momencie utworzenia. Zmienna pętli `for` jest jedną zmienną współdzieloną przez wszystkie iteracje, więc poniższy kod jest niepoprawny:
> ```csharp
> Thread[] threads = new Thread[4];
> for (int t = 0; t < threads.Length; t++)
> {
>     threads[t] = new Thread(() => Console.WriteLine($"Rendering strip {t}"));
>     threads[t].Start();
> }
> ```
> Zanim wątki zaczną działać, pętla zwykle już się kończy i wszystkie wypisują `Rendering strip 4` – pas, który nie istnieje. Rozwiązaniem jest skopiowanie wartości do zmiennej zadeklarowanej **wewnątrz** ciała pętli – każda iteracja dostaje nową zmienną:
> ```csharp
> int strip = t; // a fresh variable in every iteration
> threads[t] = new Thread(() => Console.WriteLine($"Rendering strip {strip}"));
> ```
> W naszej metodzie oblicz `startRow` i `endRow` wewnątrz ciała pętli, przed utworzeniem wątku. (Zmienna pętli `foreach` jest nowa w każdej iteracji, więc ten problem dotyczy tylko pętli `for` i zmiennych zadeklarowanych poza pętlą).

> [!NOTE]
> **Dlaczego nie potrzebujemy blokady?** Wątki odczytują współdzielone dane (scenę, kamerę), których nikt nie modyfikuje w trakcie renderowania, i zapisują rozłączne wiersze obrazu. Współbieżne odczyty są bezpieczne, a zapisy do różnych elementów tablicy nie wpływają na siebie nawzajem. Co więcej, `Join()` gwarantuje, że wszystko, co zapisał wątek, jest widoczne dla wątku, który wywołał `Join()` – po dołączeniu wszystkich wątków można bezpiecznie korzystać z obrazu.

> [!IMPORTANT]
> #### Bezpieczeństwo wątkowe klasy `Random`
>
> Renderowanie korzysta z liczb losowych (antyaliasing, rozpraszanie), a do ich generowania używamy już `Random.Shared`. `Random.Shared` to bezpieczna wątkowo instancja, której można używać z wielu wątków jednocześnie. Natomiast zwykły obiekt `Random` (`new Random()`) **nie** jest bezpieczny wątkowo – gdyby był współdzielony przez wiele wątków bez synchronizacji, jego stan wewnętrzny mógłby ulec uszkodzeniu (dokumentacja ostrzega, że może on wtedy zacząć zwracać same zera).

#### Pomiar przyspieszenia

W pliku `Program.cs`:
1. Zapisz `Environment.ProcessorCount` w zmiennej `threadCount` i wypisz tę wartość w nagłówku.
2. Zmierz czas sekwencyjnej metody `Render`, a następnie `RenderWithThreads(world, threadCount)` jednym obiektem `Stopwatch` (między pomiarami użyj `Restart()`).
3. Dodaj metodę pomocniczą, która wypisze nazwę wariantu, jego czas oraz przyspieszenie względem wersji sekwencyjnej:

```csharp
static void Report(string name, double seconds, double baseline)
{
    Console.WriteLine($"{name,-24}{seconds,6:F2} s   speedup {baseline / seconds,4:F1}x");
}
```

Zapisz obraz wyrenderowany przez wątki do pliku `image.ppm` i sprawdź, czy wygląda tak samo jak wcześniej (bez brakujących wierszy).

Przykładowe wyjście na 8-rdzeniowym procesorze z 16 procesorami logicznymi:

```text
Rendering 640x360, 64 samples per pixel, 16 logical processors
Sequential                3.18 s   speedup  1.0x
Threads (16)              0.49 s   speedup  6.5x
Saved image.ppm
```

Przyspieszenie jest dużo mniejsze niż 16. Jednym z powodów jest SMT (tylko 8 fizycznych rdzeni). Aby znaleźć kolejny, tymczasowo zmierz czas pracy każdego wątku – uruchom `Stopwatch` na początku lambdy i wypisz czas po zakończeniu pętli (na potrzeby komunikatu skopiuj `t` do zmiennej lokalnej `index` i dodaj `using System.Diagnostics;` w pliku `Camera.cs`):

```csharp
Console.WriteLine($"Thread {index,2}: rows {startRow,3}-{endRow - 1,3} took {stopwatch.Elapsed.TotalSeconds:F2} s");
```

```text
Thread  2: rows  46- 68 took 0.09 s
Thread  4: rows  92-114 took 0.09 s
Thread  0: rows   0- 22 took 0.10 s
Thread  5: rows 115-137 took 0.10 s
Thread  3: rows  69- 91 took 0.10 s
Thread  1: rows  23- 45 took 0.12 s
Thread  6: rows 138-160 took 0.22 s
Thread 15: rows 345-359 took 0.21 s
Thread  7: rows 161-183 took 0.26 s
Thread 13: rows 299-321 took 0.28 s
Thread 14: rows 322-344 took 0.30 s
Thread 12: rows 276-298 took 0.33 s
Thread  8: rows 184-206 took 0.34 s
Thread 11: rows 253-275 took 0.35 s
Thread 10: rows 230-252 took 0.42 s
Thread  9: rows 207-229 took 0.50 s
Threads (16)              0.51 s   speedup  6.3x
```

Wiersze nie są jednakowo kosztowne. Promień, który trafia w niebo, kończy się po pierwszym odbiciu, a promienie trafiające w kule i podłoże odbijają się wielokrotnie. Wątki renderujące niebo kończą pracę po 0,1 s i dalej stoją bezczynnie, a całe renderowanie trwa tyle, ile najwolniejszy wątek. Problem ten nazywamy **nierównomiernym rozłożeniem obciążenia** (*load imbalance*) i cierpi na niego podział pracy na stałe, równe części (*partycjonowanie statyczne*). Przed przejściem dalej usuń wypisywanie diagnostyczne.

> [!NOTE]
> #### Etap 1
> {{< filetree dir="labs/w08.concurrency/Raytracer.01" >}}

### Etap 2: Zadania i pula wątków

Zamiast kilku dużych pasów utworzymy teraz wiele małych fragmentów pracy – jedno zadanie na wiersz – i pozwolimy puli wątków je rozdzielić. Wątek z puli, który skończy wiersz, od razu pobiera z kolejki następny, więc wątki, którym trafiły się tanie wiersze, po prostu wyrenderują ich więcej (*dynamiczne równoważenie obciążenia*). Dodatkowo, zamiast zapisywać do współdzielonego obrazu, każde zadanie **zwróci** swój wiersz jako wynik.

#### Renderowanie z użyciem zadań

W klasie `Camera` dodaj metodę `public async Task<Image> RenderWithTasksAsync(IHittable world)`:
1. Utwórz tablicę `Task<Vector3[]>[] tasks` z jednym elementem na wiersz.
2. W pętli po wierszach uruchom każde zadanie za pomocą `Task.Run(() => RenderRow(world, row))`. Pamiętaj, aby najpierw skopiować zmienną pętli do zmiennej lokalnej (patrz poprzedni etap).
3. Poczekaj na wszystkie zadania: `Vector3[][] rows = await Task.WhenAll(tasks);`. Wyniki są zwracane w tej samej kolejności, w jakiej zadania znajdują się w tablicy, niezależnie od kolejności, w jakiej się zakończyły.
4. Utwórz obraz, wypełnij go za pomocą `SetRow` i zwróć go.

> [!IMPORTANT]
> #### Asynchroniczna metoda `Main`
>
> `await` można używać tylko w metodzie `async`. Aby wywołać `RenderWithTasksAsync` z użyciem `await`, zmień sygnaturę `Main` na `static async Task Main(string[] args)` – środowisko uruchomieniowe poczeka wtedy na zwrócone zadanie przed zakończeniem programu. Zgodnie z konwencją nazwy metod zwracających zadania kończą się na `Async`.

W pliku `Program.cs` zmień `Main` na metodę asynchroniczną i dodaj pomiar `await cam.RenderWithTasksAsync(world)` (nazwij go `Tasks (one per row)`). Zapisz obraz wyrenderowany przez zadania.

> [!IMPORTANT]
> #### Wyjątki w zadaniach
>
> Wyjątek rzucony wewnątrz zadania nie kończy programu – jest zapamiętywany w zadaniu i rzucany ponownie temu, kto na zadanie czeka. Sposób ponownego rzucenia zależy od sposobu oczekiwania:
> - Blokujące oczekiwanie (`Wait()`, `Result`, `Task.WaitAll`) rzuca `AggregateException`, który zawiera wszystkie wyjątki we właściwości `InnerExceptions`.
> - `await` rzuca ponownie oryginalny wyjątek (bez opakowania). W przypadku `Task.WhenAll` jest to pierwszy z wyjątków – wszystkie są dostępne we właściwości `Exception` zadania zwróconego przez `WhenAll`.
> ```csharp
> Task<int>[] tasks = [Task.Run(() => 42), Task.Run(() => Fail(7)), Task.Run(() => Fail(42))];
>
> try
> {
>     Task.WaitAll(tasks); // blocking wait
> }
> catch (AggregateException e)
> {
>     foreach (Exception inner in e.InnerExceptions)
>     {
>         Console.WriteLine($"WaitAll: {inner.Message}");
>     }
> }
>
> Task<int[]> all = Task.WhenAll(tasks);
> try
> {
>     int[] results = await all; // asynchronous wait
> }
> catch (InvalidOperationException e)
> {
>     Console.WriteLine($"await: {e.Message}");
>     Console.WriteLine($"all.Exception contains {all.Exception!.InnerExceptions.Count} exceptions");
> }
>
> static int Fail(int row) => throw new InvalidOperationException($"Row {row} failed");
> ```
> ```text
> WaitAll: Row 7 failed
> WaitAll: Row 42 failed
> await: Row 7 failed
> all.Exception contains 2 exceptions
> ```

Przykładowe wyjście:

```text
Rendering 640x360, 64 samples per pixel, 16 logical processors
Sequential                3.28 s   speedup  1.0x
Threads (16)              0.50 s   speedup  6.5x
Tasks (one per row)       0.31 s   speedup 10.6x
Saved image.ppm
```

Zadania są wyraźnie szybsze – ale nie dlatego, że zadania są szybsze od wątków (przecież i tak wykonują je wątki). Różnica wynika z rozmiaru fragmentów pracy: gdybyś uruchomił tylko 16 zadań, z których każde renderuje jeden pas jak w etapie 1, otrzymałbyś taki sam czas jak dla wątków. Wiele małych fragmentów pracy pozwala puli dynamicznie równoważyć obciążenie.

> [!TIP]
> Ziarnistość pracy ma znaczenie w obu kierunkach. Zbyt duże fragmenty pracy powodują nierównomierne obciążenie, a zbyt małe – narzut: utworzenie i zaplanowanie zadania kosztuje znacznie więcej niż wyrenderowanie jednego piksela, więc jedno zadanie na piksel (230 400 zadań) byłoby złym pomysłem. Wiersz (kilka milisekund pracy) to rozsądny kompromis.

> [!NOTE]
> #### Etap 2
> {{< filetree dir="labs/w08.concurrency/Raytracer.02" >}}

### Etap 3: `Parallel.For`

Zrównoleglanie pętli o niezależnych iteracjach jest na tyle częste, że .NET dostarcza gotowe rozwiązanie: klasę `Parallel`. `Parallel.For(from, to, body)` wykonuje `body(i)` dla każdego `i` z przedziału `[from, to)` na puli wątków – jest to odpowiednik algorytmów równoległych z C++17 (`std::for_each(std::execution::par, ...)`). W tym etapie dodamy też pasek postępu, który wymaga synchronizacji.

> [!IMPORTANT]
> #### `Parallel.For` i `ParallelOptions`
>
> - `Parallel.For` jest **blokujące** – kończy działanie po wykonaniu wszystkich iteracji. Wątek wywołujący również bierze udział w pracy.
> - Pracę dzieli dynamicznie: każdy wątek roboczy wielokrotnie pobiera niewielką paczkę kolejnych indeksów (paczki rosną w miarę postępu pętli), a wątki, którym skończyła się praca, przejmują pozostałe fragmenty zakresu.
> - Wyjątek rzucony w dowolnej iteracji przerywa pętlę; wszystkie wyjątki są rzucane ponownie, opakowane w `AggregateException`.
> - Przeciążenie z `ParallelOptions` pozwala ustawić `MaxDegreeOfParallelism` (maksymalną liczbę jednocześnie wykonywanych iteracji; domyślna wartość `-1` oznacza brak limitu) oraz `CancellationToken` (użyjemy go w kolejnym etapie).
>
> Więcej szczegółów znajdziesz w wykładzie o [klasie `Parallel`]({{< ref "/lectures/09.concurrency/parallel" >}}).

#### Renderowanie z użyciem `Parallel.For`

W klasie `Camera` dodaj metodę `public Image RenderParallel(IHittable world, ParallelOptions options, ProgressBar? progressBar = null)`, która renderuje wszystkie wiersze za pomocą `Parallel.For(0, _imageHeight, options, j => ...)`. Po wyrenderowaniu każdego wiersza zwiększ licznik ukończonych wierszy i przekaż jego nową wartość do paska postępu:

```csharp
int rowsDone = 0;

Parallel.For(0, _imageHeight, options, j =>
{
    image.SetRow(j, RenderRow(world, j));

    // rowsDone++ would not be atomic - many threads update the counter at once
    int done = Interlocked.Increment(ref rowsDone);
    progressBar?.Update(done);
});
```

> [!IMPORTANT]
> #### `Interlocked`
>
> `Interlocked.Increment(ref x)` atomowo zwiększa zmienną i **zwraca nową wartość**. Zawsze używaj zwróconej wartości: ponowny odczyt `rowsDone` chwilę później mógłby zwrócić wartość zwiększoną już przez inny wątek. Operacje `Interlocked` są tańsze od blokady, ale działają tylko na pojedynczych zmiennych.

#### Pasek postępu

Utwórz plik `ProgressBar.cs` z klasą `ProgressBar` rysującą tekstowy pasek postępu, np. `[##########------------------------------]  25%`. Klasa ma konstruktor podstawowy (C# 12) przyjmujący całkowitą liczbę kroków i szerokość paska: `ProgressBar(int total, int width = 40)`, oraz metodę `void Update(int done)`, która będzie wywoływana przez wiele wątków jednocześnie.

Rysowanie składa się z kilku operacji: obliczenia procentu, sprawdzenia, czy się zmienił, i wypisania paska. Nie mogą się one przeplatać między wątkami, dlatego umieść je w sekcji krytycznej chronionej instrukcją `lock`:

```csharp
public class ProgressBar(int total, int width = 40)
{
    private readonly Lock _lock = new();
    private int _lastPercent = -1;

    public void Update(int done)
    {
        int percent = done * 100 / total;

        lock (_lock)
        {
            // Another thread may have already drawn a newer state
            if (percent <= _lastPercent)
            {
                return;
            }
            _lastPercent = percent;

            // TODO: draw the bar
        }
    }
}
```

1. Aby narysować pasek, wypisz go za pomocą `Console.Write`, zaczynając od znaku `\r` (powrót karetki), który przenosi kursor na początek linii – kolejna aktualizacja nadpisze poprzednią. Napis złożony z powtórzonych znaków utworzysz za pomocą `new string('#', count)`.
2. Gdy postęp osiągnie 100%, przejdź do nowej linii (`Console.WriteLine()`).

> [!IMPORTANT]
> #### `lock` i `System.Threading.Lock`
>
> Instrukcja `lock (obj) { ... }` gwarantuje, że blok w danej chwili wykonuje co najwyżej jeden wątek; pozostałe wątki czekają na wejściu. Jest to odpowiednik `std::lock_guard<std::mutex>` z C++ – blokada jest zwalniana przy opuszczeniu bloku, również na skutek wyjątku. W starszym kodzie spotkasz dowolny prywatny obiekt używany jako blokada (`private readonly object _lock = new();`); od C# 13 (.NET 9) preferowany jest dedykowany typ `System.Threading.Lock` – wyraża intencję i może być wydajniejszy.
>
> Po co nam `_lastPercent`? `Interlocked.Increment` atomowo przydziela kolejne wartości, ale nie określa kolejności, w jakiej wątki wejdą następnie do sekcji krytycznej. Wątek z wartością 50 może rysować po wątku z wartością 51 – bez tego sprawdzenia pasek czasami cofałby się.

#### Porównanie wariantów

W pliku `Program.cs`:
1. Zmierz czas `RenderParallel` dla `MaxDegreeOfParallelism` równego 1, 2, 4, 8 oraz `threadCount` (np. iterując po tablicy `int[] degrees = [1, 2, 4, 8, threadCount];`).
2. Na koniec zmierz `RenderParallel` z domyślnymi opcjami (`new ParallelOptions()`) i paskiem postępu `new ProgressBar(cam.ImageHeight)`, a ten obraz zapisz.

Przykładowe wyjście na 8-rdzeniowym procesorze z 16 procesorami logicznymi:

```text
Rendering 640x360, 64 samples per pixel, 16 logical processors
Sequential                3.26 s   speedup  1.0x
Threads (16)              0.51 s   speedup  6.5x
Tasks (one per row)       0.31 s   speedup 10.4x
Parallel.For (1)          2.95 s   speedup  1.1x
Parallel.For (2)          1.45 s   speedup  2.2x
Parallel.For (4)          0.79 s   speedup  4.1x
Parallel.For (8)          0.45 s   speedup  7.2x
Parallel.For (16)         0.46 s   speedup  7.1x
[########################################] 100%
Parallel.For + progress   0.44 s   speedup  7.5x
Saved image.ppm
```

Obserwacje:
- Do liczby fizycznych rdzeni czas maleje prawie liniowo. Powyżej niej zysk jest niewielki – procesory logiczne jednego rdzenia współdzielą jego jednostki wykonawcze.
- `Parallel.For (1)` jest nieco szybsze od wersji sekwencyjnej, choć wykonuje tę samą pracę – pierwszy pomiar obejmuje również rozgrzewkę JIT.
- `Parallel.For` to jedna linijka kodu i jest wyraźnie lepsze od statycznych pasów. Tutaj jest nieco wolniejsze od jednego zadania na wiersz: pobierane przez nie paczki kolejnych wierszy mogą być kosztowne i pod koniec pętli część rdzeni czeka na ostatnie paczki. Przy większej ilości pracy (wyższej rozdzielczości) różnica maleje, a narzut `Parallel.For` na jedną iterację jest dużo mniejszy niż narzut zadania.

Pomiary są zaszumione – uruchom program kilka razy i zamknij inne aplikacje. Twoje wyniki będą zależeć od procesora.

> [!TIP]
> Jeśli potrzebujesz kontroli nad rozmiarem paczek, `Parallel.ForEach` może iterować po przedziałach utworzonych przez `Partitioner.Create(0, _imageHeight, rangeSize)` (przestrzeń nazw `System.Collections.Concurrent`) – każda iteracja otrzymuje krotkę `(start, end)`.

> [!NOTE]
> #### Etap 3
> {{< filetree dir="labs/w08.concurrency/Raytracer.03" >}}

### Etap 4: Anulowanie i raportowanie postępu

Renderowanie jest teraz na tyle szybkie, że możemy wrócić do jakości z Tutorialu 3: 1280x720 i 100 próbek na piksel (około 20 s sekwencyjnie, 2–3 s równolegle). Przy dłuższym renderowaniu użytkownik powinien móc je przerwać kombinacją Ctrl+C i mimo to otrzymać wyrenderowaną dotąd część obrazu. Uniezależnimy też kamerę od konsoli: kamera powinna jedynie raportować postęp, a o sposobie jego wyświetlania powinien decydować program.

> [!IMPORTANT]
> #### Kooperacyjne anulowanie
>
> W .NET operacji nie da się zatrzymać z zewnątrz – musi ona współpracować. Mechanizm składa się z dwóch typów (podobnych do `std::stop_source` i `std::stop_token` z C++20):
> - `CancellationTokenSource` – obiekt, który zgłasza żądanie anulowania (`Cancel()` lub automatycznie po upływie limitu czasu: `CancelAfter(TimeSpan)`).
> - `CancellationToken` – struktura przekazywana do operacji, która ją sprawdza: `token.IsCancellationRequested` lub `token.ThrowIfCancellationRequested()`, które rzuca `OperationCanceledException`.
>
> `Parallel.For` sprawdza token z `ParallelOptions.CancellationToken` przed rozpoczęciem kolejnych iteracji. Po anulowaniu nie rozpoczyna nowych iteracji, czeka na zakończenie tych w toku i rzuca `OperationCanceledException`. Więcej szczegółów znajdziesz w wykładzie o [programowaniu asynchronicznym]({{< ref "/lectures/09.concurrency/asynchronous-programming" >}}).

> [!IMPORTANT]
> #### `IProgress<T>` i `Progress<T>`
>
> `IProgress<T>` to standardowy interfejs z jedną metodą `void Report(T value)`. Kod wykonujący pracę przyjmuje `IProgress<T>?` i raportuje postęp, nie wiedząc nic o tym, jak jest on wyświetlany.
>
> Standardowa implementacja `Progress<T>` wywołuje funkcję zwrotną (przekazaną do konstruktora lub podpiętą do zdarzenia `ProgressChanged`) za pośrednictwem `SynchronizationContext` przechwyconego w chwili utworzenia obiektu. W aplikacjach okienkowych (WPF, WinForms, Avalonia) jest to dokładnie to, czego potrzebujemy: postęp jest raportowany z wątków roboczych, ale funkcja zwrotna wykonuje się na wątku UI, gdzie można aktualizować kontrolki. Aplikacja konsolowa nie ma kontekstu synchronizacji, więc `Progress<T>` umieszcza każde wywołanie funkcji zwrotnej w kolejce puli wątków – wywołania wykonują się współbieżnie, w przypadkowej kolejności, a nawet już po zakończeniu renderowania. W aplikacji konsolowej prościej jest więc zaimplementować `IProgress<T>` bezpośrednio – wtedy `Report` wykonuje się synchronicznie na wątku raportującym.

#### Postęp przez `IProgress<int>`

1. Niech `ProgressBar` implementuje `IProgress<int>` – zmień nazwę metody `Update` na `Report`. Blokada wciąż jest potrzebna, ponieważ `Report` wywołuje wiele wątków.
2. W klasie `Camera` zmień typ ostatniego parametru `RenderParallel` z `ProgressBar?` na `IProgress<int>? progress`.

#### Renderowanie z możliwością anulowania

Aby zachować częściowy obraz po anulowaniu, obraz musi utworzyć kod wywołujący:
1. Dodaj przeciążenie `public void RenderParallel(IHittable world, Image image, ParallelOptions options, IProgress<int>? progress = null)`, które renderuje do podanego obrazu. Przenieś do niego pętlę `Parallel.For`.
2. Istniejąca metoda `RenderParallel(world, options, progress)` tworzy obraz, wywołuje nowe przeciążenie i zwraca obraz.

W pliku `Program.cs`:
1. Przenieś kod mierzący wszystkie warianty z `Main` do metody `static async Task BenchmarkAsync(Scene world)`. Będzie ona wykonywana, gdy program zostanie uruchomiony z argumentem `benchmark`: `dotnet run -c Release -- benchmark` (sprawdź to wzorcem listy `args is ["benchmark"]`).
2. W przeciwnym razie `Main` tworzy `CancellationTokenSource` (w deklaracji `using`) i obsługuje Ctrl+C:
   ```csharp
   Console.CancelKeyPress += (sender, e) =>
   {
       e.Cancel = true; // do not terminate the process, stop the rendering gracefully
       cts.Cancel();
   };
   ```
   Domyślnie Ctrl+C natychmiast kończy proces; ustawienie `e.Cancel = true` pozwala mu działać dalej, aby mógł zakończyć się w uporządkowany sposób.
3. Następnie `Main` wywołuje nową metodę `static void RenderImage(Scene world, CancellationToken token)`, która:
   - tworzy kamerę z `ImageWidth = 1280` i `SamplesPerPixel = 100` oraz wypisuje komunikat, że Ctrl+C przerywa renderowanie,
   - tworzy obraz o rozmiarze kamery oraz `ParallelOptions` z ustawionym `CancellationToken`,
   - wywołuje `RenderParallel` z obiektem `ProgressBar` w bloku `try`; w przypadku powodzenia wypisuje czas renderowania, a w przeciwnym razie przechwytuje `OperationCanceledException`,
   - w obu przypadkach zapisuje obraz do pliku `image.ppm`.

Przykładowe wyjście, gdy renderowanie się zakończy:

```text
Rendering 1280x720, 100 samples per pixel. Press Ctrl+C to cancel.
[########################################] 100%
Rendered in 2.30 s
Saved image.ppm
```

Oraz gdy w trakcie renderowania zostanie naciśnięte Ctrl+C:

```text
Rendering 1280x720, 100 samples per pixel. Press Ctrl+C to cancel.
[##############################----------]  77%
Cancelled after 1.40 s, saving the partial image
Saved image.ppm
```

Częściowy obraz pokazuje, jak `Parallel.For` rozdzieliło wiersze: tanie wiersze nieba zostały ukończone jako pierwsze, a wiersze poniżej były pobierane w paczkach przez różne wątki. Czarne wiersze nie zostały jeszcze rozpoczęte.

> [!NOTE]
> #### Etap 4
> ![Stage 4 Render]({{< relurl "/labs/w08.concurrency/Raytracer.04/image.jpg" >}})
> {{< filetree dir="labs/w08.concurrency/Raytracer.04" >}}

### Etap 5: Asynchroniczne renderowanie animacji

W ostatnim etapie wyrenderujemy krótką animację: kamera okrąża scenę, a każda klatka jest zapisywana do osobnego pliku. Tym razem nie będziemy zrównoleglać pracy wewnątrz klatki – klatki są niezależne, więc wyrenderujemy wiele klatek jednocześnie, każdą sekwencyjnie na wątku z puli. Każda klatka powinna zostać zapisana, gdy tylko będzie gotowa, bez czekania na pozostałe, a zapis nie powinien blokować żadnego wątku.

> [!IMPORTANT]
> #### `Task.WhenEach` i `await foreach`
>
> `Task.WhenAll` czeka na wszystkie zadania, `Task.WhenAny` – na pierwsze z nich. `Task.WhenEach` (.NET 9) zwraca asynchroniczną sekwencję (`IAsyncEnumerable<Task<T>>`), która zwraca zadania **w kolejności ich zakończenia**. Po takiej sekwencji iterujemy pętlą `await foreach` – między elementami pętla czeka asynchronicznie:
> ```csharp
> await foreach (Task<Frame> finished in Task.WhenEach(renderTasks))
> {
>     Frame frame = await finished; // already completed: gives the result or rethrows
>     // ...
> }
> ```
> Więcej szczegółów znajdziesz w wykładzie o [asynchronicznych sekwencjach]({{< ref "/lectures/09.concurrency/asynchronous-streams" >}}).

#### Przygotowanie klas

1. W klasie `Image` dodaj metodę `public async Task SaveAsync(string path, CancellationToken token = default)`, która zapisuje obraz za pomocą `File.WriteAllTextAsync`.
2. W klasie `Camera` dodaj do sekwencyjnej metody `Render` opcjonalny parametr `CancellationToken token = default` i przed wyrenderowaniem każdego wiersza wywołuj `token.ThrowIfCancellationRequested()`.
3. Utwórz plik `Frame.cs` z rekordem `public record Frame(int Index, Image Image);` – wyrenderowaną klatką wraz z jej numerem.

#### Renderowanie klatek

W pliku `Program.cs` dodaj metodę `static Frame RenderFrame(Scene world, int index, int frameCount, CancellationToken token)`, która tworzy kamerę dla klatki o numerze `index` i renderuje klatkę metodą `Render(world, token)`. Użyj `ImageWidth = 480`, `SamplesPerPixel = 32`, `Fov = 60`, celu `(0, 0, -1)` oraz pozycji kamery na okręgu o promieniu 2,5 wokół celu, 0,5 nad nim:

```csharp
float angle = 2 * MathF.PI * index / frameCount;
Vector3 position = target + new Vector3(2.5f * MathF.Sin(angle), 0.5f, 2.5f * MathF.Cos(angle));
```

> [!WARNING]
> `Camera` jest klasą modyfikowalną – zmiana `Position` przelicza jej pola wewnętrzne, które są odczytywane podczas renderowania. Gdyby wszystkie klatki współdzieliły jedną kamerę i zmieniały jej pozycję, przeszkadzałyby sobie nawzajem (wyścig). Każda klatka musi mieć własny obiekt `Camera`. Scena jest współdzielona bezpiecznie, ponieważ jest tylko odczytywana.

#### Zapisywanie klatek, gdy tylko są gotowe

Dodaj metodę `static async Task RenderAnimationAsync(Scene world, CancellationToken token)`:
1. Utwórz katalog `frames` (`Directory.CreateDirectory`) i uruchom `Stopwatch`.
2. Dla każdej z `frameCount = 36` klatek uruchom zadanie `Task.Run(() => RenderFrame(world, index, frameCount, token), token)` i zapisz je w tablicy `Task<Frame>[]`. Przekazanie tokenu do `Task.Run` sprawia, że zadania, które jeszcze nie wystartowały, zostaną anulowane bez uruchamiania.
3. W bloku `try` iteruj po `Task.WhenEach(renderTasks)` pętlą `await foreach`. Zapisz każdą klatkę za pomocą `await frame.Image.SaveAsync(path)` do pliku `frames/frame_NNN.ppm` (użyj `Path.Combine` i formatu `{frame.Index:D3}`) i wypisz czas, który upłynął, nazwę pliku oraz liczbę zapisanych dotąd klatek.
4. Przechwyć `OperationCanceledException` i wypisz, ile klatek zostało zapisanych.

W metodzie `Main` wywołaj `await RenderAnimationAsync(world, cts.Token)`, gdy program zostanie uruchomiony z argumentem `animation`:

```bash
dotnet run -c Release -- animation
```

Przykładowe wyjście (skrócone) – zwróć uwagę, że klatki są zapisywane w kolejności, w jakiej się kończą, a nie w kolejności numerów:

```text
Rendering 36 frames. Press Ctrl+C to cancel.
[ 1.62 s] saved frames/frame_009.ppm (1/36)
[ 1.70 s] saved frames/frame_007.ppm (2/36)
[ 1.72 s] saved frames/frame_011.ppm (3/36)
...
[ 4.12 s] saved frames/frame_035.ppm (35/36)
[ 4.15 s] saved frames/frame_034.ppm (36/36)
```

> [!NOTE]
> Celowo nie przekazujemy tokenu do `SaveAsync`: po naciśnięciu Ctrl+C aktualnie zapisywany plik zostanie dokończony, więc na dysku nie zostanie ucięty plik. Renderowanie i tak zatrzyma się szybko, ponieważ `Render` sprawdza token przed każdym wierszem.

> [!IMPORTANT]
> #### `SynchronizationContext` i `ConfigureAwait`
>
> Po `await` metoda jest kontynuowana w przechwyconym `SynchronizationContext`, o ile taki istnieje. W aplikacjach okienkowych jest to wątek UI. W aplikacji konsolowej `SynchronizationContext.Current` ma wartość `null`, więc kontynuacje wykonują się na dowolnym wątku z puli:
> ```csharp
> Console.WriteLine($"SynchronizationContext: {SynchronizationContext.Current?.ToString() ?? "none"}");
> Console.WriteLine($"Before await: thread {Environment.CurrentManagedThreadId}");
> await Task.Delay(100);
> Console.WriteLine($"After await:  thread {Environment.CurrentManagedThreadId}");
> ```
> ```text
> SynchronizationContext: none
> Before await: thread 1
> After await:  thread 5
> ```
> `await task.ConfigureAwait(false)` sprawia, że kontynuacja nie wraca do przechwyconego kontekstu. W aplikacji konsolowej nie ma dokąd wracać, więc `ConfigureAwait(false)` niczego nie zmienia. Ma ono znaczenie w kodzie bibliotecznym, który może być wywoływany z aplikacji okienkowych – pozwala uniknąć niepotrzebnego powrotu na wątek UI (oraz zakleszczenia, gdy kod wywołujący blokuje wątek UI przez `.Result`). Zobacz wykład o [kontekście synchronizacji]({{< ref "/lectures/09.concurrency/synchronization-context" >}}).

> [!TIP]
> Klatki można połączyć w animację, np. za pomocą *ffmpeg* (`ffmpeg -framerate 12 -i frames/frame_%03d.ppm orbit.gif`) lub *ImageMagick* (`magick -delay 8 frames/*.ppm orbit.gif`).

> [!NOTE]
> #### Etap 5
> ![Stage 5 Frames]({{< relurl "/labs/w08.concurrency/Raytracer.05/image.jpg" >}})
> {{< filetree dir="labs/w08.concurrency/Raytracer.05" >}}

### Kierunki dalszego rozwoju

Raytracer wykorzystuje teraz wszystkie rdzenie i pozostaje responsywny. Poniżej zebraliśmy pomysły na dalszy rozwój:

- **Potok producent–konsument z `Channel<T>`:** W etapie 5 wszystkie zadania renderujące są uruchamiane naraz, a gotowe klatki czekają w pamięci na zapis. `System.Threading.Channels` dostarcza kolejek asynchronicznych: zadania renderujące zapisują klatki do kanału o ograniczonej pojemności (`Channel.CreateBounded<Frame>(capacity)`), a jedno zadanie zapisujące odczytuje je pętlą `await foreach (Frame frame in channel.Reader.ReadAllAsync())`. Gdy kanał jest pełny, producenci czekają (tzw. *backpressure*), co ogranicza zużycie pamięci. Porównaj to z `BlockingCollection<T>` z wykładu o [kolekcjach współbieżnych]({{< ref "/lectures/12.synchronization/concurrent-collections" >}}).
- **`Parallel.ForEachAsync`:** Wykonuje asynchroniczne ciało dla każdego elementu z ograniczonym stopniem równoległości – np. renderuje i zapisuje klatki tak, aby w toku było jednocześnie co najwyżej N klatek.
- **Kafelki i renderowanie progresywne:** Renderuj kwadratowe kafelki zamiast wierszy (lepsza lokalność pamięci podręcznej) albo najpierw wyrenderuj cały obraz z 1 próbką na piksel i poprawiaj go w kolejnych przebiegach, aby podgląd był dostępny niemal natychmiast.
- **Stan lokalny wątku:** Przeciążenie `Parallel.For` z parametrami `localInit` i `localFinally` pozwala gromadzić statystyki (np. liczbę wyśledzonych promieni) w zmiennej lokalnej każdego wątku roboczego i scalać je tylko raz na końcu, zamiast wywoływać `Interlocked` dla każdego promienia.
- **SIMD:** Równoległość wewnątrz jednego rdzenia – typ `System.Numerics.Vector3` korzysta z instrukcji wektorowych procesora. Zastąpienie nim naszego własnego `Vector3` może przyspieszyć nawet wersję sekwencyjną.
- **Wiarygodne pomiary:** Biblioteka *BenchmarkDotNet* wykonuje rozgrzewkę, powtarza pomiary i oblicza statystyki, dzięki czemu porównania są znacznie bardziej wiarygodne niż pojedynczy pomiar `Stopwatch`.
- **GUI z podglądem na żywo:** W aplikacji okienkowej (np. Avalonia) obiekt `Progress<T>` utworzony na wątku UI może przekazywać gotowe wiersze na wątek UI i wyświetlać obraz w trakcie renderowania.

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/08.concurrency/example1" >}})
- [Zadanie 2]({{< ref "/labs/08.concurrency/example2" >}})
