---
title: "Parameters"
weight: 90
---

# Parameters

Parameters are passed by value; the rules for value and reference types are described in [Type System]({{< ref "../03.basics/types#passing-parameters-to-methods" >}}).

| Modifier | Passed *by* | Definite Assignment       | Notes            |
|------------|-------------|---------------------------|------------------|
| (none)     | value       | before the call           |                  |
| `ref`      | reference   | before the call           |                  |
| `in`       | reference   | before the call           | read-only        |
| `out`      | reference   | before returning control  |                  |

Furthermore, you can pass parameters **by reference** in three different ways.

## Passing by Reference (`ref`)

Passing a parameter using `ref` allows the method to read and write to the passed variable. Passing a reference type by reference allows the reference itself to be changed from within the method.

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

## Output Parameters (`out`)

The `out` modifier works similarly, with the difference that the variable must be assigned within the method, and it can be uninitialized before the method call. This is one way to return more than one value from a method. Furthermore, if you are not interested in one of the output values, you can discard it using the `_` symbol.

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

## Input Parameters (`in`)

The `in` modifier also works similarly; the difference here is that the passed parameter becomes read-only within the method body.

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

## Variable Number of Parameters (`params`)

The `params` keyword allows a variable number of arguments to be passed to a method. It can only be used for the last parameter and must be a single-dimensional array. The compiler implicitly creates an array from the arguments provided for the `params` parameter.

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

## Optional Parameters and Named Arguments

Parameters in C# can have default values. The default value must be a compile-time constant. All default parameters must be defined after any required parameters. If there is a need to omit some optional parameters, you can specify which ones you want to assign a value to by using named arguments.

```csharp
public void ExampleMethod(int required,
                          string optionalstr = "default",
                          int optionalint = 10) {}
ExampleMethod(3, "parameter", 7);
ExampleMethod(3, "parameter");
ExampleMethod(3);
ExampleMethod(3, optionalint: 4);
```
