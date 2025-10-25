---
title: "Exceptions"
---

# Exceptions

The implementation of exceptions in C# is very similar to that in C++. Syntactically, it's nearly identical, with a few new capabilities in C#. The main difference is that in C#, we can only throw objects that inherit from `System.Exception`. In C++, we could throw virtually any object, although this is not a common practice.

## Catching Exceptions

Just as in C++, to catch an exception, we surround the code that might throw an exception with a `try` block and attach `catch` blocks to handle the exceptional situations. When an exception occurs, the program searches for a suitable `catch` block from top to bottom.

```csharp
try
{
    string text = File.ReadAllText("file.txt");
    int number = int.Parse(text);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Invalid format: {ex}");
}
catch (Exception ex) // This will catch any exception
{
    Console.WriteLine($"Unexpected exception: {ex}");
}
```

## Throwing Exceptions

To throw an exception, we use the `throw` statement followed by an exception object, usually created in-place with the `new` operator. The type of the exception itself partially documents the exceptional situation, but it is also a good practice to specify what happened by providing an additional message in the constructor's parameter.

```csharp
public static void ValidateInput(string input, int maxLength)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        throw new ArgumentNullException(nameof(input), "Input cannot be null or empty.");
    }

    if (input.Length > maxLength)
    {
        throw new ArgumentOutOfRangeException(nameof(input), $"Input exceeds the maximum length of {maxLength} characters.");
    }
}
```

## Re-throwing an Exception

In a `catch` block, we can pass the exception on while partially reacting to it (e.g., by logging the error). This is done using the `throw` statement without any parameter.

```csharp
try
{
    ValidateInput("Hello, world", 16);
}
catch (ArgumentNullException e)
{
    Console.WriteLine(e);
}
catch (ArgumentOutOfRangeException e)
{
    Console.WriteLine(e);
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
```

## Exception Filters

We can append a `when` clause to `catch` blocks, in which we can specify additional conditions that must be met:

```csharp
try
{
    throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    Console.WriteLine("Resource not found.");
    Console.WriteLine(ex);
}
catch (HttpRequestException ex)
{
    Console.WriteLine(ex);
}
```

## Custom Exception Types

The standard library provides a considerable range of built-in exception types. Sometimes, however, we need more specific exception types to better describe a situation. This makes the code more readable and error handling more precise. Typically, the three most common constructors are re-implemented, but of course, you can add your own constructors and fields to such a class.

```csharp
public class InvalidOrderException : Exception
{
    public InvalidOrderException() 
    {
    }

    public InvalidOrderException(string message)
        : base(message)
    {
    }

    public InvalidOrderException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
```

## The `finally`

A `finally` block can be attached to a `try-catch` block. The code within such a block will always execute, regardless of the circumstances under which the `try` block is exited. The main use of the `finally` block is to release resources such as file streams, database connections, or network resources.

```csharp
FileStream? fs = null;
try
{
    fs = new FileStream("file.txt", FileMode.Open);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    fs?.Dispose();
}
```
