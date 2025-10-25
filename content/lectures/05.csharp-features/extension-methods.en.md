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

Extension blocks allow you to add not only extension methods to a type but also properties, operators, and static members.

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
Console.WriteLine(str * 3);
Console.WriteLine(str.IsEmpty);
Console.WriteLine(str.WordCount());
```

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
