---
title: "Generics"
weight: 80
---

# Generics

Generics in C# look similar and serve the same purpose as templates in C++. They also allow us to write type-independent code, but they are a simpler and safer mechanism.

C++ templates operate at compile time; for each type used in a template, the C++ compiler creates a new, separate copy of the class or function. In C#, generics **operate at runtime**. The compiler always compiles generics once into intermediate language, where the types remain unspecified for the time being. Only during execution, when a generic is first used, does the JIT (*Just-In-Time*) compiler create a specialized version of it.

In C++ (at least before C++20), the compiler does not verify operations on template types until an attempt is made to use them. This often generates very obscure and hard-to-understand compilation errors. In C#, generic types are fully type-safe; to know how a type can be used, it must be constrained. A similar mechanism has existed in C++ since version C++20: [Constraints and concepts](https://en.cppreference.com/w/cpp/language/constraints.html).

In C++, this template is perfectly valid, but if we try to use it with a type `T` that does not define a comparison operator, we will get a hard-to-debug error.

```cpp
template <class T> T Max(T a, T b)
{
    return a > b ? a : b;
}
```

In C#, to know that we can compare types, we would need to, for example, constrain `T` to a type that implements `IComparable<T>`. Thanks to this, we can only use this generic method on comparable types, which prevents surprises after substitution.

```csharp
static T Max<T>(T a, T b) where T : IComparisonOperators<T, T, bool>
{
    return a > b ? a : b;
}
```

Generic types in C# can only take type parameters (i.e., other types, e.g., `string`, `int`, `MyClass`), and not non-type parameters (constant values, e.g., `10`, `true`, `"hello"`), as is the case in C++.

## Without Generics

Let's imagine we need to implement a stack that works with the types: `int`, `float`, and `string`. Without generics, we could, for example, implement three classes: `IntStack`, `FloatStack`, `StringStack`. However, this is a lot of repetitive code to maintain. Another solution is to use the `object` class to write a single stack implementation that will work with any type:

```csharp
public class Stack
{
    private object[] _items = new object[8];
    public int Count { get; private set; }

    public void Push(object item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    public object Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }
}
```

> [!NOTE]
> **Source code - ObjectStack**
> {{< filetree dir="lectures/creating-types/generics/ObjectStack" >}}


However, trying to use such a stack reveals the problems with this implementation. First, pushing value types onto the stack requires boxing. Second, such a stack does not provide us with type safety. Retrieving an element involves an unsafe downcast. Generic types solve both of these problems.

```csharp
Stack stack = new Stack();

for (int i = 0; i < 10; i++)
{
    stack.Push(i);
}

int number = (int)stack.Pop();
string str = (string)stack.Pop(); // Runtime error: InvalidCastException
```

## Generic Types

The generic implementation looks as follows:

```csharp
public class Stack<T>
{
    private T[] _items = new T[8];
    public int Count { get; private set; }

    public void Push(T item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }
}
```

> [!NOTE]
> **Source code - GenericStack**
> {{< filetree dir="lectures/creating-types/generics/GenericStack" >}}


Using such a stack no longer causes boxing operations and is type-safe:

```csharp
Stack<int> stack = new Stack<int>();

for (int i = 0; i < 10; i++)
{
    stack.Push(i);
}

// string str = stack.Pop(); // Compilation error
while (stack.Count > 0)
{
    int number = stack.Pop();
    Console.WriteLine(number);
}
```

You can define multiple generic parameters:

```csharp
class Test<T, U, W>;
```

## Generic Methods

Methods can also introduce generic parameters:

```csharp
public static void Swap<T>(ref T a, ref T b)
{
    T temp = a;
    a = b;
    b = temp;
}
```

If the compiler can infer the generic parameters, you don't have to specify them at invocation:

```csharp
int i = 3, j = 5;
Swap<int>(ref i, ref j);
Swap(ref i, ref j);
Console.WriteLine($"i: {i}, j: {j}");
```

## Generic Type Constraints

Normally, all we know about a generic parameter is that it is of type `object`, which means we can call the methods available on the `object` type. Nothing more can be done.

Constraints allow you to provide additional information about the generic type. For example, if you constrain a generic parameter to a specific class or interface, the compiler will allow you to use methods from that class/interface.

```csharp
public static T Max<T>(T value, params T[] values) where T : IComparable<T>
{
    var max = value;
    foreach (var t in values)
    {
        if (max.CompareTo(t) < 0)
        {
            max = t;
        }
    }
    return max;
}
```

Possible constraints:

* `where T : `<base class/interface>` - the most common and useful constraint, allows calling methods from the constrained classes and interfaces.
* `where T : `<base class/interface>`?` - allows calling methods from the constrained classes and interfaces, and can also be nullable.
* `where T : new()` - the type must have a parameterless constructor, useful if we need to create new instances.
* `where T : class` - the type must be a reference type.
* `where T : class?` - the type must be a reference type, can be nullable.
* `where T : struct` - the type must be a value type.
* `where T : allows ref struct` - the type can be a "ref struct".
* `where T : unmanaged` - the type must be a value type and recursively consist of other value or pointer types.
* `where T : notnull` - cannot be nullable.

We can apply several constraints to a generic type:

```csharp
class Base {}
class Test<T, U>
    where U : struct
    where T : Base, new()
{}
```

## Self-Referencing Generic Declarations

In a type declaration, you can use the declared type as a generic parameter:

```csharp
public class Product : IEquatable<Product>
{
    public string EanCode { get; }
    
    public Product(string eanCode) => EanCode = eanCode;
    
    public bool Equals(Product? other) => EanCode == other?.EanCode;
}
```

This makes sense; we are communicating that `Product` is comparable for equality with other instances of its type.

In the declaration, we can also use the generic parameter to constrain it.

```csharp
public class Finder<T> where T : IEquatable<T>
{
    public T? Find(IEnumerable<T> collection, T item)
    {
        foreach(var t in collection)
        {
            if (t.Equals(item)) return t;
        }

        return default(T);
    }
}
```

This also makes sense; we want to search for objects that are comparable for equality with each other, otherwise we wouldn't know how to search.

This is also correct: `class Foo<Bar> where Bar : Foo<Bar>`.

## Invariance

By default, generic types are invariant. You cannot cast their generic parameters up or down the inheritance chain.

Downcasting is not allowed because we could suddenly retrieve a different type of vehicle from a stack of cars.

```csharp
Stack<Vehicle> vehicleStack = new Stack<Vehicle>();
Stack<Car> carStack = vehicleStack; // Compilation error

public abstract class Vehicle;
public class Car : Vehicle;
public class Bike : Vehicle;
```

Upcasting is not allowed because we could suddenly push a different type of vehicle onto a stack of cars.

```csharp
Stack<Car> carStack = new Stack<Car>();
Stack<Vehicle> vehicleStack = carStack; // Compilation error

public abstract class Vehicle;
public class Car : Vehicle;
public class Bike : Vehicle;
```

## Variance

In interfaces, we can declare variant generic parameters. They restrict how the generic parameter can be used, but in return, they allow casting the parameter to one side of the inheritance chain. Covariant parameters (out) can only be used for return values. Contravariant parameters (in) can only be used as input parameters for methods.

```csharp
// Covariant T type parameter (can be used only as a return value)
public interface IPoppable<out T> 
{
    int Count { get; }
    T Pop();
}

// Contravariant T type parameter (can be used only as an input parameter)
public interface IPushable<in T> 
{
    void Push(T item);
}

public class Stack<T> : IPoppable<T>, IPushable<T>
{
    private T[] _items = new T[8];
    public int Count { get; private set; }

    public void Push(T item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }
}
```

> [!NOTE]
> **Source code - VariantStack**
> {{< filetree dir="lectures/creating-types/generics/VariantStack" >}}

### Covariance

Covariant generic parameters (out) allow for upcasting. This will enable passing a more specialized type to a more general method:

```csharp
var carStack = new Stack<Car>();
carStack.Push(new Car());
carStack.Push(new Car());
IPoppable<Car> vehiclePoppable = carStack;
WashVehicles(vehiclePoppable);

public void WashVehicles(IPoppable<Vehicle> vehicles)
{
    while (vehicles.Count > 0)
    {
        Vehicle vehicle = vehicles.Pop();
        Console.WriteLine($"Washing {vehicle}");
    }
}

public abstract class Vehicle;
public class Car : Vehicle;
public class Bike : Vehicle;
```

### Contravariance

Contravariant generic parameters (in) allow for downcasting. This will enable passing a more general type to a more specialized method:

```csharp
var vehiclesStack = new Stack<Vehicle>();
vehiclesStack.Push(new Car());
vehiclesStack.Push(new Bike());
IPushable<Vehicle> carPushable = vehiclesStack;
DeliverCars(carPushable, 2);

public void DeliverCars(IPushable<Car> cars, int count)
{
    for (int i = 0; i < count; i++)
    {
        Console.WriteLine("Adding car to IPushable");
        cars.Push(new Car());
    }
}

public abstract class Vehicle;
public class Car : Vehicle;
public class Bike : Vehicle;
```