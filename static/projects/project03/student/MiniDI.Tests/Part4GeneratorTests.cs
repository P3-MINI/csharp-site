using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MiniDI.Generator;

namespace MiniDI.Tests;

/// <summary>Part 4: the incremental source generator (run in memory on small programs).</summary>
public class Part4GeneratorTests
{
    private const string ShopSource = """
        using System.Collections.Generic;
        using System.Linq;
        using MiniDI;

        namespace TestApp;

        public interface IClock { }

        [Service(Lifetime.Singleton, As = typeof(IClock))]
        public sealed class Clock : IClock { }

        [Service(Lifetime.Scoped)]
        public sealed class Session { }

        public interface IGreeter { string Name { get; } }

        // declared before AGreeter on purpose: registrations are ordered by class name
        [Service(As = typeof(IGreeter))]
        public sealed class BGreeter : IGreeter { public string Name => "B"; }

        [Service(As = typeof(IGreeter))]
        public sealed class AGreeter : IGreeter { public string Name => "A"; }

        [Service]
        public sealed class Handler(IClock clock, Session session, IEnumerable<IGreeter> greeters)
        {
            public IClock Clock { get; } = clock;
            public Session Session { get; } = session;
            public string Greeters { get; } = string.Join(",", greeters.Select(g => g.Name));
        }
        """;

    private static object Get(object instance, string property) =>
        instance.GetType().GetProperty(property)!.GetValue(instance)!;

    // ---------------------------------------------------------------- generated code

    [Fact]
    public void EmitsAddGenerated_EvenWithoutServices()
    {
        var result = GeneratorHarness.Run("namespace Empty; public class Nothing { }");

        Assert.Empty(result.CompilationErrors);
        var services = GeneratorResult.AddGenerated(result.Load(), new ServiceCollection());
        Assert.Empty(services);
    }

    [Fact]
    public void ValidServices_ProduceNoDiagnostics_AndCompile()
    {
        var result = GeneratorHarness.Run(ShopSource);

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Empty(result.CompilationErrors);
    }

    [Fact]
    public void NonGenericServices_AreRegisteredWithFactories()
    {
        var result = GeneratorHarness.Run(ShopSource);
        var assembly = result.Load();

        var services = GeneratorResult.AddGenerated(assembly, new ServiceCollection());

        Assert.Equal(
            [
                ("TestApp.IGreeter", Lifetime.Transient),
                ("TestApp.IGreeter", Lifetime.Transient),
                ("TestApp.IClock", Lifetime.Singleton),
                ("TestApp.Handler", Lifetime.Transient),
                ("TestApp.Session", Lifetime.Scoped),
            ],
            services.Select(d => (d.ServiceType.FullName, d.Lifetime)));
        Assert.All(services, d =>
        {
            Assert.NotNull(d.ImplementationFactory);
            Assert.Null(d.ImplementationType);
        });
    }

    [Fact]
    public void GeneratedFactories_CallConstructorsDirectly()
    {
        var result = GeneratorHarness.Run(ShopSource);

        Assert.Matches(new Regex(@"new\s+(global::)?TestApp\.Handler\s*\("), result.GeneratedCode);
        foreach (string reflection in new[] { "Activator", "GetConstructor", "MakeGenericType", ".Invoke(" })
            Assert.DoesNotContain(reflection, result.GeneratedCode);
    }

    [Fact]
    public void GeneratedRegistrations_ResolveTheWholeGraph()
    {
        var assembly = GeneratorHarness.Run(ShopSource).Load();
        using var provider = GeneratorResult.AddGenerated(assembly, new ServiceCollection()).BuildServiceProvider();
        using var scope = provider.CreateScope();

        var handler = scope.GetRequiredService(assembly.GetType("TestApp.Handler")!);

        Assert.Same(provider.GetRequiredService(assembly.GetType("TestApp.IClock")!), Get(handler, "Clock"));
        Assert.Same(scope.GetRequiredService(assembly.GetType("TestApp.Session")!), Get(handler, "Session"));
        Assert.Equal("A,B", Get(handler, "Greeters")); // AGreeter is registered before BGreeter
    }

    [Fact]
    public void InjectConstructor_AndOptionalParameters_AreRespected()
    {
        const string source = """
            using MiniDI;

            namespace TestApp;

            public interface IMissing { }

            [Service]
            public sealed class Options
            {
                public Options() => Used = "()";

                [Inject]
                public Options(int retries = 3, string name = "default", IMissing? missing = null, System.DayOfWeek day = System.DayOfWeek.Friday)
                {
                    Used = $"{retries} {name} {missing is null} {day}";
                }

                public string Used { get; }
            }
            """;
        var result = GeneratorHarness.Run(source);
        Assert.Empty(result.GeneratorDiagnostics);
        var assembly = result.Load();
        Type options = assembly.GetType("TestApp.Options")!;

        using var defaults = GeneratorResult.AddGenerated(assembly, new ServiceCollection()).BuildServiceProvider();
        using var registered = GeneratorResult.AddGenerated(assembly, new ServiceCollection())
            .AddSingleton(typeof(string), "custom")
            .BuildServiceProvider();

        Assert.Equal("3 default True Friday", Get(defaults.GetRequiredService(options), "Used"));
        Assert.Equal("3 custom True Friday", Get(registered.GetRequiredService(options), "Used"));
    }

    [Fact]
    public void SeveralServiceAttributes_ProduceSeveralRegistrations()
    {
        const string source = """
            using MiniDI;

            namespace TestApp;

            public interface IFirst { }
            public interface ISecond { }

            [Service(Lifetime.Singleton, As = typeof(IFirst))]
            [Service(Lifetime.Scoped, As = typeof(ISecond))]
            public sealed class Both : IFirst, ISecond { }
            """;

        var services = GeneratorResult.AddGenerated(GeneratorHarness.Run(source).Load(), new ServiceCollection());

        Assert.Equal(
            [("TestApp.IFirst", Lifetime.Singleton), ("TestApp.ISecond", Lifetime.Scoped)],
            services.Select(d => (d.ServiceType.FullName, d.Lifetime)));
    }

    [Fact]
    public void OpenGenericServices_UseTypeBasedRegistration()
    {
        const string source = """
            using MiniDI;

            namespace TestApp;

            public interface IRepository<T> { }

            [Service(Lifetime.Singleton, As = typeof(IRepository<>))]
            public sealed class Repository<T> : IRepository<T> { }
            """;
        var result = GeneratorHarness.Run(source);
        Assert.Empty(result.GeneratorDiagnostics);
        var assembly = result.Load();

        var services = GeneratorResult.AddGenerated(assembly, new ServiceCollection());
        var descriptor = Assert.Single(services);
        using var provider = services.BuildServiceProvider();

        Assert.Equal(assembly.GetType("TestApp.IRepository`1"), descriptor.ServiceType);
        Assert.Equal(assembly.GetType("TestApp.Repository`1"), descriptor.ImplementationType);
        var closed = assembly.GetType("TestApp.IRepository`1")!.MakeGenericType(typeof(int));
        Assert.Same(provider.GetRequiredService(closed), provider.GetRequiredService(closed));
    }

    [Fact]
    public void SeveralConstructorsWithoutInject_ReportWarning_AndFallBackToReflection()
    {
        const string source = """
            using MiniDI;

            namespace TestApp;

            [Service]
            public sealed class TwoConstructors
            {
                public TwoConstructors() { }
                public TwoConstructors(string text) { }
            }
            """;
        var result = GeneratorHarness.Run(source);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("MDI005", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("TwoConstructors", result.TextAt(diagnostic));
        Assert.Empty(result.CompilationErrors);
        var descriptor = Assert.Single(GeneratorResult.AddGenerated(result.Load(), new ServiceCollection()));
        Assert.Equal("TestApp.TwoConstructors", descriptor.ImplementationType?.FullName);
    }

    // ---------------------------------------------------------------- diagnostics

    public static TheoryData<string, string, string> InvalidServices => new()
    {
        {
            "MDI001", "NoConstructor",
            "[Service] public sealed class NoConstructor { private NoConstructor() { } }"
        },
        {
            "MDI002", "TwoInject",
            """
            [Service] public sealed class TwoInject
            {
                [Inject] public TwoInject() { }
                [Inject] public TwoInject(string text) { }
            }
            """
        },
        {
            "MDI003", "NotDisposable",
            "[Service(As = typeof(System.IDisposable))] public sealed class NotDisposable { }"
        },
        {
            "MDI004", "AbstractService",
            "[Service] public abstract class AbstractService { }"
        },
        {
            "MDI004", "Hidden",
            "public static class Outer { [Service] private sealed class Hidden { } }"
        },
    };

    [Theory]
    [MemberData(nameof(InvalidServices))]
    public void InvalidServices_ReportErrorsAtClassName(string id, string className, string declaration)
    {
        var result = GeneratorHarness.Run($"using MiniDI;\nnamespace TestApp;\n{declaration}\n");

        var diagnostic = Assert.Single(result.GeneratorDiagnostics, d => d.Id == id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(className, result.TextAt(diagnostic));
        Assert.Contains(className, diagnostic.GetMessage());
    }

    // ---------------------------------------------------------------- incrementality

    [Fact]
    public void Models_AreEquatable_SoUnrelatedEditsAreCached()
    {
        var compilation = GeneratorHarness.CreateCompilation(ShopSource);
        var driver = GeneratorHarness.CreateDriver().RunGenerators(compilation);

        // Edit a [Service] class without changing anything the generator cares about.
        var oldTree = compilation.SyntaxTrees[0];
        string edited = ShopSource.Replace(
            "public sealed class Session { }",
            "public sealed class Session { public override string ToString() => \"session\"; }");
        var newTree = CSharpSyntaxTree.ParseText(edited, (CSharpParseOptions)oldTree.Options, oldTree.FilePath);
        driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(oldTree, newTree));

        var run = Assert.Single(driver.GetRunResult().Results);
        Assert.True(run.TrackedSteps.ContainsKey(ServiceRegistrationGenerator.ModelsTrackingName),
            $"The pipeline step '{ServiceRegistrationGenerator.ModelsTrackingName}' is missing (keep WithTrackingName).");
        var outputs = run.TrackedSteps[ServiceRegistrationGenerator.ModelsTrackingName].SelectMany(step => step.Outputs).ToList();
        Assert.NotEmpty(outputs);
        Assert.All(outputs, output =>
            Assert.True(output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                $"Model {output.Value} was {output.Reason}: models must not contain symbols, syntax nodes or Locations."));
    }
}
