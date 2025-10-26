---
title: "LINQ"
weight: 30
---

# Language INtegrated Query (LINQ)

**Language-Integrated Query (LINQ)** is a technology that introduces a uniform way of querying into the C# language. In practice, it is a set of extension methods for the `IEnumerable<T>` interface, allowing for the querying and manipulation of data sequences in a declarative way.

## Internal Implementation

The implementation of LINQ methods is usually very simple - they are iterating-extension methods.

```csharp
public static IEnumerable<TSource> Where<TSource> 
    (this IEnumerable<TSource> source, Func<TSource,bool> predicate)
{
    foreach (TSource element in source)
        if (predicate(element))
            yield return element;
}
```

## Two Query Styles

LINQ offers two equivalent ways of writing queries, which the compiler translates into the same form anyway – calls to extension methods.

1.  **Method Syntax**
    Uses a chain of extension method calls, such as `Where`, `Select`, or `OrderBy`. This is the primary and more flexible way of writing, as not all LINQ operators have their own keywords in query syntax.

2.  **Query Syntax**
    Uses SQL-like keywords (`from`, `where`, `select`), which often makes it more readable for complex filtering and joining operations.

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

Although both interfaces look similar, and `IQueryable<T>` inherits from `IEnumerable<T>`, they work in a fundamentally different way underneath. This difference is crucial when working with remote data sources, like databases.

*   **`IEnumerable<T>` (LINQ to Objects)**
    Operates on **delegates** (`Func<T>`), i.e., on compiled code. The query is executed in the application's memory. If you use it on a database table, first **all data from that table will be fetched into memory**, and only then will the filtering and sorting take place in your application.

*   **`IQueryable<T>` (e.g., LINQ to SQL)**
    Operates on **expression trees** (`Expression<Func<T>>`), i.e., on a data structure that *describes* the query's logic. The LINQ provider (e.g., Entity Framework) analyzes this tree and **translates it into the native query language** (e.g., SQL). Thanks to this, the entire filtering, sorting, and grouping operation is performed on the database server side, and only the final results are returned to the application.

The following example illustrates the difference in practice:

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

## Deferred Execution

One of the most important features of LINQ is **deferred execution**. Simply defining a LINQ query does not cause it to execute immediately. The query is executed only when we actually request the results. This most often happens during iteration (e.g., in a `foreach` loop) or after calling a method that forces the materialization of the collection (e.g., `ToList()`, `ToArray()`, `Count()`). This allows for building complex queries in a very efficient way, without creating unnecessary intermediate collections.

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

## Method Overview

| Category | Methods |
|---|---|
| **Filtering** | `Where`, `Take`, `TakeLast`, `TakeWhile`, `Skip`, `SkipLast`, `SkipWhile`, `Distinct`, `DistinctBy` |
| **Projection** | `Select`, `SelectMany` |
| **Joining** | `Join`, `GroupJoin`, `Zip` |
| **Ordering** | `OrderBy`, `OrderByDescending`, `ThenBy`, `ThenByDescending`, `Reverse` |
| **Grouping** | `GroupBy`, `Chunk` |
| **Set Operations** | `Concat`, `Union`, `UnionBy`, `Intersect`, `IntersectBy`, `Except`, `ExceptBy` |
| **Conversion** | `OfType`, `Cast`, `ToArray`, `ToList`, `ToDictionary`, `ToLookup`, `AsEnumerable`, `AsQueryable` |
| **Element Selection** | `First`, `FirstOrDefault`, `Last`, `LastOrDefault`, `Single`, `SingleOrDefault`, `ElementAt`, `ElementAtOrDefault`, `MinBy`, `MaxBy`, `DefaultIfEmpty` |
| **Aggregation** | `Aggregate`, `Average`, `Count`, `LongCount`, `Sum`, `Max`, `Min`, `All`, `Any`, `Contains`, `SequenceEqual` |
| **Generation** | `Empty`, `Range`, `Repeat` |

### Filtering

Methods in this category are used to select elements from a sequence that meet specific conditions. They allow for limiting the number of results or skipping unwanted elements.

#### The `Where` Method

Filters a sequence based on a predicate (condition). Returns a new sequence containing only those elements for which the condition is true.
```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var evenNumbers = numbers.Where(n => n % 2 == 0);
// evenNumbers contains { 2, 4 }
```

#### The `Take` Method

Returns a specified number of elements from the start of a sequence.
```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var firstThree = numbers.Take(3);
// firstThree contains { 1, 2, 3 }
```

#### The `TakeLast` Method

Returns a specified number of elements from the end of a sequence.
```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var lastTwo = numbers.TakeLast(2);
// lastTwo contains { 4, 5 }
```

#### The `TakeWhile` Method

Returns elements from the start of a sequence as long as a specified condition is true. It stops processing upon encountering the first element that does not meet the condition.
```csharp
var numbers = new[] { 1, 2, 3, 4, 1, 2 };
var lessThanFour = numbers.TakeWhile(n => n < 4);
// lessThanFour contains { 1, 2, 3 }
```

#### The `Skip` Method

Bypasses a specified number of elements from the start of a sequence and returns the remaining elements.
```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var afterFirstTwo = numbers.Skip(2);
// afterFirstTwo contains { 3, 4, 5 }
```

#### The `SkipLast` Method

Bypasses a specified number of elements from the end of a sequence and returns the remaining elements.
```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var allButLastTwo = numbers.SkipLast(2);
// allButLastTwo contains { 1, 2, 3 }
```

#### The `SkipWhile` Method

Bypasses elements from the start of a sequence as long as a specified condition is true, and then returns the remaining elements.
```csharp
var numbers = new[] { 1, 2, 3, 4, 1, 2 };
var fourAndOnward = numbers.SkipWhile(n => n < 4);
// fourAndOnward contains { 4, 1, 2 }
```

#### The `Distinct` Method

Returns a sequence of unique elements, removing duplicates.
```csharp
var numbers = new[] { 1, 2, 2, 3, 1 };
var uniqueNumbers = numbers.Distinct();
// uniqueNumbers contains { 1, 2, 3 }
```

#### The `DistinctBy` Method

Returns a sequence of unique elements based on a key generated for each element.
```csharp
var products = new[] { new { Name = "Apple", Category = "Fruit" }, new { Name = "Orange", Category = "Fruit" }, new { Name = "Carrot", Category = "Vegetable" } };
var uniqueCategories = products.DistinctBy(p => p.Category);
// Returns one product for each unique category
```

### Projection

Projection involves transforming each element of a sequence into a new form. This allows for extracting only the necessary properties from objects or creating entirely new structures based on the input data.

##### The `Select` Method

This is the basic projection operation. It transforms each element of a sequence into a new form, defined by the selector. It is often used to extract a single property or create a new value based on an object.

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

#### The `SelectMany` Method

Flattens a sequence of sequences into one. For each input element, it creates a sequence, and then flattens all of them into a single sequence.
```csharp
var sentences = new[] { "hello world", "how are you" };
var words = sentences.SelectMany(s => s.Split(' '));
// words contains { "hello", "world", "how", "are", "you" }
```

### Joining

Joining methods are used to combine two or more sequences into one, based on common keys or positions. This is the equivalent of `JOIN` operations known from databases.

#### The `Join` Method

Joins two sequences based on matching keys, acting like an `INNER JOIN` in SQL. It returns only those elements that have a match in both sequences.

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

#### The `GroupJoin` Method

Joins two sequences based on matching keys but preserves the grouping. It acts like a `LEFT OUTER JOIN` in SQL, where for each element from the first (left) sequence, we get a group of matching elements from the second.

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

#### The `Zip` Method

Joins two sequences "element by element", creating a new sequence where each element is the result of a function applied to pairs of elements from both input sequences. The length of the resulting sequence is equal to the length of the shorter of the input sequences.

```csharp
var numbers = new[] { 1, 2, 3 };
var letters = new[] { "A", "B", "C", "D" };
var zipped = numbers.Zip(letters, (n, l) => $"{n}-{l}");
// zipped contains { "1-A", "2-B", "3-C" }
```

### Ordering

This category contains methods for sorting the elements of a sequence. You can sort in ascending or descending order, and also define multi-level sorting criteria.

#### The `OrderBy` and `OrderByDescending` Methods

Sort the elements of a sequence in ascending (`OrderBy`) or descending (`OrderByDescending`) order.

```csharp
var numbers = new[] { 3, 1, 2 };
var sorted = numbers.OrderBy(n => n);
// sorted contains { 1, 2, 3 }
```

#### The `ThenBy` and `ThenByDescending` Methods

Specify an additional sorting criterion for elements that are equal according to `OrderBy`.

```csharp
var people = new[] { new { L = "Smith", F = "John" }, new { L = "Doe", F = "Jane" }, new { L = "Smith", F = "Anna" } };
var sorted = people.OrderBy(p => p.L).ThenBy(p => p.F);
// sorted: Anna Smith, Jane Doe, John Smith
```

#### The `Reverse` Method

Reverses the order of elements in a sequence. `List<T>` and arrays define their own `Reverse` method, which works differently and reverses the elements in-place.

```csharp
IEnumerable<int> numbers = new[] { 1, 2, 3 };
foreach (int i in numbers.Reverse())
{
    Console.WriteLine(i); // 3, 2, 1
}
```

### Grouping

Grouping allows for organizing the elements of a sequence into groups based on a common key. Each group contains the key and a collection of all elements that belong to it.

#### The `GroupBy` Method

Groups elements based on a key. The result is a sequence of groups (`IGrouping<TKey, TElement>`).

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var groups = numbers.GroupBy(n => n % 2 == 0 ? "Even" : "Odd");
// Creates two groups: one for "Odd" key with {1,3,5}, one for "Even" key with {2,4}
```

#### The `Chunk` Method

Divides a sequence into chunks of a given size. The last chunk may be smaller.

```csharp
var numbers = new[] { 1, 2, 3, 4, 5, 6, 7 };
var chunks = numbers.Chunk(3);
// chunks contains { {1,2,3}, {4,5,6}, {7} }
```

### Set Operations

These methods perform operations known from set theory, treating sequences as sets. They allow for finding the union, intersection, or difference of two collections, often taking into account the uniqueness of elements.

#### The `Concat` Method

Concatenates two sequences into one. It preserves all elements, including duplicates.

```csharp
var seq1 = new[] { 1, 2 };
var seq2 = new[] { 2, 3 };
var result = seq1.Concat(seq2);
// result contains { 1, 2, 2, 3 }
```

#### The `Union` and `UnionBy` Methods

Creates the set union of two sequences, removing duplicates. `UnionBy` allows specifying a key for comparing uniqueness.

```csharp
var seq1 = new[] { 1, 2 };
var seq2 = new[] { 2, 3 };
var result = seq1.Union(seq2);
// result contains { 1, 2, 3 }
```

#### The `Intersect` and `IntersectBy` Methods

Creates the set intersection of two sequences, returning only those elements that exist in both collections.

```csharp
var seq1 = new[] { 1, 2 };
var seq2 = new[] { 2, 3 };
var result = seq1.Intersect(seq2);
// result contains { 2 }
```

#### The `Except` and `ExceptBy` Methods

Creates the set difference of two sequences, returning elements from the first sequence that do not appear in the second.

```csharp
var seq1 = new[] { 1, 2, 3 };
var seq2 = new[] { 2 };
var result = seq1.Except(seq2);
// result contains { 1, 3 }
```

### Conversion

Conversion methods are used to change the type of a collection or to execute (evaluate) it immediately. They allow, for example, transforming any `IEnumerable<T>` sequence into a concrete implementation like a list, array, or dictionary.

#### The `OfType` Method

Filters the elements of a sequence based on their type.

```csharp
var mixed = new ArrayList { 1, "hello", 3.0, new object() };
var integers = mixed.OfType<int>();
// integers contains { 1 }
```

#### The `Cast` Method

Casts all elements of a sequence to a specified type. It throws an exception if the cast for any element fails.

```csharp
var mixed = new ArrayList { 1, 2, 3 };
var integers = mixed.Cast<int>();
// integers contains { 1, 2, 3 }
```

#### The `ToArray`, `ToList`, `ToDictionary`, `ToLookup` Methods

Materialize a sequence, creating a concrete collection in memory. `ToLookup` is similar to `ToDictionary` but allows for multiple values for a single key.

```csharp
var numbers = Enumerable.Range(1, 3);
List<int> list = numbers.ToList();
Dictionary<int, int> dict = numbers.ToDictionary(k => k, v => v * v);
// dict contains { 1:1, 2:4, 3:9 }
```

#### The `AsEnumerable` and `AsQueryable` Methods

Casts a collection to the `IEnumerable<T>` or `IQueryable<T>` interface. The main use of `AsEnumerable()` is to consciously change the query context from `IQueryable` to `IEnumerable`, so that further operations are executed in the application's memory, not in the external data source (e.g., a database).

```csharp
// Assume db.Products is an IQueryable<Product> from a database context
var productNames = db.Products
    .Where(p => p.IsAvailable) // This part is translated to SQL
    .AsEnumerable() // Switch to in-memory execution
    .Select(p => p.Name.ToUpper()) // This part is executed in the .NET runtime
    .ToList();
```

### Element Selection

These methods are used to extract a single, specific element from a sequence. They allow for retrieving the first, last, or only element that meets a condition, or the element at a specific position.

#### The `First` and `FirstOrDefault` Methods

Retrieve the first element of a sequence. `First` throws an exception if the sequence is empty, while `FirstOrDefault` returns the default value in that case (e.g., `null`).

```csharp
var numbers = new[] { 10, 20, 30 };
int first = numbers.First(); // 10
int firstOrDefault = new int[0].FirstOrDefault(); // 0
```

#### The `Last` and `LastOrDefault` Methods

Work analogously to `First`/`FirstOrDefault`, but for the last element of a sequence.

```csharp
var numbers = new[] { 10, 20, 30 };
int last = numbers.Last(); // 30
```

#### The `Single` and `SingleOrDefault` Methods

Retrieve the only element of a sequence. They throw an exception if the sequence contains zero or more than one element. `SingleOrDefault` allows for an empty sequence (returns `default`), but still throws an exception for more than one element.

```csharp
var singleItem = new[] { 42 }.Single(); // 42
// new[] { 1, 2 }.Single(); // Throws InvalidOperationException
```

#### The `ElementAt` and `ElementAtOrDefault` Methods

Retrieve the element at a specific index. `ElementAt` throws an exception if the index is out of range, while `ElementAtOrDefault` returns the default value.

```csharp
var letters = new[] { "A", "B", "C" };
string b = letters.ElementAt(1); // "B"
```

#### The `MinBy` and `MaxBy` Methods

Return the element from a sequence that has the minimum or maximum value based on a given key.

```csharp
var people = new[] { new { Name = "Anna", Age = 20 }, new { Name = "John", Age = 35 } };
var youngest = people.MinBy(p => p.Age);
// youngest is the object for Anna
```

#### The `DefaultIfEmpty` Method

Returns the elements of a sequence or a collection with a single default value if the sequence is empty.

```csharp
var empty = new int[0];
var withDefault = empty.DefaultIfEmpty(100);
// withDefault contains { 100 }
```

### Aggregation

Aggregation involves performing a calculation on an entire sequence to obtain a single value. These methods allow for calculating the sum, average, finding the minimum/maximum value, or checking if all/any elements meet a condition.

#### The `Aggregate` Method

Performs a general aggregation operation on a sequence. It allows for implementing custom logic, e.g., summing or joining elements in a non-standard way.

```csharp
var numbers = new[] { 1, 2, 3, 4 };
int product = numbers.Aggregate((current, next) => current * next);
// product is 1 * 2 * 3 * 4 = 24
```

#### The `Average`, `Sum`, `Max`, `Min` Methods

Calculate the average, sum, maximum, or minimum value of the elements in a sequence, respectively.

```csharp
var numbers = new[] { 10, 20, 30 };
double average = numbers.Average(); // 20
int sum = numbers.Sum(); // 60
```

#### The `Count` and `LongCount` Methods

Count the elements in a sequence. `LongCount` is used when the number of elements might exceed `Int32.MaxValue`.

```csharp
var count = new[] { 1, 2, 3 }.Count(); // 3
var evenCount = new[] { 1, 2, 3 }.Count(n => n % 2 == 0); // 1
```

#### The `All`, `Any`, `Contains` Methods

Check logical conditions. `All` returns `true` if all elements satisfy a condition. `Any` returns `true` if any element satisfies a condition. `Contains` checks if a sequence contains a specific element.

```csharp
var numbers = new[] { 1, 3, 5 };
bool allOdd = numbers.All(n => n % 2 != 0); // true
bool anyEven = numbers.Any(n => n % 2 == 0); // false
bool hasThree = numbers.Contains(3); // true
```

#### The `SequenceEqual` Method

Checks if two sequences are equal, i.e., they contain the same elements in the same order.

```csharp
var seq1 = new[] { 1, 2, 3 };
var seq2 = new[] { 1, 2, 3 };
bool areEqual = seq1.SequenceEqual(seq2); // true
```

### Generation

These methods are used to create new, simple sequences. They allow for generating an empty collection, a sequence of numbers in a given range, or a collection containing a repeated element.

#### The `Empty` Method

Creates an empty sequence of a specified type.

```csharp
var emptySequence = Enumerable.Empty<string>();
```

#### The `Range` Method

Generates a sequence of integers within a specified range.

```csharp
var tenToTwelve = Enumerable.Range(10, 3);
// tenToTwelve contains { 10, 11, 12 }
```

#### The `Repeat` Method

Creates a sequence that contains one, repeated element a specified number of times.
```csharp
var threes = Enumerable.Repeat(3, 5);
// threes contains { 3, 3, 3, 3, 3 }
```
