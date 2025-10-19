---
title: "Nullability"
---

# Nullability

## Nullable value types

Value types in C# cannot take the value `null`.

```csharp
int i = null; // Compilation error
```

However, the language provides a tool that allows them to accept the value `null`, or at least appear to do so.

```csharp
int? i = null; // OK
```

The type `int?` expands to the type `Nullable<int>`. This is a generic structure that wraps another value type along with a flag indicating whether the type has a value. The definition of this structure looks something like this:

```csharp
public struct Nullable<T> where T : struct
{
    public T Value { get; }
    public bool HasValue { get; }
    public T GetValueOrDefault();
    public T GetValueOrDefault(T defaultValue);
    //...
}
```

The syntax related to this type is just syntactic sugar. The compiler translates assignments of `null` and comparisons into the appropriate constructs from the `Nullable` structure:

```csharp
int? i = null;
Console.WriteLine(i == null);

// Equivalent:
// Nullable<int> i = new Nullable<int>();
// Console.WriteLine(!i.HasValue);
```

> Retrieving the value via the `Value` property throws an `InvalidOperationException` if the `HasValue` property is `false`.

### Conversions

You can implicitly assign a non-nullable value to a nullable type variable. The other way requires an explicit cast and can cause an exception to be thrown if `HasValue` is `false`.

```csharp
int? i = 5;     // implicit conversion
int j = (int)i; // explicit conversion

// Equivalent:
// int j = i.Value;
```

### Lifted operators

The `Nullable` structure does not define operators, but you can use operators on it just as you would on its generic parameter.

```csharp
int? x = 5;
int? y = 10;
int? z = x + y;
bool b = x < y;
```

The compiler "lifts" the operator in the following way - depending on whether it is a comparison operator or another operator:

```csharp
int? z = (x.HasValue && y.HasValue) ? (x.Value + y.Value) : null;
bool b = (x.HasValue && y.HasValue) ? (x.Value < y.Value) : false;
```

#### Three-valued logic

Thanks to lifted operators, the `bool?` type supports three-valued logic (with the additional value `null` representing `unknown`).

```csharp
bool? n = null;
bool? f = false;
bool? t = true;

Console.WriteLine(n | n); // null
Console.WriteLine(n | f); // null
Console.WriteLine(n | t); // true
Console.WriteLine(n & n); // null
Console.WriteLine(n & f); // false
Console.WriteLine(n & t); // null
```

## Nullable reference types

Reference types naturally support the value `null`. Nullable reference types mean something else. This is a language feature (from C# 8.0) that helps avoid `NullReferenceException` exceptions through static code analysis. In this case, reference types not marked with the `?` symbol have a special meaning. The compiler will check such variables or fields to ensure they always have a value assigned. If the compiler detects that such a variable might store a `null` value, it will issue a warning during compilation.

```csharp
string str = null; // Warning: Converting null literal or possible null value into non-nullable type
```

If our intention is to store a null value, we should mark the type as nullable:

```csharp
string? str = null; // OK
```

For nullable variables, the compiler also ensures that we check whether the variable stores a `null` value before using it:

```csharp
public static void PrintMessageLength(string? message)
{
    Console.WriteLine(message.Length); // Warning: Dereference of a possibly null reference

    if (message != null)
    {
        Console.WriteLine(message.Length); // OK
    }
}
```

We can also skip the `null` check if a method's parameter assumes it is a non-nullable type. Passing a potential `null` then also causes a warning:

```csharp
string? str = null;
PrintMessageLength(str); // Warning: Possible null reference argument for parameter 'message' in 'Program.PrintMessageLength'

public static void PrintMessageLength(string message)
{
    Console.WriteLine(message.Length);
}
```

> There is no runtime difference between a nullable and a non-nullable reference type. The distinction is purely a compile-time feature to enable static analysis.

### Nullable context

We can disable static analysis for nullability at the project level by setting the `Nullable` property accordingly in the project file:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

You can also disable/enable analysis for a code fragment using preprocessor directives:

```csharp
#nullable enable  // enables nullable reference checks from this point on
#nullable disable // disables nullable reference checks from this point on
#nullable restore // resets nullable reference checks to project setting
```

### Null forgiving operator

You can also silence compiler warnings with the `!` operator:

```csharp
string s1 = null!;         // `!` Silences the warning
string? s2 = null;
int s2Length = s2!.Length; // `!` Silences the warning
```

## Operators related to nullability

C# provides several operators for working with nullable types.

### Null-coalescing operator

The null-coalescing operator checks if the left side is `null`. If it is, it returns the value from the right side; otherwise, it returns the value from the left side.

```csharp
string s1 = null;
string s2 = s1 ?? "non-null";
Console.WriteLine($"Value of s2: {s2}");
```

This is equivalent to:

```csharp
string s2 = (s1 == null) ? "non-null" : s1;
```

You can also throw an exception on the right side of the operator:

```csharp
string s2 = s1 ?? throw new ArgumentNullException();
```

### Null-coalescing assignment operator

The same, but in an assignment version:

```csharp
string? s = null;
s ??= "non-null";
Console.WriteLine($"Value of s: {s}");
```

Equivalently:

```csharp
s = (s == null) ? "non-null" : s;
```

### Null-conditional operator

The null-conditional operator allows for safe access to members or elements of an object, returning null instead of causing a `NullReferenceException` if the object turns out to be `null`.

```csharp
StringBuilder? sb = null;
string? s = sb?.ToString();
```

Equivalently:

```csharp
string? s = (sb == null ? null : sb.ToString());
```