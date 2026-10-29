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