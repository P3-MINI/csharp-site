using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MiniDI.Generator;

namespace MiniDI.Tests;

/// <summary>Compiles C# source in memory, runs <see cref="ServiceRegistrationGenerator"/> on it and loads the result.</summary>
internal static class GeneratorHarness
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.CSharp12);

    private static readonly Lazy<MetadataReference[]> References = new(() =>
    {
        string runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return Directory.GetFiles(runtimeDirectory, "*.dll")
            .Where(IsManagedAssembly)
            .Append(typeof(ServiceCollection).Assembly.Location) // MiniDI.dll
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToArray();
    });

    public static CSharpCompilation CreateCompilation(string source) =>
        CSharpCompilation.Create(
            assemblyName: $"GeneratorTest_{Guid.NewGuid():N}",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, ParseOptions, path: "Services.cs")],
            references: References.Value,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

    public static GeneratorDriver CreateDriver() =>
        CSharpGeneratorDriver.Create(
            generators: [new ServiceRegistrationGenerator().AsSourceGenerator()],
            parseOptions: ParseOptions,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

    public static GeneratorResult Run(string source)
    {
        var compilation = CreateCompilation(source);
        CreateDriver().RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        string generated = string.Join(
            Environment.NewLine,
            output.SyntaxTrees.Where(t => !ReferenceEquals(t, compilation.SyntaxTrees[0])).Select(t => t.ToString()));
        return new GeneratorResult(source, output, diagnostics, generated);
    }

    private static bool IsManagedAssembly(string path)
    {
        try
        {
            AssemblyName.GetAssemblyName(path);
            return true;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
    }
}

internal sealed record GeneratorResult(
    string Source,
    Compilation Output,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    string GeneratedCode)
{
    public IEnumerable<Diagnostic> CompilationErrors =>
        Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>Emits the compilation (source + generated code) and loads it.</summary>
    public Assembly Load()
    {
        using var stream = new MemoryStream();
        var emit = Output.Emit(stream);
        Assert.True(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics));
        return Assembly.Load(stream.ToArray());
    }

    /// <summary>Calls the generated <c>MiniDI.Generated.GeneratedServices.AddGenerated(services)</c>.</summary>
    public static ServiceCollection AddGenerated(Assembly assembly, ServiceCollection services)
    {
        Type type = assembly.GetType("MiniDI.Generated.GeneratedServices")
            ?? throw new InvalidOperationException("Type MiniDI.Generated.GeneratedServices was not generated.");
        MethodInfo method = type.GetMethod("AddGenerated", BindingFlags.Public | BindingFlags.Static, [typeof(ServiceCollection)])
            ?? throw new InvalidOperationException("Method AddGenerated(ServiceCollection) was not generated.");
        return (ServiceCollection)method.Invoke(null, [services])!;
    }

    /// <summary>The text of the source code the diagnostic points to.</summary>
    public string TextAt(Diagnostic diagnostic) =>
        Source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length);
}
