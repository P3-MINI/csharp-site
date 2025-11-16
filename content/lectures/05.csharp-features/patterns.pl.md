---
title: "Wzorce"
---

# Wzorce

Wzorce (*patterns*) to mechanizm, który pozwala na sprawdzanie, czy dana zmienna lub wyrażenie ma określoną "postać".
Wzorców można używać w następujących miejscach:

* instrukcja `is`, sprawdza czy wyrażenie pasuje do wzorca, zwracając wynik w postaci `bool`a:
  ```csharp
  expression is pattern
  ```
* instrukcja `switch`
* wyrażenie `switch`

```csharp
bool IsWorkingDay(object date)
{
    if (date is null) throw new ArgumentNullException();
    if (date is DateTime dateTime)
    {
        if (dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return true;
        return false;
    }
    return date is DateOnly {DayOfWeek: not (DayOfWeek.Saturday or DayOfWeek.Sunday)}
}
```

## Wzorzec typu

Najczęściej używany wzorzec. Sprawdza typ wyrażenia w czasie wykonania i, opcjonalnie, jeśli się zgadza, przypisuje je do nowo zadeklarowanej zmiennej tego typu.

```csharp
object greeting = "Hello, World!";
if (greeting is string message)
{
    Console.WriteLine(message.ToLower());
}
```

```csharp
public abstract class Vehicle;
public class Car : Vehicle;
public class Truck : Vehicle { public float Load; }
public static class TollCalculator
{
    public static decimal CalculateToll(Vehicle vehicle) =>
    vehicle switch
    {
        Car => 2.00m,
        Truck truck => truck.Load > 100 ? 17.50m : 7.50m,
        _ => throw new ArgumentException(),
    };
}
```

## Wzorzec stałej

Sprawdza, czy wartość wyrażenia jest równa określonej stałej (w tym `null`).

```csharp
string? input = null;
if (input is null) 
{ 
    Console.WriteLine("Input is null");
}
```

```csharp
static decimal GetGroupTicketPrice(int visitorCount) => visitorCount switch
{
    1 => 12.0m,
    2 => 20.0m,
    3 => 27.0m,
    4 => 32.0m,
    0 => 0.0m,
    _ => throw new ArgumentException(),
};
```

## Wzorzec porównania (C# 9.0)

Porównują wartość wyrażenia ze stałą za pomocą operatorów porównania (`<`, `>`, `<=`, `>=`)

```csharp
static string GetCalendarSeason(DateTime date) => date.Month switch
{
    >= 3 and < 6 => "spring",
    >= 6 and < 9 => "summer",
    >= 9 and < 12 => "autumn",
    12 or (>= 1 and < 3) => "winter",
    _ => throw new ArgumentOutOfRangeException()
};
```

## Wzorce logiczne (C# 9.0)

Pozwalają łączyć inne wzorce za pomocą słów kluczowych and, or i not.

```csharp
string? input = null;
if (input is not null) { /**/ }
```

```csharp
static bool IsLetter(char c) =>
c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z');
```

## Wzorce właściwości

Sprawdza, czy obiekt nie jest null, a następnie dopasowuje wzorce do jego właściwości lub pól.

```csharp
static bool IsConferenceDay(DateTime date) 
{
    return date is { Year: 2020, Month: 5, Day: 19 or 20 or 21 };
}
```

```csharp
static string TakeFive(object input) => input switch
{
    string { Length: > 5 } s => s.Substring(0, 5),
    string s => s,
    ICollection<char> { Count: > 5 } symbols => new string(symbols.Take(5).ToArray()),
    ICollection<char> symbols => new string(symbols.ToArray()),
    null => throw new ArgumentNullException(),
    _ => throw new ArgumentException(),
};
```
## Wzorzec pozycyjny

Używany dla typów, które mają metodę `Deconstruct` (np. krotki (*tuple*) i rekordy). Dekonstruuje on obiekt i dopasowuje wzorce do jego składowych.

```csharp
public readonly struct Point
{
    public int X { get; }
    public int Y { get; }
    public Point(int x, int y) => (X, Y) = (x, y);
    public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
}

static string Classify(Point point) => point switch
{
    (0, 0) => "Origin",
    (> 0, > 0) => "First quadrant",
    (< 0, > 0) => "Second quadrant",
    (< 0, < 0) => "Third quadrant",
    (> 0, < 0) => "Fourth quadrant",
    _ => "Just a point",
};
```

## Wzorzec `var`

Wprowadza nową zmienną, do której można się potem odwoływać, zawsze pasuje do dowolnej wartości różnej od `null`.

```csharp
static bool IsAcceptable(int count, int absLimit)
{
    return SimulateDataFetch(count) is var results
        && results.Min() >= -absLimit
        && results.Max() <= absLimit;
}

static int[] SimulateDataFetch(int count)
{
    var rand = new Random();
    return Enumerable
        .Range(start: 0, count: count)
        .Select(s => rand.Next(minValue: -10, maxValue: 11))
        .ToArray();
}
```

## Wzorzec odrzucenia

Pasuje do dowolnej wartości, łącznie z `null`.

```csharp
static decimal GetDiscountPercent(DayOfWeek? dayOfWeek) => dayOfWeek switch
{
    DayOfWeek.Monday => 0.5m,
    DayOfWeek.Tuesday => 12.5m,
    DayOfWeek.Wednesday => 7.5m,
    DayOfWeek.Thursday => 12.5m,
    DayOfWeek.Friday => 5.0m,
    DayOfWeek.Saturday => 2.5m,
    DayOfWeek.Sunday => 2.0m,
    _ => 0.0m,
};
```

## Wzorzec listy (C# 11)

Pozwalają na dopasowywanie wzorców dla typów posiadających indeksator przyjmujący `int` i posiadający właściwość `Count` lub `Length` zwracającą `int`. Czyli na przykład dla list, tablic i stringów.

```csharp
int[] numbers = { 1, 2, 3 };

Console.WriteLine(numbers is [1, 2, 3]);
Console.WriteLine(numbers is [1, 2, 4]);
Console.WriteLine(numbers is [1, 2, 3, 4]);
Console.WriteLine(numbers is [0 or 1, <= 2, >= 3]);

List<int> list = new() { 1, 2, 3 };

if (list is [var first, _, _])
{
    Console.WriteLine($"The first element of a three-item list is {first}.");
}
```

We wzorcu listy można raz użyć wzorca `..` (*Slice pattern*), pasującego do reszty sekwencji. Można go łączyć ze wzorcem `var`, wprowadzając nową zmienną, lub wzorcem właściwości. Ten wzorzec wymaga implementacji od typu indeksera przyjmującego `Range` lub odpowiadającej metody `Slice`.

```csharp
Console.WriteLine(new[]{ 1, 2, 3, 4 } is [>= 0, .., 2 or 4]);
Console.WriteLine(new[]{ 1, 0, 0, 1 } is [1, 0, .., 0, 1]);
Console.WriteLine(new[]{ 1, 0, 1 } is [1, 0, .., 0, 1]);

string greeting = "Hello world";
if (greeting is ['H', .. var rest])
    Console.WriteLine(rest);

string Validate(int[] numbers)
{
    return numbers is [< 0, .. { Length: 2 or 4 }, > 0] ? "valid" : "not valid";
}

Console.WriteLine(Validate(new[] { -1, 0, 1 }));
Console.WriteLine(Validate(new[] { -1, 0, 0, 1 }));
```