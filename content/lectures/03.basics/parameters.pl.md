---
title: "Parametry"
weight: 90
---

# Parametry

Parametry są przekazywane przez wartość, zasady dla typów bezpośrednich i referencyjnych zostały opisane w [System typów]({{< ref "../03.basics/types#przekazywanie-parametrów-do-metod" >}}).

| Modyfikator | Przekazanie *przez* | Przypisanie niewątpliwe   | Uwagi            |
|-------------|---------------------|---------------------------|------------------|
| (żaden)     | wartość             | przed wywołaniem          |                  |
| `ref`       | referencję          | przed wywołaniem          |                  |
| `in`        | referencję          | przed wywołaniem          | tylko do odczytu |
| `ref readonly` | referencję       | przed wywołaniem          | tylko do odczytu (C# 12) |
| `out`       | referencję          | przed zwróceniem kontroli |                  |

Ponadto można przekazywać parametry **przez referencję** na cztery różne sposoby.

## Przekazywanie przez referencję (`ref`)

Przekazanie parametru przy użyciu `ref` pozwala metodzie odczytywać i zapisywać przekazaną zmienną. Przekazanie typu referencyjnego przez referencję pozwala zmienić w metodzie samą referencję (zmiana jest widoczna poza metodą).

```csharp
int x = 8;
Foo(ref x);
Console.WriteLine(x); // outputs 9

static void Foo(ref int p)
{
    p = p + 1;
    Console.WriteLine(p); // outputs 9
}
```

## Parametry wyjściowe (`out`)

Modyfikator `out` działa podobnie, z tą różnicą, że zmienna wymaga przypisania w metodzie, a przed wywołaniem metody może pozostać niezainicjalizowana. Jest to jeden ze sposobów na zwrócenie z metody więcej niż jednej wartości. Ponadto, jeżeli nie jesteśmy zainteresowani jedną z wartości wyjściowych, możemy ją odrzucić, używając symbolu `_`.

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

Modyfikator `in` również działa podobnie; tutaj różnica polega na tym, że przekazany parametr staje się w ciele metody tylko do odczytu.

```csharp
using System.Numerics;

Matrix4x4 A = Matrix4x4.CreateRotationX(MathF.PI / 2);
Matrix4x4 B = Matrix4x4.CreateRotationY(MathF.PI / 4);
Multiply(in A, in B, out var result);
Console.WriteLine(result);

static void Multiply(in Matrix4x4 a,
                     in Matrix4x4 b,
                     out Matrix4x4 result)
{
    // a = Matrix4x4.Identity; // Compilation error: 'a' is read-only
    result = a * b;
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

Słowo kluczowe `params` pozwala przekazać do metody zmienną liczbę argumentów. Może zostać użyte tylko przy ostatnim parametrze. Tradycyjnie jego typem jest tablica jednowymiarowa (od C# 13 dozwolone są również inne typy kolekcji); kompilator sam niejawnie tworzy tablicę z argumentów podanych w miejscu parametru `params`.

```csharp
using System.Text;

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

### Kolekcje `params` (C# 13)

Od C# 13 parametr `params` może być również typu `Span<T>`, `ReadOnlySpan<T>`, typu interfejsu kolekcji (np. `IEnumerable<T>`, `IReadOnlyList<T>`, `IList<T>`) albo typu kolekcji (np. `List<T>`). Najbardziej przydatny jest `params ReadOnlySpan<T>` (`ReadOnlySpan<T>` to widok tylko do odczytu na ciągły obszar pamięci): kompilator może umieścić argumenty na stosie, więc - w przeciwieństwie do `params T[]` - przy każdym wywołaniu nie jest alokowana tablica na stercie. Jeżeli istnieją oba przeciążenia, kompilator wybiera wersję ze spanem. Od .NET 9 wiele metod biblioteki standardowej (np. `string.Join`, `string.Concat`, `Path.Combine`, `Task.WhenAll`) ma takie przeciążenia, więc istniejący kod pozbywa się tych alokacji po samej ponownej kompilacji.

```csharp
Console.WriteLine(Sum(1, 2, 3)); // 6
Console.WriteLine(Sum());        // 0
PrintAll("a", "b", "c");         // a, b, c
PrintAll(["x", "y"]);            // x, y - a collection can also be passed directly

static int Sum(params ReadOnlySpan<int> numbers) // no array is allocated
{
    int sum = 0;
    foreach (int n in numbers)
        sum += n;
    return sum;
}

static void PrintAll(params IEnumerable<string> items)
{
    Console.WriteLine(string.Join(", ", items));
}
```

## Opcjonalne parametry, nazwane argumenty

Parametry w C# mogą mieć wartość domyślną. Wartość domyślna musi być stała w czasie kompilacji. Wszystkie parametry domyślne muszą być zdefiniowane po parametrach wymaganych. Jeżeli jest potrzeba pominięcia niektórych opcjonalnych parametrów, można wskazać, którym z nich chcemy nadać wartość, używając argumentów nazwanych.

```csharp
void ExampleMethod(int required,
                   string optionalstr = "default",
                   int optionalint = 10) {}
ExampleMethod(3, "parameter", 7);
ExampleMethod(3, "parameter");
ExampleMethod(3);
ExampleMethod(3, optionalint: 4);
```
