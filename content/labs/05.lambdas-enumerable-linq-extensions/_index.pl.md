---
title: "Wyrażenia lambda, IEnumerable, LINQ, metody rozszerzające"
weight: 50
---

## Tutorial 5: Wyrażenia lambda, IEnumerable, LINQ, metody rozszerzające

Zakres tego laboratorium obejmuje delegaty i wyrażenia lambda, metody rozszerzające, iteratory (`IEnumerable<T>` i instrukcję `yield`) oraz LINQ (*Language Integrated Query*) – standardową bibliotekę operatorów do odpytywania sekwencji danych.

Te mechanizmy są ze sobą ściśle powiązane: operatory LINQ to *metody rozszerzające* zdefiniowane dla `IEnumerable<T>`, większość z nich jest zaimplementowana jako *iteratory*, a ich działanie określamy za pomocą *wyrażeń lambda*. Dlatego w zadaniu najpierw samodzielnie zbudujemy każdy z tych elementów, a dopiero potem przejdziemy na gotowe operatory LINQ.

### Delegaty

Delegat to bezpieczna typowo referencja do metody – odpowiednik wskaźnika na funkcję lub `std::function` z C++. Własne typy delegatów definiuje się rzadko, ponieważ biblioteka standardowa dostarcza typy generyczne:

- `Func<T1, ..., TResult>` – funkcja przyjmująca do 16 argumentów i zwracająca wartość typu `TResult`,
- `Action<T1, ...>` – funkcja, która nie zwraca wartości (`void`),
- `Predicate<T>` – funkcja `T → bool`, używana przez niektóre starsze API (np. `List<T>.RemoveAll`).

```csharp
Func<int, int, int> add = Add;          // method group
Action<string> log = Console.WriteLine; // another method group
Predicate<int> isEven = n => n % 2 == 0;

log($"2 + 3 = {add(2, 3)}, 4 is even: {isEven(4)}"); // Output: 2 + 3 = 5, 4 is even: True

static int Add(int a, int b) => a + b;
```

### Wyrażenia lambda

Wyrażenie lambda to anonimowa funkcja zapisywana jako `(parametry) => wyrażenie` lub `(parametry) => { instrukcje }`. Typy parametrów są zwykle wnioskowane na podstawie typu delegata, do którego przypisujemy lambdę:

```csharp
Func<double, double> square = x => x * x;
Func<int, int, bool> divides = (d, n) => n % d == 0;
Action greet = () => Console.WriteLine("Hello!");
var cube = (double x) => x * x * x; // inferred type: Func<double, double>
```

W przeciwieństwie do C++, lambda nie ma listy przechwytywania – może korzystać z dowolnej zmiennej lokalnej lub parametru dostępnego w danym zakresie. Kompilator przechwytuje **samą zmienną** (a nie kopię jej wartości) i przenosi ją do ukrytego obiektu na stercie. W efekcie zmienna żyje tak długo jak lambda (lambda nigdy nie odwołuje się do nieistniejącej już zmiennej), a lambda zawsze widzi jej aktualną wartość – podobnie jak przy przechwytywaniu przez referencję (`[&]`) w C++:

```csharp
int counter = 0;
Action increment = () => counter++;
increment();
increment();
Console.WriteLine(counter); // Output: 2
```

Więcej informacji znajdziesz w wykładzie o [delegatach i wyrażeniach lambda]({{< ref "/lectures/05.csharp-features/delegates" >}}).

### Metody rozszerzające

Metody rozszerzające pozwalają dodawać metody do istniejącego typu – także do interfejsu lub typu, którego kodu nie możemy zmienić – bez dziedziczenia. Są to metody statyczne zdefiniowane w klasie statycznej, których pierwszy parametr jest oznaczony słowem kluczowym `this`. Wywołuje się je tak, jakby były metodami instancji rozszerzanego typu. Od C# 14 *bloki rozszerzające* pozwalają dodatkowo definiować właściwości rozszerzające, składowe statyczne i operatory:

```csharp
public static class StringExtensions
{
    // Classic extension method
    public static string Shout(this string text) => text.ToUpper() + "!";

    // C# 14 extension block - all members share the receiver 'text'
    extension(string text)
    {
        public bool IsBlank => string.IsNullOrWhiteSpace(text); // extension property
    }
}
```

```csharp
Console.WriteLine("hello".Shout()); // Output: HELLO!
Console.WriteLine("   ".IsBlank);   // Output: True
```

Więcej informacji znajdziesz w wykładzie o [metodach rozszerzających]({{< ref "/lectures/05.csharp-features/extension-methods" >}}).

### Iteratory i `IEnumerable<T>`

`IEnumerable<T>` to podstawowa abstrakcja sekwencji w C# – wszystko, po czym można iterować pętlą `foreach` (podobnie jak zakres w C++). Najprostszym sposobem utworzenia własnej sekwencji jest *metoda iterująca*: metoda zwracająca `IEnumerable<T>`, która produkuje elementy instrukcją `yield return`. Kompilator zamienia taką metodę w maszynę stanów – jej kod wykonuje się leniwie, kawałek po kawałku, za każdym razem, gdy konsument poprosi o kolejny element (tak jak korutyny z C++20, np. `std::generator` z C++23):

```csharp
var numbers = Countdown(3); // nothing is printed yet
foreach (int n in numbers)  // prints "start", then 3, 2, 1
{
    Console.WriteLine(n);
}

static IEnumerable<int> Countdown(int from)
{
    Console.WriteLine("start");
    for (int i = from; i > 0; i--)
    {
        yield return i;
    }
}
```

Więcej informacji znajdziesz w wykładzie o [enumeratorach]({{< ref "/lectures/06.collections/enumerators" >}}).

### LINQ

LINQ to zbiór metod rozszerzających dla `IEnumerable<T>` zdefiniowanych w przestrzeni nazw `System.Linq` (importowanej automatycznie dzięki `ImplicitUsings`): filtrowanie (`Where`), projekcja (`Select`), sortowanie (`OrderBy`), grupowanie (`GroupBy`), złączenia (`Join`), agregacja (`Count`, `Sum`, `Max`) i wiele innych. Zapytania można zapisywać na dwa równoważne sposoby – kompilator tłumaczy składnię zapytań na wywołania tych samych metod:

```csharp
int[] values = [5, 3, 8, 1, 4];

// Method syntax
var methodSyntax = values.Where(n => n > 2).OrderBy(n => n).Select(n => n * 10);

// Query syntax
var querySyntax = from n in values
                  where n > 2
                  orderby n
                  select n * 10;

// Both queries produce: 30, 40, 50, 80
```

Większość operatorów LINQ korzysta z **odroczonego wykonania**: zbudowanie zapytania go nie wykonuje – zapytanie wykonuje się dopiero podczas enumeracji wyniku. Operatory zwracające pojedynczą wartość (`Count`, `Sum`, `First`, …) lub kolekcję (`ToList`, `ToArray`, `ToDictionary`, …) wykonują zapytanie natychmiast.

Więcej informacji znajdziesz w wykładzie o [LINQ]({{< ref "/lectures/06.collections/linq" >}}).

## Zadanie 1 - Wrapped: analiza historii słuchania

Co roku w grudniu serwisy streamingowe pokazują swoim użytkownikom spersonalizowane podsumowanie roku: liczbę przesłuchanych minut, ulubionych wykonawców, utwory i gatunki. W ramach tego zadania zbudujesz takie podsumowanie (nazwiemy je *Wrapped*) dla historii słuchania zapisanej w plikach CSV. Aplikacja będzie się składała z pojedynczego projektu – aplikacji konsolowej.

Zaczynamy od kodu, który jedynie wczytuje dane. Następnie, etap po etapie, napiszesz filtry oparte na delegatach, własne iteratory i metody rozszerzające, zapytania LINQ, a na koniec połączysz je w roczny raport.

#### Kod początkowy

> [!NOTE]
> #### Kod początkowy
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.00" >}}

Katalog `Data` zawiera wygenerowaną historię słuchania jednego (fikcyjnego) użytkownika w 2025 roku:

- `artists.csv` – 40 wykonawców: `id,name,genres` (gatunki są oddzielone znakiem `;`),
- `tracks.csv` – 268 utworów z biblioteki użytkownika: `id,title,artist_id,length_s`,
- `plays.csv` – 3351 odtworzeń posortowanych według czasu: `timestamp,track_id,ms_played`.

```text
timestamp,track_id,ms_played
2025-01-02T07:37:43,247,211000
2025-01-02T07:41:16,103,1819
```

Kod początkowy zawiera:

- Rekordy `Artist`, `Track` i `Play`. `Play` ma dodatkową, wyliczaną właściwość `Duration` (czas odtwarzania utworu jako `TimeSpan`).
- Klasę `MusicLibrary` z listami tylko do odczytu `Artists`, `Tracks` i `Plays` oraz statyczną metodą `Load(string directory)` wczytującą dane. Zwróć uwagę na prywatną metodę `ReadCsv<T>(string path, Func<string[], T> parse)` – już w niej używamy delegata: to wywołujący decyduje, jak zamienić wiersz na obiekt, przekazując wyrażenie lambda.
- `Program.cs`, który wczytuje dane i wypisuje liczbę wczytanych obiektów oraz pierwsze odtworzenie. Linia `CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;` sprawia, że liczby i daty są formatowane identycznie na każdym komputerze, dzięki czemu wynik Twojego programu można porównać z oczekiwanymi wynikami poniżej.

Pliki z danymi są kopiowane do katalogu wyjściowego podczas budowania (zob. `Wrapped.csproj`). Uruchom program (`dotnet run` w katalogu `Wrapped`):

```text
Loaded 40 artists, 268 tracks and 3351 plays.
First play: Play { Timestamp = 01/02/2025 07:37:43, TrackId = 247, MsPlayed = 211000, Duration = 00:03:31 }
```

> [!TIP]
> Kompilator generuje dla rekordów metodę `ToString()`, która wypisuje wartości wszystkich publicznych właściwości. Dlatego `Console.WriteLine` wypisuje czytelny opis obiektu `Play`.

### Etap 1: Filtry – delegaty i wyrażenia lambda

W tym etapie utworzymy zestaw *filtrów* wielokrotnego użytku – funkcji typu `Func<Play, bool>`, które rozstrzygają, czy odtworzenie spełnia dany warunek – i poznamy różne sposoby tworzenia delegatów. Nie używaj jeszcze LINQ.

> [!IMPORTANT]
> #### Grupy metod, lambdy i domknięcia
>
> Delegat można utworzyć z:
> - **grupy metod** (*method group*) – nazwy istniejącej metody, np. `Func<Play, bool> f = Filters.IsSkip;`,
> - **wyrażenia lambda**, np. `Func<Play, bool> f = p => p.MsPlayed < 1000;`.
>
> Lambdę, która korzysta ze zmiennych lub parametrów z otaczającego zakresu, nazywamy **domknięciem** (*closure*). Metody zwracające domknięcia działają jak *fabryki delegatów* – każde wywołanie tworzy nową funkcję „skonfigurowaną” przekazanymi argumentami:
> ```csharp
> public static Func<Play, bool> InMonth(int month) =>
>     play => play.Timestamp.Month == month; // captures the parameter 'month'
> ```
> Jeśli lambda nie powinna niczego przechwytywać, oznacz ją modyfikatorem `static`. Kompilator zgłosi wtedy błąd (CS8820 lub CS8821) przy każdej próbie użycia zmiennej lokalnej, parametru lub `this`, co chroni przed przypadkowym przechwyceniem (i związanymi z nim ukrytymi alokacjami na stercie):
> ```csharp
> public static readonly Func<Play, bool> AtNight =
>     static play => play.Timestamp.Hour is >= 22 or < 6;
> ```

#### Klasa Filters

Utwórz plik `Filters.cs` z klasą statyczną `Filters` zawierającą:

1. Pole `public static readonly TimeSpan SkipThreshold` równe 30 sekundom. Serwisy streamingowe liczą odtworzenie jako *odsłuch* (ang. *stream*) tylko wtedy, gdy trwało co najmniej 30 sekund; krótsze odtworzenia to *pominięcia* (ang. *skips*).
2. Zwykłe metody statyczne `bool IsSkip(Play play)` (odtworzenie krótsze niż `SkipThreshold`) oraz `bool IsWeekend(Play play)` (odtworzenie w sobotę lub niedzielę).
3. Pokazane wyżej pole `AtNight` – lambdę `static` zapisaną w polu typu `Func<Play, bool>` (odtworzenia między 22:00 a 6:00).
4. Metody-fabryki zwracające domknięcia:
   - `Func<Play, bool> InMonth(int month)` (pokazana wyżej),
   - `Func<Play, bool> LongerThan(TimeSpan duration)` – odtworzenia dłuższe niż `duration`,
   - `Func<Play, bool> ByArtist(MusicLibrary library, string artistName)` – odtworzenia utworów danego wykonawcy. Przed zwróceniem lambdy zbierz za pomocą pętli identyfikatory utworów wykonawcy w zbiorze `HashSet<int>`. Zwracana lambda przechwytuje ten zbiór, więc jest on wyznaczany tylko raz, a każde sprawdzenie to po prostu `trackIds.Contains(play.TrackId)`.
5. Kombinatory tworzące nowe filtry z już istniejących:
   - `Func<Play, bool> Not(Func<Play, bool> filter)` – negacja,
   - `Func<Play, bool> All(params Func<Play, bool>[] filters)` – prawda, jeśli spełnione są wszystkie filtry,
   - `Func<Play, bool> Any(params Func<Play, bool>[] filters)` – prawda, jeśli spełniony jest co najmniej jeden filtr.

#### Zliczanie odtworzeń

W pliku `Program.cs`:

1. Dodaj statyczną funkcję lokalną `int Count(IEnumerable<Play> plays, Func<Play, bool> predicate)`, która za pomocą pętli `foreach` zlicza odtworzenia spełniające predykat. (W Etapie 3 zastąpimy ją metodą `Count` z LINQ.)
2. Utwórz lambdę `Action<string, int> print` wypisującą etykietę i wartość w wyrównanych kolumnach, np. `$"{label,-30}{value,6}"`.
3. Korzystając z `Count`, `print` i filtrów, wypisz liczbę: wszystkich odtworzeń, pominięć, odtworzeń w weekendy, odtworzeń w nocy, odtworzeń w weekendowe noce (`All`), nocnych odsłuchów (`All` + `Not`), odtworzeń dłuższych niż 6 minut, odtworzeń wykonawcy *Ada & The Lambdas* oraz odtworzeń o 7 rano (przekaż do `Count` lambdę zapisaną bezpośrednio w wywołaniu).

> [!TIP]
> #### `Func<Play, bool>` a `Predicate<Play>`
>
> `List<T>.RemoveAll` oczekuje argumentu typu `Predicate<T>`. Choć `Predicate<Play>` i `Func<Play, bool>` mają identyczne sygnatury, są to **różne, niekompatybilne typy**. Grupę metod lub lambdę można przekonwertować na dowolny typ delegata o zgodnej sygnaturze, ale istniejącego obiektu delegata – już nie:
> ```csharp
> List<Play> streams = [.. plays];
> int removed = streams.RemoveAll(Filters.IsSkip);         // OK - method group
> // streams.RemoveAll(Filters.AtNight);                   // error CS1503: cannot convert Func<Play, bool> to Predicate<Play>
> streams.RemoveAll(new Predicate<Play>(Filters.AtNight)); // OK - explicit conversion
> ```

4. Skopiuj odtworzenia do listy `List<Play>` (jak wyżej), usuń z niej metodą `RemoveAll` najpierw pominięcia, a następnie odtworzenia nocne, po czym wypisz liczbę usuniętych pominięć oraz liczbę pozostałych (dziennych) odsłuchów.

#### Przechwycona zmienna pętli

Na koniec policzmy odtworzenia w każdym miesiącu. Utwórz w pętli `for` listę 12 filtrów i wypisz w jednej linii wynik `Count` dla każdego z nich (skrócone nazwy miesięcy uzyskasz formatem `{date:MMM}`):

```csharp
var monthFilters = new List<Func<Play, bool>>();
for (int month = 1; month <= 12; month++)
{
    monthFilters.Add(p => p.Timestamp.Month == month);
}
```

Wszystkie wyniki są równe 0. Dlaczego?

> [!WARNING]
> #### Przechwytywanie zmiennej pętli
>
> Wszystkie dwanaście lambd przechwytuje **tę samą** zmienną `month` – samą zmienną, a nie jej wartość z chwili utworzenia lambdy. Lambdy są wykonywane po zakończeniu pętli, gdy `month` jest już równe 13. W pętli `for` istnieje tylko jedna zmienna sterująca dla całej pętli. (Pętla `foreach` od C# 5 tworzy nową zmienną w każdej iteracji, więc nie ma tego problemu.)
>
> Rozwiązaniem jest przechwycenie zmiennej tworzonej na nowo w każdej iteracji: skopiowanie zmiennej pętli do zmiennej lokalnej zadeklarowanej w ciele pętli (`int m = month;`) albo – jeszcze lepiej – użycie naszej fabryki `Filters.InMonth(month)`, której parametr jest osobną zmienną przy każdym wywołaniu.

Popraw pętlę, używając `Filters.InMonth`, i ponownie wypisz wyniki. Oczekiwany wynik:

```text
All plays                       3351
Skips                            446
Weekend plays                    955
Night plays                      875
Weekend nights                   324
Night streams                    760
Longer than 6 minutes            155
Ada & The Lambdas                201
Plays at 7 a.m.                  368
Removed skips                    446
Day streams                     2145
Plays per month (bug):
 Jan 0 Feb 0 Mar 0 Apr 0 May 0 Jun 0 Jul 0 Aug 0 Sep 0 Oct 0 Nov 0 Dec 0
Plays per month (fixed):
 Jan 273 Feb 298 Mar 299 Apr 277 May 233 Jun 301 Jul 274 Aug 262 Sep 285 Oct 294 Nov 326 Dec 229
```

> [!NOTE]
> #### Etap 1
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.01" >}}

### Etap 2: Własne iteratory i składowe rozszerzające

W tym etapie napiszemy własne operatory w stylu LINQ jako metody rozszerzające zaimplementowane za pomocą iteratorów, zaobserwujemy działanie odroczonego wykonania i dodamy składowe rozszerzające w nowej składni C# 14.

#### Klasa EnumerableExtensions

Utwórz plik `EnumerableExtensions.cs` z klasą statyczną `EnumerableExtensions`. Wszystkie metody są generycznymi metodami rozszerzającymi dla `IEnumerable<T>`. Pierwsza z nich – nasz odpowiednik `Where` – wygląda następująco:

```csharp
public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    foreach (var item in source)
    {
        if (predicate(item))
        {
            yield return item;
        }
    }
}
```

Analogicznie zaimplementuj:

1. `IEnumerable<TResult> Map<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)` – nasz odpowiednik `Select`, zwracający `selector(item)` dla każdego elementu.
2. `IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)` – wywołuje `action` dla każdego elementu i przekazuje element dalej bez zmian. Pozwoli nam podglądać (i zliczać) elementy przepływające przez zapytanie.
3. `IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)` – zwraca pary kolejnych elementów: dla `a, b, c, d` zwraca `(a, b), (b, c), (c, d)`. Dla sekwencji z mniej niż dwoma elementami zwraca sekwencję pustą.

> [!IMPORTANT]
> #### Bezpośrednie użycie enumeratora
>
> Pętla `foreach` to cukier składniowy: wywołuje `GetEnumerator()`, a następnie wywołuje `MoveNext()` i odczytuje `Current`, dopóki `MoveNext()` nie zwróci `false`. Czasem wygodniej jest sterować enumeratorem ręcznie, np. aby inaczej potraktować pierwszy element:
> ```csharp
> using var enumerator = source.GetEnumerator(); // IEnumerator<T> is IDisposable
> if (!enumerator.MoveNext())
> {
>     yield break; // empty sequence - end the iterator
> }
> T previous = enumerator.Current;
> // ... continue with a while (enumerator.MoveNext()) loop
> ```
> `yield break` kończy iterację (`MoveNext()` wygenerowanego enumeratora zwraca `false`).

Zaimplementuj `Pairwise`, korzystając bezpośrednio z enumeratora, jak pokazano wyżej.

> [!TIP]
> #### Sprawdzanie argumentów w iteratorach
>
> Całe ciało metody iterującej – łącznie ze sprawdzaniem argumentów na jej początku – wykonuje się dopiero przy rozpoczęciu enumeracji, a nie w chwili wywołania metody. Jeśli błąd ma zostać zgłoszony natychmiast, sprawdź argumenty w zwykłej metodzie, a część z `yield` przenieś do lokalnej funkcji iterującej. Przykład – okno przesuwne (uogólnienie `Pairwise`):
> ```csharp
> public static IEnumerable<T[]> Window<T>(this IEnumerable<T> source, int size)
> {
>     ArgumentOutOfRangeException.ThrowIfLessThan(size, 1); // checked immediately
>     return Iterator();
>
>     IEnumerable<T[]> Iterator() // a local iterator function
>     {
>         var window = new Queue<T>(size);
>         foreach (var item in source)
>         {
>             window.Enqueue(item);
>             if (window.Count > size)
>             {
>                 window.Dequeue();
>             }
>             if (window.Count == size)
>             {
>                 yield return window.ToArray();
>             }
>         }
>     }
> }
> ```

#### Składowe rozszerzające

> [!IMPORTANT]
> #### Bloki rozszerzające (C# 14)
>
> Blok rozszerzający `extension(Receiver receiver) { ... }`, umieszczony w klasie statycznej, grupuje składowe rozszerzające dla jednego typu odbiorcy. Oprócz metod może zawierać **właściwości** rozszerzające, składowe statyczne i operatory – czyli to, czego nie da się wyrazić klasyczną składnią z parametrem `this`. Odbiorcą może być dowolny typ, także skonkretyzowany interfejs generyczny, taki jak `IEnumerable<Play>`:
> ```csharp
> public static class ListeningExtensions
> {
>     extension(IEnumerable<Play> plays)
>     {
>         // Extension property
>         public TimeSpan TotalListeningTime
>         {
>             get
>             {
>                 var total = TimeSpan.Zero;
>                 foreach (var play in plays)
>                 {
>                     total += play.Duration;
>                 }
>                 return total;
>             }
>         }
>     }
> }
> ```
> Obie składnie są kompilowane do zwykłych metod statycznych i można je dowolnie łączyć – w istniejącym kodzie wciąż najczęściej spotyka się składnię klasyczną.

Utwórz plik `ListeningExtensions.cs` z klasą statyczną `ListeningExtensions` zawierającą dwa bloki rozszerzające:

1. Dla `IEnumerable<Play>`:
   - właściwość `TimeSpan TotalListeningTime` – suma `Duration` wszystkich odtworzeń (pokazana wyżej),
   - właściwość `int StreamCount` – liczba odtworzeń, które nie są pominięciami (użyj swojej metody `Filter` i filtrów z Etapu 1).
2. Dla `TimeSpan`: metodę `string ToHoursAndMinutes()`, która formatuje przedział czasu np. jako `"12 h 05 min"` (pełne godziny z `TotalHours` i minuty z `Minutes`).

#### Odroczone wykonanie w praktyce

Zastąp zawartość `Program.cs` (po wczytaniu biblioteki) trzema eksperymentami.

**1. Leniwe wykonanie.** Zbuduj zapytanie i przejdź tylko po jego dwóch pierwszych elementach:

```csharp
Console.WriteLine("Building the query...");
var longPlays = library.Plays
    .Tap(p => Console.WriteLine($"  pulled {p.Timestamp:MM-dd HH:mm} ({p.Duration:m\\:ss})"))
    .Filter(Filters.LongerThan(TimeSpan.FromMinutes(3)))
    .Map(p => $"{p.Timestamp:MM-dd HH:mm}");

Console.WriteLine("Enumerating...");
int taken = 0;
foreach (string time in longPlays)
{
    Console.WriteLine($"  got    {time}");
    if (++taken == 2)
    {
        break;
    }
}
```

Zwróć uwagę, że podczas budowania zapytania nic się nie dzieje, że elementy przechodzą przez `Tap`, `Filter` i `Map` pojedynczo oraz że odczytane zostają tylko trzy z 3351 odtworzeń.

**2. Wielokrotna enumeracja.** Policz, ile elementów pobiera ze źródła zapytanie użyte dwukrotnie:

```csharp
int pulled = 0;
var nightPlays = library.Plays.Tap(_ => pulled++).Filter(Filters.AtNight);
```

Wypisz `nightPlays.TotalListeningTime.ToHoursAndMinutes()` oraz `nightPlays.StreamCount`, a następnie wartość `pulled`. Potem wyzeruj `pulled`, zapisz wyniki zapytania w liście (`new List<Play>(nightPlays)`), wypisz obie właściwości dla listy i ponownie wartość `pulled`.

> [!WARNING]
> #### Każda enumeracja wykonuje zapytanie od nowa
>
> Zmienna przechowująca zapytanie nie przechowuje jego wyników – przechowuje *przepis* na ich obliczenie. Każda pętla `foreach` (i każdy operator konsumujący sekwencję, np. nasze właściwości rozszerzające) wykonuje cały potok od początku. W naszym przykładzie każde odtworzenie jest pobierane ze źródła dwa razy. Jeśli źródło jest kosztowne (odczyt pliku, zapytanie do bazy danych, złożone złączenie) albo niedeterministyczne (np. liczby losowe), wykonaj zapytanie raz i zapisz jego wyniki w kolekcji.

**3. Pairwise.** Korzystając z `Pairwise` (w połączeniu z `Filter`), policz, ile razy ten sam utwór został odtworzony dwa razy z rzędu. Następnie znajdź najdłuższą przerwę w słuchaniu – największą odległość między końcem odtworzenia (`Timestamp + Duration`) a początkiem następnego – i wypisz ją wraz z jej początkiem i końcem. Dekonstruuj pary bezpośrednio w pętli: `foreach (var (previous, current) in library.Plays.Pairwise())`.

Oczekiwany wynik:

```text
Building the query...
Enumerating...
  pulled 01-02 07:37 (3:31)
  got    01-02 07:37
  pulled 01-02 07:41 (0:01)
  pulled 01-02 07:41 (3:29)
  got    01-02 07:41

Night listening: 49 h 51 min, 760 streams
Plays pulled from the source: 6702
Night listening: 49 h 51 min, 760 streams
Plays pulled from the source: 3351

Same track twice in a row: 276 times
Longest break: 252 h 11 min (2025-08-03 19:45 - 2025-08-14 07:57)
```

> [!NOTE]
> #### Etap 2
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.02" >}}

### Etap 3: Pierwsze zapytania LINQ

`Filter` i `Map` z poprzedniego etapu to uproszczone wersje `Where` i `Select` z LINQ (prawdziwe implementacje dodają jedynie optymalizacje, np. dla tablic i list). Od teraz będziemy korzystać z LINQ. Zastąp zawartość `Program.cs` (po wczytaniu biblioteki) opisanymi niżej zapytaniami.

#### Proste agregaty

Nasze filtry to zwykłe delegaty `Func<Play, bool>`, więc można je przekazywać bezpośrednio do operatorów LINQ. Zdefiniuj sekwencję odsłuchów (jest to zapytanie z odroczonym wykonaniem, którego użyjemy kilka razy):

```csharp
var plays = library.Plays;
var streams = plays.Where(Filters.Not(Filters.IsSkip));
```

Wypisz, używając jednego wyrażenia LINQ dla każdej wartości:

1. Liczbę odtworzeń, odsłuchów i pominięć (`Count()`, `Count(predicate)`).
2. Liczbę przesłuchanych minut – sumę `MsPlayed` podzieloną przez 60 000 (`Sum`). W selektorze rzutuj `MsPlayed` na `long` – metoda `Sum` korzysta z arytmetyki sprawdzanej (*checked*) i dla większych zbiorów danych rzuciłaby wyjątek `OverflowException`.
3. Średni czas odtworzenia w sekundach (`Average`).
4. Liczbę różnych odsłuchanych utworów (`Select` + `Distinct` + `Count`) oraz liczbę dni ze słuchaniem (różne wartości `DateOnly.FromDateTime(p.Timestamp)`).
5. Czas pierwszego i ostatniego odtworzenia (`Min`, `Max`).
6. Czy było jakiekolwiek odtworzenie o 4 rano (`Any`) i czy wszystkie odtworzenia pochodzą z 2025 roku (`All`).
7. Najdłuższe odtworzenie (`MaxBy`) wraz z tytułem utworu (`library.Tracks.First(...)`).

> [!TIP]
> `MaxBy`/`MinBy` zwracają cały element o największym/najmniejszym kluczu, a `Max`/`Min` – jedynie sam klucz. `First` rzuca wyjątek, jeśli żaden element nie pasuje, a `FirstOrDefault` zwraca w takim przypadku `default` (`null` dla typów referencyjnych).

#### 10 najczęściej słuchanych utworów

> [!IMPORTANT]
> #### Składnia zapytań
>
> Składnia zapytań jest tłumaczona przez kompilator na wywołania metod LINQ: `where` → `Where`, `select` → `Select`, `orderby a descending, b` → `OrderByDescending(...).ThenBy(...)`. Klauzula `let` wprowadza zmienną pomocniczą – pod spodem kompilator przekształca każdy element w obiekt anonimowy przechowujący element i wyliczoną wartość. Operatory bez słowa kluczowego (np. `Take`, `Count`, `MaxBy`) trzeba wywołać jako metody:
> ```csharp
> var topTracksQuery =
>     (from t in library.Tracks
>      let count = streams.Count(p => p.TrackId == t.Id)
>      orderby count descending, t.Title
>      select new { Track = t, Streams = count })
>     .Take(10);
> ```
> `new { Track = t, Streams = count }` tworzy obiekt **typu anonimowego** – niemutowalnej klasy generowanej przez kompilator, z właściwościami tylko do odczytu i porównywaniem przez wartość.

Zapisz to samo zapytanie w składni metod: przekształć każdy utwór w obiekt anonimowy z liczbą odsłuchów (`Select`), posortuj malejąco według liczby odsłuchów, a następnie według tytułu (`OrderByDescending`, `ThenBy`), i weź pierwszych 10 elementów (`Take`). Wypisz wynik z numerami pozycji i nazwami wykonawców (wykonawcę znajdź za pomocą `library.Artists.First(...)`). Pozycję każdego elementu uzyskasz metodą `Index()` (.NET 9), która zwraca krotki `(int Index, T Item)`:

```csharp
foreach (var (i, x) in topTracks.Index())
{
    // ...
}
```

Następnie sprawdź metodą `SequenceEqual`, że oba zapytania dają ten sam wynik (działa to dzięki porównywaniu typów anonimowych przez wartość).

#### Najczęściej słuchany wykonawca

Znajdź wykonawcę z największą liczbą odsłuchów: dla każdego wykonawcy zsumuj (`Sum`) liczby odsłuchów jego utworów (`Where`) i wybierz maksimum metodą `MaxBy`.

> [!WARNING]
> #### Ukryta złożoność kwadratowa
>
> Powyższe zapytania są zwięzłe, ale `streams.Count(...)` wewnątrz `Select` przechodzi po wszystkich odtworzeniach **dla każdego utworu** – 268 × 3351 ≈ 900 000 wywołań predykatu, a każde z nich dodatkowo wykonuje od nowa odroczone `Where`. Dla naszych danych to wciąż ułamek sekundy, ale przy większych danych takie zapytania stają się problemem. W Etapie 5 obliczymy te same wyniki w jednym przebiegu za pomocą grupowania.

Oczekiwany wynik:

```text
Plays:            3351 (2905 streams, 446 skips)
Minutes listened: 11465
Average play:     205.3 s
Distinct tracks:  239
Listening days:   270
First and last:   2025-01-02 07:37, 2025-12-30 23:08
Any play at 4 am: False
All in 2025:      True
Longest play:     Little Hours (8:00) on 2025-01-14

Top 10 tracks:
  1. Winter Letters - Static Bloom                 134
  2. Sleepless Roads - Static Bloom                 89
  3. Lazy Satellites - Pixel Monsoon                83
  4. Analog Memories - Lena Marlowe                 73
  5. Burning Horizons - Concrete Gardens            63
  6. Crystal River - Static Bloom                   59
  7. Cosmic Cities - Pixel Monsoon                  57
  8. Lazy Evaluation - Ada & The Lambdas            54
  9. Digital Hours - Mira Volta                     53
 10. Faded Horizons - Static Bloom                  52
Query syntax gives the same result: True
Top artist: Static Bloom (430 streams)
```

> [!NOTE]
> #### Etap 3
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.03" >}}

### Etap 4: Złączenia danych

Odtworzenia zawierają jedynie identyfikatory utworów, a utwory – jedynie identyfikatory wykonawców. Do tej pory powiązane obiekty wyszukiwaliśmy metodą `First`, która dla każdego elementu przegląda całą listę. W tym etapie złączymy trzy kolekcje, tak jak w relacyjnej bazie danych.

> [!IMPORTANT]
> #### Join
>
> `Join` łączy elementy dwóch sekwencji o równych kluczach (jak `INNER JOIN` w SQL). Przyjmuje sekwencję wewnętrzną, dwa selektory kluczy oraz funkcję tworzącą wynik z dopasowanej pary:
> ```csharp
> var playsWithTracks = library.Plays.Join(
>     library.Tracks,               // inner sequence
>     play => play.TrackId,         // key of the outer element
>     track => track.Id,            // key of the inner element
>     (play, track) => (play, track));
> ```
> W składni zapytań to samo złączenie zapisujemy jako `join track in library.Tracks on play.TrackId equals track.Id`. `Join` najpierw buduje tablicę haszującą z sekwencji wewnętrznej, dzięki czemu całe złączenie zajmuje czas liniowy zamiast kwadratowego czasu zagnieżdżonych wyszukiwań. Elementy bez dopasowania są pomijane.

#### Złączone odtworzenia

1. Utwórz plik `PlayInfo.cs` z rekordem `PlayInfo(Play Play, Track Track, Artist Artist)`.
2. W klasie `MusicLibrary` dodaj metodę `IEnumerable<PlayInfo> GetPlayInfos()`, która złącza `Plays` z `Tracks`, a następnie z `Artists`. Zapisz ją w składni zapytań (z dwiema klauzulami `join`) – przy wielu złączeniach jest ona znacznie czytelniejsza niż składnia metod.
3. W `Program.cs` zapisz złączone odtworzenia w zmiennej (`var infos = library.GetPlayInfos();`) i wypisz trzy pierwsze z nich (`Take`) w formacie pokazanym w oczekiwanym wyniku.
4. Zapisz to samo złączenie w składni metod (dwa połączone wywołania `Join`) i sprawdź metodą `SequenceEqual`, że obie wersje dają ten sam wynik. Rekordy, podobnie jak typy anonimowe, są porównywane przez wartość.

#### Gatunki

> [!IMPORTANT]
> #### SelectMany
>
> `SelectMany` spłaszcza sekwencję sekwencji: dla każdego elementu pobiera sekwencję i łączy je wszystkie w jedną (podobnie jak `flatMap` w innych językach). W składni zapytań odpowiada mu druga klauzula `from`:
> ```csharp
> var artistGenres =
>     from artist in library.Artists
>     from genre in artist.Genres
>     select (artist.Name, genre); // one element for every (artist, genre) pair
> ```

1. Wypisz wszystkie gatunki z biblioteki w kolejności alfabetycznej (`SelectMany` + `Distinct` + `Order`) jako listę oddzieloną przecinkami (`string.Join`).
2. Wypisz nazwy wykonawców jazzowych – użyj składni zapytań z dwiema klauzulami `from` i klauzulą `where`.
3. Wypisz łączny czas słuchania jazzu: przefiltruj złączone odtworzenia według gatunków wykonawcy (`Contains`), wybierz odtworzenia (`Select(i => i.Play)`) i użyj właściwości rozszerzającej `TotalListeningTime` z Etapu 2.

#### Czego nigdy nie odtworzono?

> [!IMPORTANT]
> #### GroupJoin i LeftJoin
>
> `GroupJoin` łączy każdy element sekwencji zewnętrznej z sekwencją *wszystkich* pasujących elementów wewnętrznych – być może pustą. Stanowi podstawę lewostronnego złączenia zewnętrznego (w składni zapytań: `join ... into group`).
>
> W .NET 10 dodano metodę `LeftJoin` (oraz `RightJoin`), która bezpośrednio realizuje `LEFT JOIN`: każdy element zewnętrzny pojawia się w wyniku, a jeśli nie ma dopasowania, element wewnętrzny przekazany do selektora wyniku ma wartość `null` (`default`):
> ```csharp
> var result = library.Artists.LeftJoin(
>     infos,
>     artist => artist.Id,
>     info => info.Artist.Id,
>     (artist, info) => new { artist.Name, Info = info }); // info is null for artists without plays
> ```
> Przed .NET 10 ten sam efekt wymagał połączenia `GroupJoin` + `SelectMany` + `DefaultIfEmpty`.

1. Za pomocą `GroupJoin` kolekcji `library.Tracks` z `library.Plays` oblicz liczbę odtworzeń każdego utworu, wybierz tytuły utworów, których nigdy nie odtworzono, i zapisz je w liście (`ToList`). Wypisz ich liczbę i trzy pierwsze tytuły.
2. Za pomocą `LeftJoin` kolekcji `library.Artists` ze złączonymi odtworzeniami znajdź wykonawców, których nigdy nie słuchano.

> [!TIP]
> Nieodtworzone utwory można też znaleźć operacją na zbiorach: `library.Tracks.ExceptBy(library.Plays.Select(p => p.TrackId), t => t.Id)`.

Oczekiwany wynik:

```text
First plays:
  2025-01-02 07:37  Paper Satellites - Thunder Index (3:31)
  2025-01-02 07:41  Sleepless Streets - Ruby Fieldnotes (0:01)
  2025-01-02 07:41  Lunar Hours - Kasia Wren (3:29)
Method syntax gives the same result: True

Genres: ambient, classical, electronic, folk, hip-hop, indie, jazz, metal, pop, r&b, rock, synthwave
Jazz artists: Northern Lights Quartet, Blue Hour Trio, Moss & Marble
Jazz listening time: 5 h 26 min

Tracks never played: 26 (Sweet Dreams, Velvet Heart, Distant Oceans, ...)
Artists never played: The Null References
```

> [!NOTE]
> #### Etap 4
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.04" >}}

### Etap 5: Grupowanie i agregacja

W tym etapie napiszemy najważniejsze statystyki naszego podsumowania. Aby można było ich używać wielokrotnie (w następnym etapie połączymy je w raport), zdefiniujemy je jako składowe rozszerzające `IEnumerable<PlayInfo>`.

> [!IMPORTANT]
> #### GroupBy
>
> `GroupBy` dzieli sekwencję na grupy elementów o równych kluczach. Każda grupa to `IGrouping<TKey, TElement>` – sekwencja elementów z dodatkową właściwością `Key`, więc można na niej używać wszystkich operatorów LINQ (i naszych własnych metod rozszerzających):
> ```csharp
> var streamsPerArtist = infos
>     .GroupBy(i => i.Artist)
>     .Select(g => (Artist: g.Key, Streams: g.Count()));
> ```
> Rekordy mogą służyć za klucze dzięki porównywaniu przez wartość. W składni zapytań grupowanie zapisujemy jako `group info by key into g`, po czym zapytanie jest kontynuowane na grupach.

> [!IMPORTANT]
> #### CountBy i AggregateBy (.NET 9)
>
> Jeśli dla każdego klucza potrzebujemy tylko jednej liczby, grupy nie muszą być w ogóle tworzone. `CountBy` zwraca liczbę elementów dla każdego klucza, a `AggregateBy` oblicza dowolny agregat dla każdego klucza, zaczynając od wartości początkowej. Obie metody zwracają sekwencję `KeyValuePair<TKey, TValue>`:
> ```csharp
> var streamsPerTrack = infos.CountBy(i => i.Track);     // KeyValuePair<Track, int>
> var timePerMonth = infos.AggregateBy(
>     i => i.Play.Timestamp.Month,                       // key
>     TimeSpan.Zero,                                     // seed
>     (total, i) => total + i.Play.Duration);            // KeyValuePair<int, TimeSpan>
> ```

#### Klasa Stats

Utwórz plik `Stats.cs` z klasą statyczną `Stats` zawierającą blok rozszerzający `extension(IEnumerable<PlayInfo> plays)` z następującymi składowymi:

1. Właściwość `IEnumerable<PlayInfo> Streams` – odtworzenia, które nie są pominięciami (`Where`).
2. `IEnumerable<(Artist Artist, int Streams, TimeSpan Time)> TopArtists(int count)` – wykonawcy z największą liczbą odsłuchów: pogrupuj `Streams` według wykonawcy (`GroupBy`), oblicz liczbę odsłuchów i ich łączny czas (użyj ponownie `TotalListeningTime`), posortuj malejąco według liczby odsłuchów, a następnie według nazwy, i weź `count` elementów.
3. `IEnumerable<(Track Track, Artist Artist, int Streams)> TopTracks(int count)` – utwory z największą liczbą odsłuchów. Użyj `CountBy` z kluczem w postaci krotki `(i.Track, i.Artist)`; posortuj malejąco według liczby, a następnie według tytułu.
4. `IEnumerable<(string Genre, int Streams)> TopGenres(int count)` – gatunki z największą liczbą odsłuchów (`SelectMany` + `CountBy`); posortuj malejąco według liczby, a następnie według nazwy. Odsłuch wykonawcy o dwóch gatunkach liczy się dla każdego z nich.
5. `Dictionary<int, TimeSpan> TimeByMonth()` – czas słuchania w każdym miesiącu, obliczony za pomocą `AggregateBy` (zob. wyżej) i zamieniony na słownik metodą `ToDictionary()`.
6. `IEnumerable<(int Month, Artist Artist, int Streams)> TopArtistByMonth()` – najczęściej słuchany wykonawca każdego miesiąca (wyniki posortowane według miesiąca). Pogrupuj odsłuchy według miesiąca i na każdej grupie użyj ponownie `TopArtists` – tu właśnie opłaca się definiowanie zapytań jako składowych rozszerzających:
   ```csharp
   from info in plays.Streams
   group info by info.Play.Timestamp.Month into month
   orderby month.Key
   let top = month.TopArtists(1).First()
   select (month.Key, top.Artist, top.Streams);
   ```
7. `ILookup<int, PlayInfo> ByHour()` – odtworzenia pogrupowane według godziny (`ToLookup`).

> [!IMPORTANT]
> #### ToDictionary i ToLookup
>
> Obie metody natychmiast wykonują zapytanie i budują kolekcję indeksowaną kluczem. `ToDictionary` wymaga unikalnych kluczy (powtórzony klucz powoduje wyjątek), a indeksator rzuca `KeyNotFoundException` dla brakującego klucza (zamiast niego użyj `TryGetValue` lub `GetValueOrDefault`). `ToLookup` pozwala na wiele elementów dla jednego klucza: jego indeksator zwraca sekwencję wszystkich elementów o danym kluczu – **pustą sekwencję**, jeśli takich nie ma. W przeciwieństwie do `GroupBy`, obiekt `ILookup` jest obliczany tylko raz i umożliwia szybki dostęp po kluczu.

#### Wypisanie statystyk

W `Program.cs` pobierz złączone odtworzenia (`var infos = library.GetPlayInfos();`) i wypisz:

1. 5 najczęściej słuchanych wykonawców (z liczbą odsłuchów i czasem słuchania), 5 najczęściej słuchanych utworów i 3 najpopularniejsze gatunki. Pozycje uzyskasz metodą `Index()`, a krotki możesz dekonstruować bezpośrednio w pętli: `foreach (var (i, (artist, streams, time)) in infos.TopArtists(5).Index())`. Porównaj najczęściej słuchane utwory z wynikiem Etapu 3 – powinny być identyczne.
2. Tabelę miesięcy: czas słuchania, pasek ze znaków `#` proporcjonalny do czasu (najdłuższy pasek ma 20 znaków; dzielenie `TimeSpan` przez `TimeSpan` daje `double`) oraz najczęściej słuchanego wykonawcę miesiąca. Wynik `TopArtistByMonth()` zamień na słownik indeksowany miesiącem (`ToDictionary(x => x.Month)`).
3. Histogram odtworzeń według godziny dla godzin 0–23 (najdłuższy pasek ma 40 znaków). Dzięki `ToLookup` wyrażenie `byHour[hour].Count()` po prostu zwraca 0 dla godzin bez odtworzeń.

Oczekiwany wynik:

```text
Top 5 artists:
  1. Static Bloom                              430   25 h 36 min
  2. Pixel Monsoon                             232   14 h 33 min
  3. Ada & The Lambdas                         177   10 h 18 min
  4. Lena Marlowe                              174   11 h 18 min
  5. Mira Volta                                154    9 h 55 min
Top 5 tracks:
  1. Winter Letters - Static Bloom             134
  2. Sleepless Roads - Static Bloom             89
  3. Lazy Satellites - Pixel Monsoon            83
  4. Analog Memories - Lena Marlowe             73
  5. Burning Horizons - Concrete Gardens        63
Top genres: electronic (1135), pop (952), indie (821)

Jan  16 h 26 min #################    Static Bloom (38)
Feb  17 h 49 min ###################  Lena Marlowe (41)
Mar  17 h 11 min ##################   Kasia Wren (45)
Apr  15 h 38 min ################     Static Bloom (32)
May  12 h 39 min #############        Concrete Gardens (41)
Jun  17 h 01 min ##################   Static Bloom (42)
Jul  15 h 42 min ################     Lena Marlowe (46)
Aug  14 h 55 min ################     Static Bloom (43)
Sep  16 h 03 min #################    Static Bloom (38)
Oct  16 h 07 min #################    Pixel Monsoon (68)
Nov  18 h 31 min #################### Mira Volta (46)
Dec  12 h 59 min ##############       Static Bloom (30)

00:00 #####                                     66
01:00                                            8
02:00                                            0
03:00                                            0
04:00                                            0
05:00                                            0
06:00                                            0
07:00 ############################             368
08:00 #########################                332
09:00                                            0
10:00 ####                                      61
11:00 ###########                              146
12:00 #####                                     72
13:00 #####                                     75
14:00 #######                                   91
15:00 ###                                       50
16:00 ####                                      61
17:00                                           10
18:00 ######                                    90
19:00 ###########################              356
20:00 ###########################              359
21:00 ###############################          405
22:00 ######################                   286
23:00 ######################################## 515
```

> [!NOTE]
> #### Etap 5
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.05" >}}

### Etap 6: Twój Wrapped

W ostatnim etapie złożymy roczny raport z napisanych dotąd elementów i przekonamy się, dlaczego ma znaczenie, *kiedy* zapytanie jest wykonywane.

#### Jeszcze jeden iterator: SplitWhen

Serie słuchania (kolejne dni z muzyką) i sesje słuchania (odtworzenia oddzielone krótkimi przerwami) mają jedną cechę wspólną: posortowaną sekwencję trzeba podzielić na grupy kolejnych elementów. W klasie `EnumerableExtensions` dodaj metodę:

```csharp
public static IEnumerable<List<T>> SplitWhen<T>(this IEnumerable<T> source, Func<T, T, bool> startsNewGroup)
```

Metoda zbiera elementy w liście; przed dodaniem elementu wywołuje `startsNewGroup(ostatni element bieżącej grupy, element)` i jeśli wynikiem jest `true`, zwraca bieżącą grupę (`yield return`) i rozpoczyna nową. Nie zapomnij zwrócić ostatniej grupy po zakończeniu pętli (chyba że sekwencja była pusta). Przykładowo dla `1, 2, 3, 7, 8, 10` i `(a, b) => b - a > 1` metoda zwraca `[1, 2, 3]`, `[7, 8]`, `[10]`.

#### Więcej statystyk

Dodaj do bloku rozszerzającego w klasie `Stats`:

1. `(DateOnly First, DateOnly Last) LongestStreak()` – najdłuższą serię kolejnych dni z co najmniej jednym odtworzeniem. Wyznacz różne dni odtworzeń, posortuj je (`Order`), podziel metodą `SplitWhen` wszędzie tam, gdzie różnica `DayNumber` sąsiednich dni jest większa niż 1, i wybierz najdłuższą grupę (`MaxBy`).
2. `(DateTime Start, DateTime End, int Plays) LongestSession(TimeSpan maxGap)` – najdłuższą sesję słuchania: posortuj odtworzenia według czasu i podziel je wszędzie tam, gdzie przerwa między końcem odtworzenia a początkiem następnego przekracza `maxGap`. Długość sesji to czas od początku jej pierwszego do końca jej ostatniego odtworzenia.
3. `(DateTime From, DateTime To) LongestBreak()` – najdłuższą przerwę w słuchaniu. Tym razem zaimplementuj ją jako pojedyncze wyrażenie LINQ: posortuj odtworzenia, użyj `Pairwise` z Etapu 2, przekształć pary w krotki `(From, To)` i wybierz najdłuższą metodą `MaxBy`.
4. `(Track Track, Artist Artist, DateOnly Day, int Streams) OnRepeat()` – utwór odsłuchany najwięcej razy w ciągu jednego dnia (`CountBy` z kluczem w postaci krotki `(i.Track, i.Artist, Day)`, a następnie `MaxBy`).

#### Raport

Utwórz plik `WrappedReport.cs` z klasą statyczną `WrappedReport` i metodą `string Build(IEnumerable<PlayInfo> history, int year)`. Metoda zawęża historię do podanego roku i buduje tekst raportu za pomocą `StringBuilder`. Raport zawiera (zob. oczekiwany wynik poniżej):

- liczbę przesłuchanych minut (pełne minuty, sformatowane przez `N0`) oraz łączny czas,
- liczbę odsłuchów oraz liczbę różnych odsłuchanych utworów i wykonawców (`DistinctBy`),
- 5 najczęściej słuchanych wykonawców i 5 najczęściej słuchanych utworów,
- 3 najpopularniejsze gatunki,
- najbardziej muzyczny miesiąc (maksimum `TimeByMonth()`; miesiąc sformatuj przez `{date:MMMM}`),
- godzinę szczytu (największą grupę `ByHour()`) oraz typ słuchacza wyznaczony wyrażeniem `switch` ze wzorcami relacyjnymi: 5–11 – *an early bird*, 12–17 – *an afternoon listener*, 18–21 – *an evening listener*, w pozostałych przypadkach *a night owl*,
- najdłuższą serię, najdłuższą sesję (z `maxGap` równym 15 minut), najdłuższą przerwę (w dniach, `TotalDays`) i utwór słuchany w kółko.

> [!IMPORTANT]
> #### Materializacja
>
> *Materializacja* to wykonanie zapytania i zapisanie jego wyników w kolekcji: `ToList()`, `ToArray()`, `ToDictionary()`, `ToLookup()`, `ToHashSet()`. Zmaterializowana kolekcja jest migawką – wielokrotna enumeracja jest tania i zawsze daje te same elementy. Z drugiej strony zbyt wczesna materializacja marnuje pamięć, uniemożliwia przetwarzanie nieskończonych sekwencji i odbiera korzyści leniwego wykonania (np. zatrzymanie `First` po pierwszym dopasowaniu). Dobra praktyka: podczas składania zapytań pozostaw je odroczone, a materializuj wtedy, gdy wynik będzie przeglądany wiele razy.

#### Odroczone a zmaterializowane

Każda statystyka w raporcie przechodzi po swoich danych wejściowych raz, więc `Build` przegląda historię kilkanaście razy. Sprawdźmy, co to oznacza dla wejścia z odroczonym wykonaniem. W `Program.cs`:

```csharp
// Deferred: every query in the report runs the join again
int created = 0;
IEnumerable<PlayInfo> deferred = library.GetPlayInfos().Tap(_ => created++);
string report = WrappedReport.Build(deferred, 2025);
int createdDeferred = created;

// Materialized: the join runs once, the report reads a list
created = 0;
List<PlayInfo> materialized = library.GetPlayInfos().Tap(_ => created++).ToList();
string sameReport = WrappedReport.Build(materialized, 2025);
```

Wypisz raport, liczbę obiektów `PlayInfo` utworzonych przez złączenie w obu przypadkach oraz informację, czy oba raporty są identyczne. Przy wejściu odroczonym złączenie – wraz z budowaniem tablic haszujących – jest wykonywane od nowa przez każdą statystykę. Dla naszego zbioru danych to wciąż szybkie, ale łatwo wyobrazić sobie źródło, dla którego każda enumeracja oznacza odczyt pliku lub zapytanie do bazy danych. Jeśli chcesz, zmierz czas obu wariantów za pomocą `System.Diagnostics.Stopwatch`.

> [!TIP]
> Metoda, która musi wielokrotnie przeglądać swoje dane wejściowe, może się zabezpieczyć: zmaterializować argument na początku albo zadeklarować parametr jako `IReadOnlyCollection<T>`/`IReadOnlyList<T>`, aby wywołujący musiał przekazać gotową kolekcję. Środowiska IDE (np. Rider) ostrzegają przed *możliwą wielokrotną enumeracją* (*possible multiple enumeration*) `IEnumerable<T>`.

Oczekiwany wynik:

```text
============================================================
                     YOUR 2025 WRAPPED
============================================================
Minutes listened   11,465 (191 h 05 min)
Streams            2,905 streams of 239 tracks by 39 artists

Top artists
  1. Static Bloom                                  430 streams
  2. Pixel Monsoon                                 232 streams
  3. Ada & The Lambdas                             177 streams
  4. Lena Marlowe                                  174 streams
  5. Mira Volta                                    154 streams
Top tracks
  1. Winter Letters - Static Bloom                 134 streams
  2. Sleepless Roads - Static Bloom                 89 streams
  3. Lazy Satellites - Pixel Monsoon                83 streams
  4. Analog Memories - Lena Marlowe                 73 streams
  5. Burning Horizons - Concrete Gardens            63 streams

Top genres         electronic, pop, indie
Busiest month      November (18 h 31 min)
Listening clock    peak at 23:00 - you are a night owl
Longest streak     17 days (2025-09-13 - 2025-09-29)
Longest session    2 h 07 min on 2025-10-11 (34 plays)
Longest break      10.5 days (2025-08-03 - 2025-08-14)
On repeat          Paper Bridges - Lena Marlowe, 5 times on 2025-02-09
============================================================

PlayInfo objects created: deferred 43,563, materialized 3,351
Identical reports: True
```

> [!NOTE]
> #### Etap 6
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.06" >}}

### Kierunki dalszego rozwoju

Aplikację napisaną w ramach tutoriala można rozwijać w wielu kierunkach. Poniżej zebraliśmy kilka pomysłów wartych uwagi:

- **Więcej statystyk:** Stopień dosłuchania utworów (`Play.Duration` w porównaniu z `Track.Length`) i najczęściej pomijany utwór, pierwsze odtworzenie każdego wykonawcy (*odkrycia* roku) albo najintensywniejszy 7-dniowy okres wyznaczony za pomocą iteratora okna przesuwnego `Window`.
- **Prawdziwe dane:** Serwisy streamingowe pozwalają pobrać własną historię słuchania (zwykle w postaci plików JSON). Wczytaj ją za pomocą `System.Text.Json` i wygeneruj własny Wrapped.
- **Strumieniowe przetwarzanie dużych plików:** Zamiast wczytywać wszystkie dane do pamięci, czytaj plik leniwie za pomocą iteratora (`File.ReadLines` + `yield return`) lub asynchronicznie za pomocą `IAsyncEnumerable<T>` – od .NET 10 operatory LINQ dla sekwencji asynchronicznych są wbudowane w platformę (`System.Linq.AsyncEnumerable`).
- **Testy jednostkowe:** Pokryj iteratory testami (zob. [Tutorial 2]({{< ref "/labs/02.msbuild" >}})), zwłaszcza przypadki brzegowe: puste sekwencje, sekwencje z jednym elementem i sprawdzanie argumentów.
- **PLINQ:** Zrównoleglij zapytania za pomocą `AsParallel()` i zmierz, czy opłaca się to dla zbioru danych tej wielkości.
- **`IQueryable<T>` i drzewa wyrażeń:** Te same zapytania LINQ napisane dla bazy danych z użyciem Entity Framework Core nie są wykonywane w pamięci – są tłumaczone na SQL. Lambdy są wtedy kompilowane do drzew wyrażeń (`Expression<Func<T, bool>>`) zamiast do delegatów.

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/05.lambdas-enumerable-linq-extensions/example1" >}})
- [Zadanie 2]({{< ref "/labs/05.lambdas-enumerable-linq-extensions/example2" >}})
