using MiniDI.Tests.Scanning.Valid;

namespace MiniDI.Tests
{
    /// <summary>Part 3: [Service] attributes, assembly scanning and validation on build.</summary>
    public class Part3ScanningTests
    {
        private static ServiceCollection ScanNamespace(string ns) =>
            new ServiceCollection().AddFromAssembly(typeof(Part3ScanningTests).Assembly, t => t.Namespace == ns);

        // ------------------------------------------------------------ scanning

        [Fact]
        public void AddFromAssembly_RegistersEveryServiceAttribute()
        {
            var services = ScanNamespace("MiniDI.Tests.Scanning.Valid");

            var registrations = services
                .Select(d => (d.ServiceType, d.ImplementationType, d.Lifetime))
                .ToHashSet();
            Assert.Equal(
                new HashSet<(Type, Type?, Lifetime)>
                {
                    (typeof(BaseService), typeof(BaseService), Lifetime.Transient),
                    (typeof(IFirst), typeof(Both), Lifetime.Singleton),
                    (typeof(ISecond), typeof(Both), Lifetime.Singleton),
                    (typeof(IGreeter), typeof(EnglishGreeter), Lifetime.Singleton),
                    (typeof(GreetingService), typeof(GreetingService), Lifetime.Transient),
                    (typeof(IGreeter), typeof(PolishGreeter), Lifetime.Transient),
                    (typeof(Session), typeof(Session), Lifetime.Scoped),
                    (typeof(IStore<>), typeof(Store<>), Lifetime.Singleton),
                },
                registrations);
            Assert.Equal(8, services.Count); // DerivedService and NotAService are not registered
        }

        [Fact]
        public void AddFromAssembly_RegistersClassesInOrdinalOrderOfFullName()
        {
            var services = ScanNamespace("MiniDI.Tests.Scanning.Valid");

            var implementations = services.Select(d => d.ImplementationType!).Distinct().ToList();

            Assert.Equal(implementations.OrderBy(t => t.FullName, StringComparer.Ordinal), implementations);
        }

        [Fact]
        public void AddFromAssembly_IsFluent()
        {
            var services = new ServiceCollection();

            Assert.Same(services, services.AddFromAssembly(typeof(Part3ScanningTests).Assembly, _ => false));
            Assert.Empty(services);
        }

        [Fact]
        public void ScannedServices_CanBeResolved()
        {
            using var provider = ScanNamespace("MiniDI.Tests.Scanning.Valid").BuildServiceProvider(validateOnBuild: true);
            using var scope = provider.CreateScope();

            var greeting = scope.GetRequiredService<GreetingService>();

            Assert.Equal("Hello, Dzień dobry", greeting.GreetAll());
            Assert.Same(scope.GetRequiredService<Session>(), greeting.Session);
            Assert.Same(scope.GetRequiredService<IStore<int>>(), provider.GetRequiredService<IStore<int>>());
            Assert.IsType<Both>(provider.GetRequiredService<IFirst>());
        }

        [Fact]
        public void AddFromAssembly_RejectsAbstractService()
        {
            Assert.Throws<ArgumentException>(() => ScanNamespace("MiniDI.Tests.Scanning.Invalid.Abstract"));
        }

        [Fact]
        public void AddFromAssembly_RejectsServiceNotImplementingAs()
        {
            Assert.Throws<ArgumentException>(() => ScanNamespace("MiniDI.Tests.Scanning.Invalid.WrongAs"));
        }

        // ------------------------------------------------------------ validation on build

        public sealed class Counted
        {
            public static int Instances;

            public Counted(Dependency dependency) => Interlocked.Increment(ref Instances);
        }

        public sealed class Dependency { }
        public interface IMissing { }
        public sealed class NeedsMissing(IMissing missing) { }
        public sealed class Fine(Dependency dependency) { }
        public sealed class ScopedDependency { }
        public sealed class CaptiveSingleton(ScopedDependency dependency) { }
        public sealed class CycleA(CycleB b) { }
        public sealed class CycleB(CycleA a) { }
        public sealed class UsesEnumerable(IEnumerable<NeedsMissing> items) { }
        public interface IGeneric<T> { }
        public sealed class BrokenGeneric<T>(IMissing missing) : IGeneric<T> { }

        [Fact]
        public void ValidateOnBuild_ValidGraph_CreatesNothing()
        {
            int factoryCalls = 0;
            var services = new ServiceCollection()
                .AddSingleton<Dependency>()
                .AddTransient<Counted>()
                .AddScoped(_ => { factoryCalls++; return new ScopedDependency(); });

            using var provider = services.BuildServiceProvider(validateOnBuild: true);

            Assert.Equal(0, Counted.Instances);
            Assert.Equal(0, factoryCalls);
        }

        [Fact]
        public void ValidateOnBuild_ReportsOneErrorPerInvalidRegistration_InRegistrationOrder()
        {
            var services = new ServiceCollection()
                .AddSingleton<Dependency>()
                .AddTransient<NeedsMissing>()
                .AddTransient<Fine>()
                .AddScoped<ScopedDependency>()
                .AddSingleton<CaptiveSingleton>()
                .AddTransient<CycleA>()
                .AddTransient<CycleB>();

            var ex = Assert.Throws<ContainerValidationException>(() => services.BuildServiceProvider(validateOnBuild: true));

            Assert.Collection(ex.Errors,
                e => Assert.Equal(typeof(NeedsMissing), Assert.IsType<NoSuitableConstructorException>(e).ImplementationType),
                e => Assert.Equal([typeof(CaptiveSingleton), typeof(ScopedDependency)], Assert.IsType<CaptiveDependencyException>(e).ResolutionPath),
                e => Assert.Equal([typeof(CycleA), typeof(CycleB), typeof(CycleA)], Assert.IsType<CircularDependencyException>(e).ResolutionPath),
                e => Assert.Equal([typeof(CycleB), typeof(CycleA), typeof(CycleB)], Assert.IsType<CircularDependencyException>(e).ResolutionPath));
            Assert.Equal(ex.Errors, ex.InnerExceptions);
        }

        [Fact]
        public void ValidateOnBuild_FollowsEnumerableDependencies()
        {
            var services = new ServiceCollection()
                .AddTransient<UsesEnumerable>()
                .AddTransient(_ => new NeedsMissing(null!))
                .AddTransient<NeedsMissing>();

            var ex = Assert.Throws<ContainerValidationException>(() => services.BuildServiceProvider(validateOnBuild: true));

            Assert.Equal(2, ex.Errors.Count);
            Assert.Equal(
                [typeof(UsesEnumerable), typeof(IEnumerable<NeedsMissing>), typeof(NeedsMissing)],
                Assert.IsType<NoSuitableConstructorException>(ex.Errors[0]).ResolutionPath);
            Assert.Equal([typeof(NeedsMissing)], ex.Errors[1].ResolutionPath);
        }

        [Fact]
        public void ValidateOnBuild_ChecksLifetimesOfFactoryDependencies()
        {
            var services = new ServiceCollection()
                .AddScoped(_ => new ScopedDependency())
                .AddSingleton<CaptiveSingleton>();

            var ex = Assert.Throws<ContainerValidationException>(() => services.BuildServiceProvider(validateOnBuild: true));

            Assert.IsType<CaptiveDependencyException>(Assert.Single(ex.Errors));
        }

        [Fact]
        public void ValidateOnBuild_SkipsFactoriesAndOpenGenerics()
        {
            var services = new ServiceCollection()
                .AddTransient<NeedsMissing>(_ => throw new InvalidOperationException("must not be called"))
                .AddTransient(typeof(IGeneric<>), typeof(BrokenGeneric<>));

            using var provider = services.BuildServiceProvider(validateOnBuild: true);

            Assert.Throws<NoSuitableConstructorException>(() => provider.GetService(typeof(IGeneric<int>)));
        }

        [Fact]
        public void WithoutValidation_ErrorsAppearOnResolution()
        {
            var services = new ServiceCollection().AddTransient<NeedsMissing>();

            using var provider = services.BuildServiceProvider();

            Assert.Throws<NoSuitableConstructorException>(() => provider.GetService(typeof(NeedsMissing)));
        }
    }
}

namespace MiniDI.Tests.Scanning.Valid
{
    public interface IGreeter
    {
        string Greet();
    }

    [Service(Lifetime.Singleton, As = typeof(IGreeter))]
    public sealed class EnglishGreeter : IGreeter
    {
        public string Greet() => "Hello";
    }

    [Service(As = typeof(IGreeter))]
    public sealed class PolishGreeter : IGreeter
    {
        public string Greet() => "Dzień dobry";
    }

    [Service(Lifetime.Scoped)]
    public sealed class Session;

    [Service]
    public sealed class GreetingService(IEnumerable<IGreeter> greeters, Session session)
    {
        public Session Session { get; } = session;

        public string GreetAll() => string.Join(", ", greeters.Select(g => g.Greet()));
    }

    public interface IStore<T>;

    [Service(Lifetime.Singleton, As = typeof(IStore<>))]
    public sealed class Store<T> : IStore<T>;

    public interface IFirst;

    public interface ISecond;

    [Service(Lifetime.Singleton, As = typeof(IFirst))]
    [Service(Lifetime.Singleton, As = typeof(ISecond))]
    public sealed class Both : IFirst, ISecond;

    [Service]
    public class BaseService;

    // [Service] is not inherited: this class must not be registered.
    public sealed class DerivedService : BaseService;

    public sealed class NotAService;
}

namespace MiniDI.Tests.Scanning.Invalid.Abstract
{
    [Service]
    public abstract class AbstractService;
}

namespace MiniDI.Tests.Scanning.Invalid.WrongAs
{
    [Service(As = typeof(IDisposable))]
    public sealed class NotDisposable;
}
