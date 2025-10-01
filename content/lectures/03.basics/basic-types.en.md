---
title: "Basic Types"
weight: 20
---

# Basic Types

Built-in type keywords (e.g., `int`, `double`, `bool`) are direct aliases for types defined in the `System` namespace (e.g., `System.Int32`, `System.Double`, `System.Boolean`). It's worth remembering that these types are regular classes in the standard library, and they define useful constants and methods, e.g., `int.MaxValue`, `int.Parse(string)`, `float.NegativeInfinity`.

## Numeric Types

| Type | .NET Name | Size | Range (approximate) | Precision | Literal Suffix |
|---|---|---|---|---|---|
| **sbyte** | `System.SByte` | 8 bits | `-128` to `127` | - | - |
| **byte** | `System.Byte` | 8 bits | `0` to `255` | - | - |
| **short** | `System.Int16` | 16 bits | `-32,768` to `32,767` | - | - |
| **ushort** | `System.UInt16` | 16 bits | `0` to `65,535` | - | - |
| **int** | `System.Int32` | 32 bits | `-2.1×10⁹` to `2.1×10⁹` | - | - |
| **uint** | `System.UInt32` | 32 bits | `0` to `4.2×10⁹` | - | `U` or `u` |
| **long** | `System.Int64` | 64 bits | `-9×10¹⁸` to `9×10¹⁸` | - | `L` or `l` |
| **ulong** | `System.UInt64` | 64 bits | `0` to `18×10¹⁸` | - | `UL` or `ul` |
| **float** | `System.Single` | 32 bits | `±1.5×10⁻⁴⁵` to `±3.4×10³⁸` | ~6-9 digits | `F` or `f` |
| **double** | `System.Double` | 64 bits | `±5.0×10⁻³²⁴` to `±1.7×10³⁰⁸` | ~15-17 digits | `D` or `d` |
| **decimal** | `System.Decimal` | 128 bits | `±1.0×10⁻²⁸` to `±7.9×10²⁸` | 28-29 digits | `M` or `m` |


## Numeric Conversions

C# distinguishes between two types of conversions between numeric types.

### Implicit Conversions

These are safe conversions, performed automatically by the compiler when there is no risk of data loss (with minor exceptions). A conversion is possible from a type with a smaller range to a type with a larger range.

- **From integral to integral**: `sbyte` → `short` → `int` → `long`
- **From integral to floating-point**: `int` → `float` (risk of precision loss) → `double`
- `long` can be implicitly converted to `float` or `double` (risk of precision loss).

```csharp
int i = 100;
long l = i;       // OK
float f = l;      // OK, but may lose precision for large numbers
double d = f;     // OK
```

### Explicit Conversions (Casting)

These require a conscious decision from the programmer and the use of the cast operator `(type)`. They are used when there is a risk of losing information.

Converting from `double` or `float` to an integral type causes the fractional part to be **truncated**.

For integral types, if an operation causes an **overflow** (the value exceeds the range of the target type), the default behavior is to perform the operation as if it were on a larger type and then truncate the most significant bits.

```csharp
double d = 99.9;
int i = (int)d; // i = 99 (fractional part is truncated)

long l = 3_000_000_000L;
int i2 = (int)l; // i2 = -1294967296 (overflow)
```

The `checked` and `unchecked` contexts are used to control overflow. In a `checked` context, an overflow is treated as an error.

```csharp
// In a checked block, a System.OverflowException is thrown in case of an overflow
try
{
    checked
    {
        int i3 = (int)l;
    }
}
catch (OverflowException ex)
{
    Console.WriteLine(ex.Message);
}
```

## The `decimal` Type

The `decimal` type should be used for financial and monetary operations where rounding errors are unacceptable.

- `decimal` is a **base-10 floating-point type**. Unlike `float` and `double` (base-2), `decimal` accurately represents decimal fractions (e.g., 0.1, 0.2).
- In memory, it is stored as a 96-bit integer mantissa, a sign bit, and a 31-bit exponent (a power of 10 that specifies the position of the decimal point).

Conversions between `decimal` and `float`/`double` must always be explicit.

Despite its precision, `decimal` is not a universal solution and has significant disadvantages compared to `float` and `double`:

-   **Performance**: Operations on `decimal` are significantly slower. Arithmetic for `float` and `double` is performed directly by the processor (in the FPU), while operations on `decimal` are most often implemented in software, which involves more overhead.
-   **Smaller range**: `decimal` has a much smaller range of values than `double`. It is not suitable for scientific calculations that operate on very large or very small numbers.
-   **Memory consumption**: It takes up 16 bytes, which is twice as much as `double` (8 bytes) and four times as much as `float` (4 bytes).

## The `bool` Type

The `bool` type (alias for `System.Boolean`) represents the values `true` and `false`. It occupies 1 byte in memory.

```csharp
int x = 1;
bool flag = x > 0;
if (flag)
{
    // ...
    flag = false;
}
```

Unlike in C++, there are no conversions between `bool` and numeric types.

### Comparison Operators

For value types, the comparison operation **by default** checks if the objects are identical field by field.

```csharp
Point p1 = new Point {X = 5, Y = 3};
Point p2 = p1; p2.X = 0;
Point p3 = new Point {X = -1, Y = 1};
Point p4 = new Point {X = -1, Y = 1};
Console.WriteLine(p1 == p2); // false
Console.WriteLine(p3 == p4); // true

public struct Point { public float X, Y; }
```

For reference types, the comparison operation **by default** checks if the references point to the same object.

```csharp
Person p1 = new Person {Name = "Alice", Age = 30};
Person p2 = new Person {Name = "Alice", Age = 30};
Person p3 = new Person {Name = "Bob", Age = 25};
Person p4 = p3; p4.Age = 26;
Console.WriteLine(p1 == p2); // false
Console.WriteLine(p3 == p4); // true

public class Person { public string Name; public int Age; }
```

> Comparison operators can be overloaded to return any type. In practice, this doesn't make much sense.

### Short-Circuiting Operations

The logical operators `&&` and `||` perform so-called short-circuiting operations.
For example, if the left side of the `&&` operator evaluates to `false`, its right side will not be evaluated.
In contrast, the `&` and `|` operators always cause both sides of the operator to be evaluated.

```csharp
if (user != null && user.HasPermission("admin"))
{
    Console.WriteLine("User has admin permissions.");
}
```

In this example, this mechanism helps us avoid a `NullReferenceException`. If the user is `null`, the method will not be called on it.

## The `char` Type

The `char` type (alias for `System.Char`) in .NET has a fixed size of 2 bytes - it's a UTF-16 encoded character.

The `char` type can be implicitly converted to other numeric types if that type can accommodate a `ushort`. Otherwise, an explicit conversion is required.

```csharp
char a = 'A';
char newLine = '\n';
char copyright = '\u00A9';
```

## The `Convert` Class

The `Convert` class contains many useful methods for converting between types. It allows for conversions between basic types and strings, or conversions between the `bool` type and numeric types. Conversions using this class round floating-point numbers when converting to integers.

```csharp
int number = Convert.ToInt32("42");
int rounded = Convert.ToInt32(3.5);
bool isTrue = Convert.ToBoolean(1);
int binaryInt = Convert.ToInt32("101010", 2);
string hex = Convert.ToString(255, 16);
```