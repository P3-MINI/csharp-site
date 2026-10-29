namespace MiniDI.Tests;

/// <summary>Part 1: registration API, lifetimes, scopes, open generics and disposal.</summary>
public class Part1RegistrationTests
{
    public interface IFoo { }
    public sealed class Foo : IFoo { }
    public sealed class OtherFoo : IFoo { }
    public sealed class Unrelated { }
    public abstract class AbstractFoo : IFoo { }

    public interface IRepository<T> { }
    public sealed class Repository<T> : IRepository<T> { }
    public sealed class IntRepository : IRepository<int> { }
    public sealed class ClassOnlyRepository<T> : IRepository<T> where T : class { }
    public sealed class TwoParameterRepository<TKey, TValue> : IRepository<TKey> { }

    public sealed class Log
    {
        public List<string> Entries { get; } = [];
    }

    public interface IFirst { }
    public interface ISecond { }
    public interface IThird { }

    public sealed class Tracked(string name, Log log) : IDisposable, IFirst, ISecond, IThird
    {
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
            log.Entries.Add(name);
        }
    }

    public sealed class AsyncOnly : IAsyncDisposable
    {
        public bool Disposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class SyncAndAsync : IDisposable, IAsyncDisposable
    {
        public bool DisposedSync { get; private set; }
        public bool DisposedAsync { get; private set; }

        public void Dispose() => DisposedSync = true;

        public ValueTask DisposeAsync()
        {
            DisposedAsync = true;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class Holder(Tracked dependency)
    {
        public Tracked Dependency { get; } = dependency;
    }

    // ---------------------------------------------------------------- registration API

    [Fact]
    public void AddMethods_AreFluent_AndKeepRegistrationOrder()
    {
        var services = new ServiceCollection();

        var returned = services
            .AddSingleton(typeof(IFoo), typeof(Foo))
            .AddScoped(typeof(IFoo), typeof(OtherFoo))
            .AddTransient(typeof(Foo), typeof(Foo));

        Assert.Same(services, returned);
        Assert.Equal(3, services.Count);
        Assert.Equal([typeof(Foo), typeof(OtherFoo), typeof(Foo)], services.Select(d => d.ImplementationType));
        Assert.Equal([Lifetime.Singleton, Lifetime.Scoped, Lifetime.Transient], services.Select(d => d.Lifetime));
        Assert.All(services.Take(2), d => Assert.Equal(typeof(IFoo), d.ServiceType));
    }

    [Fact]
    public void GenericOverloads_CreateExpectedDescriptors()
    {
        var instance = new Foo();
        var services = new ServiceCollection()
            .AddSingleton<IFoo, Foo>()
            .AddScoped<Foo>()
            .AddSingleton<IFoo>(instance)
            .AddTransient<IFoo>(_ => new OtherFoo());

        Assert.Equal(typeof(IFoo), services[0].ServiceType);
        Assert.Equal(typeof(Foo), services[0].ImplementationType);
        Assert.Equal(typeof(Foo), services[1].ServiceType);
        Assert.Equal(typeof(Foo), services[1].ImplementationType);
        Assert.Equal(Lifetime.Scoped, services[1].Lifetime);
        Assert.Same(instance, services[2].ImplementationInstance);
        Assert.Equal(Lifetime.Singleton, services[2].Lifetime);
        Assert.Null(services[2].ImplementationType);
        Assert.NotNull(services[3].ImplementationFactory);
        Assert.Equal(Lifetime.Transient, services[3].Lifetime);
    }

    [Fact]
    public void Descriptor_RejectsInvalidRegistrations()
    {
        Assert.Throws<ArgumentNullException>(() => new ServiceDescriptor(null!, typeof(Foo), Lifetime.Transient));
        Assert.Throws<ArgumentNullException>(() => new ServiceDescriptor(typeof(IFoo), (Type)null!, Lifetime.Transient));
        Assert.Throws<ArgumentNullException>(() => new ServiceDescriptor(typeof(IFoo), (object)null!));
        Assert.Throws<ArgumentNullException>(() => new ServiceDescriptor(typeof(IFoo), (Func<IServiceProvider, object>)null!, Lifetime.Scoped));

        // not assignable / not instantiable
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IFoo), typeof(Unrelated), Lifetime.Transient));
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IFoo), typeof(AbstractFoo), Lifetime.Transient));
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IFoo), typeof(IFoo), Lifetime.Transient));
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IFoo), new Unrelated()));

        // open generics
        _ = new ServiceDescriptor(typeof(IRepository<>), typeof(Repository<>), Lifetime.Transient);
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IRepository<>), typeof(Foo), Lifetime.Transient));
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IRepository<>), typeof(Repository<int>), Lifetime.Transient));
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IRepository<>), typeof(TwoParameterRepository<,>), Lifetime.Transient));
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IRepository<int>), typeof(Repository<>), Lifetime.Transient));
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IRepository<>), _ => new Repository<int>(), Lifetime.Transient));
        Assert.Throws<ArgumentException>(() => new ServiceDescriptor(typeof(IRepository<>), new Repository<int>()));
    }

    [Fact]
    public void BuildServiceProvider_UsesSnapshotOfRegistrations()
    {
        var services = new ServiceCollection().AddTransient<IFoo, Foo>();
        using var provider = services.BuildServiceProvider();

        services.AddTransient<IFoo, OtherFoo>();

        Assert.IsType<Foo>(provider.GetService(typeof(IFoo)));
    }

    // ---------------------------------------------------------------- basic resolution

    [Fact]
    public void GetService_ReturnsNull_WhenNotRegistered()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Null(provider.GetService(typeof(IFoo)));
        Assert.Null(provider.GetService<IFoo>());
    }

    [Fact]
    public void GetRequiredService_Throws_WhenNotRegistered()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        var ex = Assert.Throws<ServiceNotRegisteredException>(() => provider.GetRequiredService<IFoo>());

        Assert.Equal(typeof(IFoo), ex.ServiceType);
        Assert.Equal([typeof(IFoo)], ex.ResolutionPath);
    }

    [Fact]
    public void TypeRegistration_CreatesImplementation()
    {
        using var provider = new ServiceCollection().AddTransient<IFoo, Foo>().BuildServiceProvider();

        Assert.IsType<Foo>(provider.GetRequiredService<IFoo>());
    }

    [Fact]
    public void LastRegistrationWins()
    {
        using var provider = new ServiceCollection()
            .AddTransient<IFoo, Foo>()
            .AddTransient<IFoo, OtherFoo>()
            .BuildServiceProvider();

        Assert.IsType<OtherFoo>(provider.GetRequiredService<IFoo>());
    }

    // ---------------------------------------------------------------- lifetimes

    [Fact]
    public void Transient_ReturnsNewInstanceEveryTime()
    {
        using var provider = new ServiceCollection().AddTransient<IFoo, Foo>().BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotSame(provider.GetRequiredService<IFoo>(), provider.GetRequiredService<IFoo>());
        Assert.NotSame(scope.GetRequiredService<IFoo>(), scope.GetRequiredService<IFoo>());
    }

    [Fact]
    public void Singleton_IsSharedByRootAndAllScopes()
    {
        using var provider = new ServiceCollection().AddSingleton<IFoo, Foo>().BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var fromScope = scope1.GetRequiredService<IFoo>();

        Assert.Same(fromScope, provider.GetRequiredService<IFoo>());
        Assert.Same(fromScope, scope2.GetRequiredService<IFoo>());
    }

    [Fact]
    public void Singleton_IsCachedPerRegistration()
    {
        using var provider = new ServiceCollection()
            .AddSingleton<IFoo, Foo>()
            .AddSingleton<Foo>()
            .BuildServiceProvider();

        Assert.NotSame(provider.GetRequiredService<IFoo>(), provider.GetRequiredService<Foo>());
    }

    [Fact]
    public void Scoped_IsSameWithinScope_AndDifferentAcrossScopes()
    {
        using var provider = new ServiceCollection().AddScoped<IFoo, Foo>().BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        Assert.Same(scope1.GetRequiredService<IFoo>(), scope1.GetRequiredService<IFoo>());
        Assert.NotSame(scope1.GetRequiredService<IFoo>(), scope2.GetRequiredService<IFoo>());
    }

    [Fact]
    public void Scoped_ResolvedFromRoot_Throws()
    {
        using var provider = new ServiceCollection().AddScoped<IFoo, Foo>().BuildServiceProvider();

        var ex = Assert.Throws<CaptiveDependencyException>(() => provider.GetService(typeof(IFoo)));

        Assert.Null(ex.ConsumerType);
        Assert.Equal([typeof(IFoo)], ex.ResolutionPath);
    }

    [Fact]
    public void InstanceRegistration_ReturnsTheInstance()
    {
        var instance = new Foo();
        using var provider = new ServiceCollection().AddSingleton<IFoo>(instance).BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.Same(instance, provider.GetRequiredService<IFoo>());
        Assert.Same(instance, scope.GetRequiredService<IFoo>());
    }

    [Fact]
    public void Factories_AreCalledAccordingToLifetime()
    {
        int singletonCalls = 0, scopedCalls = 0, transientCalls = 0;
        using var provider = new ServiceCollection()
            .AddSingleton(_ => { singletonCalls++; return new Foo(); })
            .AddScoped<IFoo>(_ => { scopedCalls++; return new OtherFoo(); })
            .AddTransient(_ => { transientCalls++; return new Unrelated(); })
            .BuildServiceProvider();

        for (int i = 0; i < 2; i++)
        {
            using var scope = provider.CreateScope();
            for (int j = 0; j < 3; j++)
            {
                scope.GetRequiredService<Foo>();
                scope.GetRequiredService<IFoo>();
                scope.GetRequiredService<Unrelated>();
            }
        }

        Assert.Equal(1, singletonCalls);
        Assert.Equal(2, scopedCalls);
        Assert.Equal(6, transientCalls);
    }

    [Fact]
    public void Factories_ReceiveRootForSingletons_AndResolvingProviderOtherwise()
    {
        IServiceProvider? singletonArg = null, scopedArg = null, transientFromScopeArg = null, transientFromRootArg = null;
        using var provider = new ServiceCollection()
            .AddSingleton(sp => { singletonArg = sp; return new Foo(); })
            .AddScoped(sp => { scopedArg = sp; return new OtherFoo(); })
            .AddTransient(sp => { transientFromScopeArg ??= sp; transientFromRootArg = sp; return new Unrelated(); })
            .BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.GetRequiredService<Foo>();
        scope.GetRequiredService<OtherFoo>();
        scope.GetRequiredService<Unrelated>();
        provider.GetRequiredService<Unrelated>();

        Assert.Same(provider, singletonArg);
        Assert.Same(scope, scopedArg);
        Assert.Same(scope, transientFromScopeArg);
        Assert.Same(provider, transientFromRootArg);
    }

    [Fact]
    public void Factories_CanResolveOtherServices()
    {
        using var provider = new ServiceCollection()
            .AddSingleton<Log>()
            .AddTransient(sp => new Tracked("x", sp.GetRequiredService<Log>()))
            .BuildServiceProvider();

        var tracked = provider.GetRequiredService<Tracked>();
        tracked.Dispose();

        Assert.Equal(["x"], provider.GetRequiredService<Log>().Entries);
    }

    // ---------------------------------------------------------------- open generics

    [Fact]
    public void OpenGeneric_IsClosedForRequestedType()
    {
        using var provider = new ServiceCollection()
            .AddTransient(typeof(IRepository<>), typeof(Repository<>))
            .BuildServiceProvider();

        Assert.IsType<Repository<int>>(provider.GetRequiredService<IRepository<int>>());
        Assert.IsType<Repository<string>>(provider.GetRequiredService<IRepository<string>>());
    }

    [Fact]
    public void OpenGeneric_Singleton_IsCachedPerClosedType()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(typeof(IRepository<>), typeof(Repository<>))
            .BuildServiceProvider();

        Assert.Same(provider.GetRequiredService<IRepository<int>>(), provider.GetRequiredService<IRepository<int>>());
        Assert.NotSame((object)provider.GetRequiredService<IRepository<int>>(), provider.GetRequiredService<IRepository<long>>());
    }

    [Fact]
    public void ClosedRegistration_TakesPrecedenceOverOpenGeneric()
    {
        using var provider = new ServiceCollection()
            .AddTransient<IRepository<int>, IntRepository>()
            .AddTransient(typeof(IRepository<>), typeof(Repository<>))
            .BuildServiceProvider();

        Assert.IsType<IntRepository>(provider.GetRequiredService<IRepository<int>>());
        Assert.IsType<Repository<long>>(provider.GetRequiredService<IRepository<long>>());
    }

    [Fact]
    public void OpenGeneric_ViolatingConstraints_IsSkipped()
    {
        using var provider = new ServiceCollection()
            .AddTransient(typeof(IRepository<>), typeof(Repository<>))
            .AddTransient(typeof(IRepository<>), typeof(ClassOnlyRepository<>))
            .BuildServiceProvider();

        Assert.IsType<ClassOnlyRepository<string>>(provider.GetRequiredService<IRepository<string>>());
        Assert.IsType<Repository<int>>(provider.GetRequiredService<IRepository<int>>());

        using var onlyConstrained = new ServiceCollection()
            .AddTransient(typeof(IRepository<>), typeof(ClassOnlyRepository<>))
            .BuildServiceProvider();
        Assert.Null(onlyConstrained.GetService(typeof(IRepository<int>)));
    }

    // ---------------------------------------------------------------- thread safety

    [Fact]
    public void Singleton_ConcurrentFirstRequests_CreateExactlyOneInstance()
    {
        int created = 0;
        using var provider = new ServiceCollection()
            .AddSingleton<IFoo>(_ =>
            {
                Interlocked.Increment(ref created);
                Thread.Sleep(50);
                return new Foo();
            })
            .BuildServiceProvider();

        const int threadCount = 16;
        var results = new object?[threadCount];
        using var start = new ManualResetEventSlim();
        var threads = Enumerable.Range(0, threadCount)
            .Select(i => new Thread(() =>
            {
                start.Wait();
                results[i] = provider.GetService(typeof(IFoo));
            }))
            .ToList();
        threads.ForEach(t => t.Start());
        start.Set();
        threads.ForEach(t => t.Join());

        Assert.Equal(1, created);
        Assert.All(results, r => Assert.Same(results[0], r));
    }

    [Fact]
    public void Singleton_FailedCreation_IsNotCached()
    {
        int calls = 0;
        using var provider = new ServiceCollection()
            .AddSingleton<IFoo>(_ => ++calls == 1 ? throw new InvalidOperationException("first call fails") : new Foo())
            .BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetService(typeof(IFoo)));
        var second = provider.GetRequiredService<IFoo>();

        Assert.Same(second, provider.GetRequiredService<IFoo>());
        Assert.Equal(2, calls);
    }

    // ---------------------------------------------------------------- disposal

    [Fact]
    public void Scope_DisposesOwnedInstances_InReverseCreationOrder()
    {
        var log = new Log();
        using var provider = new ServiceCollection()
            .AddScoped<IFirst>(_ => new Tracked("dependency", log))
            .AddScoped<ISecond>(sp =>
            {
                sp.GetRequiredService<IFirst>(); // finished before this factory returns
                return new Tracked("consumer", log);
            })
            .AddTransient<IThird>(_ => new Tracked("transient", log))
            .BuildServiceProvider();

        var scope = provider.CreateScope();
        scope.GetRequiredService<ISecond>();
        scope.GetRequiredService<IThird>();
        scope.Dispose();

        Assert.Equal(["transient", "consumer", "dependency"], log.Entries);
    }

    [Fact]
    public void Scope_DisposesScopedAndTransient_ButNotSingletons()
    {
        var log = new Log();
        int next = 0;
        using var provider = new ServiceCollection()
            .AddSingleton<IFirst>(_ => new Tracked("singleton", log))
            .AddScoped<ISecond>(_ => new Tracked("scoped", log))
            .AddTransient<IThird>(_ => new Tracked($"transient{next++}", log))
            .BuildServiceProvider();

        var scope = provider.CreateScope();
        scope.GetRequiredService<IFirst>();
        scope.GetRequiredService<ISecond>();
        scope.GetRequiredService<IThird>();
        scope.GetRequiredService<IThird>();
        scope.Dispose();

        Assert.Equal(["transient1", "transient0", "scoped"], log.Entries);

        provider.Dispose();
        Assert.Equal(["transient1", "transient0", "scoped", "singleton"], log.Entries);
    }

    [Fact]
    public void Root_DisposesSingletons_InReverseCreationOrder()
    {
        var log = new Log();
        var provider = new ServiceCollection()
            .AddSingleton<IFirst>(_ => new Tracked("a", log))
            .AddSingleton<ISecond>(sp =>
            {
                sp.GetRequiredService<IFirst>();
                return new Tracked("b", log);
            })
            .BuildServiceProvider();

        provider.GetRequiredService<ISecond>(); // creates a, then b
        provider.Dispose();

        Assert.Equal(["b", "a"], log.Entries);
    }

    [Fact]
    public void TransientResolvedFromRoot_IsDisposedByRoot()
    {
        var log = new Log();
        var provider = new ServiceCollection()
            .AddTransient(_ => new Tracked("t", log))
            .BuildServiceProvider();

        provider.GetRequiredService<Tracked>();
        provider.Dispose();

        Assert.Equal(["t"], log.Entries);
    }

    [Fact]
    public void DependenciesOfSingletons_AreOwnedByRoot()
    {
        var log = new Log();
        var provider = new ServiceCollection()
            .AddTransient(_ => new Tracked("transient dependency", log))
            .AddSingleton(sp => new Holder(sp.GetRequiredService<Tracked>()))
            .BuildServiceProvider();

        using (var scope = provider.CreateScope())
            scope.GetRequiredService<Holder>();
        Assert.Empty(log.Entries);

        provider.Dispose();
        Assert.Equal(["transient dependency"], log.Entries);
    }

    [Fact]
    public void InstanceRegistrations_AreNeverDisposed()
    {
        var log = new Log();
        var instance = new Tracked("instance", log);
        var provider = new ServiceCollection().AddSingleton(instance).BuildServiceProvider();

        provider.GetRequiredService<Tracked>();
        provider.Dispose();

        Assert.Equal(0, instance.DisposeCount);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var log = new Log();
        var provider = new ServiceCollection().AddSingleton(_ => new Tracked("s", log)).BuildServiceProvider();
        var scope = provider.CreateScope();
        var singleton = scope.GetRequiredService<Tracked>();

        scope.Dispose();
        scope.Dispose();
        provider.Dispose();
        provider.Dispose();

        Assert.Equal(1, singleton.DisposeCount);
    }

    [Fact]
    public async Task DisposeAsync_PrefersIAsyncDisposable()
    {
        var log = new Log();
        var provider = new ServiceCollection()
            .AddScoped(_ => new SyncAndAsync())
            .AddScoped(_ => new AsyncOnly())
            .AddScoped(_ => new Tracked("sync only", log))
            .BuildServiceProvider();
        var scope = provider.CreateScope();
        var both = scope.GetRequiredService<SyncAndAsync>();
        var asyncOnly = scope.GetRequiredService<AsyncOnly>();
        scope.GetRequiredService<Tracked>();

        await scope.DisposeAsync();

        Assert.True(both.DisposedAsync);
        Assert.False(both.DisposedSync);
        Assert.True(asyncOnly.Disposed);
        Assert.Equal(["sync only"], log.Entries);
        await provider.DisposeAsync();
    }

    [Fact]
    public void Dispose_Throws_WhenAnOwnedInstanceIsOnlyAsyncDisposable()
    {
        using var provider = new ServiceCollection().AddScoped(_ => new AsyncOnly()).BuildServiceProvider();
        var scope = provider.CreateScope();
        scope.GetRequiredService<AsyncOnly>();

        Assert.Throws<InvalidOperationException>(() => scope.Dispose());
    }

    [Fact]
    public void DisposedProviderAndScope_ThrowObjectDisposedException()
    {
        var provider = new ServiceCollection().AddTransient<IFoo, Foo>().BuildServiceProvider();
        var scope = provider.CreateScope();

        scope.Dispose();
        Assert.Throws<ObjectDisposedException>(() => scope.GetService(typeof(IFoo)));
        Assert.Throws<ObjectDisposedException>(() => scope.GetRequiredService(typeof(IFoo)));
        Assert.IsType<Foo>(provider.GetService(typeof(IFoo))); // the root is still alive

        provider.Dispose();
        Assert.Throws<ObjectDisposedException>(() => provider.GetService(typeof(IFoo)));
        Assert.Throws<ObjectDisposedException>(() => provider.CreateScope());
    }
}
