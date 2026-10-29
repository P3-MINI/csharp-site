---
title: "Przeciążanie operatorów"
---

# Przeciążanie operatorów

C# wspiera przeciążanie operatorów. Operatory pozwalają na bardziej zwięzły zapis operacji. Nie powinno się ich nadużywać - stosujemy je głównie wtedy, gdy jest to intuicyjne i nie budzi wątpliwości, np. dla własnych typów numerycznych. Operator powinien robić to, czego byśmy się spodziewali.

## Przykład

Operatory są definiowane jako publiczne, statyczne składowe typu. Nazwą metody musi być symbol operacji poprzedzony słowem kluczowym `operator`. W zależności od operacji metoda przyjmuje jeden lub dwa parametry.

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

- Niektóre operatory muszą być implementowane parami: `==` i `!=`, `<` i `>`, `<=` i `>=`, `true` i `false`
- Operatory złożonego przypisania (np. `+=`, `/=`) są niejawnie przeciążone przez przeciążenie odpowiadających im operatorów niezłożonych. Od C# 14 można je także definiować jawnie.
- Podczas przeciążania operatora `==` należy też nadpisać metody `Equals` i `GetHashCode`, w przeciwnym wypadku kompilator zgłosi ostrzeżenie.
- Warto też implementować interfejsy `IEquatable<T>` i `IComparable<T>`. W .NET 7 (C# 11) i nowszych, przy przeciążaniu operatorów arytmetycznych, warto rozważyć implementację interfejsów generycznych, np. `IAdditionOperators<TSelf, TOther, TResult>`, `IMultiplyOperators<TSelf, TOther, TResult>`, itp. Dzięki temu będziemy mogli potem ograniczać typ generyczny, a także jest to forma dokumentacji kodu.

## Operatory przypisania złożonego (C# 14)

Przed C# 14 wyrażenie `x += y` było zawsze obliczane jako `x = x + y`. Dla mutowalnych typów przechowujących dużo danych (np. wektorów czy macierzy) oznacza to alokację nowego obiektu i kopiowanie danych przy każdym `+=`. Od C# 14 typ może definiować operatory przypisania złożonego (`+=`, `-=`, `*=`, ...) oraz operatory `++` i `--` jako metody **instancyjne**, które modyfikują obiekt w miejscu. Taki operator musi być publiczny, nie może być statyczny i zwraca `void`; operator przypisania złożonego przyjmuje jeden parametr (prawą stronę przypisania), a `++` i `--` nie przyjmują żadnego.

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

Jeżeli typ nie definiuje operatora przypisania złożonego, kompilator nadal oblicza `x += y` jako `x = x + y`. Instancyjny operator `++` nie jest używany, gdy wynik postfiksowego `x++` jest wykorzystywany (np. `y = x++`), ponieważ potrzebna jest wartość sprzed inkrementacji - wtedy wymagany jest statyczny `operator ++`.

> [!NOTE]
> Od C# 14 operatory można deklarować także w blokach rozszerzeń (*extension blocks*), co pozwala dodawać operatory do typów, których nie kontrolujemy - zobacz [Metody rozszerzające]({{< ref "/lectures/05.csharp-features/extension-methods" >}}).

## Dziedziczenie operatorów

Operatory są składowymi statycznymi, więc nie są polimorficzne: nie mogą być wirtualne ani nadpisywane, a operator jest wybierany w czasie kompilacji na podstawie statycznych typów operandów (operator zadeklarowany w klasie bazowej działa także dla obiektów klas pochodnych). Od C# 11 operatory można deklarować w interfejsach jako `static abstract` lub `static virtual` - typy implementujące muszą (a w przypadku `static virtual` mogą) dostarczyć własną implementację, a operatora można używać poprzez parametr generyczny ograniczony tym interfejsem.

```csharp
public interface IIncrementable<TSelf> where TSelf : IIncrementable<TSelf>
{
    public static abstract TSelf operator ++(TSelf self);
}
```

### Matematyka generyczna (C# 11)

Statyczne abstrakcyjne operatory w interfejsach są podstawą *matematyki generycznej* (*generic math*, .NET 7+). Wbudowane typy liczbowe implementują interfejsy generyczne, takie jak `INumber<TSelf>` (przestrzeń nazw `System.Numerics`), który udostępnia operatory arytmetyczne i porównania oraz składowe takie jak `T.Zero` i `T.One`. Dzięki temu jedna metoda generyczna może działać dla dowolnego typu liczbowego - w C++ zwykle realizuje się to szablonami:

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

## Operatory `checked` (C# 11)

Od C# 11 typ może zdefiniować osobną wersję operatora dla kontekstu `checked`, np. rzucającą wyjątek `OverflowException` przy przepełnieniu. Można ją zadeklarować dla operatorów `++`, `--`, jednoargumentowego `-`, dwuargumentowych `+`, `-`, `*`, `/` oraz dla jawnych konwersji. Zawsze wymaga ona zwykłej wersji tego samego operatora, która jest używana w kontekście `unchecked` (domyślnym):

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

## Definiowanie konwersji

Można definiować własne konwersje dla typu, zarówno jawne, jak i niejawne.
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
