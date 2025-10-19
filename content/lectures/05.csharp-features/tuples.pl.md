---
title: "Krotki"
---

# Krotki

Krotka (*Tuples*) to typ przechowujący kilka podtypów, podobny do `std::tuple` z C++. W C# typ krotki to generyczna struktura `System.ValueTuple<T1, T2, ...>`. Krotki w C# mają duże wsparcie ze strony języka. Zarówno deklaracja jak i tworzenie krotek ma swoje odzwierciedlenie w składni. Typy `t1` i `t2` są tego samego typu - `(double, int, string)` to alias na `ValueTuple<double, int, string>`.

```csharp
(double, int, string) t1 = (4.5, 3, "Bob");
ValueTuple<double, int, string> t2 = new ValueTuple<double, int, string>(4.5, 3, "Bob");
```

Domyślnie pola krotki mają nazwy `Item1`, `Item2`, itd.

```csharp
(double, int, string) t1 = (4.5, 3, "Bob");
Console.WriteLine($"double: {t1.Item1}, int: {t1.Item2}, string: {t1.Item3}");
```

> Istnieje również typ referencyjny System.Tuple<T1, T2, ...>, jest to nieudany eksperyment związany z wprowadzaniem krotek do języka, pozostawiony w bibliotece standardowej dla kompatybilności. Typy bezpośrednie są dużo bardziej wydajne do zastosowań, w których używa się krotek.

## Nazywanie elementów

Elementom krotki można nadać bardziej przyjazne nazwy niż generyczne `ItemX`.

```csharp
(double Length, int Count, string Name) t1 = (4.5, 3, "Bob");
var t2 = (Length: 4.5, Count: 3, Name: "Bob");
Console.WriteLine($"double: {t1.Length}, int: {t1.Count}, string: {t1.Name}");
```

Jeżeli jawnie nie podamy nazwy elementów krotki, kompilator sam spróbuje nadać przyjazną nazwę elementom krotki na podstawie nazw zmiennych lub właściwości:

```csharp
int count = 3;
string name = "Alice";
var t1 = (DateTime.Now, count, name);
Console.WriteLine($"DateTime: {t1.Now}, int: {t1.count}, string: {t1.name}");
```

> Przyjazne nazwy elementów nie wpływają na typ krotki. Jeżeli typy elementów krotki się zgadzają to jest to ten sam typ.

## Porównywanie krotek

Operatory `==` i `!=` są przeciążone i sprawdzają po prostu element po elemencie czy są one równe/różne używając operatorów `==` i `!=`:

```csharp
(int a, byte b) left = (5, 10);
(long a, int b) right = (5, 10);
Console.WriteLine(left == right); // output: True
Console.WriteLine(left != right); // output: False

var t1 = (A: 5, B: 10);
var t2 = (B: 5, A: 10);
Console.WriteLine(t1 == t2); // output: True
Console.WriteLine(t1 != t2); // output: False
```

> Krotki nadpisują też metody `Equals` i `GetHashCode`, dzięki czemu można ich skutecznie używać jako kluczy w słownikach.

## Dekonstrukcja krotek

Krotki wspierają [dekonstrukcję]({{< ref "/lectures/04.creating-types/classes#dekonstruktory" >}}):

```csharp
var bob = ("Bob", 23);
{
    (string name, int age) = bob;
}
{
    var (name, age) = bob;
}
```

## Przykładowe zastosowania krotek

### Zwracanie kilku wartości z funkcji

Kolejny sposób na zwrócenie kilku wartości z funkcji, alternatywa dla parametrów `out`.

```csharp
(int min, int max) FindMinMax(int[] input)
{
    if (input == null || input.Length == 0)
    {
        throw new ArgumentException("Cannot find minimum and maximum of a null or empty array.");
    }

    var min = int.MaxValue;
    var max = int.MinValue;
    foreach (var i in input)
    {
        if (i < min) min = i;
        if (i > max) max = i;
    }
    return (min, max);
}
```

### Zamiana wartości

Krotki mogą byś użyte do zamiany wartości dwóch lub więcej zmiennych, bez wprowadzania wartości tymczasowych. Nie jest to optymalne rozwiązanie, ale za to bardzo czytelne:

```csharp
public static void Swap<T>(ref T a, ref T b)
{
    (a, b) = (b, a); // create tuple and deconstruct it into a and b
}
```
