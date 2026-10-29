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
| `out`       | referencję          | przed zwróceniem kontroli |                  |

Ponadto można przekazywać parametry **przez referencję** na trzy różne sposoby.

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

## Zmienna liczba parametrów (`params`)

Słowo kluczowe `params` pozwala przekazać do metody zmienną liczbę argumentów. Może zostać użyte tylko przy ostatnim parametrze. Tradycyjnie jego typem jest tablica jednowymiarowa (od C# 13 dozwolone są również inne typy kolekcji); kompilator sam niejawnie tworzy tablicę z argumentów podanych w miejscu parametru `params`.

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
