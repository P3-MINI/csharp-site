---
title: "Instrukcje"
weight: 70
---

# Instrukcje

Instrukcje `if`, `for`, `while` i `do while` wyglądają identycznie jak w C++. Jedyną różnicą jest warunek logiczny w tych instrukcjach, który musi się ewaluuować do wartości boolowskiej.

## Instrukcja `switch`

W przeciwieństwie do C++, instrukcja switch w C# nie pozwala na niejawne przejście kontroli z jednego bloku `case` do następnego. Można natomiast przyczepić wiele etykiet `case` do jednego bloku.

Poza dopasowaniem do stałych, instrukcja `switch` może używać dopasowywania do dowolnego wzorca. Więcej o wzorcach później.

Dodatkowo po wzorcu możemy podać dodatkowy warunek który będzie rozważany dodatkowo po sprawdzeniu wzorca.

```csharp
switch (expression)
{
    case pattern:
        statement list
    case pattern:
    case pattern when condition: // optional case guard
        statement list
    default:
        statement list
}
```

Przykład instrukcji z dwoma najczęściej używanymi typami wzorców i opcjonalnym warunkiem:

```csharp
void TellMeAboutTheObject(object obj)
{
    switch (obj)
    {
        case 0: // constant pattern
            Console.WriteLine("It's a zero.");
            break;
        case string str: // type pattern
            Console.WriteLine($"It's a string: {str}");
            break;
        // type pattern with case guard
        case DateTime dt when dt.DayOfWeek == DayOfWeek.Monday:
            Console.WriteLine("It's a Monday");
            break;
        default:
            Console.WriteLine("IDK");
            break;
    }
}
```

## Wyrażenie `switch`

Wyrażenie `switch` jest bardziej zwięzłe i służy do zwracania pojedynczej wartości na podstawie dopasowania do wzorca.

W tym przypadku każdy `case` musi zostać obsłużony, w przeciwnym wypadku runtime rzuca wyjątkiem. Odpowiednikiem `default` w wyrażeniu `switch` jest wzorzec odrzucenia (*discard*) `_`.

```csharp
type variable = input expression switch
{
    pattern => candidate expression ,
    pattern when condition => candidate expression ,
    pattern => candidate expression
}
```

Przykład:

```csharp
string cardName = cardNumber switch
{
    13 => "King", // constant pattern
    12 => "Queen",
    11 => "Jack",
    > 1 and < 11 => "Pip card", // relational pattern
    1 => "Ace",
    // discard pattern, equivalent of default:
    _ => throw new ArgumentOutOfRangeException()
};
```

## Instrukcja `foreach`

Podobnie jak w C++ mamy w c# *range for loopa*, ma trochę inną składnię. Można go używać na wszystkim co jest **iterowalne**. Iterowalne są np. wszystkie kolekcje wbudowane, tablice i stringi. Czym jest iterowalność powiemy sobie później.

```csharp
int[] array = new int[] {0, 1, 2, 3, 4};
foreach (var i in array)
    Console.WriteLine(i);
foreach (char c in "foreach")
    Console.WriteLine(c);
```
