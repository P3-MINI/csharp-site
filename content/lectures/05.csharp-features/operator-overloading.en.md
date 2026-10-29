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
    
    public override bool Equals(object obj)
    {
        return obj is Complex complex && complex == this;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Real, Imaginary);
    }
}
```

- Some operators must be implemented in pairs: `==` and `!=`, `<` and `>`, `<=` and `>=`
- Compound assignment operators (e.g., `+=`, `/=`) are implicitly overloaded by overloading their corresponding non-compound operators. From C# 14, it will be possible to define them explicitly.
- When overloading the `==` operator, you must also override the `Equals` and `GetHashCode` methods, otherwise the compiler will issue a warning.
- It is also worth implementing the `IEquatable<T>` and `IComparable<T>` interfaces. In .NET 7 (C# 11) and newer, when overloading arithmetic operators, it is worth considering the implementation of generic interfaces, e.g., `IAdditionOperators<TSelf, TOther, TResult>`, `IMultiplyOperators<TSelf, TOther, TResult>`, etc. This will allow us to later constrain the generic type, and it is also a form of code documentation.

## Operator Inheritance

Operators defined in classes are not inherited, but operators defined in interfaces as `static abstract` or `static virtual` are.

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