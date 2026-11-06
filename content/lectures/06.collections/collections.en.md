--- 
title: "Collections"
weight: 20
---

# Collections

The collections built into the C# standard library are organized into different namespaces:

* **System.Collections** - contains non-generic collections that use the `object` type to store objects.
* **System.Collections.Generic** - the most commonly used, generic collections, which we will be focusing on.
* **System.Collections.Concurrent** - generic, thread-safe collections.
* **System.Collections.Immutable** - generic collections that do not allow modification.
* **System.Collections.ObjectModel** - provides templates for custom collections.

## Collection Hierarchy

The base for all collections is the `IEnumerable<T>` interface, which only guarantees that an object can be iterated over. This can be an infinite sequence of elements generated on the fly.

`ICollection<T>` extends `IEnumerable<T>` and represents a modifiable, finite collection.

Further in the hierarchy, we have only specialized collections: lists (`IList<T>`), dictionaries (`IDictionary<TKey, TValue>`), and sets (`ISet<T>`).

```mermaid
---
config:
  class:
    hideEmptyMembersBox: true
---
classDiagram
IEnumerable~T~ --> ICollection~T~
ICollection~T~ --> IList~T~
ICollection~T~ --> IDictionary~TKey, TValue~ : implements<br>ICollection&ltKeyValuePair&ltTKey,TValue&gt&gt
ICollection~T~ --> ISet~T~
IEnumerable~T~ ..> IEnumerator~T~ : GetEnumerator()
class IEnumerable~T~{
    <<interface>>
}
class IEnumerator~T~{
    <<interface>>
}
class ICollection~T~{
    <<interface>>
}
class IList~T~{
    <<interface>>
}
class IDictionary~TKey, TValue~{
    <<interface>>
}
class ISet~T~{
    <<interface>>
}
```

### The `ICollection<T>` Interface

`ICollection<T>` represents a modifiable, finite collection to which elements can be added and removed. It extends the possibility of iteration with basic operations of adding and removing elements, checking their count (`Count`), and whether a given element is in the collection (`Contains`).

```csharp
public interface ICollection<T> : IEnumerable<T>
{
    int Count { get; }
    bool IsReadOnly { get; }
    void Add(T item);
    bool Remove(T item);
    bool Contains(T item);
    void Clear();
    void CopyTo(T[] array, int arrayIndex);
}
```

### The `IList<T>` Interface

The list interface additionally introduces the ability to reference elements using an index.

```csharp
public interface IList<T> : ICollection<T>
{
    T this [int index] { get; set; }
    int IndexOf(T item);
    void Insert(int index, T item);
    void RemoveAt(int index);
}
```

#### The `List<T>` Class

The most popular concrete implementation of this interface is `List<T>`. Underneath, it is a dynamically expanding array, similar to the `std::vector` type from C++. The `List<T>` class additionally provides several useful methods: `Sort`, `BinarySearch`, `Find`.

```csharp
var planets = new List<string> { "Mercury", "Earth", "Mars" };
Console.WriteLine("Initial list:");
planets.ForEach(p => Console.WriteLine($"- {p}"));

planets.Insert(1, "Venus");

var gasGiants = new List<string> { "Jupiter", "Saturn", "Uranus", "Neptune" };
planets.AddRange(gasGiants);

planets.RemoveAt(planets.Count - 1);

bool hasMars = planets.Contains("Mars");
Console.WriteLine($"\nDoes the list contain Mars? {hasMars}");

string? sPlanet = planets.Find(p => p.StartsWith("S"));
Console.WriteLine($"Found planet starting with 'S': {sPlanet}");

int indexOfEarth = planets.IndexOf("Earth");
Console.WriteLine($"Index of Earth in the list: {indexOfEarth}");

planets.Sort();

Console.WriteLine("\nFinal list of planets (sorted):");
foreach (var planet in planets)
{
    Console.WriteLine($"- {planet}");
}

planets.Clear();
Console.WriteLine($"\nNumber of planets after clearing: {planets.Count}");
```

> Arrays (`T[]`) also implement the `IList<T>` interface. The `Add`, `Remove`, `Insert`, `RemoveAt`, and `Clear` methods throw a `NotSupportedException`, as these are operations that change the size of the collection.

### The `IDictionary<TKey, TValue>` Interface

Dictionaries are collections of key-value pairs in which the keys are unique. The dictionary interface provides two properties that allow access to a collection of only the keys (`Keys`) or only the values (`Values`). Most often, working with a dictionary involves using the indexer. We can use it to add new values, update existing ones, and retrieve existing values. Accessing a value that is not in the dictionary throws a `KeyNotFoundException`, so if we are not sure if a key exists, it is better to use the `TryGetValue` method. Inserting a new value or updating a value using the indexer always succeeds. The `Add` method, on the other hand, throws an `ArgumentException` if the key is already in the dictionary.

```csharp
public interface IDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
{
    TValue this[TKey key] { get; set; }
    ICollection<TKey> Keys { get; }
    ICollection<TValue> Values { get; }
    void Add(TKey key, TValue value);
    bool Remove(TKey key);
    bool ContainsKey(TKey key);
    bool TryGetValue(TKey key, out TValue value);
}
```

#### The `Dictionary<TKey, TValue>` Class

The most commonly used implementation of a dictionary is `Dictionary<TKey, TValue>`. Its internal implementation is a hash table, which corresponds to the `std::unordered_map` type from C++. In C#, every object has an `Equals` and `GetHashCode` method, making it a candidate for being a key in such a dictionary. Of course, these methods must be implemented correctly for `Dictionary<TKey, TValue>` to work properly.

- **If two objects are equal (according to `Equals()`), they must return the same hash code (from `GetHashCode()`)**.
  - The reverse rule does not have to be met.
- `GetHashCode` and `Equals` should be implemented together.
- `GetHashCode` and `Equals` should use the same fields.
- The key's hash must not change; the fields used by `GetHashCode` should be immutable.

Example of a correct implementation of `GetHashCode` and `Equals`:

```csharp
public sealed class BookId : IEquatable<BookId>
{
    public string Isbn { get; }
    public string Format { get; }

    public BookId(string isbn, string format)
    {
        Isbn = isbn;
        Format = format;
    }

    public bool Equals(BookId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Isbn == other.Isbn && Format == other.Format;
    }

    public override bool Equals(object? obj)
    {
        return obj is BookId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Isbn, Format);
    }
}
```

Example usage of the `Dictionary<TKey, TValue>` implementation:

```csharp
var openWith = new Dictionary<string, string>();

openWith.Add("txt", "notepad.exe");
openWith.Add("bmp", "paint.exe");
openWith.Add("dib", "paint.exe");
openWith.Add("rtf", "wordpad.exe");

try
{
    openWith.Add("txt", "vim.exe");
}
catch (ArgumentException)
{
    Console.WriteLine("An element with Key = \"txt\" already exists.");
}

Console.WriteLine($"Initial value for 'rtf': {openWith["rtf"]}");
openWith["rtf"] = "winword.exe"; // This updates the value.
Console.WriteLine($"New value for 'rtf': {openWith["rtf"]}");

openWith["doc"] = "winword.exe"; // This adds a new value.

if (openWith.TryGetValue("tif", out string? tifProgram))
{
    Console.WriteLine($"Value for 'tif': {tifProgram}");
}
else
{
    Console.WriteLine("Key 'tif' is not found.");
}

if (!openWith.ContainsKey("ht"))
{
    openWith.Add("ht", "hypertrm.exe");
    Console.WriteLine($"Value added for key 'ht': {openWith["ht"]}");
}

Console.WriteLine("\n--- Dictionary Contents ---");
foreach (var pair in openWith)
{
    Console.WriteLine($"Key: {pair.Key}, Value: {pair.Value}");
}

Console.WriteLine("\n--- Just Values ---");
foreach (var value in openWith.Values)
{
    Console.WriteLine($"Value: {value}");
}

Console.WriteLine("\n--- Just Keys ---");
foreach (var key in openWith.Keys)
{
    Console.WriteLine($"Key: {key}");
}

Console.WriteLine("\nRemoving 'doc'...");
if (openWith.Remove("doc"))
{
    Console.WriteLine("Key 'doc' removed from the dictionary.");
}
```

> If we want the dictionary to be sorted by keys, there is also the `SortedDictionary<TKey, TValue>` implementation, which provides this. It is the equivalent of `std::map` from C++, internally using red-black trees, so lookup, insertion, and removal take O(log n) time. A similar collection, `SortedList<TKey, TValue>`, keeps the pairs in sorted arrays instead - it uses less memory, but insertion and removal take O(n) time.

##### The `OrderedDictionary<TKey, TValue>` Class (.NET 9)

Since .NET 9, there is also `OrderedDictionary<TKey, TValue>` - a dictionary that preserves the insertion order of its elements and additionally allows accessing them by index (`GetAt`, `SetAt`, `Insert`, `IndexOf`, `RemoveAt`). Previously, only the non-generic `System.Collections.Specialized.OrderedDictionary` was available.

```csharp
var steps = new OrderedDictionary<string, int>
{
    ["one"] = 1,
    ["two"] = 2
};
steps.Insert(0, "zero", 0);
steps.Add("three", 3);
// steps contains [zero, 0], [one, 1], [two, 2], [three, 3] - in this order

KeyValuePair<string, int> second = steps.GetAt(1); // [one, 1]
int index = steps.IndexOf("two"); // 2
```

##### Alternate Lookup (.NET 9)

A `Dictionary<string, TValue>` (or a `HashSet<string>`) can also be searched using a key of a different type - most often `ReadOnlySpan<char>`, which avoids allocating a new string for each lookup (a string is created only when a new key is added). Such a view of the collection is obtained with the `GetAlternateLookup<TAlternateKey>()` method. It requires a comparer that supports the alternate key type - the default string comparer and `StringComparer.Ordinal`/`StringComparer.OrdinalIgnoreCase` do.

```csharp
var counts = new Dictionary<string, int>();
var lookup = counts.GetAlternateLookup<ReadOnlySpan<char>>();

ReadOnlySpan<char> text = "to be or not to be";
foreach (Range range in text.Split(' '))
{
    ReadOnlySpan<char> word = text[range]; // no string allocation
    lookup[word] = lookup.TryGetValue(word, out int count) ? count + 1 : 1;
}
// counts contains [to, 2], [be, 2], [or, 1], [not, 1]
```

### The `ISet<T>` Interface

The `ISet<T>` interface represents a set in the mathematical sense. It stores only unique elements - duplicates are automatically ignored. The `Add` method returns `true` if the element was added, and `false` if the element already existed in the set. The rest of the methods in the set are set operations or methods for checking relationships between sets.

```csharp
public interface ISet<T> : ICollection<T>
{
    bool Add(T item);
    void UnionWith(IEnumerable<T> other);
    void IntersectWith(IEnumerable<T> other);
    void ExceptWith(IEnumerable<T> other);
    void SymmetricExceptWith(IEnumerable<T> other);
    bool IsSubsetOf(IEnumerable<T> other);
    bool IsSupersetOf(IEnumerable<T> other);
    bool IsProperSupersetOf(IEnumerable<T> other);
    bool IsProperSubsetOf(IEnumerable<T> other);
    bool Overlaps(IEnumerable<T> other);
    bool SetEquals(IEnumerable<T> other);
}
```

The most popular implementations of this interface are the `HashSet<T>` class, which operates based on a hash table (like `Dictionary`), and `SortedSet<T>`, which operates based on red-black trees (like `SortedDictionary`).

### Other Collections

Besides the main branches of the hierarchy, there are other important collections that are not lists, dictionaries, or sets:

- `LinkedList<T>` - A generic doubly-linked list. Unlike `List<T>`, it is optimized for frequent addition and removal of elements at any place in the list, as this does not require shifting subsequent elements (with a reference to a `LinkedListNode<T>`, these operations take O(1) time).
- `Queue<T>` - Implements a **FIFO** (First-In, First-Out) queue, internally using a **dynamically sized array**. The main methods are `Enqueue` (adds to the end) and `Dequeue` (retrieves from the beginning).
- `Stack<T>` - Implements a **LIFO** (Last-In, First-Out) stack, also internally using a **dynamically sized array**. The main methods are `Push` (adds to the top) and `Pop` (retrieves from the top).
- `PriorityQueue<TElement, TPriority>` - Implements a priority queue where elements are dequeued based on an associated priority rather than FIFO order. The main methods are `Enqueue` and `Dequeue`. It is a min-heap: the element with the **lowest** priority value is dequeued first (unlike `std::priority_queue` from C++, which by default returns the largest element).

Of these, only `LinkedList<T>` implements `ICollection<T>`. `Queue<T>` and `Stack<T>` implement `IReadOnlyCollection<T>` (and thus `IEnumerable<T>`), but not `ICollection<T>` - elements can be added and removed only at their ends, so they do not provide `Add` and `Remove`. In turn, `PriorityQueue<TElement, TPriority>` does not even implement `IEnumerable<T>` - its elements can be viewed (in no particular order) through the `UnorderedItems` property.

> It is also worth mentioning `BitArray` from the `System.Collections` namespace. It is a specialized, **non-generic** collection that allows for the efficient storage and manipulation of an array of bits (true/false values), using only one bit for each value.

Since .NET 9, `PriorityQueue<TElement, TPriority>` also has a `Remove` method, which removes a given element (found in O(n) time) and returns it along with its priority. Since the queue does not support changing the priority of an element in place, it can be done by removing the element and enqueuing it again:

```csharp
var tasks = new PriorityQueue<string, int>();
tasks.Enqueue("write report", 2);
tasks.Enqueue("fix bug", 1);
tasks.Enqueue("drink coffee", 3);

// The report has become urgent - change its priority.
if (tasks.Remove("write report", out _, out _))
{
    tasks.Enqueue("write report", 0);
}

while (tasks.TryDequeue(out string? task, out int priority))
{
    Console.WriteLine($"{priority}: {task}");
}
// 0: write report
// 1: fix bug
// 3: drink coffee
```

### Read-Only and Frozen Collections

Next to the interfaces of modifiable collections, there are read-only interfaces: `IReadOnlyCollection<T>` (`Count`), `IReadOnlyList<T>` (read-only indexer), `IReadOnlyDictionary<TKey, TValue>` and, since .NET 5, `IReadOnlySet<T>` (`Contains` and the methods checking relationships between sets). They are implemented by the standard collections (`List<T>`, `Dictionary<TKey, TValue>`, `HashSet<T>`, ...), so they are a good choice for parameters and return types when the code should only read the collection.

The `System.Collections.ObjectModel` namespace provides wrappers implementing these interfaces: `ReadOnlyCollection<T>` (returned, e.g., by `List<T>.AsReadOnly()`), `ReadOnlyDictionary<TKey, TValue>` and, since .NET 9, `ReadOnlySet<T>`. Such a wrapper is only a read-only **view**: it does not copy the elements, so changes made to the underlying collection are visible through it.

```csharp
using System.Collections.ObjectModel;

var set = new HashSet<int> { 1, 2 };
var readOnlySet = new ReadOnlySet<int>(set);
// readOnlySet.Add(3); // Compilation error: ReadOnlySet<T> has no Add method
set.Add(3);
Console.WriteLine(readOnlySet.Count); // 3 - the change is visible through the view
```

Truly unmodifiable collections are provided by the `System.Collections.Immutable` namespace (`ImmutableList<T>`, `ImmutableDictionary<TKey, TValue>`, ... - "modifying" operations return a new collection) and, since .NET 8, by `System.Collections.Frozen`: `FrozenDictionary<TKey, TValue>` and `FrozenSet<T>`. Frozen collections are created once from existing data (`ToFrozenDictionary()`, `ToFrozenSet()`). Their creation is relatively expensive, because the internal structure is optimized for the particular set of keys, but lookups are then typically faster than in `Dictionary<TKey, TValue>` and `HashSet<T>`. They are well suited for data created once (e.g., at application startup) and read many times afterwards.

```csharp
using System.Collections.Frozen;

FrozenSet<string> keywords = new[] { "if", "else", "while", "for" }.ToFrozenSet();
FrozenDictionary<string, int> httpCodes = new Dictionary<string, int>
{
    ["OK"] = 200,
    ["Not Found"] = 404
}.ToFrozenDictionary();

Console.WriteLine(keywords.Contains("while")); // True
Console.WriteLine(httpCodes["Not Found"]); // 404
```

### Collection Initializers

This syntax, available for a long time, requires the use of `new` and curly braces `{...}`. The compiler translates it into a series of `Add` method calls or insertions via an indexer (since C# 6.0).

```csharp
// Initializer for a List<T>
var numbers = new List<int> { 1, 2, 3 };

// Initializer for a Dictionary<TKey, TValue> (using pairs)
var cityPopulations = new Dictionary<string, int>
{
    { "Tokyo", 37_000_000 },
    { "Delhi", 32_000_000 }
};

// For types defining an indexer (C# 6.0):
var cityPopulationsByIndexer = new Dictionary<string, int>
{
    ["Shanghai"] = 28_000_000,
    ["London"] = 9_000_000
};
```

### Collection Expressions (C# 12)

Starting with C# 12, a new, even more concise syntax known as **collection expressions** was introduced. A collection expression is a terse syntax using square brackets `[...]` that, when evaluated, can be assigned to many different collection types. The compiler automatically infers how to insert the provided elements into the collection.

```csharp
List<int> numbers1 = [1, 2, 3, 4, 5];

int[] numbers2 = [4, 5, 6, 7, 8];

// Spread operator `..` to concatenate collections.
SortedSet<int> combined = [9, ..numbers1, ..numbers2, 10];
// 'combined' contains { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } in order without repetitions
```

The target type can also be an interface (e.g., `IEnumerable<T>`, `IReadOnlyList<T>`) - the compiler then chooses a suitable implementation - or a span. However, a collection expression has no type of its own, so the target type must be known:

```csharp
IEnumerable<int> sequence = [1, 2, 3];
ReadOnlySpan<char> vowels = ['a', 'e', 'i', 'o', 'u'];
List<string> empty = [];
// var numbers3 = [1, 2, 3]; // Compilation error: there is no target type for the collection expression
```
