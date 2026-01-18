---
title: "Generatory źródeł"
weight: 10
---

## Typy częściowe (`partial`)

Słowo kluczowe `partial` pozwala na podział definicji typu na wiele plików. Informuje to kompilator, że definicja typu może nie być kompletna i należy poszukać pozostałych części w innych plikach projektu. Podczas kompilacji wszystkie części definicji są scalane w jedną spójną całość. W czasie działania programu środowisko uruchomieniowe widzi już tylko jedną kompletną definicję typu.

Jednym z zastosowań jest organizacja kodu. Dla dużych definicji typów możemy zdecydować się rozdzielić na przykład inicjalizację i logikę do różnych plików.

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

Jednak słowo kluczowe `partial` pozwala nam przede wszystkim na oddzielenie kodu napisanego przez człowieka od kodu wygenerowanego automatycznie przez kompilator za pomocą generatorów. Najczęściej korzystają z tego systemy UI (np. WPF, WinForms), które na podstawie plików opisujących wygląd (np. `.xaml`) generują kod inicjalizujący komponenty. Poza tym można tego mechanizmu używać do generacji powtarzalnego kodu (np. `ToString()`), automatycznej implementacji interfejsów, generowaniu kodu sprawdzającego wyrażenie regularne lub serializującego typy na podstawie atrybutów.

### Zasady scalania

Podczas łączenia części typu, kompilator stosuje ścisłe reguły:
*   **Modyfikatory:** Wszystkie części muszą mieć zgodne modyfikatory dostępu. Jeśli w jednej części użyjesz `abstract` lub `sealed`, cała klasa taka się staje.
*   **Interfejsy i Atrybuty:** Są sumowane. Jeśli różne pliki implementują różne interfejsy, klasa wynikowa implementuje je wszystkie.
*   **Klasa bazowa:** Jeśli jest podana, musi być identyczna w każdej części (lub pominięta).

Warto pamiętać, że aby generator mógł rozszerzyć klasę napisaną przez programistę, **musi** ona również posiadać modyfikator `partial`.

### Metody częściowe (`partial`)

Metody częściowe pozwalają zadeklarować sygnaturę metody w jednym pliku, a implementację w innym. Działają one w dwóch trybach:

1.  **Opcjonalne:** Jeśli metoda zwraca `void`, jest prywatna i nie ma parametrów `out`, jej implementacja jest opcjonalna. Jeśli nie zostanie dostarczona, kompilator całkowicie usuwa wywołanie metody z kodu wynikowego.
2.  **Wymagane (C# 9.0+):** Jeśli metoda ma modyfikator dostępu (np. `public`) lub zwraca wartość, implementacja **musi** zostać dostarczona (zazwyczaj przez generator).

```csharp
public partial class User
{
    partial void OnLoaded(); // Optional

    [GeneratedRegex(".*@.*\\..*")]
    private partial Regex EmailRegex(); // Required
}
```

## Generatory źródeł (*Source Generators*)

Generatory źródeł to mechanizm kompilatora wprowadzony w C# 9.0 (znacznie ulepszony w nowszych wersjach jako *Incremental Generators*), który umożliwia inspekcję kodu użytkownika lub innych plików podczas kompilacji i generowanie na ich podstawie nowych plików źródłowych. Jest to forma metaprogramowania w czasie kompilacji.

W przeciwieństwie do refleksji, generatory źródeł integrują się bezpośrednio z procesem kompilacji. Generator najpierw analizuje istniejący kod i pliki. Na podstawie analizy generator tworzy tekstowy kod źródłowy C#. Wygenerowany kod jest dodawany do procesu kompilacji jako "wirtualne" pliki i kompilowany razem z resztą projektu.

Generatory mogą dodawać nowy kod, ale nie mogą modyfikować istniejącego. Dlatego tak istotna jest współpraca z typami częściowymi (`partial`), które pozwalają na rozszerzanie klas użytkownika o wygenerowane metody.

### Implementacja generatora

Generator jest biblioteką .NET Standard 2.0, która implementuje interfejs `IIncrementalGenerator`. Musi być oznaczona atrybutem `[Generator]`.

Poniżej znajduje się przykład bardziej zaawansowanego generatora, który implementuje wzorzec **Builder** dla dowolnej klasy oznaczonej atrybutem `[GenerateBuilder]`. Generator analizuje publiczne właściwości klasy i tworzy dla nich metody `With...`.

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
                .Where(p => !p.IsReadOnly && p.DeclaredAccessibility == Accessibility.Public && p.SetMethod != null)
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

            // Generate partial class
            code.AppendLine($"public partial class {model.ClassName}");
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

Aby użyć takiego generatora w projekcie aplikacji, musimy dodać go jako `Analyzer`. Dzięki temu możemy korzystać z wygenerowanych builderów do tworzenia obiektów, mimo że sami ich nie napisaliśmy.

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
            // Method CreateBuilder() and Builder class was generated:
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
> Kod źródłowy:
> {{< filetree dir="lectures/other/source-generators" >}}

### Zastosowania w platformie .NET

*   **`System.Text.Json`**: Generuje kod serializacji w czasie kompilacji, co pozwala na pracę bez użycia refleksji (istotne dla AOT - *Ahead-of-Time compilation*).
*   **`System.Text.RegularExpressions` (.NET 7+)**: Atrybut `[GeneratedRegex]` generuje zoptymalizowany kod C# realizujący logikę wyrażenia regularnego, zamiast interpretować je w czasie wykonywania.
*   **Współdziałanie (`[LibraryImport]`, .NET 7+)**: Generuje kod marshalingu (przekazywania danych) dla P/Invoke, zastępując generowane dynamicznie stuby w `[DllImport]`.
*   **Logowanie (`[LoggerMessage]`)**: Pozwala na generowanie silnie typowanych, wysokowydajnych metod logowania, które unikają kosztownego parsowania szablonów i boksowania argumentów w czasie działania programu.
