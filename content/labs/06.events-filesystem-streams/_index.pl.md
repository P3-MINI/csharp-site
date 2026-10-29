---
title: "Zdarzenia, system plików, strumienie"
weight: 60
---

## Tutorial 6: Zdarzenia, system plików, strumienie

Zakres tego laboratorium obejmuje pracę z systemem plików (ścieżki, pliki, katalogi, `FileSystemWatcher`), strumienie (odczyt i zapis plików, adaptery i dekoratory strumieni, kompresja, archiwa ZIP), zwalnianie zasobów za pomocą `IDisposable` i `using` oraz zdarzenia (`event`, `EventHandler<T>`, własne klasy `EventArgs`).

Podobnie jak w C++, program w C# komunikuje się z systemem operacyjnym za pośrednictwem biblioteki – tutaj jest to przestrzeń nazw `System.IO`. Same koncepcje wydadzą Ci się znajome (`std::filesystem`, `std::fstream`), ale są dwie istotne różnice. Po pierwsze, w C# nie ma destruktorów wywoływanych deterministycznie na końcu zakresu, dlatego pliki i inne zasoby zwalnia się jawnie, za pomocą `IDisposable` i `using`. Po drugie, język ma wbudowane wsparcie dla wzorca obserwatora: **zdarzenia**. Przed rozpoczęciem zadania krótko omówimy najważniejsze zagadnienia.

### Ścieżki i katalogi

W C# ścieżki są zwykłymi napisami (`string`). Do operacji na nich używaj statycznej klasy `Path`, zamiast ręcznie sklejać napisy – zna ona separator katalogów bieżącego systemu operacyjnego (`/` na Linuksie i macOS, `\` na Windowsie):

```csharp
string path = Path.Combine("data", "2026", "measurements.csv");
Console.WriteLine(path);                         // data/2026/measurements.csv (data\2026\measurements.csv on Windows)
Console.WriteLine(Path.GetFileName(path));       // measurements.csv
Console.WriteLine(Path.GetExtension(path));      // .csv
Console.WriteLine(Path.GetDirectoryName(path));  // data/2026
Console.WriteLine(Path.GetFullPath(path));       // e.g. /home/student/data/2026/measurements.csv

string root = Path.GetFullPath("data");
Console.WriteLine(Path.GetRelativePath(root, Path.GetFullPath(path))); // 2026/measurements.csv
```

Ścieżki względne są rozwiązywane względem **bieżącego katalogu roboczego** procesu – katalogu, z którego uruchomiono program, a nie katalogu, w którym znajduje się plik wykonywalny.

Do plików i katalogów można odwoływać się przez klasy statyczne (`File`, `Directory`) albo przez obiekty opisujące konkretny plik lub katalog (`FileInfo`, `DirectoryInfo`). Zawartość katalogu można pobrać na dwa sposoby:

- `GetFiles`, `GetDirectories` – zwracają od razu tablicę ze wszystkimi elementami,
- `EnumerateFiles`, `EnumerateDirectories` – zwracają leniwą sekwencję `IEnumerable<T>`; elementy są odczytywane z dysku w miarę iterowania, dzięki czemu można zacząć przetwarzać pierwszy plik, zanim całe drzewo zostanie przeszukane.

Więcej szczegółów znajdziesz w wykładzie [System plików]({{< ref "/lectures/07.filesystem/filesystem" >}}).

### Strumienie i zwalnianie zasobów

**Strumień** (`Stream`) to abstrakcja sekwencji bajtów, którą można czytać, zapisywać i (czasami) pozycjonować. To samo API służy do pracy z plikami (`FileStream`), pamięcią (`MemoryStream`), połączeniami sieciowymi czy skompresowanymi danymi. Strumienie operują na bajtach; aby pracować z tekstem, opakowuje się strumień w **adapter** (`StreamReader`, `StreamWriter`) – podobnie jak `std::ifstream` w C++ łączy bufor pliku z formatowanym odczytem tekstu.

Otwarty plik jest zasobem systemu operacyjnego, o którym Garbage Collector nic nie wie. Obiekty posiadające takie zasoby implementują interfejs `IDisposable`, a instrukcja `using` gwarantuje, że ich metoda `Dispose()` zostanie wywołana na końcu zakresu – nawet jeśli zostanie rzucony wyjątek. Jest to odpowiednik RAII z C++:

```csharp
// Count the lines of a (possibly huge) file without loading it into memory
using FileStream stream = File.OpenRead("data/measurements.csv"); // closed when the scope ends
using var reader = new StreamReader(stream);                      // adapter: bytes -> text
int lines = 0;
while (reader.ReadLine() is not null)
{
    lines++;
}
Console.WriteLine(lines); // 1441
```

Więcej szczegółów znajdziesz w wykładach [Strumienie]({{< ref "/lectures/07.filesystem/streams" >}}) oraz [Disposable]({{< ref "/lectures/05.csharp-features/disposable" >}}).

### Delegaty i zdarzenia

**Delegat** to bezpieczna typowo referencja do metody (wraz z obiektem, na rzecz którego ma zostać wywołana) – odpowiednik `std::function` z C++. Delegaty są **multicastowe**: za pomocą `+=` można podpiąć kilka metod, które zostaną wywołane jedna po drugiej.

**Zdarzenie** to pole typu delegatu udostępnione ze słowem kluczowym `event`. Z zewnątrz dozwolone są tylko operacje `+=` (subskrypcja) i `-=` (anulowanie subskrypcji); zdarzenie może wywołać wyłącznie klasa, która je deklaruje. Zgodnie z konwencją zdarzenia korzystają z delegatu `EventHandler<TEventArgs>`, czyli metod o sygnaturze `void Handler(object? sender, TEventArgs e)`, gdzie `sender` to obiekt wywołujący zdarzenie, a `e` przenosi jego dane (klasa dziedzicząca po `EventArgs`):

```csharp
var countdown = new Countdown(3);

// Subscribing: a lambda and a method group
countdown.Tick += (sender, e) => Console.WriteLine($"{e.Remaining}...");
countdown.Finished += OnFinished;

countdown.Run(); // prints: 3... 2... 1... Liftoff!

countdown.Finished -= OnFinished; // unsubscribing
// countdown.Finished(countdown, EventArgs.Empty); // error CS0070: only the owner can raise the event

static void OnFinished(object? sender, EventArgs e) => Console.WriteLine("Liftoff!");

public class TickEventArgs(int remaining) : EventArgs
{
    public int Remaining { get; } = remaining;
}

public class Countdown(int start)
{
    public event EventHandler<TickEventArgs>? Tick;
    public event EventHandler? Finished;

    public void Run()
    {
        for (int i = start; i > 0; i--)
        {
            OnTick(new TickEventArgs(i));
        }
        OnFinished(EventArgs.Empty);
    }

    // null when nobody has subscribed - hence ?.Invoke
    protected virtual void OnTick(TickEventArgs e) => Tick?.Invoke(this, e);
    protected virtual void OnFinished(EventArgs e) => Finished?.Invoke(this, e);
}
```

Więcej szczegółów znajdziesz w wykładach [Delegaty]({{< ref "/lectures/05.csharp-features/delegates" >}}) oraz [Zdarzenia]({{< ref "/lectures/05.csharp-features/events" >}}).

## Zadanie 1 - Snapshot: narzędzie do przyrostowych kopii zapasowych

W ramach tego zadania stworzysz **Snapshot** – proste narzędzie konsolowe do wykonywania przyrostowych kopii zapasowych katalogu. Będzie się ono składało z pojedynczego projektu – aplikacji wykonywalnej obsługującej kilka poleceń:

```bash
Snapshot scan <source>                                     # summary of a directory tree
Snapshot backup <source> <repository> [max-file-size-KB]   # make a backup
Snapshot restore <repository> <target> [snapshot-name]     # restore the files
Snapshot watch <source> <repository>                       # back up automatically after every change
```

**Przyrostowa** kopia zapasowa zapisuje tylko to, co zmieniło się od poprzedniej kopii. Każda kopia tworzy tzw. *migawkę* (ang. *snapshot*): pierwsza zawiera wszystkie pliki, a każda kolejna – tylko pliki nowe i zmodyfikowane. Każda migawka zawiera ponadto **manifest** – listę wszystkich plików wraz z ich rozmiarami i skrótami SHA-256. Porównując manifesty, program wie, które pliki zostały dodane, zmodyfikowane lub usunięte, i potrafi odtworzyć stan katalogu z dowolnego momentu.

#### Przykładowe dane

Do zadania dołączony jest mały przykładowy katalog (fikcyjny projekt stacji pogodowej, ok. 100 KB), którego kopie zapasowe będziemy wykonywać:

> [!NOTE]
> #### Przykładowy katalog
> {{< filetree dir="labs/w06.events-filesystem-streams/sample" >}}

Utwórz rozwiązanie z projektem konsolowym i rozpakuj przykładowy katalog do katalogu rozwiązania, **obok katalogu projektu**:

```bash
dotnet new sln -n Snapshot
dotnet new console -n Snapshot
dotnet sln add Snapshot/Snapshot.csproj
```

```text
Snapshot/              <- solution directory
├── Snapshot.slnx
├── Snapshot/          <- project directory, run all commands from here
│   └── Snapshot.csproj
└── sample/            <- the sample directory
```

Wszystkie polecenia w tym tutorialu uruchamiamy z katalogu projektu, więc przykładowy katalog jest dostępny jako `../sample`, a kopie zapasowe będą zapisywane w `../backups`. Argumenty programu podaje się po `--`, np. `dotnet run -- scan ../sample` (w Riderze lub Visual Studio ustaw je jako *Program arguments* w konfiguracji uruchomieniowej). W trakcie zadania będziesz modyfikować przykładowy katalog – gdy zechcesz zacząć od nowa, po prostu rozpakuj go ponownie.

> [!WARNING]
> Kopie zapasowe i odtworzone pliki nie powinny trafiać do repozytorium Git. Pamiętaj, aby dodać `backups/` oraz `restored/` do pliku `.gitignore`.

### Etap 1: Skanowanie drzewa katalogów

Zaczniemy od znalezienia wszystkich plików w katalogu źródłowym wraz z ich rozmiarami. Niektóre pliki nigdy nie powinny trafiać do kopii zapasowej – pliki tymczasowe, wyniki kompilacji, pamięci podręczne – dlatego będziemy pomijać pliki pasujące do *wzorców ignorowania*.

> [!IMPORTANT]
> #### `EnumerationOptions`
>
> Metody `Enumerate*` i `Get*` mają przeciążenia przyjmujące obiekt `EnumerationOptions`, który steruje przeszukiwaniem:
> ```csharp
> var options = new EnumerationOptions
> {
>     RecurseSubdirectories = true, // search the whole tree, not just the top directory
>     IgnoreInaccessible = true,    // skip entries we are not allowed to read (the default)
> };
> foreach (FileInfo file in new DirectoryInfo(path).EnumerateFiles("*", options)) { /* ... */ }
> ```
> Domyślnie `EnumerationOptions` pomija także pliki ukryte i systemowe (`AttributesToSkip`). Na Linuksie i macOS ukryte są pliki i katalogi, których nazwy zaczynają się od kropki. Zwróć uwagę, że starsze przeciążenia przyjmujące `SearchOption.AllDirectories` **nie** pomijają plików ukrytych.

#### Rekord FileEntry

Utwórz plik `FileEntry.cs` z rekordem opisującym pojedynczy plik:

```csharp
public sealed record FileEntry(string RelativePath, string FullPath, long Size, DateTime LastWriteTime);
```

`RelativePath` to ścieżka względem katalogu źródłowego. Będziemy ją zapisywać w kopiach zapasowych, więc musi wyglądać tak samo w każdym systemie operacyjnym – zawsze używaj `/` jako separatora (np. `docs/guide.md`).

#### Klasa DirectoryScanner

Utwórz klasę `DirectoryScanner` (plik `DirectoryScanner.cs`) z konstruktorem głównym `DirectoryScanner(string rootDirectory, IReadOnlyList<string> ignorePatterns)`. Klasa powinna zawierać:

1. Właściwość tylko do odczytu `string RootDirectory`, zainicjalizowaną pełną ścieżką `rootDirectory` (`Path.GetFullPath`).
2. Metodę `string ToRelativePath(string fullPath)`, zamieniającą pełną ścieżkę na ścieżkę względem `RootDirectory` z separatorem `/`. Użyj `Path.GetRelativePath` i zamień `Path.DirectorySeparatorChar` na `'/'` (na Linuksie nic to nie zmienia).
3. Metodę `bool IsIgnored(string relativePath)`, zwracającą `true`, jeśli **dowolny segment** ścieżki (nazwa katalogu lub pliku) pasuje do **dowolnego** wzorca ignorowania. Dzięki temu wzorzec `cache` pomija cały katalog `cache/`, a `*.bak` pomija `docs/guide.md.bak`. Do dopasowania nazwy do wzorca z symbolami wieloznacznymi `*` i `?` użyj `FileSystemName.MatchesSimpleExpression(pattern, name)` z przestrzeni nazw `System.IO.Enumeration`.
4. Metodę `IEnumerable<FileEntry> Scan()`, leniwie przeglądającą wszystkie pliki w drzewie. Użyj `DirectoryInfo.EnumerateFiles` z opcjami pokazanymi powyżej i zwracaj nieignorowane pliki za pomocą `yield return`. `FileInfo` udostępnia rozmiar (`Length`) i czas ostatniej modyfikacji (`LastWriteTime`).

> [!TIP]
> `Scan()` jest metodą iteratora, więc jej ciało – łącznie z wywołaniem `EnumerateFiles` – wykona się dopiero wtedy, gdy ktoś zacznie iterować po wyniku. Błąd taki jak brak katalogu pojawi się w pętli `foreach`, a nie przy wywołaniu `Scan()`. Zauważ też, że `IsIgnored` odfiltrowuje pliki dopiero po ich znalezieniu: przeszukiwanie i tak przechodzi przez ignorowane katalogi. Dla ogromnych katalogów (np. `node_modules`) warto napisać rekurencyjne przeszukiwanie, które w ogóle do nich nie wchodzi.

#### Czytelne rozmiary

W pliku `SizeFormatting.cs` utwórz klasę statyczną z metodą rozszerzającą `string ToHumanSize(this long bytes)`. Dziel rozmiar przez 1024, dopóki jest on nie mniejszy niż 1024, wybierając jednostkę spośród `B`, `KB`, `MB`, `GB`, `TB`. Bajty formatuj jako liczbę całkowitą (`749 B`), a pozostałe jednostki z jednym miejscem po przecinku (`52.7 KB`). Użyj `CultureInfo.InvariantCulture`, aby separatorem dziesiętnym zawsze była kropka, niezależnie od ustawień systemu:

```csharp
string.Create(CultureInfo.InvariantCulture, $"{size:0.0} {Units[unit]}")
```

#### Polecenie `scan`

Do rozpoznawania poleceń w `Program.cs` użyjemy **wzorców listy** (list patterns). W metodzie `Main`:

1. Zdefiniuj wzorce ignorowania: `private static readonly string[] IgnorePatterns = ["*.tmp", "*.bak", "bin", "obj", "cache"];`.
2. Dopasuj argumenty instrukcją `switch` i wywołaj metodę realizującą polecenie. Dla nieznanych argumentów wypisz instrukcję użycia i zwróć `1`:
   ```csharp
   switch (args)
   {
       case ["scan", var source]:
           return Scan(source);
       default:
           PrintUsage();
           return 1;
   }
   ```
3. Umieść instrukcję `switch` w bloku `try`/`catch`, który wypisuje komunikat wyjątku `IOException` lub `UnauthorizedAccessException` (`catch (Exception e) when (e is IOException or UnauthorizedAccessException)`) i zwraca `1`.

Zaimplementuj metodę `Scan(string source)`:

1. Jeśli katalog nie istnieje (`Directory.Exists`), wypisz błąd i zwróć `1`.
2. Przeskanuj katalog i posortuj pliki według `RelativePath` za pomocą `StringComparer.Ordinal`. Kolejność przeglądania zależy od systemu plików, więc bez sortowania wynik mógłby się różnić między komputerami.
3. Wypisz każdy plik wraz z rozmiarem, następnie liczbę plików i ich łączny rozmiar (`Sum`), a na końcu trzy największe pliki (`OrderByDescending`, `Take`).

```bash
dotnet run -- scan ../sample
```

```text
Scanning ../sample
     749 B  README.md
     180 B  data/config.json
   52.7 KB  data/measurements.csv
     504 B  docs/changelog.txt
    2.4 KB  docs/guide.md
   42.7 KB  media/render.jpg
     658 B  scripts/report.py
7 files, 99.8 KB in total
Largest files:
   52.7 KB  data/measurements.csv
   42.7 KB  media/render.jpg
    2.4 KB  docs/guide.md
```

Ignorowane pliki `cache/thumbnails.db` oraz `docs/guide.md.bak` nie pojawiają się na liście.

> [!WARNING]
> Nigdy nie buduj ścieżek, sklejając napisy z wpisanym na sztywno separatorem (`dir + "/" + name`, `dir + "\\" + name`). Taki kod działa tylko w jednym systemie operacyjnym. Używaj `Path.Combine`, `Path.GetRelativePath` i pozostałych metod klasy `Path`.

> [!NOTE]
> #### Etap 1
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.01" >}}

### Etap 2: Skróty plików i manifest

Aby stwierdzić, czy plik zmienił się od poprzedniej kopii zapasowej, będziemy porównywać **skróty** (ang. *hash*) jego zawartości. SHA-256 przekształca dowolne dane w 32 bajty (64 cyfry szesnastkowe); zmiana choćby jednego bitu w pliku daje zupełnie inny skrót. Porównywanie czasów modyfikacji byłoby szybsze, ale nie jest niezawodne – zmieniają się one nawet wtedy, gdy zawartość pozostaje taka sama (np. plik zostanie ponownie zapisany bez zmian), a niektóre narzędzia przywracają je po zmianie pliku (np. przy rozpakowywaniu archiwum), więc zmiana mogłaby przejść niezauważona.

Plik może być większy niż dostępna pamięć, dlatego skrót obliczymy, czytając plik fragmentami, za pomocą klasycznej pętli odczytu ze strumienia.

> [!IMPORTANT]
> #### Odczyt strumienia fragmentami
>
> `Stream.Read(buffer, offset, count)` odczytuje **co najwyżej** `count` bajtów i zwraca liczbę faktycznie odczytanych bajtów – może ona być mniejsza od żądanej, nawet przed końcem strumienia. Koniec strumienia jest sygnalizowany zwróceniem wartości `0`. Dlatego zawsze przetwarzaj dokładnie `read` bajtów:
> ```csharp
> byte[] buffer = new byte[64 * 1024];
> int read;
> while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
> {
>     // process buffer[0..read]
> }
> ```

#### FileHasher

W pliku `FileHasher.cs` utwórz klasę statyczną z metodą `string ComputeSha256(string path)`:

1. Otwórz plik za pomocą `File.OpenRead` w deklaracji `using`.
2. Utwórz obiekt `IncrementalHash` za pomocą `IncrementalHash.CreateHash(HashAlgorithmName.SHA256)` (przestrzeń nazw `System.Security.Cryptography`). On również implementuje `IDisposable`.
3. W pętli odczytu przekazuj każdy fragment do `AppendData(buffer, 0, read)`.
4. Pobierz skrót metodą `GetHashAndReset()` i zamień go na napis z małymi cyframi szesnastkowymi za pomocą `Convert.ToHexStringLower`.

> [!TIP]
> Pętla jest tu pisana ręcznie, aby przećwiczyć pracę ze strumieniami. W praktyce `SHA256.HashData(stream)` robi to samo w jednej linijce – również czyta strumień fragmentami. Wyniki możesz sprawdzić narzędziem `sha256sum` na Linuksie lub `Get-FileHash` w PowerShellu.

#### Zmiany

W pliku `FileChange.cs` zdefiniuj:

1. Typ wyliczeniowy `ChangeKind` z wartościami `Unchanged`, `New`, `Modified`, `Deleted`.
2. Rekord `FileChange(string Path, ChangeKind Kind)`.
3. Klasę statyczną `ChangeKindExtensions` z metodą rozszerzającą `char ToSymbol(this ChangeKind kind)`, zwracającą `'+'` dla nowych, `'~'` dla zmodyfikowanych, `'-'` dla usuniętych i spację dla niezmienionych plików (użyj wyrażenia `switch`).

#### Manifest

Manifest to plik tekstowy z wierszem nagłówka, po którym następuje po jednym wierszu na plik: skrót, rozmiar i ścieżka względna, oddzielone spacjami. Ścieżka jest ostatnim polem, więc sama może zawierać spacje:

```text
# Snapshot manifest v1
8feb8de0381868e97c0ce5a1b3b6ffa68bd4869cea347a2037cca15929543e72 749 README.md
0b5bac44713986cbedf46cc217fe929e591975d1a336ba5fe5b7a80a104929aa 180 data/config.json
754fd7ae82bde0fb6657986fcad1f2b10a16540553ce281dabff0cebe3a06156 53990 data/measurements.csv
...
```

> [!IMPORTANT]
> #### Zapis do strumienia i odczyt ze strumienia, a nie z pliku
>
> Teraz manifest będzie zapisywany w zwykłym pliku, a w Etapie 4 – wewnątrz archiwum ZIP. Jeśli metody zapisujące i odczytujące manifest przyjmą `Stream` zamiast ścieżki pliku, ten sam kod zadziała w obu przypadkach. Adaptery tekstu `StreamWriter` i `StreamReader` domyślnie **zamykają opakowany strumień** w momencie zwolnienia. Metoda, która jedynie pożycza strumień od wywołującego, nie powinna go zamykać – przekaż `leaveOpen: true`:
> ```csharp
> using var writer = new StreamWriter(stream, leaveOpen: true);
> ```

W pliku `Manifest.cs` zdefiniuj rekord `ManifestEntry(string Path, long Size, string Hash)` oraz klasę `Manifest`:

1. Prywatne pole `SortedDictionary<string, ManifestEntry>`, odwzorowujące ścieżki na wpisy, utworzone z `StringComparer.Ordinal` (wpisy są przechowywane posortowane według ścieżki).
2. Właściwości `IReadOnlyCollection<ManifestEntry> Entries`, `int Count`, `long TotalSize` oraz metody `void Add(ManifestEntry entry)` i `bool Contains(string path)`.
3. Metodę `ChangeKind Classify(ManifestEntry current)`, która porównuje wpis opisujący **bieżący** stan pliku z **tym** (poprzednim) manifestem: `New`, jeśli ścieżki nie ma w manifeście, `Unchanged`, jeśli skróty są równe, `Modified` w przeciwnym razie.
4. Metodę `void WriteTo(Stream stream)`, zapisującą nagłówek i wpisy za pomocą `StreamWriter`.
5. Metodę statyczną `Manifest ReadFrom(Stream stream)`, odczytującą wiersze za pomocą `StreamReader` (`while (reader.ReadLine() is { } line)`). Pomijaj puste wiersze i wiersze zaczynające się od `#`, pozostałe dziel za pomocą `line.Split(' ', 3)` i rzuć `InvalidDataException`, jeśli wiersz ma niepoprawny format.
6. Pomocnicze metody `void Save(string path)` i `static Manifest Load(string path)`, które otwierają plik (`File.Create`, `File.OpenRead`) i wywołują `WriteTo`/`ReadFrom`.

> [!WARNING]
> `StreamWriter` gromadzi tekst w buforze i zapisuje go do strumienia większymi porcjami. Jeśli obiekt zapisujący nie zostanie zwolniony (lub opróżniony metodą `Flush()`), końcówka danych nigdy nie trafi do pliku – możesz skończyć z pustym manifestem. Deklaracja `using` załatwia to za Ciebie.

#### Gdzie przechowywać kopie zapasowe

Kopie zapasowe będą przechowywane w osobnym katalogu – **repozytorium**. Nie może się ono znajdować wewnątrz katalogu źródłowego – w przeciwnym razie każda kopia zapasowa obejmowałaby również poprzednie kopie. W pliku `PathUtils.cs` utwórz statyczną metodę `bool IsInside(string path, string directory)`, która zwraca `true`, jeśli `path` jest samym katalogiem `directory` lub znajduje się wewnątrz niego. Oblicz `Path.GetRelativePath` między obiema pełnymi ścieżkami: ścieżka leży na zewnątrz, jeśli wynikiem jest `..`, wynik zaczyna się od `..` i separatora katalogów albo jest ścieżką bezwzględną (`Path.IsPathRooted` – np. inny dysk na Windowsie).

#### Polecenie `backup`

Dodaj do `Program.cs` przypadek `case ["backup", var source, var repository]` i zaimplementuj metodę `Backup`:

1. Sprawdź, czy katalog źródłowy istnieje i czy repozytorium nie znajduje się wewnątrz niego (`PathUtils.IsInside`).
2. Wczytaj poprzedni manifest z `<repository>/latest.manifest`, o ile istnieje.
3. Dla każdego znalezionego pliku (posortowanego według ścieżki) oblicz jego skrót, dodaj `ManifestEntry` do bieżącego manifestu i sklasyfikuj go na podstawie poprzedniego manifestu (lub jako `New`, jeśli poprzedniego manifestu nie ma).
4. Pliki obecne w poprzednim manifeście, a brakujące w bieżącym, dodaj jako `Deleted`.
5. Wypisz wszystkie zmiany z pominięciem niezmienionych plików oraz liczbę plików każdego rodzaju.
6. Utwórz katalog repozytorium (`Directory.CreateDirectory` nic nie robi, jeśli katalog już istnieje) i zapisz bieżący manifest jako `latest.manifest`, aby stał się podstawą następnej kopii.

Wykonaj pierwszą kopię zapasową:

```bash
dotnet run -- backup ../sample ../backups
```

```text
No previous manifest - all files are new.
+ README.md
+ data/config.json
+ data/measurements.csv
+ docs/changelog.txt
+ docs/guide.md
+ media/render.jpg
+ scripts/report.py
New: 7, modified: 0, deleted: 0, unchanged: 0
Manifest saved to ../backups/latest.manifest
```

Teraz zmodyfikuj przykładowy katalog – poniższe polecenia działają zarówno w Bashu, jak i w PowerShellu (możesz też po prostu zmienić pliki w edytorze):

```bash
echo "Clean the sensors before winter." >> ../sample/docs/guide.md
echo "# FAQ" > ../sample/docs/faq.md
rm ../sample/docs/changelog.txt
```

Kolejna kopia zapasowa wykrywa dokładnie te zmiany (jej ponowne uruchomienie zgłasza `New: 0, modified: 0, deleted: 0, unchanged: 7`):

```text
Previous manifest: 7 files, 99.8 KB
+ docs/faq.md
~ docs/guide.md
- docs/changelog.txt
New: 1, modified: 1, deleted: 1, unchanged: 5
Manifest saved to ../backups/latest.manifest
```

> [!NOTE]
> #### Etap 2
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.02" >}}

### Etap 3: Silnik kopii zapasowych ze zdarzeniami

Obecnie cała logika kopii zapasowej znajduje się w `Program.cs` i pisze bezpośrednio na konsolę. W tym etapie przeniesiemy ją do klasy `BackupEngine`, która niczego nie wypisuje – zamiast tego **wywołuje zdarzenia** opisujące, co się dzieje. Może je zasubskrybować każdy zainteresowany: konsolowy pasek postępu, logger, okno aplikacji graficznej albo (w Etapie 5) automatyczny tryb obserwowania. Silnik nie musi nic wiedzieć o swoich subskrybentach.

> [!IMPORTANT]
> #### Deklarowanie i wywoływanie zdarzeń
>
> Standardowy wzorzec zdarzenia w .NET składa się z:
> - klasy dziedziczącej po `EventArgs`, przenoszącej dane zdarzenia (np. `FileProcessedEventArgs`),
> - zdarzenia typu `EventHandler<TEventArgs>`, zadeklarowanego jako dopuszczające `null` – ma ono wartość `null`, gdy nie ma subskrybentów,
> - metody `protected virtual void OnEventName(TEventArgs e)`, która wywołuje zdarzenie. Zdarzenie jest wywoływane wyłącznie przez tę metodę, dzięki czemu klasy pochodne mogą ją nadpisać, aby zareagować na zdarzenie (lub je wstrzymać).
> ```csharp
> public event EventHandler<FileProcessedEventArgs>? FileProcessed;
>
> protected virtual void OnFileProcessed(FileProcessedEventArgs e) => FileProcessed?.Invoke(this, e);
> ```
> Wywołanie zdarzenia uruchamia wszystkich subskrybentów **synchronicznie**, jednego po drugim, w wątku, który je wywołał. Gdy `Invoke` zwraca sterowanie, wszystkie procedury obsługi zdarzenia już się zakończyły.

#### Dane zdarzeń

W pliku `BackupEventArgs.cs` utwórz poniższe klasy. Ich właściwości powinny być tylko do odczytu (inicjalizowane z konstruktora głównego):

1. `FileFoundEventArgs` z właściwością `FileEntry File`. Dziedziczy po `CancelEventArgs` (przestrzeń nazw `System.ComponentModel`, patrz niżej).
2. `FileProcessedEventArgs : EventArgs` z właściwościami `FileEntry File` i `ChangeKind Kind`.
3. `BackupProgressEventArgs : EventArgs` z właściwościami `int ProcessedFiles`, `int TotalFiles`, `long ProcessedBytes`, `long TotalBytes` oraz obliczaną właściwością `double Fraction` – postępem od 0 do 1 liczonym na podstawie bajtów (zwróć `1.0`, jeśli `TotalBytes` wynosi `0`).
4. `BackupCompletedEventArgs : EventArgs` z właściwością `BackupSummary Summary`.

W pliku `BackupSummary.cs` utwórz klasę `BackupSummary` z właściwością `IReadOnlyList<FileChange> Changes`, właściwościami `New`, `Modified`, `Deleted`, `Unchanged` zliczającymi zmiany każdego rodzaju oraz nadpisaną metodą `ToString()`, zwracającą wiersz `New: 1, modified: 1, deleted: 1, unchanged: 5`.

> [!IMPORTANT]
> #### Zdarzenia z możliwością anulowania
>
> Argumenty zdarzenia są obiektami, więc subskrybent może za ich pomocą przekazać informację z powrotem do wydawcy. Klasa `CancelEventArgs` z biblioteki standardowej zawiera właściwość `bool Cancel` właśnie w tym celu: wydawca wywołuje zdarzenie, a następnie sprawdza, czy któryś z subskrybentów nie anulował operacji:
> ```csharp
> var args = new FileFoundEventArgs(file);
> OnFileFound(args);  // all subscribers run here
> if (!args.Cancel)
> {
>     files.Add(file);
> }
> ```
> Gdy zdarzenie ma kilku subskrybentów, wszyscy otrzymują ten sam obiekt argumentów. Subskrybent powinien więc jedynie ustawiać `Cancel = true`, a nigdy nie przywracać wartości `false` – unieważniłby w ten sposób decyzję subskrybentów wywołanych przed nim.

#### Klasa BackupEngine

Utwórz klasę `BackupEngine` (plik `BackupEngine.cs`) z konstruktorem głównym `BackupEngine(string sourceDirectory, string repositoryDirectory, IReadOnlyList<string> ignorePatterns)`, właściwościami `SourceDirectory` i `RepositoryDirectory` oraz czterema zdarzeniami:

- `EventHandler<FileFoundEventArgs>? FileFound` – wywoływane dla każdego znalezionego pliku, przed jego przetworzeniem; subskrybent może wykluczyć plik z kopii zapasowej,
- `EventHandler<FileProcessedEventArgs>? FileProcessed` – wywoływane po obliczeniu skrótu pliku i jego sklasyfikowaniu,
- `EventHandler<BackupProgressEventArgs>? ProgressChanged` – wywoływane po każdym przetworzonym pliku,
- `EventHandler<BackupCompletedEventArgs>? Completed` – wywoływane na końcu kopii zapasowej.

Dla każdego zdarzenia dodaj metodę `protected virtual` – `OnFileFound`, `OnFileProcessed`, `OnProgressChanged`, `OnCompleted` – która je wywołuje.

Przenieś logikę kopii zapasowej z `Program.Backup` do metody `BackupSummary Run()`:

1. Przeskanuj katalog źródłowy i posortuj pliki według ścieżki. Dla każdego pliku wywołaj `FileFound` i dodaj plik do listy, o ile żaden subskrybent go nie anulował. Lista jest potrzebna, ponieważ do raportowania postępu trzeba znać łączny rozmiar wszystkich plików, zanim zaczniemy obliczać skróty.
2. Dla każdego pliku z listy: oblicz skrót, dodaj wpis do bieżącego manifestu, sklasyfikuj go, wywołaj `FileProcessed`, a następnie `ProgressChanged` ze zaktualizowanymi licznikami.
3. Dodaj usunięte pliki, zapisz manifest (jak w Etapie 2), utwórz `BackupSummary`, wywołaj `Completed` i zwróć podsumowanie.

#### Konsolowy pasek postępu

Utwórz klasę `ConsoleReporter` (plik `ConsoleReporter.cs`), która wyświetla postęp kopii zapasowej. Implementuje ona `IDisposable`:

1. Konstruktor przyjmuje `BackupEngine` i **subskrybuje** zdarzenia `FileProcessed`, `ProgressChanged` i `Completed` prywatnymi metodami obsługi.
2. `Dispose()` **anuluje** te subskrypcje (`-=`). Dzięki temu `using var reporter = new ConsoleReporter(engine);` ogranicza subskrypcję do zakresu.
3. Obsługa `FileProcessed` wypisuje zmienione pliki (np. `+ docs/faq.md`) i pomija niezmienione.
4. Obsługa `ProgressChanged` rysuje pasek postępu, np. `[###############...............]  50% (3/6 files, 1.4 KB)`. Aby rysować go w miejscu, zacznij tekst od `'\r'` (powrót karetki – przenosi kursor na początek wiersza) i użyj `Console.Write` zamiast `WriteLine`. Uzupełnij wiersz spacjami (`PadRight`), aby krótszy tekst całkowicie przykrył poprzedni.
5. Ponieważ pasek postępu pozostaje w bieżącym wierszu, obsługa `FileProcessed` również powinna zaczynać komunikat od `'\r'` i uzupełniać go spacjami – komunikat nadpisuje pasek, który zostanie narysowany ponownie przy następnym zdarzeniu `ProgressChanged`.
6. Obsługa `Completed` kończy wiersz paska postępu, wypisuje usunięte pliki i podsumowanie.

> [!TIP]
> Zdarzenie przechowuje referencje do wszystkich swoich subskrybentów. Jeśli obiekt krótko żyjący zasubskrybuje zdarzenie obiektu długo żyjącego i nigdy nie anuluje subskrypcji, nie zostanie usunięty przez Garbage Collector, dopóki żyje wydawca – to jedna z najczęstszych przyczyn wycieków pamięci w .NET. Zauważ też, że subskrypcję lambdy można anulować tylko wtedy, gdy przechowujesz referencję do niej w zmiennej – ponowne napisanie tej samej lambdy po `-=` tworzy inny delegat.

#### Użycie silnika

W `Program.cs` przepisz metodę `Backup` tak, aby tworzyła `BackupEngine` i `ConsoleReporter` oraz wywoływała `Run()`. Następnie dodaj obsługę opcjonalnego czwartego argumentu – maksymalnego rozmiaru pliku w kilobajtach:

```csharp
case ["backup", var source, var repository, var maxSize] when long.TryParse(maxSize, out long maxSizeKb):
    return Backup(source, repository, maxSizeKb);
```

Jeśli limit został podany, zasubskrybuj zdarzenie `FileFound` lambdą, która anuluje pliki większe niż limit i wypisuje, które pliki zostały pominięte. Pominięty plik jest traktowany dokładnie tak jak ignorowany – w ogóle nie jest częścią kopii zapasowej.

Usuń katalog `../backups`, rozpakuj świeżą kopię przykładowego katalogu i wykonaj kopię zapasową z limitem 50 KB:

```text
Skipping data/measurements.csv (52.7 KB > 50 KB)
+ README.md
+ data/config.json
+ docs/changelog.txt
+ docs/guide.md
+ media/render.jpg
+ scripts/report.py
[##############################] 100% (6/6 files, 47.1 KB)
New: 6, modified: 0, deleted: 0, unchanged: 0
```

Bez limitu pominięty wcześniej plik trafia do kopii jako nowy (`New: 1, modified: 0, deleted: 0, unchanged: 6`). Po zmodyfikowaniu przykładowego katalogu tak jak w Etapie 2 wynik wygląda następująco:

```text
+ docs/faq.md
~ docs/guide.md
[##############################] 100% (7/7 files, 99.4 KB)
- docs/changelog.txt
New: 1, modified: 1, deleted: 1, unchanged: 5
```

> [!NOTE]
> #### Etap 3
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.03" >}}

### Etap 4: Migawki ZIP i odtwarzanie plików

Do tej pory "kopia zapasowa" składa się wyłącznie z manifestu. W tym etapie każda kopia utworzy **migawkę** – archiwum ZIP nazwane według czasu UTC wykonania kopii (`snapshot-20261105-101530.zip`), zawierające:

- `manifest.txt` – manifest **wszystkich** plików w chwili wykonania kopii,
- `files/...` – tylko pliki **nowe lub zmodyfikowane** od poprzedniej migawki.

```text
backups/
├── snapshot-20261105-101530.zip   <- manifest.txt + files/ (all 7 files)
└── snapshot-20261105-102012.zip   <- manifest.txt + files/docs/faq.md, files/docs/guide.md
```

Manifest najnowszej migawki staje się podstawą następnej kopii, więc plik `latest.manifest` nie jest już potrzebny (możesz go usunąć).

> [!IMPORTANT]
> #### Archiwa ZIP
>
> `ZipFile.Open(path, mode)` (przestrzeń nazw `System.IO.Compression`) otwiera archiwum jako obiekt `ZipArchive` w jednym z trzech trybów: `Read`, `Create`, `Update`. `ZipFile.OpenRead(path)` to skrót dla trybu `Read`. `ZipArchive` implementuje `IDisposable` – dopiero w momencie zwolnienia archiwum zostaje dokończone (zapisywany jest m.in. spis jego zawartości) i zamknięte.
> - `archive.CreateEntry(name, CompressionLevel.Optimal)` dodaje nowy wpis (nazwa może zawierać `/`, co tworzy katalogi wewnątrz archiwum), a `entry.Open()` zwraca strumień do zapisu jego zawartości.
> - `archive.Entries` i `archive.GetEntry(name)` dają dostęp do istniejących wpisów; `entry.Open()` zwraca strumień do odczytu, a metoda rozszerzająca `entry.ExtractToFile(path, overwrite)` wypakowuje wpis do pliku.
>
> W trybie `Create` w danej chwili może być otwarty tylko jeden wpis – zwolnij strumień wpisu, zanim utworzysz następny. Więcej szczegółów znajdziesz w wykładzie [Archiwa]({{< ref "/lectures/07.filesystem/archives" >}}).

> [!IMPORTANT]
> #### Dekoratory strumieni
>
> Strumień może opakowywać inny strumień i dodawać mu nową funkcjonalność: buforowanie (`BufferedStream`), kompresję (`GZipStream`, `DeflateStream`, `BrotliStream`), szyfrowanie (`CryptoStream`). Taki **dekorator** sam jest strumieniem (`Stream`), więc dekoratory można łączyć w łańcuchy, a na końcu łańcucha zazwyczaj ląduje adapter (`StreamReader`, `StreamWriter`):
> ```csharp
> // Writing: StreamWriter -> GZipStream -> FileStream
> using (FileStream file = File.Create("notes.txt.gz"))
> using (var gzip = new GZipStream(file, CompressionLevel.Optimal))
> using (var writer = new StreamWriter(gzip))
> {
>     writer.WriteLine("Hello, compressed world!");
> } // disposed in reverse order: writer (flushes text), gzip (writes the footer), file (closes the handle)
>
> // Reading: disposing the adapter disposes the whole chain
> using var reader = new StreamReader(new GZipStream(File.OpenRead("notes.txt.gz"), CompressionMode.Decompress));
> Console.WriteLine(reader.ReadToEnd()); // Hello, compressed world!
> ```
> Strumień zwracany przez `ZipArchiveEntry.Open()` działa tak samo – kompresuje zapisywane do niego dane (lub dekompresuje odczytywane) w locie. Ponieważ `Manifest.WriteTo` i `Manifest.ReadFrom` przyjmują dowolny `Stream`, działają na nim bez żadnych zmian.

#### Klasa SnapshotArchive

W pliku `SnapshotArchive.cs` utwórz klasę statyczną `SnapshotArchive` ze stałymi `ManifestEntryName = "manifest.txt"` i `FilesPrefix = "files/"` oraz następującymi metodami:

1. `List<string> List(string repository)` – zwraca ścieżki wszystkich plików `snapshot-*.zip` w repozytorium, posortowane według nazwy za pomocą `StringComparer.Ordinal` (nazwy zawierają czas UTC, więc w ten sposób sortujemy je od najstarszej do najnowszej). Jeśli repozytorium nie istnieje, zwróć pustą listę.
2. `string Create(string repository, Manifest manifest, IEnumerable<FileEntry> filesToStore)` – tworzy katalog repozytorium i nowe archiwum `snapshot-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip` za pomocą `ZipFile.Open(path, ZipArchiveMode.Create)` (ten tryb rzuca wyjątek, jeśli plik już istnieje, więc starsza migawka nigdy nie zostanie nadpisana). Dla każdego pliku utwórz wpis `files/<ścieżka względna>`, ustaw jego `LastWriteTime` i skopiuj zawartość pliku do strumienia wpisu metodą `CopyTo`. Na koniec utwórz wpis `manifest.txt` i zapisz manifest do jego strumienia. Zwróć ścieżkę archiwum.
3. `Manifest ReadManifest(string snapshotPath)` – otwiera archiwum do odczytu, znajduje wpis `manifest.txt` (jeśli go brakuje, rzuć `InvalidDataException`) i odczytuje manifest z jego strumienia.

Kluczowy fragment metody `Create`:

```csharp
foreach (FileEntry file in filesToStore)
{
    ZipArchiveEntry entry = archive.CreateEntry(FilesPrefix + file.RelativePath, CompressionLevel.Optimal);
    entry.LastWriteTime = file.LastWriteTime;

    // Only one entry can be open at a time, so the entry stream is disposed
    // at the end of each iteration, before the next entry is created
    using Stream entryStream = entry.Open();
    using FileStream source = File.OpenRead(file.FullPath);
    source.CopyTo(entryStream);
}
```

#### Aktualizacja silnika

Zmodyfikuj metodę `BackupEngine.Run`:

1. Pobieraj poprzedni manifest z najnowszej migawki (`SnapshotArchive.List(...).LastOrDefault()`, `SnapshotArchive.ReadManifest`) zamiast z `latest.manifest`.
2. Podczas przetwarzania plików zbieraj nowe i zmodyfikowane pliki w osobnej liście.
3. Zamiast zapisywać `latest.manifest`, utwórz migawkę za pomocą `SnapshotArchive.Create` – ale tylko wtedy, gdy wystąpiła co najmniej jedna zmiana (nowy, zmodyfikowany lub usunięty plik). Kopia, w której tylko usunięto pliki, tworzy migawkę zawierającą sam manifest.
4. Dodaj do `BackupSummary` właściwość `string? SnapshotPath` (przekazywaną do konstruktora; `null`, jeśli migawka nie została utworzona). W `ConsoleReporter` wypisz ścieżkę i rozmiar utworzonej migawki albo komunikat `No changes - snapshot not created.`.

#### Odtwarzanie plików

Odtworzenie stanu z chwili wykonania migawki *N* wymaga wszystkich migawek do *N* włącznie: pierwsza zawiera wszystkie pliki, a każda kolejna – nowsze wersje zmienionych plików. W klasie `SnapshotArchive` dodaj rekord `RestoreResult(string SnapshotName, int FileCount, long TotalSize, int ChecksumErrors)` i metodę:

```csharp
public static RestoreResult Restore(string repository, string target, string? snapshotName,
    Action<string>? log = null)
```

1. Znajdź indeks wybranej migawki – po nazwie bez rozszerzenia (`Path.GetFileNameWithoutExtension`) lub najnowszej, jeśli `snapshotName` ma wartość `null`. Jeśli nie ma żadnych migawek lub nie znaleziono podanej nazwy, rzuć `FileNotFoundException`.
2. Wymagaj, aby katalog docelowy był pusty lub nie istniał (w przeciwnym razie rzuć `IOException`), i utwórz go.
3. Otwieraj migawki od najstarszej do wybranej. Wypakuj każdy wpis zaczynający się od `files/` do katalogu docelowego (`entry.ExtractToFile(destination, overwrite: true)`), tworząc wcześniej brakujące katalogi. Nowsze wersje plików nadpisują starsze. Po każdej migawce przekaż jej nazwę i liczbę wypakowanych plików przez wywołanie zwrotne `log`.
4. Odczytaj manifest wybranej migawki i usuń z katalogu docelowego wszystkie pliki, których nie ma w manifeście – zostały one usunięte przed wykonaniem tej migawki.
5. Zweryfikuj odtworzone pliki: oblicz skrót każdego pliku z manifestu i policz pliki, których brakuje lub które mają inny skrót.

> [!WARNING]
> #### Zip Slip
> Nigdy nie ufaj ścieżkom zapisanym w archiwum. Specjalnie przygotowane archiwum może zawierać wpis o nazwie `files/../../.bashrc` – naiwne wypakowanie go nadpisałoby plik poza katalogiem docelowym. Przed wypakowaniem wpisu oblicz jego pełną ścieżkę (`Path.GetFullPath`) i sprawdź, czy leży ona wewnątrz katalogu docelowego – dokładnie to robi `PathUtils.IsInside` z Etapu 2. Jeśli nie, rzuć `InvalidDataException`.

> [!WARNING]
> W kroku 4 usuwasz pliki podczas iterowania po katalogu. Nie modyfikuj katalogu, który jest właśnie leniwie przeglądany za pomocą `EnumerateFiles` – wynik jest nieprzewidywalny. Użyj `Directory.GetFiles`, która wczytuje wszystkie elementy do tablicy, zanim rozpocznie się pętla.

W `Program.cs` dodaj polecenie `restore` z opcjonalną nazwą migawki i dodaj `InvalidDataException` do filtra wyjątków w `Main`:

```csharp
case ["restore", var repository, var target]:
    return Restore(repository, target, snapshotName: null);
case ["restore", var repository, var target, var snapshotName]:
    return Restore(repository, target, snapshotName);
```

Metoda `Restore` przekazuje jako wywołanie zwrotne `log` lambdę wypisującą komunikaty, a następnie wypisuje wynik.

> [!TIP]
> Dlaczego `log` jest parametrem typu delegatu, a nie zdarzeniem? Zdarzenie jest przeznaczone dla **dowolnej liczby** niezależnych subskrybentów, o których wydawca nic nie wie, i żyje tak długo jak obiekt. Tutaj operacja odtwarzania ma dokładnie jednego wywołującego, który chce otrzymywać powiadomienia tylko podczas tego jednego wywołania – parametr z wywołaniem zwrotnym (`Action<string>`) jest prostszy.

Zacznij od świeżej kopii przykładowego katalogu i pustego repozytorium (usuń `../backups`). Wykonaj kopię zapasową – pierwsza migawka zawiera wszystkie 7 plików (`Snapshot created: ../backups/snapshot-20261105-101530.zip (52.5 KB)`). Następnie zmodyfikuj przykładowy katalog tak jak w Etapie 2 i ponownie wykonaj kopię (nazwy migawek na Twoim komputerze będą inne):

```text
+ docs/faq.md
~ docs/guide.md
[##############################] 100% (7/7 files, 99.4 KB)
- docs/changelog.txt
New: 1, modified: 1, deleted: 1, unchanged: 5
Snapshot created: ../backups/snapshot-20261105-102012.zip (1.8 KB)
```

Teraz odtwórz najnowszy stan oraz stan z pierwszej migawki:

```bash
dotnet run -- restore ../backups ../restored
dotnet run -- restore ../backups ../restored-old snapshot-20261105-101530
```

```text
  Applied snapshot-20261105-101530.zip (7 files)
  Applied snapshot-20261105-102012.zip (2 files)
  Removed docs/changelog.txt
Restored snapshot-20261105-102012: 7 files, 99.4 KB, all checksums OK.
```

```text
  Applied snapshot-20261105-101530.zip (7 files)
Restored snapshot-20261105-101530: 7 files, 99.8 KB, all checksums OK.
```

Otwórz migawki w dowolnym menedżerze archiwów i sprawdź ich zawartość. Rozmiary archiwów mogą się nieznacznie różnić w zależności od wersji biblioteki kompresującej.

> [!NOTE]
> #### Etap 4
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.04" >}}

### Etap 5: Obserwowanie zmian

W ostatnim etapie dodamy polecenie `watch`, które automatycznie wykonuje kopię zapasową za każdym razem, gdy coś zmieni się w katalogu źródłowym.

> [!IMPORTANT]
> #### FileSystemWatcher
>
> Klasa `FileSystemWatcher` korzysta z mechanizmów systemu operacyjnego (np. `inotify` na Linuksie), aby wywoływać zdarzenia przy zmianach plików w katalogu: `Created`, `Changed`, `Deleted`, `Renamed` (z argumentami `RenamedEventArgs`, zawierającymi również starą nazwę) oraz `Error`. Najważniejsze właściwości:
> - `Path`, `Filter` – który katalog i które pliki obserwować,
> - `IncludeSubdirectories` – czy obserwować całe drzewo,
> - `NotifyFilter` – jakie rodzaje zmian wywołują zdarzenia (`FileName`, `DirectoryName`, `LastWrite`, `Size`, …),
> - `EnableRaisingEvents` – zdarzenia są wywoływane dopiero po ustawieniu tej właściwości na `true`.
>
> `FileSystemWatcher` implementuje `IDisposable` – jego zwolnienie kończy obserwację. `FileSystemEventArgs.Name` zawiera ścieżkę zmienionego pliku względem obserwowanego katalogu.

Pojedyncze "zapisz" w edytorze często generuje całą serię zdarzeń: plik jest obcinany, zapisywany w kilku porcjach, czasem zapisywany do pliku tymczasowego i przemianowywany. Wykonywanie kopii zapasowej po każdym z nich byłoby marnotrawstwem. Zamiast tego zastosujemy **debouncing**: każde zdarzenie (ponownie) uruchamia odliczanie, a kopia zapasowa zaczyna się dopiero, gdy odliczanie dojdzie do zera – czyli gdy przez pewien czas nic się nie zmieniło (tzw. *okres ciszy*, np. 2 sekundy):

```text
events:   x  x x x                 x   x
time:     ------------------------------------------------------->
                  |<-- 2 s -->|          |<-- 2 s -->|
                              backup                 backup
```

> [!IMPORTANT]
> #### Zdarzenia wywoływane w innych wątkach
>
> Do tej pory wszystkie zdarzenia były wywoływane synchronicznie, w wątku, który wywołał `Run()`. `FileSystemWatcher` wywołuje swoje zdarzenia w **wątku działającym w tle** (w wątku z puli wątków na Windowsie, w dedykowanym wątku obserwatora na Linuksie), a `System.Threading.Timer` wywołuje swoją metodę zwrotną w **wątku z puli wątków**, gdy odliczanie dobiegnie końca. Konsekwencje:
> - procedury obsługi mogą działać równocześnie z wątkiem głównym i ze sobą nawzajem,
> - wyjątek rzucony z takiej procedury nie zostanie przechwycony przez `try`/`catch` w `Main` – **kończy on cały proces**. W takich procedurach przechwytuj wyjątki samodzielnie.
>
> Gdy kilka wątków korzysta ze wspólnego stanu, chroni się go instrukcją `lock`. Od .NET 9 jako obiektu blokady używa się dedykowanego typu `System.Threading.Lock`. Kod wewnątrz `lock (obj) { ... }` może w danej chwili wykonywać tylko jeden wątek – to odpowiednik `std::lock_guard<std::mutex>` z C++. Wątki i synchronizację szczegółowo omówimy w dalszej części kursu.

#### Klasa DebouncedWatcher

Utwórz zapieczętowaną klasę `DebouncedWatcher` (plik `DebouncedWatcher.cs`) implementującą `IDisposable`:

1. Dwa zdarzenia: `EventHandler<FileSystemEventArgs>? ChangeDetected`, wywoływane przy każdej istotnej zmianie, oraz `EventHandler? ChangesSettled`, wywoływane, gdy okres ciszy upłynie bez zmian.
2. Konstruktor `DebouncedWatcher(string directory, TimeSpan quietPeriod, Func<string, bool> isIgnored)`. Delegat `isIgnored` decyduje, które ścieżki są nieistotne – przekażemy metodę `DirectoryScanner.IsIgnored`, aby np. zmiany plików `*.tmp` nie wywoływały kopii zapasowych.
3. W konstruktorze utwórz zatrzymany, jednorazowy timer: `new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite)`, oraz `FileSystemWatcher` obserwujący całe drzewo (`IncludeSubdirectories = true`) z `NotifyFilter` ustawionym na `FileName | DirectoryName | LastWrite | Size`. Zasubskrybuj zdarzenia `Created`, `Changed`, `Deleted` i `Renamed` jedną metodą `OnFileSystemChanged(object sender, FileSystemEventArgs e)`. Działa to również dla `Renamed`, mimo że jego delegat oczekuje `RenamedEventArgs` – metoda przyjmująca parametr ogólniejszego typu jest zgodna z delegatem (kontrawariancja).
4. Metodę `void Start()`, która ustawia `EnableRaisingEvents = true`.
5. `OnFileSystemChanged`: zamień `e.Name` na ścieżkę z separatorami `/`, zakończ metodę, jeśli ścieżka jest ignorowana, wywołaj `ChangeDetected` i uruchom ponownie odliczanie za pomocą `_timer.Change(_quietPeriod, Timeout.InfiniteTimeSpan)`.
6. Obsługę zdarzenia `Error` (wywoływanego np. wtedy, gdy wewnętrzny bufor obserwatora się przepełni i część zdarzeń zostanie utracona) – po prostu uruchamia ona ponownie odliczanie. Kopia zapasowa i tak skanuje cały katalog, więc utracone zdarzenia nie mają znaczenia.
7. `OnTimerElapsed(object? state)`: wywołuje `ChangesSettled`.
8. `Dispose()`: zwalnia obserwatora i timer (tylko raz).

Chroń timer i flagę `_disposed` jedną blokadą `Lock _lock`. Dzięki temu dwie kopie zapasowe nigdy nie wykonują się jednocześnie, wywołanie timera, które nadejdzie po `Dispose()`, nic nie robi, a `Dispose()` czeka na zakończenie trwającej kopii zapasowej:

```csharp
private void OnTimerElapsed(object? state)
{
    lock (_lock)
    {
        if (!_disposed)
        {
            ChangesSettled?.Invoke(this, EventArgs.Empty);
        }
    }
}
```

> [!TIP]
> Klasa jest zapieczętowana (`sealed`), więc nie potrzebuje metod `protected virtual OnX` – nikt nie może po niej dziedziczyć, a zdarzenia można wywoływać bezpośrednio.

#### Polecenie `watch`

Dodaj do `Program.cs` przypadek `case ["watch", var source, var repository]` i zaimplementuj metodę `Watch`:

1. Sprawdź katalogi, utwórz `BackupEngine` i `ConsoleReporter` (tak jak w `Backup`) i od razu wykonaj kopię zapasową, aby zaktualizować repozytorium.
2. Utwórz `DebouncedWatcher` z okresem ciszy 2 sekund, przekazując `IsIgnored` obiektu `DirectoryScanner` jako grupę metod: `new DebouncedWatcher(source, TimeSpan.FromSeconds(2), scanner.IsIgnored)`.
3. Zasubskrybuj zdarzenie `ChangeDetected` lambdą wypisującą rodzaj zmiany i ścieżkę (dla `RenamedEventArgs` wypisz starą i nową nazwę – użyj wzorca `is`).
4. Zasubskrybuj zdarzenie `ChangesSettled` lambdą, która wywołuje `engine.Run()` wewnątrz bloku `try`/`catch` i wypisuje błędy.
5. Uruchom obserwatora i poczekaj, aż użytkownik naciśnie Enter (`Console.ReadLine()`). Deklaracje `using` zwolnią obserwatora i anulują subskrypcje obiektu `ConsoleReporter` na końcu metody.

Rozpakuj świeżą kopię przykładowego katalogu, usuń `../backups`, wykonaj kopię zapasową poleceniem `backup` i uruchom `dotnet run -- watch ../sample ../backups`. Podczas działania programu zmodyfikuj pliki: dopisz wiersz do `docs/guide.md`, utwórz `docs/faq.md`, utwórz plik tymczasowy `docs/faq.md.tmp` i poczekaj na kopię zapasową. Następnie zmień nazwę `scripts/report.py` na `scripts/daily_report.py` i usuń `docs/changelog.txt`:

```text
[##############################] 100% (7/7 files, 99.8 KB)
New: 0, modified: 0, deleted: 0, unchanged: 7
No changes - snapshot not created.
Watching ../sample - press Enter to stop.
Changed: docs/guide.md
Created: docs/faq.md
Changed: docs/faq.md
Changes settled - starting backup
+ docs/faq.md
~ docs/guide.md
[##############################] 100% (8/8 files, 99.9 KB)
New: 1, modified: 1, deleted: 0, unchanged: 6
Snapshot created: ../backups/snapshot-20261105-103348.zip (1.9 KB)
Renamed: scripts/report.py -> scripts/daily_report.py
Deleted: docs/changelog.txt
Changes settled - starting backup
+ scripts/daily_report.py
[##############################] 100% (7/7 files, 99.4 KB)
- docs/changelog.txt
- scripts/report.py
New: 1, modified: 0, deleted: 2, unchanged: 6
Snapshot created: ../backups/snapshot-20261105-103402.zip (1.0 KB)
```

Zmiana ignorowanego pliku `faq.md.tmp` w ogóle nie została zgłoszona. Dokładna sekwencja surowych zdarzeń zależy od systemu operacyjnego i edytora (np. na Windowsie możesz zobaczyć również zdarzenia `Changed` dla katalogów albo dwa zdarzenia `Changed` dla jednego zapisu), ale dzięki debouncingowi każda seria zmian kończy się jedną kopią zapasową.

> [!NOTE]
> #### Etap 5
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.05" >}}

### Kierunki dalszego rozwoju

Napisana w ramach tutoriala aplikacja to dobra podstawa do dalszej rozbudowy. Poniżej zebraliśmy najważniejsze mechanizmy, które warto dodać w kolejnych krokach, aby rozwinąć projekt:

- **Szybkie wykrywanie zmian:** Obliczanie skrótu każdego pliku przy każdej kopii zapasowej to metoda najbardziej niezawodna, ale najwolniejsza. Narzędzia takie jak `rsync` czy Git najpierw porównują rozmiar i czas modyfikacji zapisane w manifeście, a skrót obliczają tylko dla plików, w których te wartości się różnią.
- **Deduplikacja:** Plik, któremu zmieniono nazwę, jest obecnie zapisywany ponownie – jako plik usunięty i nowy. Jeśli pliki będą przechowywane w repozytorium pod nazwami wyznaczonymi z ich skrótów (*content-addressable storage*, tak jak robi to Git), każda zawartość zostanie zapisana tylko raz, a zmiany nazw nic nie będą kosztować.
- **Retencja i konsolidacja:** Łańcuch migawek rośnie w nieskończoność. Stare migawki można scalić w jedną pełną migawkę, aby odtwarzanie nie musiało stosować ich wszystkich, a najstarsze – usunąć.
- **Szyfrowanie:** Ponieważ dekoratory strumieni można łączyć w łańcuchy, szyfrowanie migawek sprowadza się do dodania `CryptoStream` (np. z algorytmem `Aes`) między archiwum a strumieniem pliku.
- **Asynchroniczne wejście/wyjście i anulowanie:** Większość operacji na strumieniach ma asynchroniczne odpowiedniki (`ReadAsync`, `CopyToAsync`, a od .NET 10 także asynchroniczne API do archiwów ZIP). Razem z `CancellationToken` pozwalają one budować responsywne aplikacje, np. taką, w której można anulować długotrwałą kopię zapasową.
- **Konfiguracja:** Wzorce ignorowania można by wczytywać z pliku `.snapshotignore` w katalogu źródłowym, podobnie jak `.gitignore`.

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/06.events-filesystem-streams/example1" >}})
