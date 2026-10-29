---
title: "Parametry"
weight: 90
---

# Parametry

Parametry są przekazywane przez wartość, zasady dla typów bezpośrednich i referencyjnych zostały opisane w [Types System]({{< ref "../03.basics/types#przekazywanie-parametrów-do-metod" >}}).

| Modyfikator | przekazanie *przez* | Przypisanie niewątpliwe   | Uwagi            |
|-------------|---------------------|---------------------------|------------------|
| (żaden)     | wartość             | przed wywołaniem          |                  |
| `ref`       | referencję          | przed wywołaniem          |                  |
| `in`        | referencję          | przed wywołaniem          | tylko do odczytu |
| `ref readonly` | referencję       | przed wywołaniem          | tylko do odczytu (C# 12) |
| `out`       | referencję          | przed zwróceniem kontroli |                  |

Ponadto można przekazywać parametry **przez referencję** na cztery różne sposoby.

## Przekazywanie przez referencję (`ref`)

Przekazanie parametru przy użyciu `ref` pozwala w metodzie odczyt i zapis do przekazanej zmiennej. Przekazanie typu referencyjnego przez referencję pozwala na zmianę referencji poza metodą.

```csharp
int x = 8;
Foo(ref x);
Console.WriteLine(x);

static void Foo(ref int p)
{
    p = p + 1;
    Console.WriteLine(p);
}
```

## Parametry wyjściowe (`out`)

Modyfikator `out`, w zasadzie działa podobnie, z tą różnicą, że zmienna wymaga przypisania w metodzie, a przed wywołaniem metody może pozostać niezainicjalizowana. Jest to jeden ze sposobów na zwrócenie z metody więcej niż jednej wartości. Ponadto jeżeli nie jesteśmy zainteresowani jedną z wartości wyjściowych, możemy ją odrzucić używając nazwy `_`.

```csharp
string[] firstNames; string lastName;
GetFirstAndLastNames("Tim Berners Lee",
                     out firstNames, out lastName);
GetFirstAndLastNames("John Fitzgerald Kennedy",
                     out var firsts, out string last);
GetFirstAndLastNames("Julius Robert Oppenheimer",
                     out firsts, out _); // last parameter discarded
                     
static void GetFirstAndLastNames(string name,
                                 out string[] firstNames,
                                 out string lastName)
{
    string[] words = name.Split(' ');
    firstNames = words[..^1];
    lastName = words[^1];
}
```

## Parametry wejściowe (`in`)

Modyfikator `in`, również działa podobnie, tutaj różnica polega na tym, że przekazany parametr staje się w ciele metody tylko do odczytu.

```csharp
Matrix4 A = Matrix.CreateRotationX(90.0f);
Matrix4 B = Matrix.CreateRotationY(45.0f);
Multiply(in A, in B, out var result);
Console.WriteLine(result);

static void Multiply(in Matrix4 a,
                     in Matrix4 b,
                     out Matrix4 result)
{
    /**/
}
```

## Referencje tylko do odczytu (`ref readonly`, C# 12)

Modyfikator `ref readonly`, podobnie jak `in`, przekazuje argument przez referencję i sprawia, że w ciele metody jest on tylko do odczytu. Różnica dotyczy miejsca wywołania. Argument `in` można przekazać bez żadnego modyfikatora i nie musi on nawet być zmienną (może to być np. stała lub wynik wywołania metody) - kompilator po cichu przekazuje wtedy referencję do tymczasowej kopii. Parametr `ref readonly` wyraża, że metoda faktycznie potrzebuje referencji do istniejącej zmiennej: argument powinien być zmienną przekazaną z `ref` lub `in`, w przeciwnym razie kompilator zgłasza ostrzeżenie. Modyfikator ten jest używany głównie w API, których parametry pierwotnie zadeklarowano jako `ref`, ale nigdy nie są one modyfikowane - zmiana na `ref readonly` nie psuje istniejących wywołań.

```csharp
using System.Numerics;

Matrix4x4 m = Matrix4x4.CreateScale(2.0f);
Print(ref m);              // OK
Print(in m);               // OK
Print(m);                  // Warning: argument should be passed with 'ref' or 'in'
Print(Matrix4x4.Identity); // Warning: argument should be a variable

static void Print(ref readonly Matrix4x4 matrix)
{
    // matrix.M11 = 0; // Compilation error: 'matrix' is read-only
    Console.WriteLine(matrix.M11);
}
```

## Zmienna liczba parametrów (`params`)

Słówko kluczowe `params` pozwala przekazać zmienną liczbę parametrów do metody. Może zostać użyte tylko do ostatniego parametru tylko w postaci tablicy jednowymiarowej. Kompilator sam niejawnie tworzy z podanych parametrów w miejscu `params` tablicę.

```csharp
var s1 = Concat("The", "Quick", "Brown", "Fox");
var s2 = Concat("Jumps", "Over", "The", "Lazy", "Dog");
Console.WriteLine(Concat(s1, s2));
Concat("This function accepts at least 1 parameter");

static string Concat(string str, params string[] strings)
{
    StringBuilder sb = new StringBuilder(str);
    foreach (string s in strings)
        sb.Append(s);
    return sb.ToString();
}
```

## Opcjonalne parametry, nazwane argumenty

Parametry w C# mogą mieć wartość domyślną. Wartość domyślna musi być stała w czasie kompilacji. Wszystkie parametry domyślne muszą być zdefiniowane po parametrach wymaganych. Jeżeli jest potrzeba pominięcia niektórych opcjonalnych parametrów, można wybrać którym z nich chcemy nadać wartość używając argumentów nazwanych.

```csharp
public void ExampleMethod(int required,
                          string optionalstr = "default",
                          int optionalint = 10) {}
ExampleMethod(3, "parameter", 7);
ExampleMethod(3, "parameter");
ExampleMethod(3);
ExampleMethod(3, optionalint: 4);
```
