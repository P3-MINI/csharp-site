---
title: "Object"
weight: 60
---

# Object

`object` (alias dla `System.Object`) jest **klasą bazową dla wszystkich typów**. Każdy typ może zostać rzutowany w górę do tego typu. `object` sam w sobie jest **typem referencyjnym** - typy bezpośrednie jednak również po nim dziedziczą, to za sprawą zunifikowanego systemu typów (*unified type system*). Nie jest to jednak perfekcyjne rozwiązanie.

## Pakowanie (*boxing*)

Rzutowanie typu bezpośredniego do/z `object`u wiąze się z tzw. operacją pakowania/rozpakowania (*boxing/unboxing*). Rzutowanie w dół wymaga jawnego rzutowania, jeżeli się nie powiedzie to Runtime rzuca wyjątkiem `InvalidCastException`. Operacja pakowania polega na zaalokowaniu typu bezpośredniego na stercie i skopiowaniu go tam. Jest to operacja kosztowna, należy jej unikać jeżeli się da. Rozpakowanie polega na skopiowaniu typu bezpośredniego z powrotem na stos.

```csharp
int num = 42;
object obj = num; // Boxing
int unboxedNum = (int)obj; // Unboxing
```

## Definicja typu `object`

Definicja tego typu w bibliotece standardowej wygląda następująco. Metody oznaczone `virtual` są przeznaczone do przeciążania w klasach pochodnych.

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

## Metody w typie `object`

Jako, że wszystkie typy dziedziczą po `object`, metody (np. `ToString()`, `GetType()`, `GetHashCode()` i `Equals(object)`) zdefiniowane w `object` są dostępne także w innych typach. Wywołanie tych metod (za wyjątkiem `GetType`) na typie bezpośrednim nie powoduje operacji pakowania.

```csharp
int i = 15;
Console.WriteLine(i.ToString());
Console.WriteLine(i.GetType());
Console.WriteLine(i.GetHashCode());
Console.WriteLine(3.Equals(i));
```

## `ToString`

Metoda `ToString` zwraca tekstową reprezentację obiektu. Wszystkie typy wbudowane ją przeciążają. Jeżeli typ by jej nie przeciążył, to wtedy `ToString` zwraca nazwę typu. Przeciążyć tą metodę można następująco:

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

## `GetType` i operator `typeof`

Operator `typeof` pozwala pozyskać informację o typie w czasie kompilacji, metoda `GetType` w czasie wykonania programu. Poza tym obie te metody działają identycznie: zwracają obiekt będący reprezentacją typ typu.

```csharp
DateTime now = DateTime.Now;

Console.WriteLine(now.GetType()); // evaluated at runtime
Console.WriteLine(typeof(now)); // evaluated at compile time

Console.WriteLine(typeof(now) == now.GetType());
```
