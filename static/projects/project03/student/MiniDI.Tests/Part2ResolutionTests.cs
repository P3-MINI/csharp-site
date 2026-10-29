namespace MiniDI.Tests;

/// <summary>Part 2: constructor injection, IEnumerable, optional parameters, cycles and captive dependencies.</summary>
public class Part2ResolutionTests
{
    public interface IA { }
    public interface IB { }
    public interface IMissing { }
    public sealed class A : IA { }
    public sealed class B : IB { }

    public sealed class Clock { }
    public sealed class Repository(Clock clock)
    {
        public Clock Clock { get; } = clock;
    }
    public sealed class OrderService(Repository repository, Clock clock)
    {
        public Repository Repository { get; } = repository;
        public Clock Clock { get; } = clock;
    }

    public sealed class Multi
    {
        public Multi() => Used = "()";
        public Multi(IA a) => Used = "(IA)";
        public Multi(IA a, IB b) => Used = "(IA, IB)";
        public Multi(IA a, IB b, IMissing m) => Used = "(IA, IB, IMissing)";
        public string Used { get; }
    }

    public sealed class Marked
    {
        public Marked(IA a, IB b) => Used = "(IA, IB)";
        [Inject] public Marked(IA a) => Used = "(IA)";
        public string Used { get; }
    }

    public sealed class TwoMarked
    {
        [Inject] public TwoMarked(IA a) { }
        [Inject] public TwoMarked(IB b) { }
    }

    public sealed class Tie
    {
        public Tie(IA a) { }
        public Tie(IB b) { }
    }

    public sealed class NeedsMissing(IMissing missing) { }
    public sealed class NeedsString(string text) { }
    public sealed class Outer(NeedsMissing inner) { }

    public sealed class InternalConstructor
    {
        internal InternalConstructor() { }
    }

    public sealed class WithDefaults(IA a, int retries = 3, string name = "default", IB? b = null)
    {
        public IA A { get; } = a;
        public int Retries { get; } = retries;
        public string Name { get; } = name;
        public IB? B { get; } = b;
    }

    public interface IPlugin { }
    public sealed class Plugin1 : IPlugin { }
    public sealed class Plugin2 : IPlugin { }
    public sealed class Plugin3 : IPlugin { }
    public sealed class PluginHost(IEnumerable<IPlugin> plugins)
    {
        public IEnumerable<IPlugin> Plugins { get; } = plugins;
    }

    public interface IRepository<T> { }
    public sealed class GenericRepository<T>(Clock clock) : IRepository<T>
    {
        public Clock Clock { get; } = clock;
    }
    public sealed class IntRepository : IRepository<int> { }

    public sealed class CycleA(CycleB b) { }
    public sealed class CycleB(CycleA a) { }
    public sealed class Self(Self self) { }
    public interface IX { }
    public interface IY { }
    public interface IZ { }
    public sealed class X(IY y) : IX { }
    public sealed class Y(IZ z) : IY { }
    public sealed class Z(IX x) : IZ { }
    public sealed class FactoryA(FactoryB b) { }
    public sealed class FactoryB(FactoryA a) { }

    public sealed class ScopedDependency { }
    public sealed class SingletonConsumer(ScopedDependency dependency) { }
    public sealed class TransientMiddle(ScopedDependency dependency) { }
    public sealed class SingletonOverTransient(TransientMiddle middle) { }
    public sealed class ScopedConsumer(ScopedDependency dependency, Clock clock)
    {
        public ScopedDependency Dependency { get; } = dependency;
        public Clock Clock { get; } = clock;
    }

    public sealed class Throwing
    {
        public Throwing() => throw new InvalidOperationException("boom");
    }

    public sealed class SlowSingleton
    {
        public static int Instances;

        public SlowSingleton(Clock clock)
        {
            Interlocked.Increment(ref Instances);
            Thread.Sleep(50);
        }
    }

    public sealed class Disposable : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }
    public sealed class SingletonWithDisposable(Disposable dependency)
    {
        public Disposable Dependency { get; } = dependency;
    }

    // ---------------------------------------------------------------- constructor injection

    [Fact]
    public void ConstructorInjection_ResolvesDependenciesRecursively()
    {
        using var provider = new ServiceCollection()
            .AddSingleton<Clock>()
            .AddTransient<Repository>()
            .AddTransient<OrderService>()
            .BuildServiceProvider();

        var service = provider.GetRequiredService<OrderService>();

        Assert.Same(service.Clock, service.Repository.Clock);
        Assert.Same(provider.GetRequiredService<Clock>(), service.Clock);
    }

    [Fact]
    public void ChoosesConstructorWithMostResolvableParameters()
    {
        using var onlyA = new ServiceCollection().AddTransient<IA, A>().AddTransient<Multi>().BuildServiceProvider();
        using var aAndB = new ServiceCollection().AddTransient<IA, A>().AddTransient<IB, B>().AddTransient<Multi>().BuildServiceProvider();
        using var none = new ServiceCollection().AddTransient<Multi>().BuildServiceProvider();

        Assert.Equal("(IA)", onlyA.GetRequiredService<Multi>().Used);
        Assert.Equal("(IA, IB)", aAndB.GetRequiredService<Multi>().Used);
        Assert.Equal("()", none.GetRequiredService<Multi>().Used);
    }

    [Fact]
    public void InjectAttribute_SelectsConstructor()
    {
        using var provider = new ServiceCollection()
            .AddTransient<IA, A>().AddTransient<IB, B>().AddTransient<Marked>()
            .BuildServiceProvider();

        Assert.Equal("(IA)", provider.GetRequiredService<Marked>().Used);
    }

    [Fact]
    public void SeveralInjectAttributes_AreAmbiguous()
    {
        using var provider = new ServiceCollection()
            .AddTransient<IA, A>().AddTransient<IB, B>().AddTransient<TwoMarked>()
            .BuildServiceProvider();

        var ex = Assert.Throws<AmbiguousConstructorException>(() => provider.GetService(typeof(TwoMarked)));

        Assert.Equal(typeof(TwoMarked), ex.ImplementationType);
        Assert.Equal(2, ex.Candidates.Count);
    }

    [Fact]
    public void TwoLongestSatisfiableConstructors_AreAmbiguous()
    {
        using var provider = new ServiceCollection()
            .AddTransient<IA, A>().AddTransient<IB, B>().AddTransient<Tie>()
            .BuildServiceProvider();

        var ex = Assert.Throws<AmbiguousConstructorException>(() => provider.GetService(typeof(Tie)));

        Assert.Equal(typeof(Tie), ex.ImplementationType);
        Assert.Equal([typeof(Tie)], ex.ResolutionPath);
        Assert.Equal(2, ex.Candidates.Count);
    }

    [Fact]
    public void NoSatisfiableConstructor_Throws()
    {
        using var provider = new ServiceCollection()
            .AddTransient<NeedsMissing>()
            .AddTransient<NeedsString>()
            .AddTransient<InternalConstructor>()
            .AddTransient<Outer>()
            .BuildServiceProvider();

        var direct = Assert.Throws<NoSuitableConstructorException>(() => provider.GetService(typeof(NeedsMissing)));
        Assert.Equal(typeof(NeedsMissing), direct.ImplementationType);
        Assert.Equal([typeof(NeedsMissing)], direct.ResolutionPath);

        var nested = Assert.Throws<NoSuitableConstructorException>(() => provider.GetService(typeof(Outer)));
        Assert.Equal(typeof(NeedsMissing), nested.ImplementationType);
        Assert.Equal([typeof(Outer), typeof(NeedsMissing)], nested.ResolutionPath);

        Assert.Throws<NoSuitableConstructorException>(() => provider.GetService(typeof(NeedsString)));
        Assert.Throws<NoSuitableConstructorException>(() => provider.GetService(typeof(InternalConstructor)));
    }

    [Fact]
    public void OptionalParameters_UseDefaultValues_WhenNotRegistered()
    {
        using var provider = new ServiceCollection().AddTransient<IA, A>().AddTransient<WithDefaults>().BuildServiceProvider();

        var service = provider.GetRequiredService<WithDefaults>();

        Assert.IsType<A>(service.A);
        Assert.Equal(3, service.Retries);
        Assert.Equal("default", service.Name);
        Assert.Null(service.B);
    }

    [Fact]
    public void OptionalParameters_AreResolved_WhenRegistered()
    {
        using var provider = new ServiceCollection()
            .AddTransient<IA, A>().AddTransient<IB, B>().AddSingleton(typeof(string), "custom")
            .AddTransient<WithDefaults>()
            .BuildServiceProvider();

        var service = provider.GetRequiredService<WithDefaults>();

        Assert.IsType<B>(service.B);
        Assert.Equal("custom", service.Name);
    }

    [Fact]
    public void ConstructorExceptions_AreNotWrapped()
    {
        using var provider = new ServiceCollection().AddTransient<Throwing>().BuildServiceProvider();

        var ex = Assert.Throws<InvalidOperationException>(() => provider.GetService(typeof(Throwing)));

        Assert.Equal("boom", ex.Message);
    }

    // ---------------------------------------------------------------- IEnumerable<T>

    [Fact]
    public void Enumerable_ReturnsAllRegistrations_InRegistrationOrder()
    {
        using var provider = new ServiceCollection()
            .AddTransient<IPlugin, Plugin1>()
            .AddSingleton<IPlugin, Plugin2>()
            .AddTransient<IPlugin, Plugin3>()
            .BuildServiceProvider();

        var plugins = provider.GetRequiredService<IEnumerable<IPlugin>>();

        Assert.IsType<IPlugin[]>(plugins);
        Assert.Equal([typeof(Plugin1), typeof(Plugin2), typeof(Plugin3)], plugins.Select(p => p.GetType()));
        Assert.Same(plugins.ElementAt(1), provider.GetRequiredService<IEnumerable<IPlugin>>().ElementAt(1));
        Assert.IsType<Plugin3>(provider.GetRequiredService<IPlugin>());
    }

    [Fact]
    public void Enumerable_IsEmpty_WhenNothingIsRegistered()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        var plugins = provider.GetService(typeof(IEnumerable<IPlugin>));

        Assert.Empty(Assert.IsType<IPlugin[]>(plugins));
    }

    [Fact]
    public void Enumerable_CanBeInjected()
    {
        using var provider = new ServiceCollection()
            .AddTransient<IPlugin, Plugin1>()
            .AddTransient<IPlugin, Plugin2>()
            .AddTransient<PluginHost>()
            .BuildServiceProvider();

        var host = provider.GetRequiredService<PluginHost>();

        Assert.Equal([typeof(Plugin1), typeof(Plugin2)], host.Plugins.Select(p => p.GetType()));
    }

    [Fact]
    public void Enumerable_IncludesOpenGenericRegistrations()
    {
        using var provider = new ServiceCollection()
            .AddSingleton<Clock>()
            .AddTransient(typeof(IRepository<>), typeof(GenericRepository<>))
            .AddTransient<IRepository<int>, IntRepository>()
            .BuildServiceProvider();

        var repositories = provider.GetRequiredService<IEnumerable<IRepository<int>>>();

        Assert.Equal([typeof(GenericRepository<int>), typeof(IntRepository)], repositories.Select(r => r.GetType()));
        Assert.IsType<IntRepository>(provider.GetRequiredService<IRepository<int>>());
        Assert.Same(provider.GetRequiredService<Clock>(),
            ((GenericRepository<string>)provider.GetRequiredService<IRepository<string>>()).Clock);
    }

    // ---------------------------------------------------------------- cycles

    [Fact]
    public void CircularDependency_IsDetected()
    {
        using var provider = new ServiceCollection().AddTransient<CycleA>().AddTransient<CycleB>().BuildServiceProvider();

        var ex = Assert.Throws<CircularDependencyException>(() => provider.GetService(typeof(CycleA)));

        Assert.Equal([typeof(CycleA), typeof(CycleB), typeof(CycleA)], ex.ResolutionPath);
        Assert.Contains("Resolution path: CycleA -> CycleB -> CycleA", ex.Message);
    }

    [Fact]
    public void CircularDependency_SelfReference()
    {
        using var provider = new ServiceCollection().AddSingleton<Self>().BuildServiceProvider();

        var ex = Assert.Throws<CircularDependencyException>(() => provider.GetService(typeof(Self)));

        Assert.Equal([typeof(Self), typeof(Self)], ex.ResolutionPath);
    }

    [Fact]
    public void CircularDependency_ThroughInterfaces()
    {
        using var provider = new ServiceCollection()
            .AddScoped<IX, X>().AddTransient<IY, Y>().AddScoped<IZ, Z>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();

        var ex = Assert.Throws<CircularDependencyException>(() => scope.GetService(typeof(IY)));

        Assert.Equal([typeof(IY), typeof(IZ), typeof(IX), typeof(IY)], ex.ResolutionPath);
    }

    [Fact]
    public void CircularDependency_ThroughFactory()
    {
        using var provider = new ServiceCollection()
            .AddTransient(sp => new FactoryA(sp.GetRequiredService<FactoryB>()))
            .AddTransient<FactoryB>()
            .BuildServiceProvider();

        var ex = Assert.Throws<CircularDependencyException>(() => provider.GetService(typeof(FactoryA)));

        Assert.Equal([typeof(FactoryA), typeof(FactoryB), typeof(FactoryA)], ex.ResolutionPath);
    }

    // ---------------------------------------------------------------- captive dependencies

    [Fact]
    public void SingletonDependingOnScoped_IsCaptive()
    {
        using var provider = new ServiceCollection()
            .AddScoped<ScopedDependency>().AddSingleton<SingletonConsumer>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();

        var ex = Assert.Throws<CaptiveDependencyException>(() => scope.GetService(typeof(SingletonConsumer)));

        Assert.Equal(typeof(SingletonConsumer), ex.ConsumerType);
        Assert.Equal([typeof(SingletonConsumer), typeof(ScopedDependency)], ex.ResolutionPath);
        Assert.StartsWith("Cannot consume scoped service 'ScopedDependency' from singleton 'SingletonConsumer'.", ex.Message);
    }

    [Fact]
    public void SingletonDependingOnScopedThroughTransient_IsCaptive()
    {
        using var provider = new ServiceCollection()
            .AddScoped<ScopedDependency>().AddTransient<TransientMiddle>().AddSingleton<SingletonOverTransient>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();

        var ex = Assert.Throws<CaptiveDependencyException>(() => scope.GetService(typeof(SingletonOverTransient)));

        Assert.Equal(typeof(SingletonOverTransient), ex.ConsumerType);
        Assert.Equal([typeof(SingletonOverTransient), typeof(TransientMiddle), typeof(ScopedDependency)], ex.ResolutionPath);
    }

    [Fact]
    public void TransientDependingOnScoped_ResolvedFromRoot_Throws()
    {
        using var provider = new ServiceCollection()
            .AddScoped<ScopedDependency>().AddTransient<TransientMiddle>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.GetService(typeof(TransientMiddle)));
        var ex = Assert.Throws<CaptiveDependencyException>(() => provider.GetService(typeof(TransientMiddle)));

        Assert.Null(ex.ConsumerType);
        Assert.Equal([typeof(TransientMiddle), typeof(ScopedDependency)], ex.ResolutionPath);
    }

    [Fact]
    public void ScopedDependencies_AreSharedWithinScope()
    {
        using var provider = new ServiceCollection()
            .AddSingleton<Clock>().AddScoped<ScopedDependency>().AddScoped<ScopedConsumer>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();

        var consumer = scope.GetRequiredService<ScopedConsumer>();

        Assert.Same(scope.GetRequiredService<ScopedDependency>(), consumer.Dependency);
        Assert.Same(provider.GetRequiredService<Clock>(), consumer.Clock);
    }

    [Fact]
    public void DependenciesOfSingletons_AreResolvedFromRoot()
    {
        var provider = new ServiceCollection()
            .AddTransient<Disposable>().AddSingleton<SingletonWithDisposable>()
            .BuildServiceProvider();

        SingletonWithDisposable singleton;
        using (var scope = provider.CreateScope())
            singleton = scope.GetRequiredService<SingletonWithDisposable>();
        Assert.False(singleton.Dependency.Disposed);

        provider.Dispose();
        Assert.True(singleton.Dependency.Disposed);
    }

    // ---------------------------------------------------------------- thread safety

    [Fact]
    public void SingletonCreatedByConstructor_IsCreatedOnce_UnderConcurrency()
    {
        using var provider = new ServiceCollection().AddSingleton<Clock>().AddSingleton<SlowSingleton>().BuildServiceProvider();

        const int threadCount = 16;
        var results = new object?[threadCount];
        using var start = new ManualResetEventSlim();
        var threads = Enumerable.Range(0, threadCount)
            .Select(i => new Thread(() =>
            {
                start.Wait();
                results[i] = provider.GetService(typeof(SlowSingleton));
            }))
            .ToList();
        threads.ForEach(t => t.Start());
        start.Set();
        threads.ForEach(t => t.Join());

        Assert.Equal(1, SlowSingleton.Instances);
        Assert.All(results, r => Assert.Same(results[0], r));
    }
}
