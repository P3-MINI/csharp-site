---
title: "Object"
weight: 60
---

# Object

`object` (an alias for `System.Object`) is the **base class for all types**. Every type can be upcast to this type. `object` itself is a **reference type** - however, value types also inherit from it, thanks to the unified type system. This is not a perfect solution, though.

## Boxing

Casting a value type to/from an `object` involves a so-called boxing/unboxing operation. Downcasting requires an explicit cast; if it fails, the Runtime throws an `InvalidCastException`. The boxing operation consists of allocating the value type on the heap and copying it there. This is a costly operation and should be avoided if possible. Unboxing consists of copying the value type back to the stack.

```csharp
int num = 42;
object obj = num; // Boxing
int unboxedNum = (int)obj; // Unboxing
```

## Definition of the `object` type

The definition of this type in the standard library looks as follows. Methods marked as `virtual` are intended to be overridden in derived classes.

```csharp
public class Object
{
    public Object();
    public extern Type GetType();
    public virtual bool Equals(object obj);
    public static bool Equals(object objA, object objB);
    public static bool ReferenceEquals(object objA, object objB);
    public virtual int GetHashCode();
    public virtual string ToString();
    protected virtual void Finalize();
    protected extern object MemberwiseClone();
}
```

## Methods in the `object` type

Since all types inherit from `object`, the methods (e.g., `ToString()`, `GetType()`, `GetHashCode()`, and `Equals(object)`) defined in `object` are also available in other types. Calling these methods (except for `GetType`) on a value type does not cause a boxing operation.

```csharp
int i = 15;
Console.WriteLine(i.ToString());
Console.WriteLine(i.GetType());
Console.WriteLine(i.GetHashCode());
Console.WriteLine(3.Equals(i));
```

## `ToString`

The `ToString` method returns a string representation of the object. All built-in types override it. If a type did not override it, then `ToString` returns the type name. You can override this method as follows:

```csharp
Person alice = new Person {Name = "Alice", Age = 30};
Console.WriteLine(alice); // Alice, 30

class Person
{
    public string Name;
    public int Age;
    public override string ToString() => $"{Name}, {Age}";
}
```

## `GetType` and the `typeof` operator

The `typeof` operator allows you to obtain type information at compile time, while the `GetType` method does so at runtime. Apart from that, both methods work identically: they return an object that is a representation of the type.

```csharp
DateTime now = DateTime.Now;

Console.WriteLine(now.GetType()); // evaluated at runtime
Console.WriteLine(typeof(DateTime)); // evaluated at compile time

Console.WriteLine(typeof(DateTime) == now.GetType());
```
