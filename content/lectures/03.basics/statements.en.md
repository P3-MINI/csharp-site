---
title: "Statements"
weight: 70
---

# Statements

The `if`, `for`, `while`, and `do while` statements look identical to those in C++. The only difference is the logical condition in these statements, which must evaluate to a boolean value.

## The `switch` statement

Unlike in C++, the `switch` statement in C# does not allow control to fall through from one `case` block to the next implicitly. However, you can attach multiple `case` labels to a single block.

In addition to matching constants, the `switch` statement can use pattern matching for any pattern. More on patterns later.

Additionally, after the pattern, we can provide an additional condition that will be considered after the pattern is checked.

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

An example of a statement with the two most commonly used types of patterns and an optional condition:

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

## The `switch` expression

The `switch` expression is more concise and is used to return a single value based on pattern matching.

In this case, every `case` must be handled; otherwise, the runtime throws an exception. The equivalent of `default` in a `switch` expression is the discard pattern `_`.

```csharp
type variable = input expression switch
{
    pattern => candidate expression ,
    pattern when condition => candidate expression ,
    pattern => candidate expression
}
```

Example:

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

## The `foreach` statement

Similar to C++, in C# we have a *range for loop*, but with a slightly different syntax. It can be used on anything that is **iterable**. For example, all built-in collections, arrays, and strings are iterable. We will discuss what makes something iterable later.

```csharp
int[] array = new int[] {0, 1, 2, 3, 4};
foreach (var i in array)
    Console.WriteLine(i);
foreach (char c in "foreach")
    Console.WriteLine(c);
```
