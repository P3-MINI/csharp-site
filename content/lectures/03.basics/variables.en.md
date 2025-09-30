---
title: "Variables"
weight: 80
---

# Variables

## Definite Assignment

The definite assignment rule is a rule enforced by the compiler to ensure that every local variable is initialized before it is used. Using a potentially uninitialized variable results in a compilation error. This does not apply to array elements and class fields, which are default-initialized to zero (bit-wise).

```csharp
int x;
Console.WriteLine(x); // Compilation error

int[] ints = new int[2];
Console.WriteLine(ints[1]); // OK

Test test = new Test();
Console.WriteLine(test.X); // OK

public class Test { public int X; }
```

## Default Values

A variable can be explicitly initialized to its default value using the `default` operator. The default value is always a bitwise-zeroed object for value types, or `null` for reference types. Note that this always results in the value `0` for numeric types.

```csharp
float x = default;
Console.WriteLine(default(float));
```

## Implicitly Typed Local Variables

If the compiler can infer the variable's type from the right-hand side of the assignment, we can replace the type declaration with the `var` keyword. Variables must be initialized in the same line where they are declared.

```csharp
var greeting = "Hello class";
var i = 0;
var x = 0.15f;
var list = new List<int>();
var dayOfWeek = DateTime.Now.DayOfWeek;
```
