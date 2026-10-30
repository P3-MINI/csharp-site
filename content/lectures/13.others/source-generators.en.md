---
title: "Source Generators"
weight: 10
---

## Partial Types (`partial`)

The `partial` keyword allows for splitting a type definition across multiple files. This informs the compiler that the type definition might not be complete and to look for remaining parts in other project files. During compilation, all parts of the definition are merged into one cohesive whole. At runtime, the execution environment sees only one complete type definition.

One application is code organization. For large type definitions, we might decide to split, for example, initialization and logic into different files.

```csharp
// File User.Data.cs
public partial class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Pesel { get; set; }
}
```

```csharp
// File User.Logic.cs
public partial class User
{
    public void Save()
    {
        // ...
    }
}
```

However, the `partial` keyword primarily allows us to separate human-written code from automatically generated code (e.g., by source generators or other build tools). This is most commonly used by UI systems (e.g., WPF, WinForms), which generate component initialization code based on files describing the appearance (e.g., `.xaml` files). Additionally, this mechanism can be used to generate repetitive code (e.g., `ToString()`), automatically implement interfaces, generate code checking regular expressions, or serialize types based on attributes.

### Merging Rules

When merging type parts, the compiler applies strict rules:
*   **Modifiers:** All parts that specify an access modifier must specify the same one (a part may also omit it). If you use `abstract` or `sealed` in one part, the entire class becomes so.
*   **Interfaces and Attributes:** These are cumulative. If different files implement different interfaces, the resulting class implements all of them.
*   **Base Class:** If specified, it must be identical in every part (or omitted).

It is worth remembering that for a generator to extend a programmer-written class, that class **must** also have the `partial` modifier.

### Partial Methods (`partial`)

Partial methods allow declaring a method signature in one file and its implementation in another. They operate in two modes:

1.  **Optional:** If the method has no access modifier (it is then implicitly private), returns `void`, and has no `out` parameters, its implementation is optional. If not provided, the compiler completely removes the method call from the resulting code.
2.  **Required (C# 9.0+):** If the method has an explicit access modifier (e.g., `public`, but also `private`), the implementation **must** be provided (usually by a generator). An access modifier is mandatory if the method returns a value or has `out` parameters.

```csharp
public partial class User
{
    partial void OnLoaded(); // Optional

    [GeneratedRegex(".*@.*\\..*")]
    private partial Regex EmailRegex(); // Required
}
```

### Other Partial Members (C# 13, C# 14)

Starting with C# 13 (.NET 9), properties and indexers can also be partial, and C# 14 (.NET 10) adds partial instance constructors and partial events. As with partial methods, one part is the *defining declaration* (signature only) and the other is the *implementing declaration*, which is always required. This gives generators new hooks: the programmer declares a member, and the generator provides its implementation.

```csharp
// Written by the programmer
public partial class User
{
    public partial User(string name);           // C# 14: partial constructor
    public partial string Name { get; set; }    // C# 13: partial property
    public partial event EventHandler? Changed; // C# 14: partial event
}

// Generated code (e.g., by a generator implementing change notifications)
public partial class User
{
    private string _name = "";
    private EventHandler? _changed;

    public partial User(string name) => _name = name;

    public partial string Name
    {
        get => _name;
        set
        {
            _name = value;
            _changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public partial event EventHandler? Changed
    {
        add => _changed += value;
        remove => _changed -= value;
    }
}
```

Restrictions: the defining declaration of a partial property looks like an auto-property (`{ get; set; }`), while the implementing one cannot be an auto-property. Only the implementing declaration of a partial constructor can have a `this(...)`/`base(...)` initializer. The defining declaration of a partial event looks like a field-like event, and the implementing one must provide `add` and `remove` accessors.

Since .NET 9, `[GeneratedRegex]` can also be applied to a partial property:

```csharp
[GeneratedRegex(".*@.*\\..*")]
private static partial Regex EmailRegex { get; } // .NET 9+: generated property
```

## Source Generators

Source Generators are a compiler mechanism introduced together with C# 9.0 and .NET 5 (since .NET 6 superseded by the more efficient *Incremental Generators* - the original `ISourceGenerator` interface is now deprecated) that enables inspection of user code or other files during compilation and generation of new source files based on them. It is a form of compile-time metaprogramming.

Unlike reflection, source generators integrate directly with the compilation process. The generator first analyzes existing code and files. Based on the analysis, the generator creates C# source code text. The generated code is added to the compilation process as "virtual" files and compiled together with the rest of the project.

Generators can add new code but cannot modify existing code. This is why cooperation with partial types (`partial`) is so important, as they allow extending user classes with generated methods.

### Generator Implementation

A generator is a .NET Standard 2.0 library that implements the `IIncrementalGenerator` interface. It must be marked with the `[Generator]` attribute.

Below is an example of a more advanced generator that implements the **Builder** pattern for any class marked with the `[GenerateBuilder]` attribute. The generator analyzes the public, non-static properties of the class that have a `set` accessor and creates `With...` methods for them.

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;

namespace BuilderGenerator
{
    [Generator]
    public class BuilderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Add the marker attribute source code
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("GenerateBuilderAttribute.g.cs", SourceText.From(
                    """
                    using System;

                    namespace BuilderGenerator
                    {
                        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
                        public sealed class GenerateBuilderAttribute : Attribute { }
                    }
                    """, Encoding.UTF8));
            });

            // Create the pipeline to find and transform classes
            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                "BuilderGenerator.GenerateBuilderAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, ct) => GetClassToGenerate(ctx, ct))
                .Where(m => m != null);

            // Register the source output
            context.RegisterSourceOutput(pipeline, (ctx, data) => GenerateCode(ctx, data));
        }

        private static ClassModel? GetClassToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
        {
            if (context.TargetSymbol is not INamedTypeSymbol symbol) return null;

            ct.ThrowIfCancellationRequested();

            var properties = symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic && !p.IsIndexer && p.DeclaredAccessibility == Accessibility.Public &&
                            p.SetMethod is { IsInitOnly: false })
                .Select(p => new PropertyModel(
                    p.Name, 
                    p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) 
                ))
                .ToList();

            string ns = symbol.ContainingNamespace.IsGlobalNamespace 
                ? string.Empty 
                : symbol.ContainingNamespace.ToDisplayString();

            return new ClassModel(symbol.Name, ns, properties);
        }

        private static void GenerateCode(SourceProductionContext context, ClassModel? model)
        {
            if (model == null) return;

            var code = new CodeWriter();

            // Handle Namespace
            if (!string.IsNullOrEmpty(model.Namespace))
            {
                code.AppendLine($"namespace {model.Namespace}");
                code.StartBlock();
            }

            // Generate partial class (accessibility is taken from the user's declaration)
            code.AppendLine($"partial class {model.ClassName}");
            using (code.Block())
            {
                code.AppendLine($"public static Builder CreateBuilder() => new Builder();");
                code.AppendLine();

                code.AppendLine($"public class Builder");
                using (code.Block())
                {
                    code.AppendLine($"private readonly {model.ClassName} _target = new {model.ClassName}();");
                    code.AppendLine();

                    foreach (var prop in model.Properties)
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        code.AppendLine($"public Builder With{prop.Name}({prop.Type} value)");
                        using (code.Block())
                        {
                            code.AppendLine($"_target.{prop.Name} = value;");
                            code.AppendLine("return this;");
                        }
                        code.AppendLine();
                    }

                    code.AppendLine($"public {model.ClassName} Build() => _target;");
                }
            }

            if (!string.IsNullOrEmpty(model.Namespace))
            {
                code.EndBlock();
            }

            context.AddSource($"{model.ClassName}.Builder.g.cs", SourceText.From(code.ToString(), Encoding.UTF8));
        }
    }
}
```

The `Initialize` method does not generate code directly - it only builds a *pipeline* of transformations, which the compiler executes and whose intermediate results it caches (in the IDE, the pipeline runs after almost every keystroke):

*   `RegisterPostInitializationOutput` adds code that does not depend on the user's code - here, the marker attribute.
*   `SyntaxProvider.ForAttributeWithMetadataName` (available since Roslyn 4.3) finds declarations marked with an attribute of the given fully qualified name. It is much more efficient than `CreateSyntaxProvider`, which inspects every syntax node, because the compiler examines only declarations that carry an attribute with a matching name. The cheap `predicate` filters syntax nodes, and `transform` gets access to the semantic model and the symbol (`TargetSymbol`).
*   `transform` returns a simple model (`ClassModel`) with value equality. Thanks to that, the generator reuses cached results and regenerates code only when the model changes - this is why models should not contain syntax nodes or symbols.
*   `RegisterSourceOutput` produces source files from the models.

To use such a generator in an application project, we must add it as an `Analyzer`. This allows us to use the generated builders to create objects, even though we didn't write them ourselves.

```xml
<ItemGroup>
    <ProjectReference Include="..\BuilderGenerator\BuilderGenerator.csproj" 
                      OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```

```csharp
using BuilderGenerator;

namespace App
{
    [GenerateBuilder]
    public partial class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    internal class Program
    {
        static void Main()
        {
            // The CreateBuilder() method and the Builder class were generated:
            var user = User.CreateBuilder()
                .WithFirstName("John")
                .WithLastName("Doe")
                .WithAge(30)
                .Build();
        }
    }
}
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/other/source-generators" >}}

> [!TIP]
> To see the generated files, add `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` to the application project - they will be written to `obj/<configuration>/<target framework>/generated/`. IDEs (Visual Studio, Rider) also show generated files among the project's dependencies (in the analyzers/source generators node); you can navigate to them and debug them like ordinary code.

### Applications in the .NET Platform

*   **`System.Text.Json`**: Generates serialization code at compile time, allowing operation without reflection (crucial for AOT - *Ahead-of-Time compilation*).
*   **`System.Text.RegularExpressions` (.NET 7+)**: The `[GeneratedRegex]` attribute generates optimized C# code implementing regular expression logic instead of interpreting it at runtime.
*   **Interoperability (`[LibraryImport]`, .NET 7+)**: Generates marshalling code (data passing) for P/Invoke, replacing dynamically generated stubs in `[DllImport]`.
*   **Logging (`[LoggerMessage]`)**: Allows generating strongly-typed, high-performance logging methods that avoid costly template parsing and argument boxing at runtime.

### Interceptors

Generators cannot modify existing code, but the compiler offers a limited mechanism for redirecting calls - *interceptors*. A generated method marked with the `[InterceptsLocation]` attribute replaces, at compile time, a call at a specific location in the user's code (generators obtain the location using `SemanticModel.GetInterceptableLocation`). For example, ASP.NET Core minimal APIs (with Native AOT) and the configuration binding generator use them to replace reflection-based calls with generated code. Interceptors were introduced as an experimental feature in .NET 8 (C# 12); according to the [Roslyn documentation](https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md), they are supported as stable since the .NET 9.0.2xx SDK (Microsoft Learn still describes them as experimental). A project must opt in by listing the namespaces allowed to contain interceptors in the `<InterceptorsNamespaces>` property.
