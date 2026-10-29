---
title: "Structs"
weight: 30
---

# Structs

Structs and classes are different in certain aspects:

* they are value types
* they do not support inheritance
* they cannot contain finalizers

Other than that, structs can have the same members as a class.

## Struct or Class

We should use structs if we care about value-type semantics. This is particularly beneficial when the **object is small** (Microsoft's design guidelines recommend under 16 bytes; up to about 64 bytes - one cache line - copying is still cheap), as the value will be copied in at most a few processor cycles. Typically, structs are a better fit for objects with a **short lifecycle**, e.g., mathematical types that are frequently created and quickly destroyed when using operators - the stack is better for this. If we need a **large number of objects**, it's worth remembering that an array of structs will create these objects in a single allocation in a contiguous block of memory. Access to contiguous memory is much faster.

Classes, on the other hand, should be chosen for types that have an **'identity'**, e.g., `Customer`, `Order`, `Window`, and should not be accidentally copied. For **large objects** (> 64 bytes), copying a reference is much more efficient (almost always in 1 cycle). Objects with a **longer lifecycle** are usually better suited for storage on the heap, which classes guarantee. If we need to model a hierarchy of objects that **require inheritance**, we must use classes.

## Default Constructor

Structs always have a parameterless default constructor that zeroes out all the struct's fields. Even if we define our own parameterless constructor (C# 10), we can still obtain the zeroed-out value via `default`. Furthermore, an array of structs is bitwise-zeroed by default, and classes also bitwise-zero their fields. It's worth protecting against this and always treating a zeroed-out struct as a possible valid value.

If a struct does not declare a parameterless constructor, `new Point()` is equivalent to `default`: field and property initializers are not executed, which is why `p2.Y` in the example below is `0` and not `1`.

```csharp
Point p1 = new Point(1); // 1, 1
Point p2 = new Point();  // 0, 0

public struct Point
{
    public float X { get; set; }
    public float Y { get; set; } = 1;
    
    public Point(float x) => X = x;
}
```

## `readonly` structs

The `readonly` keyword in a struct definition enforces that the struct is immutable, meaning its fields must also be read-only, and auto-implemented properties can only have a `get` (or `init`) accessor.

```csharp
public readonly struct Point
{
    public readonly float X;
    public float Y { get; } = 1;
    
    public Point(float x) => X = x;
}
```

Individual methods can also be marked as `readonly`, which prevents them from modifying the struct's fields.

## `ref` structs

Structs with the `ref` keyword can only be allocated on the stack. Any construct that would force such a variable to be placed on the heap results in a compilation error. Among other things, you cannot create arrays of such structs, they cannot be fields of classes or non-`ref` structs, they cannot be boxed (converted to `object` or to an interface type), captured by lambdas or local functions, or used as generic type arguments (unless the type parameter is declared with `allows ref struct`, C# 13). Since C# 13, they can implement interfaces and can be used as local variables in iterators and asynchronous methods, as long as their value does not have to be preserved across a `yield return` or an `await`.

```csharp
public ref struct Point
{
    public float X { get; set; }
    public float Y { get; set; }
}
```
