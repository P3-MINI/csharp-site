---
title: "LINQ"
weight: 30
---

# Language INtegrated Query (LINQ)

**Language-Integrated Query (LINQ)** to technologia wprowadzająca jednolity sposób zapytań do języka C#. W praktyce jest to zbiór metod rozszerzających dla interfejsu `IEnumerable<T>`, pozwalający na odpytywanie i manipulowanie sekwencjami danych w sposób deklaratywny.

## Wewnętrzna implementacja

Implementacja metod LINQ jest zazwyczaj bardzo prosta - to metody iterująco-rozszerzające.

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

1.  **Składnia Metod (Method Syntax)**
    Używa łańcucha wywołań metod rozszerzających, takich jak `Where`, `Select` czy `OrderBy`. Jest to podstawowy i bardziej elastyczny sposób zapisu - nie wszystkie operatory LINQ mają swoje słowa kluczowe w składni zapytań.

2.  **Składnia Zapytań (Query Syntax)**
    Używa słów kluczowych podobnych do SQL (`from`, `where`, `select`), co czyni ją często bardziej czytelną przy złożonych operacjach filtrowania i łączenia.

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };

// 1. Query Syntax
var querySyntaxResult = from num in numbers
                        where num % 2 == 0
                        select num * num;

// 2. Method Syntax
var methodSyntaxResult = numbers.Where(num => num % 2 == 0)
                                .Select(num => num * num);

// Both queries produce the sequence { 4, 16, 36 }
```

### IEnumerable<T> vs IQueryable<T>

Choć oba interfejsy wyglądają podobnie, a `IQueryable<T>` dziedziczy po `IEnumerable<T>`, pod spodem działają w fundamentalnie różny sposób. Różnica ta jest kluczowa przy pracy ze zdalnymi źródłami danych, jak bazy danych.

*   **`IEnumerable<T>` (LINQ to Objects)**
    Operuje na **delegatach** (`Func<T>`), czyli na skompilowanym kodzie. Zapytanie jest wykonywane w pamięci aplikacji. Jeśli użyjesz go na tabeli z bazy danych, najpierw **wszystkie dane z tej tabeli zostaną pobrane do pamięci**, a dopiero potem filtrowanie i sortowanie odbędzie się w Twojej aplikacji.

*   **`IQueryable<T>` (np. LINQ to SQL)**
    Operuje na **drzewach wyrażeń** (`Expression<Func<T>>`), czyli na strukturze danych, która *opisuje* logikę zapytania. Dostawca LINQ (np. Entity Framework) analizuje to drzewo i **tłumaczy je na natywny język zapytania** (np. SQL). Dzięki temu cała operacja filtrowania, sortowania i grupowania jest wykonywana po stronie serwera bazy danych, a do aplikacji wracają tylko ostateczne wyniki.

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

## Odroczone wykonanie (Deferred Execution)

Jedną z najważniejszych cech LINQ jest **odroczone wykonanie**. Samo zdefiniowanie zapytania nie powoduje jego natychmiastowego uruchomienia. Zapytanie jest wykonywane dopiero wtedy, gdy faktycznie zażądamy wyników. Dzieje się to najczęściej podczas iteracji (np. w pętli `foreach`) lub po wywołaniu metody, która wymusza materializację kolekcji (np. `ToList()`, `ToArray()`, `Count()`). Pozwala to na budowanie skomplikowanych zapytań w sposób bardzo wydajny, bez tworzenia zbędnych kolekcji pośrednich.

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

## Przegląd metod

| Kategoria | Metody |
|---|---|
| **Filtrowanie** | `Where`, `Take`, `TakeLast`, `TakeWhile`, `Skip`, `SkipLast`, `SkipWhile`, `Distinct`, `DistinctBy` |
| **Projekcja** | `Select`, `SelectMany` |
| **Łączenie** | `Join`, `GroupJoin`, `Zip` |
| **Porządek** | `OrderBy`, `OrderByDescending`, `ThenBy`, `ThenByDescending`, `Reverse` |
| **Grupowanie** | `GroupBy`, `Chunk` |
| **Operacje na zbiorach** | `Concat`, `Union`, `UnionBy`, `Intersect`, `IntersectBy`, `Except`, `ExceptBy` |
| **Konwersja** | `OfType`, `Cast`, `ToArray`, `ToList`, `ToDictionary`, `ToLookup`, `AsEnumerable`, `AsQueryable` |
| **Wybór elementu** | `First`, `FirstOrDefault`, `Last`, `LastOrDefault`, `Single`, `SingleOrDefault`, `ElementAt`, `ElementAtOrDefault`, `MinBy`, `MaxBy`, `DefaultIfEmpty` |
| **Agregacja** | `Aggregate`, `Average`, `Count`, `LongCount`, `Sum`, `Max`, `Min`, `All`, `Any`, `Contains`, `SequenceEqual` |
| **Generacja** | `Empty`, `Range`, `Repeat` |

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

Projekcja polega na transformacji każdego elementu sekwencji w nowy inny element. Umożliwia to wyodrębnienie tylko potrzebnych właściwości z obiektów lub stworzenie zupełnie nowych struktur na podstawie danych wejściowych.

##### Metoda `Select`

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

#### Metoda `SelectMany`

Spłaszcza sekwencję sekwencji w jedną. Dla każdego elementu wejściowego tworzy sekwencję, a następnie spłaszcza je wszystkie w jedną.

```csharp
var sentences = new[] { "hello world", "how are you" };
var words = sentences.SelectMany(s => s.Split(' '));
// words contains { "hello", "world", "how", "are", "you" }
```

### Łączenie

Metody łączenia służą do kombinowania dwóch lub więcej sekwencji w jedną, na podstawie wspólnych kluczy lub pozycji. Jest to odpowiednik operacji `JOIN` znanych z baz danych.

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

#### Metoda `GroupJoin`

Łączy dwie sekwencje na podstawie pasujących kluczy, ale zachowuje grupowanie. Działa jak `LEFT OUTER JOIN` w SQL, gdzie dla każdego elementu z pierwszej (lewej) sekwencji dostajemy grupę pasujących elementów z drugiej.

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

#### Metoda `Zip`

Łączy dwie sekwencje "element po elemencie", tworząc nową sekwencję, której każdy element jest wynikiem funkcji zastosowanej do par elementów z obu sekwencji wejściowych. Długość wynikowej sekwencji jest równa długości krótszej z sekwencji wejściowych.

```csharp
var numbers = new[] { 1, 2, 3 };
var letters = new[] { "A", "B", "C", "D" };
var zipped = numbers.Zip(letters, (n, l) => $"{n}-{l}");
// zipped contains { "1-A", "2-B", "3-C" }
```

### Porządek

Ta kategoria zawiera metody do sortowania elementów w sekwencji. Można sortować rosnąco lub malejąco, a także definiować wielopoziomowe kryteria sortowania.

#### Metody `OrderBy` i `OrderByDescending`

Sortują elementy sekwencji w porządku rosnącym (`OrderBy`) lub malejącym (`OrderByDescending`).

```csharp
var numbers = new[] { 3, 1, 2 };
var sorted = numbers.OrderBy(n => n);
// sorted contains { 1, 2, 3 }
```

#### Metody `ThenBy` i `ThenByDescending`

Określają dodatkowe kryterium sortowania dla elementów, które są równe według `OrderBy`.

```csharp
var people = new[] { new { L = "Smith", F = "John" }, new { L = "Doe", F = "Jane" }, new { L = "Smith", F = "Anna" } };
var sorted = people.OrderBy(p => p.L).ThenBy(p => p.F);
// sorted: Anna Smith, Jane Doe, John Smith
```

#### Metoda `Reverse`

Odwraca kolejność elementów w sekwencji. `List<T>` i tablice definiują własne `Reverse`, które działa inaczej i odwraca kolejność elementów w miejscu.

```csharp
IEnumerable<int> numbers = [ 1, 2, 3 ];
foreach (int i in numbers.Reverse())
{
    Console.WriteLine(i); // 3, 2, 1
}
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

#### Metoda `Chunk`

Dzieli sekwencję na kawałki (*chunks*) o zadanym rozmiarze. Ostatni kawałek może być mniejszy.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5, 6, 7 };
var chunks = numbers.Chunk(3);
// chunks contains { {1,2,3}, {4,5,6}, {7} }
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

Tworzy sumę dwóch zbiorów, usuwając duplikaty. `UnionBy` pozwala określić klucz do porównywania unikalności.

```csharp
var seq1 = new[] { 1, 2 };
var seq2 = new[] { 2, 3 };
var result = seq1.Union(seq2);
// result contains { 1, 2, 3 }
```

#### Metody `Intersect` i `IntersectBy`

Tworzy część wspólną (iloczyn) dwóch zbiorów, zwracając tylko te elementy, które istnieją w obu kolekcjach.

```csharp
var seq1 = new[] { 1, 2 };
var seq2 = new[] { 2, 3 };
var result = seq1.Intersect(seq2);
// result contains { 2 }
```

#### Metody `Except` i `ExceptBy`

Tworzy różnicę zbiorów, zwracając elementy z pierwszej sekwencji, które nie występują w drugiej.

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

Materializują sekwencję, tworząc w pamięci konkretną kolekcję. `ToLookup` jest podobny do `ToDictionary`, ale pozwala na wiele wartości dla jednego klucza.

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

Pobierają jedyny element sekwencji. Rzucają wyjątkiem, jeśli sekwencja zawiera zero lub więcej niż jeden element. `SingleOrDefault` pozwala na pustą sekwencję (zwraca `default`), ale nadal rzuca wyjątkiem przy więcej niż jednym elemencie.

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

Zliczają elementy w sekwencji. `LongCount` jest używany, gdy liczba elementów może przekroczyć `Int32.MaxValue`.

```csharp
var count = new[] { 1, 2, 3 }.Count(); // 3
var evenCount = new[] { 1, 2, 3 }.Count(n => n % 2 == 0); // 1
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
