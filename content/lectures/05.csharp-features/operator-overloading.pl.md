---
title: "Przeciążanie operatorów"
---

# Przeciążanie operatorów

C# wspiera przeciążanie operatorów. Operatory pozwalają na bardziej zwięzły zapis operacji. Nie powinno się ich nadużywać - stosujemy głównie wtedy, gdy jest to intuicyjne i nie budzi wątpliwości, np. dla własnych typów numerycznych. Operator powinien robić to, czego byśmy się spodziewali.

## Przykład

Operatory są definiowane jako publiczne, statyczne składowe typu. Nazwą metody musi być symbol operacji poprzedzony słówkiem kluczowym `operator`. W zależności od operacji metoda przyjmuje jeden lub dwa parametry.

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

- Niektóre operatory muszą być implementowane parami: `==` i `!=`, `<` i `>`, `<=` i `>=`
- Operatory złożonego przypisania (np. `+=`, `/=`) są niejawnie przeciążone przez przeciążenie odpowiadających im operatorów niezłożonych. Od C# 14 będzie można je jawnie definiować.
- Podczas przeciążania operatora `==` trzeba też nadpisać metody `Equals` i `GetHashCode`, w przeciwnym wypadku kompilator rzuci ostrzeżenie.
- Warto też implementować interfejsy `IEquatable<T>` i `IComparable<T>`. W .NET 7 (C# 11) i nowszych, przy przeciążaniu operatorów arytmetycznych, warto rozważyć implementację interfejsów generycznych, np. `IAdditionOperators<TSelf, TOther, TResult>`, `IMultiplyOperators<TSelf, TOther, TResult>`, itp. Dzięki temu będziemy mogli potem ograniczać typ generyczny, a także jest to forma dokumentacji kodu.

## Dziedziczenie operatorów

Operatory zdefiniowane w klasach nie podlegają dziedziczeniu, ale operatory zdefiniowane w interfejsach jako `static abstract` lub `static virtual` już tak.

```csharp
public interface IIncrementable<TSelf> where TSelf : IIncrementable<TSelf>
{
    public static abstract TSelf operator ++(TSelf self);
}
```

## Definiowanie konwersji

Można definiować własne konwersje dla typu, zarówno jawne jak i niejawne.
- **Niejawne (`implicit`)** konwersje powinny być zawsze bezpieczne, nie powodować utraty informacji i nie rzucać wyjątków.
- **Jawne (`explicit`)** konwersje mogą się nie powieść (np. rzucając wyjątek) lub mogą powodować utratę informacji - powinny wymagać od programisty świadomej decyzji (użycia rzutowania).

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

Użycie:

```csharp
Complex c = new Complex(3.0, 4.0);
double d = (double)c; // explicit conversion
c = 5.0;              // implicit conversion
```
