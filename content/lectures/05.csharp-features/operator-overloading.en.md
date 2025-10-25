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