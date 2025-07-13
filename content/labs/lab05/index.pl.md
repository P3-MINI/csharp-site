---
title: "Lab05"
weight: 10
---

# Laboratorium 5: Enumerable, LINQ i Metody Rozszerzające

## Metody rozszerzające dla istniejących typów

{{% hint info %}}
**Reguły i konwencje nazewnictwa identyfikatorów**

Jednym z podstawowych elementów pisania czytelnego i spójnego kodu jest trzymanie się konwencji. Różne języki programowania mają swoje własne preferencje w zakresie nadawania nazw identyfikatorom - zmiennym, metodom, właściwościom czy klasom.

**Najpopularniejsze style**

- `PascalCase`:
  - Pierwsza litera każdego wyrazu jest duża, bez użycia separatorów (np. `UserProfileId`, `HttpRequestHeaders`).
  - Stosowany głównie w: **C#**, **Java** (dla nazw klas, metod, właściwości).
- `camelCase`:
  - Pierwsza litera pierwszego wyrazu jest mała, a kolejne wyrazy zaczynają się wielką literą (np. `startDateTime`, `xmlParserSettings`).
  - Popularny w: **JavaScript** (dla zmiennych i funkcji), **Java** (dla zmiennych i metod).
- `snake_case`:
  - Wszystkie litery są małe, a słowa oddzielone znakiem podkreślenia (np. `user_profile_id`, `json_response_data`).
  - Często używany w: **Python**, bazy danych, nazwy pól w obiektach JSON.
- `kebab-case`:
  - Słowa oddzielane są myślnikiem (np. `html-element-id`).
  - Spotykany głównie w: URL, atrybuty HTML, nazwy plików (np. w projektach opartych o **JavaScript**/**Node.js**).

W projektach, które łączą różne warstwy technologiczne (np. backend w C#, frontend w JavaScript, komunikacja poprzez JSON), często zachodzi potrzeba konwersji nazw między stylami. Przykładowo, właściwość klasy C# może nazywać się `EmailAddress`, ale gdy serializujemy ją do JSON-a, powinna zostać zapisana jako `email_address`.
{{% /hint %}}

### Opis zadania

Twoim zadaniem jest zaimplementowanie dwóch metod rozszerzających dla klasy `string`:

- `PascalToSnakeCase`, która dla poprawnego identyfikatora zapisanego w stylu `PascalCase` zwróci jego odpowiednik w stylu `snake_case`,
- `SnakeToPascalCase`, która dla poprawnego identyfikatora zapisanego w stylu `snake_case` zwróci jego odpowiednik w stylu `PascalCase`.

Przykładowe użycie powinno wyglądać następująco:

```csharp
var pascal = "HtmlElementId";
var snake = pascal.PascalToSnakeCase();

Console.WriteLine(snake); // "html_element_id"
```

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: C# identifier naming rules and conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [Microsoft Learn: Common C# code conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Microsoft Learn: How to implement and call a custom extension method](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/how-to-implement-and-call-a-custom-extension-method)
- [Microsoft Learn: Using the StringBuilder Class in .NET](https://learn.microsoft.com/en-us/dotnet/standard/base-types/stringbuilder)
  {{% /hint %}}

### Przykładowe rozwiązanie

Rozwiązanie wraz z przykładami do testowania można znaleźć w pliku [Task01.cs](/labs/lab05/solution/tasks/Task01.cs).

<!-- ## Zadanie 2

### Opis zadania

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: ](link)
  {{% /hint %}}

### Przykładowe rozwiązanie

Przykładowe rozwiązanie można znaleźć w pliku [Task02.cs](/labs/lab05/solution/tasks/Task02.cs). -->

## Iteratory, `yield` i generowanie liczb pierwszych

{{% hint info %}}
**Iteratory w praktyce:**

W wielu przypadkach potrzebujemy generować sekwencje danych, których długość nie jest znana z góry albo których stworzenie "na raz" byłoby zbyt kosztowne. Zamiast budować całą kolekcję w pamięci i zwracać ją jako całość, możemy wykorzystać iteratory — mechanizm pozwalający zwracać elementy na żądanie.

W języku C# iteratory tworzymy przy użyciu dwóch słów kluczowych:

- `yield return` — zwraca pojedynczy element i zawiesza wykonanie metody do czasu, aż kolejny element zostanie zażądany,

- `yield break` — natychmiast przerywa działanie iteratora i kończy sekwencję.

Zaletą tego podejścia jest to, że nie musimy pisać własnych klas implementujących `IEnumerator`, ponieważ kompilator robi to za nas automatycznie. Dodatkowo iterator zachowuje swój stan pomiędzy kolejnymi wywołaniami, dzięki czemu kod jest bardziej zwięzły i czytelny.

**Generowanie liczb pierwszych**

Sito Eratostenesa to klasyczny algorytm pozwalający na wyznaczenie wszystkich liczb pierwszych mniejszych od danej liczby `n`, czyli z zadanego przedziału `[2, n]`. Algorytm ten opiera się na eliminacji liczb złożonych.
{{% /hint %}}

### Opis zadania

Twoim zadaniem jest zaimplementowanie metody:

```csharp
public static IEnumerable<int> SieveOfEratosthenes(int upperBound);
```

wykorzystującej `yield break` oraz `yield return` do generowania liczb pierwszych na żądanie.

Przykładowe użycie powinno wyglądać następująco:

```csharp
foreach (var prime in SieveOfEratosthenes(1000))
{
    if (prime > 850) break;
    Console.WriteLine(prime);
}
```

{{% hint info %}}
**Materiały pomocnicze:**

- [Sieve of Eratosthenes](https://en.wikipedia.org/wiki/Sieve_of_Eratosthenes)
- [Microsoft Learn: yield statement - provide the next element](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/yield)
  {{% /hint %}}

### Przykładowe rozwiązanie

Przykładowe rozwiązanie można znaleźć w pliku [Task03.cs](/labs/lab05/solution/tasks/Task03.cs).

## `IEnumerable`, typy generyczne i LINQ

{{% hint info %}}
**LINQ w praktyce**

W C# mechanizm LINQ (_Language Integrated Query_) pozwala nam wygodnie zapisywać zapytania i operacje na sekwencjach danych (`IEnumerable<T>`) w spójny, deklaratywny sposób. Dzięki metodom rozszerzającym możemy natomiast dopisać do `IEnumerable<T>` własne operatory, które rozszerzają możliwości LINQ o często powtarzające się schematy przetwarzania, takie jak:

- `Fold(seed, func, resultSelector)` – uogólnia operację agregacji (redukcji), pozwalając na zachowanie stanu akumulatora między kolejnymi elementami i zwrócenie dowolnego wyniku końcowego.
- `Batch(size)` – dzieli sekwencję na kolejne porcje o zadanym rozmiarze `size`, przydatne np. przy wsadowym przesyłaniu danych do serwera.
- `SlidingWindow(size)` – tworzy nakładające się okna przesuwne o zadanym rozmiarze `size`, wykorzystywane np. przy wykrywaniu trendów.

Dzięki leniwej ewaluacji LINQ i metodom rozszerzającym, przetwarzanie może być zarówno czytelne, jak i wydajne – elementy są generowane i filtrowane dopiero wtedy, gdy są potrzebne.
{{% /hint %}}

### Opis zadania

Zadanie podzielone jest na trzy części. Każda część rozpoczyna się od implementacji własnej operacji, przypominającej istniejące metody biblioteki LINQ, a następnie rozwiązaniu kilku praktycznych problemów z wykorzystaniem danej operacji.

**Fold**

Zaimplementuj generyczną metodę rozszerzającą dla dowolnej sekwencji (`IEnumerable<T>`), która:

- Przyjmuje początkową wartość akumulatora `seed` (o typie potencjalnie innym niż elementy sekwencji).
- Przy każdej iteracji po elemencie sekwencji wywołuje przekazaną funkcję akumulującą, aktualizując stan akumulatora.
- Po przejściu całej sekwencji wywołuje funkcję przekształcającą końcowy stan akumulatora w wynik zwracany przez metodę.

Metoda powinna zwracać wynik tej ostatniej funkcji, a wszystkie kroki – inicjalizację akumulatora, pętlę po elementach i obliczenie końcowego rezultatu – należy zaimplementować ręcznie, używając jawnego obiektu enumeratora kolekcji.

**Wyzwania**

{{% details "Wyznaczanie statystyk dla kolekcji liczb całkowitych" false %}}
Zaimplementuj metodę rozszerzającą dla sekwencji liczb całkowitych, o następujących założeniach:

- Metoda nazywa się `ComputeStatistics`.
- Jeśli kolekcja jest równa `null` lub nie zawiera żadnych elementów, rzuca `ArgumentException` z komunikatem `Source sequence must contain at least one element.`.
- W jednym przebiegu (używając wcześniej zaimplementowanej metody `Fold`) oblicza:
  - wartość minimalną,
  - wartość maksymalną,
  - średnią arytmetyczną,
  - odchylenie standardowe.
- Zwraca krotkę `(min, max, average, standardDeviation)`.

Przykład użycia:

```csharp
var source = new[] { 2, 5, 3, 9, 4 };
var (min, max, average, std) = source.ComputeStatistics();

Console.WriteLine($"Min = {min}");            // 2
Console.WriteLine($"Max = {max}");            // 9
Console.WriteLine($"Average = {average:F2}"); // 4.60
Console.WriteLine($"StdDev = {std:F2}");      // 2.42
```

{{% /details %}}

{{% details "Znajdowanie najdłuższej sekwencji jednakowych elementów" false %}}
Zaimplementuj metodę rozszerzającą dla sekwencji liczb całkowitych, o następujących założeniach:

- Metoda nazywa się `LongestSequence`.
- Jeśli kolekcja jest równa `null` lub nie zawiera żadnych elementów, rzuca `ArgumentException` z komunikatem `Source sequence must contain at least one element.`.
- W jednym przebiegu (używając wcześniej zaimplementowanej metody `Fold`) znajduje maksymalnie długą, spójną podsekwencję jednakowych wartości, zwracając:
  - `start` – indeks pierwszego elementu tej podsekwencji,
  - `end` – indeks ostatniego elementu tej podsekwencji (włącznie),
  - `value` – wartość, która się powtarza.
- Indeksy są numerowane od zera i odnoszą się do oryginalnej sekwencji.

Przykład użycia:

```csharp
var source = new[] { 1, 1, 2, 2, 2, 3, 3, 2 };
//                   0  1  2  3  4  5  6  7

var (start, end, value) = source.LongestSequence();

Console.WriteLine($"Start = {start}"); // 2
Console.WriteLine($"End = {end}");     // 4
Console.WriteLine($"Value = {value}"); // 2

```

{{% /details %}}

**Batch**

**Wyzwania**

**SlidingWindow**

**Wyzwania**

### Przykładowe rozwiązanie

Przykładowe rozwiązanie można znaleźć w pliku [Task04.cs](/labs/lab05/solution/tasks/Task04.cs).
