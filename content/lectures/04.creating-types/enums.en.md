---
title: "Enums"
weight: 60
---

# Enums

An enumeration type in C# is similar to the one in C++. It is represented in memory as an integer type and groups several constants. In C#, it is a value type. By default, the underlying type is `int`.

The simplest definition of an enumeration looks like this:

```csharp
public enum BorderSide 
{
    Left, 
    Right, 
    Top, 
    Bottom
}
```

In this case, the enum members will be assigned `int` constants: 0, 1, 2, 3.

We can change both the underlying type of the enumeration and the assigned constants:

```csharp
public enum BorderSide : byte
{
    Left = 1, 
    Right, 
    Top = 65, 
    Bottom
}
```

The underlying type can be any integer type that can 'accommodate' all the constants. Constants without an explicitly assigned value are sequentially 1 greater than the previous one.

## Conversion between numeric types

An enumeration type can be explicitly cast to and from its corresponding numeric type. The value zero can be implicitly assigned to an enumeration type:

```csharp
int i = (int) BorderSide.Top; // requires explicit cast
BorderSide side = (BorderSide) i; // requires explicit cast
BorderSide side = 0; // might assign 0 implicitly
```

## Operations on enumeration types

The enumeration type supports operations using bitwise (`~`, `^`, `&`, `|`), arithmetic (`+`, `-`, `++`, `--`, `+=`, `-=`), and comparison (`!=`, `==`, `<=`, `>=`, `<`, `>`) operators.

Due to conversion and operator operations, an enumeration type can go beyond its allowed range. Therefore, when checking possible values, we should expect an invalid value.

```csharp
BorderSide side = BorderSide.Bottom;
side++;

if (side == BorderSide.Right) {}
else if (side == BorderSide.Left) {}
else if (side == BorderSide.Top) {}
else if (side == BorderSide.Bottom) {}
else
{
    throw new ArgumentException($"Enum value out of bounds: {side}")
}
```

## Flags

Usually, an enumeration value can take one of the values. We can also treat an enumeration in such a way that a single enumeration value can represent a combination of several values. For this to work correctly, we must assign subsequent powers of two to subsequent constants:

```csharp
[Flags]
public enum BorderSides
{
    None = 0, 
    Left = 1, 
    Right = 1 << 1, 
    Top = 1 << 2, 
    Bottom = 1 << 3,
    LeftRight = Left | Right,
    TopBottom = Top | Bottom,
    All = LeftRight | TopBottom
}
```

Such enumerations are customarily marked with the `Flags` attribute, primarily to indicate the intention of using the enumeration. This attribute also causes the `ToString` method to correctly format the combined enumeration values.

To check if an enumeration value contains any of the options, we use bitwise operations:

```csharp
BorderSides allButTop = BorderSides.All ^ BorderSides.Top;
Console.WriteLine((allButTop & BorderSides.Left) != 0);
Console.WriteLine((allButTop & BorderSides.Right) != 0);
Console.WriteLine((allButTop & BorderSides.Top) != 0);
Console.WriteLine((allButTop & BorderSides.Bottom) != 0);

Console.WriteLine(allButTop);
```
