---
title: "Attributes"
weight: 10
---

# Attributes

Attributes are "labels" that can be attached to various code elements: classes, methods, properties, fields, parameters, etc.

Attributes store additional information (i.e., metadata) about the element they are attached to.

Attributes do nothing on their own. They are passive information. Tools, the compiler, or the program itself can read these attributes and take action based on them.

We attach attributes by listing them in `[]` before the chosen element:

```csharp
[Obsolete("Use iterator `GetFibonacciIter` method instead.")]
public static int GetFibonacci(int n)
{
    if (n < 0) throw new ArgumentException(nameof(n), "Input must be a non-negative integer.");
    if (n is 0 or 1) return 1;
    return GetFibonacci(n - 1) + GetFibonacci(n - 2);
}
```

> [!INFO]
> When trying to use a method marked with `Obsolete`, the compiler will generate a warning: `warning CS0618: 'FibUtils.GetFibonacci(int)' is obsolete: 'Use iterator method instead.'`

An attribute is a class itself. The definition of the system's `Obsolete` attribute looks like this:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
public sealed class ObsoleteAttribute : Attribute
{
  public ObsoleteAttribute();
  public ObsoleteAttribute(string? message);
  public ObsoleteAttribute(string? message, bool error);

  public string? DiagnosticId { get; set; }
  public bool IsError { get; }
  public string? Message { get; }
  public string? UrlFormat { get; set; }
}

```

Attributes inherit from `System.Attribute`, and by convention, their names end with the `...Attribute` suffix (we can omit this suffix when using the attribute).

## Attribute Parameters

Values can be passed to attributes in two ways:

* **Positional Parameters**: These correspond to the parameters of the attribute class's constructor. They are passed in parentheses `()` after the attribute name, in the same order as in the constructor's signature.
* **Named Parameters**: These correspond to the public properties or fields of the attribute class. They are optional and can be assigned after positional parameters using the `Name = value` syntax.

If an attribute has no parameters, we can omit the `()`.

For example, for the `Obsolete` attribute:

```csharp
[Obsolete]
public void OldMethod() { }

[Obsolete("This method is deprecated.")]
public void VeryOldMethod() { }

[Obsolete("This method is deprecated. See documentation.", UrlFormat = "https://csharp.mini.pw.edu.pl/")]
public void AnotherVeryOldMethod() { }
```

> [!WARNING]
> **Attribute arguments must be compile-time constants.**

## Attaching an Attribute

An attribute can be attached to almost any code element: 

- Assembly
- Module
- Class
- Struct
- Interface
- Enum
- Delegate
- Constructor
- Method
- Property
- Field
- Event
- Return value
- Generic parameter

### Examples of Attaching Attributes

Sometimes, to attach an attribute to the desired element, you must explicitly specify the attribute's target.

```csharp
[assembly: Description("Applied to an assembly")]
[module: Description("Applied to a module")]

[Description("Applied to a class")]
public class Stack<[Description("Applied to a generic parameter")] T>
{
    [Description("Applied to a field")]
    private T[] _items = new T[8];
    [field: Description("Applied to a backing field")]
    [Description("Applied to a property")]
    public int Count { get; private set; }

    [Description("Applied to a method")]
    public void Push([Description("Applied to a parameter")] T item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    [return: Description("Applied to a return value")]
    [method: Description("Implicitly applied to a method")]
    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }
}
```

### Attaching Multiple Attributes:

We can attach multiple attributes to a single element. This can be done in two ways, in a single set of `[]`:

```csharp
[Description("Multiple attributes applied to a method"), Conditional("DEBUG")]
private static void Log(string message) 
{ 
    Console.WriteLine($"{DateTime.Now}: {message}");
}
```

Or by opening a new set of `[]`:

```csharp
[Description("Multiple attributes applied to a method")]
[Conditional("DEBUG")]
private static void Log(string message) 
{ 
    Console.WriteLine($"{DateTime.Now}: {message}");
}
```

> [!INFO]
> The `Conditional` attribute conditionally includes or excludes calls to a method. If the compilation symbol (e.g., `DEBUG`) is not defined, all calls to the method marked with this attribute (along with its arguments) will be completely removed from the compiled code. The method must have a void return type.

## Caller Info Attributes

*Caller Info* attributes are a special set of attributes that allow you to automatically obtain information about the code that called a given method. *Caller Info* attributes are applied to optional parameters of a method. These parameters must have a default value (e.g., null, 0, ""). The compiler automatically overwrites this default value with the appropriate caller information at the call site.

```csharp
public static void Err(
    string message,
    [CallerMemberName] string memberName = "", // The name of the calling method/property
    [CallerFilePath] string filePath = "",     // The path of the calling file
    [CallerLineNumber] int lineNumber = 0)     // The line number of the call
{
    Console.WriteLine($"{message}");
    Console.WriteLine($"  Called from: {memberName}");
    Console.WriteLine($"  File: {filePath}");
    Console.WriteLine($"  Line: {lineNumber}");
    Environment.Exit(-1);
}
```

For the following call:

```csharp
public static void LoadConfiguration(string configFilePath)
{
    if (!File.Exists(configFilePath))
    {
        Err($"Configuration file not found at: '{configFilePath}'"); // line 24
    }
    Console.WriteLine($"Configuration loaded successfully from: '{configFilePath}'");
}

public static void Main(string[] args)
{
    string config = "appsettings.json"; 
    LoadConfiguration(config);
}
```

We might get the following output:

```bash
Configuration file not found at: 'appsettings.json'
  Called from: LoadConfiguration
  File: /home/tomasz/Workspace/CSharp/ConsoleApp/ConsoleApp/Program.cs
  Line: 24

Process finished with exit code 255.
```

Unlike the other *Caller Info* attributes, which provide information about the call site (method name, file, line number), `CallerArgumentExpression` allows you to capture the source code expression passed as an argument to another parameter of the same method.

```csharp
public static void Assert(bool condition, 
    [CallerArgumentExpression(nameof(condition))] string? message = null)
{
    if (!condition)
    {
        Console.Error.WriteLine($"Assertion failed: {message}");
    }
}
```

```csharp
Assertion.Assert("Hello, World!" is { Length: < 5 });
// Output:
// Assertion failed: "Hello, World!" is { Length: < 5 }
```

> [!INFO]
> The `nameof` operator obtains the name of a variable, type, or member as a string constant. This is evaluated at compile time and protects against errors related to name refactoring. If the name does not exist, the compiler will report an error.
