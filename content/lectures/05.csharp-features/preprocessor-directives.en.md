---
title: "Preprocessor Directives"
---

## Preprocessor Directives

The preprocessor in C# is much simpler than in C/C++.
* There is no `#include` directive (in C#, we use `using` to import namespaces).
* `#define` does not allow creating macros with values or functional macros. It is only used for defining conditional symbols.

## Conditional Compilation (#if, #elif, #else, #endif)

This is the most common and important use of directives. It allows compiling blocks of code only when a specific symbol is defined. Symbols can be defined and undefined using the `#define` and `#undef` directives.

Logical operators `!`, `&&`, and `||` can be used within conditions.

```csharp
#define TEST
#undef TRACE
#if TEST
    Console.WriteLine("TEST is defined");
#endif
#if !TEST
    Console.WriteLine("TEST is not defined");
#endif
#if TEST && DEBUG
    Console.WriteLine("DEBUG and TEST is defined");
#else
    Console.WriteLine("DEBUG and TEST are not both defined");
#endif
```

They can also be defined in the project file. Some symbols, such as `DEBUG`, are automatically included, for example, in the `Debug` configuration.

```xml
<PropertyGroup>
    <DefineConstants>TRACE;DEMO</DefineConstants>
</PropertyGroup>
```

## Nullable Context

The `#nullable` directive can be used to enable and disable the *nullable* context, in which the compiler performs static analysis for type nullability.

```csharp
#nullable disable
string str1 = null;
#nullable enable
string str2 = null!;
#nullable restore
string str3 = null!;
```

## Regions

Regions themselves do nothing. They group code into blocks that can be collapsed in the IDE.

```csharp
#region REGION_EXPLANATION
private static void Region()
{
    PrintCurrentMethodName();
    Console.WriteLine("""
        region is a preprocessor directive
        that is used to group related pieces
        of code together in a way that can
        be collapsed or expanded in the code editor.
        """);
}
#endregion
```

## Warning Control

The `#pragma warning` directive allows suppressing and restoring compiler warnings.

```csharp
#pragma warning disable CS0219 // unused variable
string str = "Hello, world!";
#pragma warning restore CS0219
```

> Error codes can be found in the [GenerateCSharpErrors repository](https://github.com/thomaslevesque/GenerateCSharpErrors/blob/master/CSharpErrorsAndWarnings.md).

Directives can also be used to generate compilation warnings and errors, which is useful in conjunction with conditional compilation:

```csharp
#if !DEMO
    #error DEMO is not defined
    Console.WriteLine("This code will not "+
    "compile if DEMO is not defined.");
#else
    #warning DEMO is defined
    Console.WriteLine("This code will generate a warning message if DEMO is defined.");
#endif
```