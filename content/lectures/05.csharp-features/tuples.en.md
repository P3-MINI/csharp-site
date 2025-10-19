---
title: "Tuples"
---

# Tuples

A tuple is a type that stores several subtypes, similar to `std::tuple` in C++. In C#, the tuple type is the generic `System.ValueTuple<T1, T2, ...>` structure. Tuples in C# have significant language support, with special syntax for both declaration and creation. The types `t1` and `t2` below are the same type - `(double, int, string)` is a syntax alias for `ValueTuple<double, int, string>`.

```csharp
(double, int, string) t1 = (4.5, 3, "Bob");
ValueTuple<double, int, string> t2 = new ValueTuple<double, int, string>(4.5, 3, "Bob");
```

By default, a tuple's fields are named `Item1`, `Item2`, and so on.

```csharp
(double, int, string) t1 = (4.5, 3, "Bob");
Console.WriteLine($"double: {t1.Item1}, int: {t1.Item2}, string: {t1.Item3}");
```

> There is also a reference type, `System.Tuple<T1, T2, ...>`. It was an earlier, less successful experiment in bringing tuples to the language, left in the standard library for compatibility. Value types are much more efficient for the scenarios where tuples are typically used.

## Naming Elements

You can give tuple elements more descriptive names than the generic `ItemX`.

```csharp
(double Length, int Count, string Name) t1 = (4.5, 3, "Bob");
var t2 = (Length: 4.5, Count: 3, Name: "Bob");
Console.WriteLine($"double: {t1.Length}, int: {t1.Count}, string: {t1.Name}");
```

If you don't explicitly name the tuple elements, the compiler will try to give them friendly names based on the variables or properties used during creation:

```csharp
int count = 3;
string name = "Alice";
var t1 = (DateTime.Now, count, name);
Console.WriteLine($"DateTime: {t1.Now}, int: {t1.count}, string: {t1.name}");
```

> The friendly names of elements do not affect the tuple's type. If the element types in a tuple match, it is considered the same type, regardless of the names.

## Comparing Tuples

The `==` and `!=` operators are overloaded and perform a simple element-by-element comparison using the `==` and `!=` operators of the elements themselves:

```csharp
(int a, byte b) left = (5, 10);
(long a, int b) right = (5, 10);
Console.WriteLine(left == right); // output: True
Console.WriteLine(left != right); // output: False

var t1 = (A: 5, B: 10);
var t2 = (B: 5, A: 10);
Console.WriteLine(t1 == t2); // output: True
Console.WriteLine(t1 != t2); // output: False
```

> Tuples also override the `Equals` and `GetHashCode` methods, which allows them to be used effectively as keys in a dictionary.

## Deconstructing Tuples

Tuples support deconstruction:

```csharp
var bob = ("Bob", 23);
{
    (string name, int age) = bob;
}
{
    var (name, age) = bob;
}
```

## Example Uses of Tuples

### Returning multiple values from a function

This is another way to return multiple values from a function, providing an alternative to `out` parameters.

```csharp
(int min, int max) FindMinMax(int[] input)
{
    if (input == null || input.Length == 0)
    {
        throw new ArgumentException("Cannot find minimum and maximum of a null or empty array.");
    }

    var min = int.MaxValue;
    var max = int.MinValue;
    foreach (var i in input)
    {
        if (i < min) min = i;
        if (i > max) max = i;
    }
    return (min, max);
}
```

### Swapping values

Tuples can be used to swap the values of two or more variables without needing a temporary variable. This might not be the most performant solution in all cases, but it is very readable:

```csharp
public static void Swap<T>(ref T a, ref T b)
{
    (a, b) = (b, a); // create tuple and deconstruct it into a and b
}
```