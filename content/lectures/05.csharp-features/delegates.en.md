---
title: "Delegates"
---

## Delegates

A delegate is an object that knows how to call a method - it's a type-safe equivalent of a function pointer in C/C++.

A delegate type is declared as follows:

```csharp
delegate double Function(double x);
```

The parameters and return type of a delegate define what methods the delegate can call. A delegate defined this way will be compatible with methods that take a `double` parameter and return a `double`. For example:

```csharp
double QuadraticFunction(double x) => x * x - 2 * x + 1;
```

A delegate object instance can be created by assigning a compatible method to a variable of the delegate type.

```csharp
Function quadratic = QuadraticFunction;
```

> [!NOTE]
> All delegates implicitly inherit from the `System.Delegate` class. They store a reference to the method they call and, optionally, a reference to the object on which the method is invoked if the method is not static. It's worth remembering this, as a delegate can thereby extend the lifetime of objects and even cause memory leaks.

A delegate can be invoked just like a method:

```csharp
double y = quadratic(2.0);
```

> This is equivalent to calling `quadratic.Invoke(2.0)`

A complete example of using delegates:

```csharp
// Numerics.cs
public static class Numerics
{
    public delegate double Function(double x);

    public static double NewtonRootFinding(Function f, Function df, double x0 = 0, double eps = 1e-6)
    {
        double x;
        double xn = x0;

        do
        {
            x = xn;
            xn = x - f(x) / df(x);
        } while (Math.Abs(x - xn) >= eps);

        return xn;
    }
}

// Quadratic.cs
public class Quadratic
{
    public double A { get; }
    public double B { get; }
    public double C { get; }

    public Quadratic(double a, double b, double c)
    {
        A = a;
        B = b;
        C = c;
    }

    public double Function(double x) => A * x * x + B * x + C;

    public double Derivative(double x) => 2 * A * x + B;

    public override string ToString() => $"f(x) = {A}x^2 + {B}x + {C}";
}

// Program.cs
public static class Program
{
    public static void Main()
    {
        var quadratic = new Quadratic(1.0, -7.0, 10.0);

        Numerics.Function function = quadratic.Function;
        Numerics.Function derivative = quadratic.Derivative;

        double root = Numerics.NewtonRootFinding(function, derivative);

        Console.WriteLine($"Root of {quadratic}: {root:F2}");
    }
}
```

> [!NOTE]
> **Source Code**
> {{< filetree dir="lectures/csharp-features/delegates/Numerics/" >}}

### Generic Delegates

A Delegate type can be generic:

```csharp
Function<double> function = QuadraticFunction;
double result = function(3);
Console.WriteLine(result);

// Compatible method:
double QuadraticFunction(double x) => x * x - 2 * x + 1;

// Delegate type declaration:
delegate T Function<T>(T x);
```

### System Delegates

The standard library provides two kinds of generic delegates: `Func` and `Action`. **There is no need to define your own delegate types**; these built-in ones are general enough to represent any delegate.

```csharp
delegate TResult Func<out TResult>();
delegate TResult Func<in T, out TResult>(T arg);
delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
// ... and so on, up to T16
delegate void Action();
delegate void Action<in T>(T arg);
delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
// ... and so on, up to T16
```

> [!NOTE]
> Generic parameters for delegates can be marked with `in` (contravariant) and `out` (covariant), just like generic parameters in interfaces. This allows for greater flexibility when assigning delegates with compatible but not identical type parameters.

### Multicast Delegates

Delegates can hold references to multiple methods. The `+=` and `-=` operators allow adding and removing methods from a delegate. Invoking such a delegate will call all the methods it holds, in the order they were added. Such delegates implicitly inherit from `System.MulticastDelegate`, which in turn inherits from `System.Delegate`.

```csharp
Action<string> writeLog = Console.WriteLine;
writeLog += WriteLogToFile;

writeLog("DEBUG: This is a test entry");

private static void WriteLogToFile(string log)
{
    File.AppendAllText("test.log", $"{log}");
}
```

Using the `+=` operator on a delegate that is `null` will also work. This will be equivalent to assigning a new delegate.

```csharp
Action<string>? writeLog = null;
writeLog += Console.WriteLine;

writeLog?.Invoke("DEBUG: This is a test entry");
```

> If the delegate returns a value, the value from the last method on the invocation list is returned.

## Lambda Expressions

A lambda expression is an unnamed method that can be assigned to a compatible delegate type.

```csharp    
Numerics.Function function = (x) => x * x - 2 * x + 1;
Numerics.Function derivative = (x) => 2 * x - 2;

double root = Numerics.NewtonRootFinding(function, derivative);
```

The general syntax of a lambda is as follows:

```csharp
(parameters) => expression-or-statement-block
```

```csharp
Func<double, double> square = x => x * x;
Func<char, int, string> repeat = (c, i) => new string(c, i);
Action<string> write = str => Console.Write(str);
Func<string> greet = () => { return "Hello, world"; };
```

- If a lambda has only one parameter, the parentheses can be omitted.
- If a lambda has only one expression, we can use the expression body syntax and omit the `return` keyword.
- The compiler can infer the parameter types and the return value of the lambda from the type on the left-hand side.

We can also explicitly specify the parameter types and, since C# 10, the return type (written before the parameter list). Sometimes this simply improves code readability. Explicit parameter types also allow the compiler to infer the delegate type on its own, enabling us to use the `var` keyword on the left-hand side of the assignment.

```csharp
var square = double (double x) => x * x;
var repeat = string (char c, int i) => new string(c, i);
var write = void (string str) => Console.Write(str);
var greet = string () => { return "Hello, world"; };
```

### Natural Type (C# 10)

Since C# 10, a lambda with explicitly typed parameters (or without parameters) has a *natural type*: the compiler picks a matching `Func` or `Action` delegate (or synthesizes an anonymous delegate type if none fits, e.g., for `ref` parameters). The return type is inferred from the body – it has to be specified explicitly only if it cannot be inferred. A lambda with implicitly typed parameters has no natural type, so it needs a target delegate type (e.g., `Func<int, int>` on the left-hand side). Method groups also have a natural type, as long as they refer to a single method.

```csharp
var cube = (double x) => x * x * x;            // Func<double, double>
var choose = object (bool b) => b ? 1 : "one"; // Func<bool, object>
var read = Console.ReadLine;                   // Func<string?>
Delegate d = (int x) => x + 1;                 // Func<int, int>

var identity = x => x;         // Error CS8917: The delegate type could not be inferred
var print = Console.WriteLine; // Error CS8917 (WriteLine has many overloads)
```

Also since C# 10, lambdas can have attributes – on the lambda itself, on its parameters, and on its return value (the parameter list must then be in parentheses). They are used mainly by frameworks that inspect delegates through reflection, e.g., ASP.NET Core minimal APIs.

```csharp
using System.ComponentModel;
using System.Reflection;

var greet = [Description("Greets a person")] (string name) => $"Hello, {name}!";

var attribute = greet.Method.GetCustomAttribute<DescriptionAttribute>();
Console.WriteLine(attribute?.Description); // Greets a person
```

### Default and `params` Parameters (C# 12)

Since C# 12, lambda parameters can have default values, and the last parameter can be a `params` array:

```csharp
var write = (string str = "hello") => Console.WriteLine(str);
write();
write("world");

var sum = (params int[] values) => values.Sum();
Console.WriteLine(sum(1, 2, 3)); // 6
```

Such lambdas cannot be represented by `Func` or `Action`, so their natural type is a delegate type synthesized by the compiler.

### Parameter Modifiers Without Types (C# 14)

Parameters with modifiers such as `ref`, `out`, `in`, `ref readonly` or `scoped` used to require an explicitly specified type. Since C# 14, the types of such parameters can be inferred from the target delegate type as well:

```csharp
TryParse<int> parse = (text, out result) => int.TryParse(text, out result);
// Before C# 14: (string text, out int result) => int.TryParse(text, out result)

if (parse("42", out int number))
{
    Console.WriteLine(number); // 42
}

delegate bool TryParse<T>(string text, out T result);
```

### Capturing Variables

In lambda expressions, we can reference outside variables. We say that such variables are "captured". What happens when we capture a variable is that the compiler generates a special class to store the captured variables. If multiple lambdas refer to the same variable, they will all refer to the same instance of the generated class and the same field. Captured variables live on the heap as part of the generated class. It's important to remember that capturing variables involves a heap allocation.

The variable `i` will be captured and placed on the heap, and it will be shared by all lambdas:

```csharp
Action[] actions = new Action[3];
for (int i = 0; i < 3; i++)
{
    actions[i] = () => Console.Write(i);
}
foreach(Action action in actions) 
{
    action();
}
```

{{% details title="The compiler will generate the following code:" open=false %}}
```csharp
[CompilerGenerated]
private sealed class <>c__DisplayClass0_0
{
    public int i;

    internal void <<Main>$>b__0()
    {
        Console.Write(i);
    }
}

private static void <Main>$(string[] args)
{
    Action[] array = new Action[3];
    <>c__DisplayClass0_0 <>c__DisplayClass0_ = new <>c__DisplayClass0_0();
    <>c__DisplayClass0_.i = 0;
    while (<>c__DisplayClass0_.i < 3)
    {
        array[<>c__DisplayClass0_.i] = new Action(<>c__DisplayClass0_.<<Main>$>b__0);
        <>c__DisplayClass0_.i++;
    }
    Action[] array2 = array;
    int num = 0;
    while (num < array2.Length)
    {
        Action action = array2[num];
        action();
        num++;
    }
}
```
{{% /details %}}

If a lambda should not capture anything (e.g., to avoid accidental heap allocations), we can mark it with the `static` modifier (C# 9). Referencing a local variable, a parameter, or `this` inside such a lambda is a compile-time error:

```csharp
int factor = 3;
Func<int, int> triple = static x => x * 3;
Func<int, int> multiply = static x => x * factor; // Error CS8820: A static anonymous function cannot contain a reference to 'factor'
```

## Anonymous Methods

Anonymous methods were introduced in C# 2.0 and have been completely superseded by lambda expressions, which were introduced in the next version. There are no advantages to using anonymous methods over lambda expressions. All subsequent language enhancements have focused exclusively on lambdas. You won't find any bells and whistles here.

Unlike lambda expressions, the body of an anonymous method must always be a code block. However, the parameter list can be omitted if the parameters are not used.

```csharp
Numerics.Function function = delegate (double x) { return x * x - 2 * x + 1; };
Numerics.Function derivative = delegate (double x) { return 2 * x - 2; };

double root = Numerics.NewtonRootFinding(function, derivative);
```

The only useful feature of anonymous methods is the ability to create an empty method by omitting the parameters; such an anonymous method can be assigned to any delegate that has no return value. This is an alternative to initializing a delegate with `null`, which helps avoid `NullReferenceException` when trying to invoke an 'empty' delegate.

```csharp
Action<string> writeLog = delegate {};

writeLog("DEBUG: This is a test entry");
```
