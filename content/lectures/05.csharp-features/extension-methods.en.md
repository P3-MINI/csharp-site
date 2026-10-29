---
title: "Extension Methods"
---

# Extension Methods

Extension methods allow you to add methods to existing types without modifying their code. This is particularly useful if you want to extend the functionality of a type to which you do not have access. They also allow you to divide and group the functionality of an object across several classes.

Extension methods must be defined in a static class. The first parameter specifies the type being extended and must be preceded by the `this` keyword.

```csharp
public static class StringExtensions
{
    public static int WordCount(this string str)
    {
        return str.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
```

Although extension methods are defined as static methods, they can be called as if they were instance methods of the extended type.

```csharp
string greeting = "The quick brown fox jumps over the lazy dog.";
int j = greeting.WordCount();
```

Extending interfaces is particularly useful, as such methods become available to all types that implement the given interface. This is the principle on which LINQ (*Language INtegrated Query*) operates - it defines extension methods for the `IEnumerable<T>` type, which in turn is implemented by all collections. Thanks to this, collections have methods like `Where`, `Select`, `GroupBy`, etc., defined. Since most of these methods also return `IEnumerable<T>`, they can be chained together in a single sequence of calls (*fluent syntax*).

```csharp
List<int> numbers = [5, 10, 3, 8, 1];
IEnumerable<int> query = numbers
                            .Where(n => n % 2 == 0)
                            .OrderBy(n => n)
                            .Select(n => n * n);
foreach (int n in query)
{
    Console.WriteLine(n);
}
```

## Extension Blocks (C# 14)

C# 14 introduces *extension members*, declared in `extension` blocks inside a static class. The parameter of the block (`string s` below) is the *receiver*: it specifies the extended type and is accessible in all instance members of the block. Extension blocks allow you to add not only extension methods to a type but also properties, operators, and static members.

```csharp
using System.Text;

public static class StringExtensions
{
    extension(string s)
    {
        public static string operator *(string str, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
            }
            if (count == 0 || string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(str.Length * count);
            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        public bool IsEmpty => s.Length == 0;

        public int WordCount()
        {
            return s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
```

Usage:

```csharp
string str = "Quick brown fox jumps over the lazy dog.";
Console.WriteLine("ab" * 3);        // ababab
Console.WriteLine(str.IsEmpty);     // False
Console.WriteLine(str.WordCount()); // 8
```

Operators are static members, so `operator *` does not use the receiver `s` – both operands are its parameters, and at least one of them must be of the extended type.

> [!NOTE]
> Extension members cannot add state to a type: an extension block cannot contain fields or auto-implemented properties. Extension properties must therefore be computed from the receiver, like `IsEmpty` above.

### Static Extension Members

Static extension members are called on the extended type itself, as if they were its own static members. If a block contains only static members, the receiver does not need a name:

```csharp
public static class DateTimeExtensions
{
    extension(DateTime)
    {
        public static DateTime Tomorrow => DateTime.Today.AddDays(1);

        public static DateTime FromUnixSeconds(long seconds) => DateTime.UnixEpoch.AddSeconds(seconds);
    }
}
```

```csharp
DateTime deadline = DateTime.Tomorrow;
Console.WriteLine(DateTime.FromUnixSeconds(1_000_000_000).ToString("o")); // 2001-09-09T01:46:40.0000000Z
```

### Generic Extension Blocks

An extension block can be generic. Its type parameters are inferred from the receiver, just like for generic extension methods, so the following members are available on any sequence:

```csharp
public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public bool IsEmpty => !source.Any();

        public IEnumerable<T> WhereNot(Func<T, bool> predicate) => source.Where(x => !predicate(x));
    }
}
```

```csharp
List<int> numbers = [5, 10, 3, 8, 1];
Console.WriteLine(numbers.IsEmpty);                                 // False
Console.WriteLine(string.Join(", ", numbers.WhereNot(n => n > 4))); // 3, 1
```

### `ref` Receivers

If the extended type is a value type, the receiver can be passed by reference (`ref`, `in` or `ref readonly`), just like the first parameter of a classic `this ref` extension method. A `ref` receiver allows the extension to modify the original variable instead of its copy:

```csharp
public static class CounterExtensions
{
    extension(ref int number)
    {
        public void Increment() => number++;
    }
}
```

```csharp
int counter = 5;
counter.Increment();
Console.WriteLine(counter); // 6
```

### Compatibility and Lookup Rules

An extension method declared in a block compiles to the same IL as a classic `this` method. Callers cannot tell the difference, and the method can still be called as an ordinary static method, e.g., `StringExtensions.WordCount(str)`. Existing extension methods can therefore be rewritten in the new syntax without breaking source or binary compatibility, and both styles can be mixed in a single static class.

Extension members of both kinds are considered only if the extended type itself has no applicable member with the given name – **instance members always win**. For example, an extension property `Length` declared for `string` would never be used, because `string` already has its own `Length` property (the compiler does not even warn about it). Also, as with classic extension methods, extension members are visible only if the namespace of their static class is in scope (e.g., imported with a `using` directive).

## Trivia

We can extend non-iterable types with a method that returns an iterator.

```csharp
public static class IntExtensions
{
    public static IEnumerator<int> GetEnumerator(this int i)
    {
        while (i-- > 0)
        {
            yield return i;
        }
    }
}
```

This allows them to be used in a `foreach` loop.

```csharp
foreach (int i in 10)
{
    Console.WriteLine(i);
}
```

Similarly, we can add the ability for a type to be used with the `await` operator: [await anything](https://devblogs.microsoft.com/dotnet/await-anything/).
