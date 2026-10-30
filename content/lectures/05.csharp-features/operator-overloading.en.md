---
title: "Operator Overloading"
---

# Operator Overloading

C# supports operator overloading. Operators allow for a more concise notation of operations. They should not be overused - we use them mainly when it is intuitive and does not raise doubts, e.g., for our own numeric types. An operator should do what we would expect it to do.

## Example

Operators are defined as public, static members of a type. The method name must be the operation symbol preceded by the `operator` keyword. Depending on the operation, the method takes one or two parameters.

```csharp
public struct Complex
{
    public double Real { get; }
    public double Imaginary { get; }
    
    public Complex(double real, double imaginary)
    {
        Real = real;
        Imaginary = imaginary;
    }
    
    public static Complex operator +(Complex a, Complex b)
    {
        return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
    }

    public static Complex operator -(Complex a, Complex b)
    {
        return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
    }

    public static Complex operator -(Complex a)
    {
        return new Complex(-a.Real, -a.Imaginary);
    }
    
    public static Complex operator *(Complex a, Complex b)
    {
        return new Complex(
            a.Real * b.Real - a.Imaginary * b.Imaginary,
            a.Real * b.Imaginary + a.Imaginary * b.Real
        );
    }

    public static bool operator ==(Complex a, Complex b)
    {
        return a.Real == b.Real && a.Imaginary == b.Imaginary;
    }

    public static bool operator !=(Complex a, Complex b)
    {
        return !(a == b);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Complex complex && complex == this;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Real, Imaginary);
    }
}
```

- Some operators must be implemented in pairs: `==` and `!=`, `<` and `>`, `<=` and `>=`, `true` and `false`
- Compound assignment operators (e.g., `+=`, `/=`) are implicitly overloaded by overloading their corresponding non-compound operators. Since C# 14, they can also be defined explicitly.
- When overloading the `==` operator, you should also override the `Equals` and `GetHashCode` methods, otherwise the compiler will issue a warning.
- It is also worth implementing the `IEquatable<T>` and `IComparable<T>` interfaces. In .NET 7 (C# 11) and newer, when overloading arithmetic operators, it is worth considering the implementation of generic interfaces, e.g., `IAdditionOperators<TSelf, TOther, TResult>`, `IMultiplyOperators<TSelf, TOther, TResult>`, etc. This will allow us to later constrain the generic type, and it is also a form of code documentation.

## Compound Assignment Operators (C# 14)

Before C# 14, `x += y` was always evaluated as `x = x + y`. For mutable types storing a lot of data (e.g., vectors or matrices), this means allocating a new object and copying the data on every `+=`. Since C# 14, a type can define compound assignment operators (`+=`, `-=`, `*=`, ...) and the `++` and `--` operators as **instance** methods that modify the object in place. Such an operator must be `public`, must not be `static`, and returns `void`; a compound assignment operator takes one parameter (the right-hand side), while `++` and `--` take none.

```csharp
public class Vector
{
    private readonly double[] _data;

    public Vector(int size) => _data = new double[size];

    public double this[int i]
    {
        get => _data[i];
        set => _data[i] = value;
    }

    public static Vector operator +(Vector a, Vector b)
    {
        var result = new Vector(a._data.Length);
        for (int i = 0; i < a._data.Length; i++)
            result._data[i] = a._data[i] + b._data[i];
        return result;
    }

    public void operator +=(Vector other)
    {
        for (int i = 0; i < _data.Length; i++)
            _data[i] += other._data[i];
    }

    public void operator ++()
    {
        for (int i = 0; i < _data.Length; i++)
            _data[i]++;
    }
}
```

```csharp
var v = new Vector(3);
var w = new Vector(3);
w[0] = 1;
var sum = v + w; // static operator +: allocates a new Vector
v += w;          // instance operator +=: modifies v in place
v++;             // instance operator ++
Console.WriteLine($"{v[0]} {v[1]} {sum[0]}"); // 2 1 1
// var old = v++; // error: the instance ++ cannot be used when the result of x++ is used
```

If a type does not define a compound assignment operator, the compiler still evaluates `x += y` as `x = x + y`. The instance `++` is not used when the result of a postfix `x++` is used (e.g., `y = x++`), because the value from before the increment is needed - in that case a static `operator ++` is required.

> [!NOTE]
> Since C# 14, operators can also be declared in extension blocks, which allows adding operators to types we do not control - see [Extension Methods]({{< ref "/lectures/05.csharp-features/extension-methods" >}}).

## Operator Inheritance

Operators are static members, so they are not polymorphic: they cannot be `virtual` or overridden, and the operator is selected at compile time based on the static types of the operands (an operator declared in a base class also works for objects of derived classes). Since C# 11, operators can be declared in interfaces as `static abstract` or `static virtual` - implementing types then must (or, for `static virtual`, may) provide their own implementation, and the operator can be used through a generic type parameter constrained by the interface.

```csharp
public interface IIncrementable<TSelf> where TSelf : IIncrementable<TSelf>
{
    public static abstract TSelf operator ++(TSelf self);
}
```

### Generic Math (C# 11)

Static abstract operators in interfaces are the basis of *generic math* (.NET 7+). Built-in numeric types implement generic interfaces such as `INumber<TSelf>` (namespace `System.Numerics`), which provides arithmetic and comparison operators as well as members such as `T.Zero` and `T.One`. Thanks to that, a single generic method can work with any numeric type - something that in C++ is typically done with templates:

```csharp
using System.Numerics;

static T Sum<T>(IEnumerable<T> values) where T : INumber<T>
{
    T result = T.Zero;
    foreach (T value in values)
        result += value;
    return result;
}

Console.WriteLine(Sum([1, 2, 3]));    // 6
Console.WriteLine(Sum([1.5, 2.5]));   // 4
Console.WriteLine(Sum([0.1m, 0.2m])); // 0.3
```

## Checked Operators (C# 11)

Since C# 11, a type can define a separate version of an operator for the `checked` context, e.g., one that throws an `OverflowException` on overflow. It can be declared for the `++`, `--`, unary `-`, binary `+`, `-`, `*`, `/` operators and for explicit conversions, and always requires the regular version of the same operator, which is used in the `unchecked` context (the default):

```csharp
public readonly struct Money(int cents)
{
    public int Cents { get; } = cents;

    public static Money operator +(Money a, Money b) => new(a.Cents + b.Cents);
    public static Money operator checked +(Money a, Money b) => new(checked(a.Cents + b.Cents));
}
```

```csharp
var max = new Money(int.MaxValue);
var one = new Money(1);
Console.WriteLine(unchecked(max + one).Cents); // -2147483648
Console.WriteLine(checked(max + one).Cents);   // OverflowException
```

## Defining Conversions

You can define your own conversions for a type, both explicit and implicit.
- **Implicit** conversions should always be safe, not cause loss of information, and not throw exceptions.
- **Explicit** conversions may fail (e.g., by throwing an exception) or may cause loss of information - they should require a conscious decision from the programmer (using a cast).

```csharp
public struct Complex
{
    public double Real { get; }
    public double Imaginary { get; }
    
    public Complex(double real, double imaginary)
    {
        Real = real;
        Imaginary = imaginary;
    }
    
    public static implicit operator Complex(double d)
    {
        return new Complex(d, 0.0);
    }
    
    public static explicit operator double(Complex c)
    {
        return c.Real;
    }
}
```

Usage:

```csharp
Complex c = new Complex(3.0, 4.0);
double d = (double)c; // explicit conversion
c = 5.0;              // implicit conversion
```