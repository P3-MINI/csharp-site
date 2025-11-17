---
title: "Patterns"
---

# Patterns

Patterns are a mechanism that allows you to check if a given variable or expression has a specific "form".
Patterns can be used in the following places:

* The `is` statement, which checks if an expression matches a pattern, returning the result as a `bool`:
  ```csharp
  expression is pattern
  ```
* The `switch` statement
* The `switch` expression

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

## Type Pattern

This is the most commonly used pattern. It checks the runtime type of an expression and, optionally, if it matches, assigns it to a newly declared variable of that type.

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

## Constant Pattern

Checks if the value of an expression is equal to a specified constant (including `null`).

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

## Relational Patterns (C# 9.0)

Compare the value of an expression with a constant using relational operators (`<`, `>`, `<=`, `>=`).

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

## Logical Patterns (C# 9.0)

Allow you to combine other patterns using the `and`, `or`, and `not` keywords.

```csharp
string? input = null;
if (input is not null) { /**/ }
```

```csharp
static bool IsLetter(char c) =>
c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z');
```

## Property Patterns

Checks if an object is not null, and then matches patterns against its properties or fields.

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
## Positional Pattern

Used for types that have a `Deconstruct` method (e.g., tuples and records). It deconstructs the object and matches patterns against its components.

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

## `var` Pattern

Introduces a new variable that can be referenced later. It always matches any value other than `null`.

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

## Discard Pattern

Matches any value, including `null`.

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

## List Patterns (C# 11)

Allow matching patterns against types that have an indexer accepting an `int` and a `Count` or `Length` property returning an `int`. This means, for example, lists, arrays, and strings.

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

Within a list pattern, you can use the `..` pattern (the *slice pattern*) once to match the rest of the sequence. It can be combined with the `var` pattern to introduce a new variable, or with a property pattern. This pattern requires the type to implement an indexer that accepts a `Range` or a corresponding `Slice` method.

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
