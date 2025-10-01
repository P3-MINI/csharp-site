---
title: "Namespaces"
weight: 100
---

# Namespaces

Namespaces work on the same principle as in C++. The difference is the way elements are extracted from a namespace; in C#, this is done using the `.` operator. Additionally, there are several ways to define namespaces.

> The exception is the global namespace, from which elements are extracted using the `global::` operator. In practice, however, this is rarely useful (if at all).

```csharp
Outer.Middle.Inner.ClassA a;

namespace Outer
{
    namespace Middle
    {
        namespace Inner
        {
            class ClassA {}
        }
    }
}

namespace Outer.Middle.Inner
{
    class ClassB {}
}

namespace Outer.Middle.Inner; // Applies to the end of the file

class classC {}
```

## The `using` directive

The `using` directive allows you to import namespaces into the current file, so you don't have to use fully qualified names for the types they contain.

```csharp
using Outer.Middle.Inner;

Outer.Middle.Inner.ClassB b1;
ClassB b2; // This is also possible

namespace Outer.Middle.Inner
{
    class ClassB {}
}
```

## Static `using`

Static `using` **imports types**, not namespaces. The effect is that you can use all static members of classes, and in the case of enums, their elements, without providing the full qualifier.

```csharp
using static System.Console; // class
using static System.DayOfWeek; // enum

WriteLine("Hello, world!");
WriteLine(Monday); // without using, it would be `DayOfWeek.Monday`
```

## Global `using`

Global `using` propagates to all files in the project. It must be placed before other `using` directives.

```csharp
global using System;
global using System.IO;
```

## Implicit global `using`

Since .NET 6.0, SDK-style projects have an option that adds a file with global `using` directives to your program when it is built.

These directives depend on the selected SDK; for "Microsoft.NET.Sdk" they are:

- System
- System.Collections.Generic
- System.IO
- System.Linq
- System.Net.Http
- System.Threading
- System.Threading.Tasks

You can disable this option by setting the `ImplicitUsings` property to `false` in the project file.

## Type aliases

The `using` directive can also be used to introduce an alias for a single type or namespace, instead of importing everything from a namespace. This is useful when two types with the same name from different namespaces conflict, and you want to use both, e.g., `Vector3` from `System.Numerics` and `Vector3` from an external library. Since C# 12, we can alias almost any type, including arrays and tuples.

```csharp
using Vec3 = System.Numerics.Vector3;
using Refl = System.Reflection;
using NumberList = double[];
using PersonInfo = (string Name, int Age);

Vec3 vector;
Refl.PropertyInfo propInfo;
NumberList numbers = { -0.5, 0.5 };
PersonInfo person = ("Alice", 42);
```
