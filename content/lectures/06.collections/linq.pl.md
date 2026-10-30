---
title: "LINQ"
weight: 30
---

# Language INtegrated Query (LINQ)

**Language-Integrated Query (LINQ)** to technologia wprowadzająca do języka C# jednolity sposób formułowania zapytań. W praktyce jest to zbiór metod rozszerzających dla interfejsu `IEnumerable<T>`, pozwalający na odpytywanie i manipulowanie sekwencjami danych w sposób deklaratywny.

## Wewnętrzna implementacja

Implementacja metod LINQ jest zazwyczaj bardzo prosta - to metody rozszerzające zaimplementowane jako metody iterujące.

```csharp
public static IEnumerable<TSource> Where<TSource> 
    (this IEnumerable<TSource> source, Func<TSource,bool> predicate)
{
    foreach (TSource element in source)
        if (predicate(element))
            yield return element;
}
```

## Dwa style zapytań

LINQ oferuje dwa równoważne sposoby zapisu zapytań, które kompilator i tak sprowadza do tej samej formy – wywołań metod rozszerzających.

1.  **Składnia metod (*method syntax*)**
    Używa łańcucha wywołań metod rozszerzających, takich jak `Where`, `Select` czy `OrderBy`. Jest to podstawowy i bardziej elastyczny sposób zapisu - nie wszystkie operatory LINQ mają swoje słowa kluczowe w składni zapytań.

2.  **Składnia zapytań (*query syntax*)**
    Używa słów kluczowych podobnych do SQL (`from`, `where`, `select`), co czyni ją często bardziej czytelną przy złożonych operacjach filtrowania i łączenia.

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };

// 1. Method Syntax
var methodSyntaxResult = numbers.Where(num => num % 2 == 0)
                                .Select(num => num * num);

// 2. Query Syntax
var querySyntaxResult = from num in numbers
                        where num % 2 == 0
                        select num * num;

// Both queries produce the sequence { 4, 16, 36 }
```

### `IEnumerable<T>` vs `IQueryable<T>`

Choć oba interfejsy wyglądają podobnie, a `IQueryable<T>` dziedziczy po `IEnumerable<T>`, pod spodem działają w fundamentalnie różny sposób. Różnica ta jest kluczowa przy pracy ze zdalnymi źródłami danych, jak bazy danych.

*   **`IEnumerable<T>` (LINQ to Objects)**
    Operuje na **delegatach** (`Func<T>`), czyli na skompilowanym kodzie. Zapytanie jest wykonywane w pamięci aplikacji. Jeśli użyjesz go na tabeli z bazy danych, najpierw **wszystkie dane z tej tabeli zostaną pobrane do pamięci**, a dopiero potem filtrowanie i sortowanie odbędzie się w Twojej aplikacji.

*   **`IQueryable<T>` (np. LINQ to SQL)**
    Operuje na **drzewach wyrażeń** (`Expression<Func<T>>`), czyli na strukturze danych, która *opisuje* logikę zapytania. Dostawca LINQ (np. Entity Framework) analizuje to drzewo i **tłumaczy je na natywny język zapytań** (np. SQL). Dzięki temu cała operacja filtrowania, sortowania i grupowania jest wykonywana po stronie serwera bazy danych, a do aplikacji wracają tylko ostateczne wyniki.

Poniższy przykład ilustruje różnicę w praktyce:

```csharp
// Assume db.Products is an IQueryable<Product> from a database context.

// --- EFFICIENT: IQueryable ---
// The entire query is translated to SQL and executed by the database.
var efficientQuery = db.Products
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name)
    .Take(10);
// SQL generated is similar to: 
// SELECT TOP 10 * FROM Products WHERE Price > 100 ORDER BY Name

// --- INEFFICIENT: IEnumerable ---
// AsEnumerable() switches the context to in-memory execution.
var inefficientQuery = db.Products
    .AsEnumerable() // DANGER: All products are fetched from the database here!
    .Where(p => p.Price > 100) // This filtering happens in your app's memory.
    .OrderBy(p => p.Name)
    .Take(10);
// SQL generated is simply: SELECT * FROM Products
```

> [!NOTE]
> Od .NET 10 LINQ jest dostępny bez dodatkowych pakietów także dla sekwencji asynchronicznych (`IAsyncEnumerable<T>`): klasa `System.Linq.AsyncEnumerable` dostarcza te same operatory (np. `Where`, `Select`), a operatory zwracające pojedynczą wartość lub kolekcję mają asynchroniczne odpowiedniki, na które trzeba poczekać za pomocą `await` (np. `ToListAsync`, `CountAsync`). Wcześniej wymagało to pakietu NuGet `System.Linq.Async`. Więcej w rozdziale [Asynchroniczne sekwencje]({{< ref "/lectures/09.concurrency/asynchronous-streams" >}}).

## Odroczone wykonanie (Deferred Execution)

Jedną z najważniejszych cech LINQ jest **odroczone wykonanie**. Samo zdefiniowanie zapytania nie powoduje jego natychmiastowego uruchomienia. Zapytanie jest wykonywane dopiero wtedy, gdy faktycznie zażądamy wyników. Dzieje się to najczęściej podczas iteracji (np. w pętli `foreach`) lub po wywołaniu metody, która wymusza natychmiastowe wykonanie - materializuje wyniki w kolekcji (np. `ToList()`, `ToArray()`) albo oblicza pojedynczą wartość (np. `Count()`, `First()`). Pozwala to na budowanie skomplikowanych zapytań w sposób bardzo wydajny, bez tworzenia zbędnych kolekcji pośrednich.

```csharp
string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

IEnumerable<string> query = names
    .Where(n => n.Contains("a"))
    .OrderBy(n => n.Length)
    .Select(n => n.ToUpper());

// Query is not executed, unless enumerated:
foreach (var name in query)
{
    Console.WriteLine(name);
}
```

> [!NOTE]
> Odroczone wykonanie nie oznacza, że każdy operator przetwarza elementy pojedynczo. Operatory *strumieniowe* (np. `Where`, `Select`, `Take`) przekazują dalej każdy element, gdy tylko go odczytają, natomiast operatory *buforujące* (np. `OrderBy`, `GroupBy`, `Reverse`) muszą odczytać całe źródło przy pierwszym wywołaniu `MoveNext` - w powyższym przykładzie `OrderBy` sortuje wszystkie imiona, które przeszły przez filtr `Where`, zanim pierwsze z nich trafi do pętli `foreach`. Ponadto odroczone zapytanie jest wykonywane od nowa przy każdym przeglądaniu, dlatego jeśli jego wyniki są potrzebne wielokrotnie, lepiej zmaterializować je raz za pomocą `ToList()` lub `ToArray()`.

## Przegląd metod

| Kategoria | Metody |
|---|---|
| **Filtrowanie** | `Where`, `Take`, `TakeLast`, `TakeWhile`, `Skip`, `SkipLast`, `SkipWhile`, `Distinct`, `DistinctBy` |
| **Projekcja** | `Select`, `SelectMany`, `Index` |
| **Łączenie** | `Join`, `GroupJoin`, `LeftJoin`, `RightJoin`, `Zip` |
| **Porządek** | `OrderBy`, `OrderByDescending`, `Order`, `OrderDescending`, `ThenBy`, `ThenByDescending`, `Reverse`, `Shuffle` |
| **Grupowanie** | `GroupBy`, `Chunk`, `CountBy`, `AggregateBy` |
| **Operacje na zbiorach** | `Concat`, `Union`, `UnionBy`, `Intersect`, `IntersectBy`, `Except`, `ExceptBy` |
| **Konwersja** | `OfType`, `Cast`, `ToArray`, `ToList`, `ToDictionary`, `ToLookup`, `AsEnumerable`, `AsQueryable` |
| **Wybór elementu** | `First`, `FirstOrDefault`, `Last`, `LastOrDefault`, `Single`, `SingleOrDefault`, `ElementAt`, `ElementAtOrDefault`, `MinBy`, `MaxBy`, `DefaultIfEmpty` |
| **Agregacja** | `Aggregate`, `Average`, `Count`, `LongCount`, `TryGetNonEnumeratedCount`, `Sum`, `Max`, `Min`, `All`, `Any`, `Contains`, `SequenceEqual` |
| **Generacja** | `Empty`, `Range`, `Repeat`, `Sequence`, `InfiniteSequence` |

### Filtrowanie

Metody z tej kategorii służą do wybierania elementów z sekwencji, które spełniają określone warunki. Pozwalają na ograniczenie liczby wyników lub pomijanie niechcianych elementów.

#### Metoda `Where`

Filtruje sekwencję na podstawie predykatu (warunku). Zwraca nową sekwencję zawierającą tylko te elementy, dla których warunek jest prawdziwy.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var evenNumbers = numbers.Where(n => n % 2 == 0);
// evenNumbers contains { 2, 4 }
```

#### Metoda `Take`

Zwraca określoną liczbę elementów od początku sekwencji.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var firstThree = numbers.Take(3);
// firstThree contains { 1, 2, 3 }
```

#### Metoda `TakeLast`

Zwraca określoną liczbę elementów od końca sekwencji.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var lastTwo = numbers.TakeLast(2);
// lastTwo contains { 4, 5 }
```

#### Metoda `TakeWhile`

Zwraca elementy od początku sekwencji tak długo, jak spełniony jest określony warunek. Przestaje działać po napotkaniu pierwszego elementu, który nie spełnia warunku.

```csharp
var numbers = new[] { 1, 2, 3, 4, 1, 2 };
var lessThanFour = numbers.TakeWhile(n => n < 4);
// lessThanFour contains { 1, 2, 3 }
```

#### Metoda `Skip`

Pomija określoną liczbę elementów od początku sekwencji i zwraca pozostałe.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var afterFirstTwo = numbers.Skip(2);
// afterFirstTwo contains { 3, 4, 5 }
```

#### Metoda `SkipLast`

Pomija określoną liczbę elementów od końca sekwencji i zwraca pozostałe.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var allButLastTwo = numbers.SkipLast(2);
// allButLastTwo contains { 1, 2, 3 }
```

#### Metoda `SkipWhile`

Pomija elementy od początku sekwencji tak długo, jak spełniony jest określony warunek, a następnie zwraca pozostałe elementy.

```csharp
var numbers = new[] { 1, 2, 3, 4, 1, 2 };
var fourAndOnward = numbers.SkipWhile(n => n < 4);
// fourAndOnward contains { 4, 1, 2 }
```

#### Metoda `Distinct`

Zwraca sekwencję unikalnych elementów, usuwając duplikaty.

```csharp
var numbers = new[] { 1, 2, 2, 3, 1 };
var uniqueNumbers = numbers.Distinct();
// uniqueNumbers contains { 1, 2, 3 }
```

#### Metoda `DistinctBy`

Zwraca sekwencję unikalnych elementów na podstawie klucza wygenerowanego dla każdego elementu.

```csharp
var products = new[] { new { Name = "Apple", Category = "Fruit" }, new { Name = "Orange", Category = "Fruit" }, new { Name = "Carrot", Category = "Vegetable" } };
var uniqueCategories = products.DistinctBy(p => p.Category);
// Returns one product for each unique category
```

### Projekcja

Projekcja polega na transformacji każdego elementu sekwencji w nową postać. Umożliwia to wyodrębnienie tylko potrzebnych właściwości z obiektów lub stworzenie zupełnie nowych struktur na podstawie danych wejściowych.

#### Metoda `Select`

Jest to podstawowa operacja projekcji. Przekształca każdy element sekwencji w nową formę, zdefiniowaną przez selektor. Często używana do wyciągania pojedynczej właściwości lub tworzenia nowej wartości na podstawie obiektu.

```csharp
var people = new List<Person>
{
    new("John", "Smith", 42),
    new("Jane", "Doe", 35),
    new("Peter", "Jones", 50)
};

// Project the list of people into a sequence of formatted strings.
var fullNames = people.Select(p => $"{p.FirstName} {p.LastName}");

// Result:
// John Smith
// Jane Doe
// Peter Jones
foreach(var name in fullNames)
{
    Console.WriteLine(name);
}

public record Person(string FirstName, string LastName, int Age);
```

#### Metoda `Index` (.NET 9)

Łączy każdy element z jego pozycją, zwracając sekwencję krotek `(int Index, T Item)`. Jest czytelniejszą alternatywą dla przeciążenia `Select((item, index) => ...)`.

```csharp
var planets = new[] { "Mercury", "Venus", "Earth" };
foreach (var (index, planet) in planets.Index())
{
    Console.WriteLine($"{index}: {planet}");
}
// 0: Mercury
// 1: Venus
// 2: Earth
```

#### Metoda `SelectMany`

Spłaszcza sekwencję sekwencji w jedną. Dla każdego elementu wejściowego tworzy sekwencję, a następnie spłaszcza je wszystkie w jedną.

```csharp
var sentences = new[] { "hello world", "how are you" };
var words = sentences.SelectMany(s => s.Split(' '));
// words contains { "hello", "world", "how", "are", "you" }
```

### Łączenie

Metody łączenia służą do scalania dwóch lub więcej sekwencji w jedną, na podstawie wspólnych kluczy lub pozycji. Jest to odpowiednik operacji `JOIN` znanych z baz danych.

#### Metoda `Join`

Łączy dwie sekwencje na podstawie pasujących kluczy, działając jak `INNER JOIN` w SQL. Zwraca tylko te elementy, które mają dopasowanie w obu sekwencjach.

```csharp
var categories = new List<Category>
{
    new(1, "Electronics"),
    new(2, "Food"),
    new(3, "Toys") // This category has no products
};
var products = new List<Product>
{
    new("Laptop", 1),
    new("Milk", 2),
    new("Keyboard", 1),
    new("Bread", 2),
    new("Monitor", 1),
    new("Spaceship", 4) // This product has no category
};

var query = products.Join(categories,
    prod => prod.CategoryId,
    cat => cat.Id,
    (prod, cat) => new { prod.Name, CategoryName = cat.Name });

foreach (var item in query)
{
    Console.WriteLine($"- {item.Name}, {item.CategoryName}");
}

public record Category(int Id, string Name);
public record Product(string Name, int CategoryId);
```
Łączymy dwie listy po `products.CategoryId` i `categories.Id`. Wynik powyższego przykładu przedstawiony w formie tabeli wygląda następująco:

```text
+-----------+-------------+
| Product   | Category    |
+-----------+-------------+
| Laptop    | Electronics |
| Milk      | Food        |
| Keyboard  | Electronics |
| Bread     | Food        |
| Monitor   | Electronics |
+-----------+-------------+
```

#### Metoda `GroupJoin`

Łączy dwie sekwencje na podstawie pasujących kluczy, ale zachowuje grupowanie. Działa jak `LEFT OUTER JOIN` połączony z `GROUP BY` w SQL, gdzie dla każdego elementu z pierwszej (lewej) sekwencji dostajemy grupę pasujących elementów z drugiej.

```csharp
var categories = new List<Category>
{
    new(1, "Electronics"),
    new(2, "Food"),
    new(3, "Toys") // This category has no products
};
var products = new List<Product>
{
    new("Laptop", 1),
    new("Milk", 2),
    new("Keyboard", 1),
    new("Spaceship", 4) // This product has no category
};

var query = categories.GroupJoin(products,
    cat => cat.Id,
    prod => prod.CategoryId,
    (cat, prods) => new { Category = cat.Name, Products = prods });

foreach (var group in query)
{
    Console.WriteLine($"Category: {group.Category}");
    if (group.Products.Any())
    {
        foreach (var product in group.Products)
        {
            Console.WriteLine($"  - {product.Name}");
        }
    }
    else
    {
        Console.WriteLine("  - (No products in this category)");
    }
}

public record Category(int Id, string Name);
public record Product(string Name, int CategoryId);
```

Wynik powyższego przykładu:

```text
Category: Electronics
  - Laptop
  - Keyboard
Category: Food
  - Milk
Category: Toys
  - (No products in this category)
```

#### Metoda `Zip`

Łączy dwie sekwencje "element po elemencie", tworząc nową sekwencję, której każdy element jest wynikiem funkcji zastosowanej do par elementów z obu sekwencji wejściowych. Długość wynikowej sekwencji jest równa długości krótszej z sekwencji wejściowych.

```csharp
var numbers = new[] { 1, 2, 3 };
var letters = new[] { "A", "B", "C", "D" };
var zipped = numbers.Zip(letters, (n, l) => $"{n}-{l}");
// zipped contains { "1-A", "2-B", "3-C" }
```

Bez selektora wyniku `Zip` zwraca sekwencję krotek. Od .NET 6 potrafi też połączyć jednocześnie trzy sekwencje:

```csharp
var numbers = new[] { 1, 2, 3 };
var letters = new[] { "A", "B", "C", "D" };
var symbols = new[] { 'x', 'y', 'z' };
var pairs = numbers.Zip(letters);
// pairs contains { (1, "A"), (2, "B"), (3, "C") }
var triples = numbers.Zip(letters, symbols);
// triples contains { (1, "A", 'x'), (2, "B", 'y'), (3, "C", 'z') }
```

#### Metody `LeftJoin` i `RightJoin` (.NET 10)

`LeftJoin` działa jak `LEFT OUTER JOIN` w SQL: zwraca wszystkie elementy pierwszej (lewej) sekwencji, a dla elementu bez dopasowania w drugiej sekwencji selektor wyniku otrzymuje `default` (np. `null`) zamiast elementu wewnętrznego. `RightJoin` działa analogicznie, ale zachowuje wszystkie elementy drugiej (prawej) sekwencji. Przed .NET 10 złączenie lewostronne trzeba było wyrażać, łącząc `GroupJoin`, `SelectMany` i `DefaultIfEmpty`.

```csharp
var categories = new List<Category>
{
    new(1, "Electronics"),
    new(2, "Food"),
    new(3, "Toys") // This category has no products
};
var products = new List<Product>
{
    new("Laptop", 1),
    new("Milk", 2),
    new("Keyboard", 1),
    new("Spaceship", 4) // This product has no category
};

var leftJoin = categories.LeftJoin(products,
    cat => cat.Id,
    prod => prod.CategoryId,
    (cat, prod) => $"{cat.Name}: {prod?.Name ?? "(none)"}");
// leftJoin contains { "Electronics: Laptop", "Electronics: Keyboard", "Food: Milk", "Toys: (none)" }

var rightJoin = categories.RightJoin(products,
    cat => cat.Id,
    prod => prod.CategoryId,
    (cat, prod) => $"{prod.Name}: {cat?.Name ?? "(none)"}");
// rightJoin contains { "Laptop: Electronics", "Milk: Food", "Keyboard: Electronics", "Spaceship: (none)" }

public record Category(int Id, string Name);
public record Product(string Name, int CategoryId);
```

### Porządek

Ta kategoria zawiera metody do sortowania elementów w sekwencji. Można sortować rosnąco lub malejąco, a także definiować wielopoziomowe kryteria sortowania.

#### Metody `OrderBy` i `OrderByDescending`

Sortują elementy sekwencji w porządku rosnącym (`OrderBy`) lub malejącym (`OrderByDescending`). Sortowanie jest stabilne: elementy o równych kluczach zachowują swoją pierwotną względną kolejność (w przeciwieństwie do `List<T>.Sort` i `Array.Sort`, które są niestabilne - podobnie jak `std::stable_sort` i `std::sort` w C++).

```csharp
var numbers = new[] { 3, 1, 2 };
var sorted = numbers.OrderBy(n => n);
// sorted contains { 1, 2, 3 }
```

#### Metody `Order` i `OrderDescending` (.NET 7)

Sortują same elementy (używając domyślnego komparatora), bez selektora klucza. Są skrótem dla `OrderBy(x => x)` i `OrderByDescending(x => x)`.

```csharp
var numbers = new[] { 3, 1, 2 };
var ascending = numbers.Order();
// ascending contains { 1, 2, 3 }
var descending = numbers.OrderDescending();
// descending contains { 3, 2, 1 }
```

#### Metody `ThenBy` i `ThenByDescending`

Określają dodatkowe kryterium sortowania dla elementów, które są równe według `OrderBy`.

```csharp
var people = new[] { new { L = "Smith", F = "John" }, new { L = "Doe", F = "Jane" }, new { L = "Smith", F = "Anna" } };
var sorted = people.OrderBy(p => p.L).ThenBy(p => p.F);
// sorted: Jane Doe, Anna Smith, John Smith
```

#### Metoda `Reverse`

Odwraca kolejność elementów w sekwencji. `List<T>` definiuje własną metodę instancyjną `Reverse` (a dla tablic istnieje statyczna metoda `Array.Reverse`), która działa inaczej: odwraca kolejność elementów w miejscu i zwraca `void`. Ponieważ metoda instancyjna ma pierwszeństwo przed metodą rozszerzającą, wywołanie `Reverse()` na zmiennej typu `List<T>` nie wywołałoby metody LINQ - stąd zmienna typu `IEnumerable<int>` w przykładzie.

```csharp
IEnumerable<int> numbers = new[] { 1, 2, 3 };
foreach (int i in numbers.Reverse())
{
    Console.WriteLine(i); // 3, 2, 1
}
```

#### Metoda `Shuffle` (.NET 10)

Zwraca elementy sekwencji w losowej kolejności. Jak inne metody LINQ, nie modyfikuje źródła. Do przetasowania tablicy w miejscu można użyć `Random.Shared.Shuffle(array)` (.NET 8).

```csharp
var numbers = Enumerable.Range(1, 5);
var shuffled = numbers.Shuffle();
// shuffled contains e.g. { 3, 1, 5, 2, 4 } - a new random order on each enumeration
```

### Grupowanie

Grupowanie pozwala na organizowanie elementów sekwencji w grupy na podstawie wspólnego klucza. Każda grupa zawiera klucz oraz kolekcję wszystkich elementów, które do niej należą.

#### Metoda `GroupBy`

Grupuje elementy na podstawie klucza. Wynikiem jest sekwencja grup (`IGrouping<TKey, TElement>`).

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var groups = numbers.GroupBy(n => n % 2 == 0 ? "Even" : "Odd");
// Creates two groups: one for "Odd" key with {1,3,5}, one for "Even" key with {2,4}
```

Metodę `GroupBy` często stosuje się razem z [funkcjami agregującymi](#agregacja).
W poniższym przykładzie użyto jej razem z `Count` do zliczenia produktów w każdej kategorii.

```csharp
var categories = new List<Category>
{
    new(1, "Electronics"),
    new(2, "Food"),
    new(3, "Toys") 
};
var products = new List<Product>
{
    new("Laptop", 1),
    new("Milk", 2),
    new("Keyboard", 1),
    new("Bread", 2),
    new("Monitor", 1),
    new("Spaceship", 4) 
};

var counts = products
    .GroupBy(p => p.CategoryId)
    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
    .OrderBy(x => x.CategoryId);

// Print as a table
Console.WriteLine("+------------+-------+");
Console.WriteLine("| CategoryId | Count |");
Console.WriteLine("+------------+-------+");
foreach (var row in counts)
    Console.WriteLine($"| {row.CategoryId,-10} | {row.Count,5} |");
Console.WriteLine("+------------+-------+");

public record Category(int Id, string Name);
public record Product(string Name, int CategoryId);
```

Wynik powyższego przykładu:

```text
+------------+-------+
| CategoryId | Count |
+------------+-------+
| 1          |     3 |
| 2          |     2 |
| 4          |     1 |
+------------+-------+
```

#### Metoda `Chunk`

Dzieli sekwencję na kawałki (*chunks*) o zadanym rozmiarze. Ostatni kawałek może być mniejszy.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5, 6, 7 };
var chunks = numbers.Chunk(3);
// chunks contains { {1,2,3}, {4,5,6}, {7} }
```

#### Metody `CountBy` i `AggregateBy` (.NET 9)

`CountBy` zlicza elementy dla każdego klucza, a `AggregateBy` agreguje elementy dla każdego klucza (tak jak `Aggregate`) - bez tworzenia pośrednich grup. Obie zwracają sekwencję `KeyValuePair<TKey, TValue>`. Zatem `products.GroupBy(p => p.CategoryId).Select(g => new { g.Key, Count = g.Count() })` można zastąpić przez `products.CountBy(p => p.CategoryId)`.

```csharp
var products = new List<Product>
{
    new("Laptop", 1, 3500m),
    new("Milk", 2, 4m),
    new("Keyboard", 1, 200m),
    new("Bread", 2, 6m),
    new("Monitor", 1, 900m)
};

foreach (var (categoryId, count) in products.CountBy(p => p.CategoryId))
{
    Console.WriteLine($"Category {categoryId}: {count} products");
}
// Category 1: 3 products
// Category 2: 2 products

var totalPrices = products.AggregateBy(
    keySelector: p => p.CategoryId,
    seed: 0m,
    func: (total, p) => total + p.Price);
// totalPrices contains { [1, 4600], [2, 10] }

public record Product(string Name, int CategoryId, decimal Price);
```

### Operacje na zbiorach

Metody te wykonują operacje znane z teorii mnogości, traktując sekwencje jako zbiory. Pozwalają na znajdowanie sumy, części wspólnej czy różnicy dwóch kolekcji, często z uwzględnieniem unikalności elementów.

#### Metoda `Concat`

Łączy dwie sekwencje w jedną. Zachowuje wszystkie elementy, w tym duplikaty.

```csharp
var seq1 = new[] { 1, 2 };
var seq2 = new[] { 2, 3 };
var result = seq1.Concat(seq2);
// result contains { 1, 2, 2, 3 }
```

#### Metody `Union` i `UnionBy`

Tworzą sumę dwóch zbiorów, usuwając duplikaty. `UnionBy` pozwala określić klucz do porównywania unikalności.

```csharp
var seq1 = new[] { 1, 2 };
var seq2 = new[] { 2, 3 };
var result = seq1.Union(seq2);
// result contains { 1, 2, 3 }
```

#### Metody `Intersect` i `IntersectBy`

Tworzą część wspólną (iloczyn) dwóch zbiorów, zwracając tylko te (unikalne) elementy, które istnieją w obu kolekcjach.

```csharp
var seq1 = new[] { 1, 2 };
var seq2 = new[] { 2, 3 };
var result = seq1.Intersect(seq2);
// result contains { 2 }
```

#### Metody `Except` i `ExceptBy`

Tworzą różnicę zbiorów, zwracając (unikalne) elementy z pierwszej sekwencji, które nie występują w drugiej.

```csharp
var seq1 = new[] { 1, 2, 3 };
var seq2 = new[] { 2 };
var result = seq1.Except(seq2);
// result contains { 1, 3 }
```

### Konwersja

Metody konwersji służą do zmiany typu kolekcji lub jej natychmiastowego wykonania (ewaluacji). Pozwalają na przykład na przekształcenie dowolnej sekwencji `IEnumerable<T>` w konkretną implementację listy, tablicy lub słownika.

#### Metoda `OfType`

Filtruje elementy sekwencji na podstawie ich typu.

```csharp
var mixed = new ArrayList { 1, "hello", 3.0, new object() };
var integers = mixed.OfType<int>();
// integers contains { 1 }
```

#### Metoda `Cast`

Rzutuje wszystkie elementy sekwencji na określony typ. Rzuca wyjątkiem, jeśli rzutowanie dla któregokolwiek elementu się nie powiedzie.

```csharp
var mixed = new ArrayList { 1, 2, 3 };
var integers = mixed.Cast<int>();
// integers contains { 1, 2, 3 }
```

#### Metody `ToArray`, `ToList`, `ToDictionary`, `ToLookup`

Materializują sekwencję, tworząc w pamięci konkretną kolekcję. `ToLookup` jest podobna do `ToDictionary`, ale pozwala na wiele wartości dla jednego klucza.

```csharp
var numbers = Enumerable.Range(1, 3);
List<int> list = numbers.ToList();
Dictionary<int, int> dict = numbers.ToDictionary(k => k, v => v * v);
// dict contains { 1:1, 2:4, 3:9 }
```

#### Metody `AsEnumerable` i `AsQueryable`

Rzutują kolekcję na interfejs `IEnumerable<T>` lub `IQueryable<T>`. Głównym zastosowaniem `AsEnumerable()` jest świadoma zmiana kontekstu zapytania z `IQueryable` na `IEnumerable`, aby dalsze operacje były wykonywane w pamięci aplikacji, a nie w zewnętrznym źródle danych (np. bazie danych).

```csharp
// Assume db.Products is an IQueryable<Product> from a database context
var productNames = db.Products
    .Where(p => p.IsAvailable) // This part is translated to SQL
    .AsEnumerable() // Switch to in-memory execution
    .Select(p => p.Name.ToUpper()) // This part is executed in the .NET runtime
    .ToList();
```

### Wybór elementu

Te metody służą do wyciągnięcia z sekwencji jednego, konkretnego elementu. Pozwalają na pobranie pierwszego, ostatniego lub jedynego elementu, który spełnia warunek, lub elementu na określonej pozycji.

#### Metody `First` i `FirstOrDefault`

Pobierają pierwszy element sekwencji. `First` rzuca wyjątkiem, jeśli sekwencja jest pusta, a `FirstOrDefault` zwraca w takim przypadku wartość domyślną (np. `null`).

```csharp
var numbers = new[] { 10, 20, 30 };
int first = numbers.First(); // 10
int firstOrDefault = new int[0].FirstOrDefault(); // 0
```

#### Metody `Last` i `LastOrDefault`

Działają analogicznie do `First`/`FirstOrDefault`, ale dla ostatniego elementu sekwencji.

```csharp
var numbers = new[] { 10, 20, 30 };
int last = numbers.Last(); // 30
```

#### Metody `Single` i `SingleOrDefault`

Pobierają jedyny element sekwencji. `Single` rzuca wyjątkiem, jeśli sekwencja zawiera zero lub więcej niż jeden element. `SingleOrDefault` pozwala na pustą sekwencję (zwraca `default`), ale nadal rzuca wyjątkiem przy więcej niż jednym elemencie.

```csharp
var singleItem = new[] { 42 }.Single(); // 42
// new[] { 1, 2 }.Single(); // Throws InvalidOperationException
```

#### Metody `ElementAt` i `ElementAtOrDefault`

Pobierają element na konkretnym indeksie. `ElementAt` rzuca wyjątkiem, jeśli indeks jest poza zakresem, a `ElementAtOrDefault` zwraca wartość domyślną.

```csharp
var letters = new[] { "A", "B", "C" };
string b = letters.ElementAt(1); // "B"
```

#### Metody `MinBy` i `MaxBy`

Zwracają element z sekwencji, który ma minimalną lub maksymalną wartość na podstawie podanego klucza.

```csharp
var people = new[] { new { Name = "Anna", Age = 20 }, new { Name = "John", Age = 35 } };
var youngest = people.MinBy(p => p.Age);
// youngest is the object for Anna
```

#### Metoda `DefaultIfEmpty`

Zwraca elementy sekwencji lub kolekcję z pojedynczą wartością domyślną, jeśli sekwencja jest pusta.

```csharp
var empty = new int[0];
var withDefault = empty.DefaultIfEmpty(100);
// withDefault contains { 100 }
```

### Agregacja

Agregacja polega na wykonaniu obliczeń na całej sekwencji w celu uzyskania pojedynczej wartości. Metody te pozwalają na obliczenie sumy, średniej, znalezienie wartości minimalnej/maksymalnej czy sprawdzenie, czy wszystkie/jakiekolwiek elementy spełniają warunek.

#### Metoda `Aggregate`

Wykonuje ogólną operację agregacji na sekwencji. Pozwala na zaimplementowanie własnej logiki, np. zsumowanie lub złączenie elementów w niestandardowy sposób.

```csharp
var numbers = new[] { 1, 2, 3, 4 };
int product = numbers.Aggregate((current, next) => current * next);
// product is 1 * 2 * 3 * 4 = 24
```

#### Metody `Average`, `Sum`, `Max`, `Min`

Obliczają odpowiednio średnią, sumę, wartość maksymalną lub minimalną z elementów sekwencji.

```csharp
var numbers = new[] { 10, 20, 30 };
double average = numbers.Average(); // 20
int sum = numbers.Sum(); // 60
```

#### Metody `Count` i `LongCount`

Zliczają elementy w sekwencji. `LongCount` jest używana, gdy liczba elementów może przekroczyć `Int32.MaxValue`.

```csharp
var count = new[] { 1, 2, 3 }.Count(); // 3
var evenCount = new[] { 1, 2, 3 }.Count(n => n % 2 == 0); // 1
```

`Count()` może wymagać przejścia przez całą sekwencję (O(n)) - chyba że źródłem jest kolekcja, która zna swój rozmiar. `TryGetNonEnumeratedCount` (.NET 6) zwraca liczbę elementów tylko wtedy, gdy da się ją ustalić bez przeglądania sekwencji (np. dla `ICollection<T>`), a w przeciwnym razie zwraca `false`:

```csharp
IEnumerable<int> list = new List<int> { 1, 2, 3 };
bool known = list.TryGetNonEnumeratedCount(out int count); // true, count == 3

IEnumerable<int> filtered = list.Where(n => n > 1);
bool knownFiltered = filtered.TryGetNonEnumeratedCount(out int filteredCount); // false, filteredCount == 0
```

#### Metody `All`, `Any`, `Contains`

Sprawdzają warunki logiczne. `All` zwraca `true`, jeśli wszystkie elementy spełniają warunek. `Any` zwraca `true`, jeśli jakikolwiek element spełnia warunek. `Contains` sprawdza, czy sekwencja zawiera określony element.

```csharp
var numbers = new[] { 1, 3, 5 };
bool allOdd = numbers.All(n => n % 2 != 0); // true
bool anyEven = numbers.Any(n => n % 2 == 0); // false
bool hasThree = numbers.Contains(3); // true
```

#### Metoda `SequenceEqual`

Sprawdza, czy dwie sekwencje są równe, tzn. zawierają te same elementy w tej samej kolejności.

```csharp
var seq1 = new[] { 1, 2, 3 };
var seq2 = new[] { 1, 2, 3 };
bool areEqual = seq1.SequenceEqual(seq2); // true
```

### Generacja

Metody te służą do tworzenia nowych, prostych sekwencji. Pozwalają na wygenerowanie pustej kolekcji, sekwencji liczb w danym zakresie lub kolekcji zawierającej powtórzony element.

#### Metoda `Empty`

Tworzy pustą sekwencję określonego typu.

```csharp
var emptySequence = Enumerable.Empty<string>();
```

#### Metoda `Range`

Generuje sekwencję liczb całkowitych w podanym zakresie.

```csharp
var tenToTwelve = Enumerable.Range(10, 3);
// tenToTwelve contains { 10, 11, 12 }
```

#### Metoda `Repeat`

Tworzy sekwencję, która zawiera jeden, powtórzony element określoną liczbę razy.
```csharp
var threes = Enumerable.Repeat(3, 5);
// threes contains { 3, 3, 3, 3, 3 }
```

#### Metody `Sequence` i `InfiniteSequence` (.NET 10)

Uogólniają `Range` na dowolny typ liczbowy i krok. `Sequence(start, endInclusive, step)` generuje wartości aż do `endInclusive` włącznie, a `InfiniteSequence(start, step)` generuje nieskończoną sekwencję, którą trzeba ograniczyć, np. za pomocą `Take`.

```csharp
var evens = Enumerable.Sequence(0, 10, 2);
// evens contains { 0, 2, 4, 6, 8, 10 }
var halves = Enumerable.InfiniteSequence(0.0, 0.5).Take(4);
// halves contains { 0, 0.5, 1, 1.5 }
```
