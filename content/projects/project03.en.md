---
title: "Project03"
weight: 30
---

# MiniDI: A Dependency Injection Container and a Source Generator

**Total Points:** 8

> [!NOTE]
> ## Starting code
> {{< filetree dir="projects/project03/student" >}}

## 1. Overview
In this assignment, you will implement **MiniDI**, a small dependency injection (DI) container modelled on `Microsoft.Extensions.DependencyInjection` (the container used by ASP.NET Core). Services are registered in a `ServiceCollection`; the `ServiceProvider` built from it creates objects together with all their constructor dependencies and manages their lifetime (singleton, scoped, transient) and disposal.

You will build the container twice: first with **reflection** (constructors are discovered and invoked at runtime, classes are found by scanning an assembly for attributes), then with an **incremental source generator** that finds the same classes at compile time and emits plain `new` expressions, so no reflection is needed at runtime. C++ has no runtime reflection, so DI libraries for C++ rely on templates, i.e. they work at compile time, similarly to Part 4.

Related lectures: [Reflection]({{< ref "/lectures/08.reflection/reflection" >}}), [Attributes]({{< ref "/lectures/08.reflection/attributes" >}}), [Assembly]({{< ref "/lectures/08.reflection/assembly" >}}), [Generics]({{< ref "/lectures/04.creating-types/generics" >}}), [Disposable]({{< ref "/lectures/05.csharp-features/disposable" >}}), [Source Generators]({{< ref "/lectures/13.others/source-generators" >}}).

The solution `MiniDI.slnx` contains:

| Project | Content |
|---|---|
| `MiniDI` | The container library (`net10.0`). |
| `MiniDI.Generator` | The incremental source generator (`netstandard2.0`, Roslyn 4.8). |
| `MiniDI.Demo` | A console application (a tiny shop) that uses the container. It references the generator as an analyzer, which is already configured in `MiniDI.Demo.csproj`. |
| `MiniDI.Tests` | xUnit tests for all parts. The generator tests compile small programs in memory and run the generator on them. |

The following code is **given** and must not be changed: `Lifetime`, `ServiceAttribute`, `InjectAttribute`, the exceptions in `Exceptions.cs`, `TypeNameFormatter`, `ISupportRequiredService` and `ServiceProviderExtensions` (generic `GetService<T>()`/`GetRequiredService<T>()`); in the generator: the pipeline in `Initialize`, `EquatableArray<T>`, `LocationInfo`, `DiagnosticDescriptors`, `IsExternalInit`; the demo (except `Benchmarks.cs`) and the tests.

General rules:
1. Do not change the public API used by the tests and the demo. You may add any private/internal members and types and change the `internal` constructors of `ServiceProvider` and `ServiceScope`.
2. All tests must pass (`dotnet test`). With the starting code, all tests fail except three trivial generator tests. The tests do not cover everything: all requirements below are evaluated.
3. Do not use any DI library; the `MiniDI` project must not reference any NuGet packages.

Terminology: the **service type** is what is requested (`IClock`); a **registration** (`ServiceDescriptor`) says how to create it; the **root provider** is created by `BuildServiceProvider()`; a **scope** is created by `CreateScope()` (e.g. one web request).

## 2. Requirements

### Part 1: Registration API and Lifetimes [2 Points]
**Files:** `MiniDI/ServiceDescriptor.cs`, `MiniDI/ServiceCollection.cs`, `MiniDI/ServiceProvider.cs`, `MiniDI/ServiceScope.cs`

1.  **Descriptors:** `ServiceDescriptor` has three constructors: type-based `(serviceType, implementationType, lifetime)`, instance `(serviceType, instance)` (always `Singleton`) and factory `(serviceType, factory, lifetime)`. Exactly one of `ImplementationType`, `ImplementationInstance`, `ImplementationFactory` is set. The constructors validate their arguments:
    *   a `null` argument: `ArgumentNullException`;
    *   an implementation type that is not a non-abstract class (an interface, abstract or static class, struct), or is not assignable to the service type, or an instance that is not an instance of the service type: `ArgumentException`;
    *   **open generics:** a service type that is a generic type definition (`typeof(IRepository<>)`) is allowed only in a type-based registration whose implementation is a generic type definition with the same number of type parameters, implementing (or deriving from) the service type with its own type parameters in the same order, e.g. `class Repository<T> : IRepository<T>`. Every other combination (an open service with a closed or non-generic implementation, a closed service with an open implementation, an instance or factory for an open service) throws `ArgumentException`.
2.  **`ServiceCollection`:** An ordered list of descriptors (`IReadOnlyList<ServiceDescriptor>`; `Add` is given). Implement the `AddSingleton`/`AddScoped`/`AddTransient` overloads (type → type, `<TService, TImplementation>`, self-registration `<TService>`, factory; for singletons also an instance). Each one adds one descriptor and returns the same collection. Registering a service type again does not remove the earlier registrations.
3.  **Building:** `BuildServiceProvider()` creates the root provider from a **snapshot** of the collection; later changes to the collection do not affect the provider.
4.  **Resolution:** `GetService(Type)` returns `null` when no registration matches; `GetRequiredService(Type)` throws `ServiceNotRegisteredException` instead. If several registrations match, **the last one wins**. In this part it is enough to create type-based services through a public parameterless constructor (Part 2 generalizes this).
5.  **Lifetimes:** Instances are created lazily, on the first request:

    | Lifetime | Instance | Owner (disposes it) |
    |---|---|---|
    | `Singleton` | one per root provider and registration (for an open generic registration: per closed type), shared by all scopes | the root provider |
    | `Scoped` | one per scope and registration; resolving it from the root provider throws `CaptiveDependencyException` with `ConsumerType == null` | the scope |
    | `Transient` | a new instance on every request | the scope (or root) it was resolved from |

    Two registrations of the same implementation type are two separate singletons.
6.  **Factories:** A factory receives the provider that creates the instance: the root provider for singletons, otherwise the scope (or root) from which the service is resolved. All dependencies of a singleton are resolved from the root provider, also in Part 2, so they are owned by the root.
7.  **Open generics:** For a closed type such as `IRepository<User>`, closed registrations (exact match) are used first. Only if there is none, the last open-generic registration of `IRepository<>` is closed with `MakeGenericType` (`Repository<User>`). A registration whose generic constraints are not satisfied by the type arguments is skipped as if it did not exist.
8.  **Thread safety:** The root provider can be used by many threads at once. The constructor/factory of a singleton runs **exactly once**, even if many threads request it for the first time simultaneously. If it throws, nothing is cached and the next request tries again. A scope is used by one thread at a time and does not need to be thread-safe.
9.  **Disposal:** A provider/scope disposes the `IDisposable`/`IAsyncDisposable` instances it owns (see the table; instances passed to `AddSingleton(instance)` belong to the user and are never disposed). `Dispose()`/`DisposeAsync()` dispose them in **reverse order of creation**. An instance counts as created when its constructor or factory returns, so a service is disposed before its dependencies.
    *   `DisposeAsync()` calls `DisposeAsync()` on `IAsyncDisposable` instances and `Dispose()` on the others. `Dispose()` throws `InvalidOperationException` if an owned instance implements only `IAsyncDisposable`.
    *   A second call does nothing. After disposal, `GetService`, `GetRequiredService` and `CreateScope` throw `ObjectDisposedException`. Disposing the root provider does not dispose scopes.

### Part 2: Reflection-Based Resolution [2 Points]
**Files:** `MiniDI/ServiceProvider.cs`, `MiniDI/ServiceScope.cs` (and your own internal types)

1.  **Constructor selection** (for a type-based registration, applied to the closed implementation type):
    1.  Only **public instance** constructors are candidates. If some of them are marked with `[Inject]`, only those are candidates; more than one `[Inject]` constructor throws `AmbiguousConstructorException`.
    2.  A parameter is **resolvable** if its type has a matching registration (closed or open generic), if it is `IEnumerable<T>`, or if it has a default value (`ParameterInfo.HasDefaultValue`). This is a shallow check: nothing is created and dependencies of the dependency are not examined.
    3.  From the candidates whose parameters are all resolvable, choose the one with **the most parameters**. If there is none, throw `NoSuitableConstructorException`; if two or more have the same number of parameters, throw `AmbiguousConstructorException` (`Candidates` = these constructors).
    4.  The choice depends only on the registrations: compute it once per implementation type and cache it (thread-safely). Do not search the constructors on every resolution.
2.  **Arguments:** A parameter whose type is registered (or is `IEnumerable<T>`) is resolved recursively by the same provider/scope, following the lifetime rules; otherwise its default value is used. An optional parameter whose type is registered is resolved, not defaulted.
3.  **Exceptions:** Exceptions thrown by constructors and factories propagate unchanged, i.e. not wrapped in `TargetInvocationException` (hint: `BindingFlags.DoNotWrapExceptions`). A `ResolutionException` from a nested dependency is not wrapped either.
4.  **`IEnumerable<T>`:** `GetService(typeof(IEnumerable<T>))` returns a `T[]` with one element per matching registration (closed and open generic), in registration order, each created according to its own lifetime. Without registrations, it returns an empty array (never `null`).
5.  **Resolution path:** The provider tracks the chain of service requests in progress on the current thread, including requests made from inside factories (e.g. `sp => new A(sp.GetRequiredService<B>())`) and `IEnumerable<T>` requests. Every `ResolutionException` receives the path: the requested service types from the outermost request to the one that failed, e.g. `[IOrderService, PriceCalculator, IDiscountRule]`. The given exception classes include it in the message (`... Resolution path: A -> B -> A.`).
6.  **Circular dependencies:** Requesting a service type that is already on the chain throws `CircularDependencyException`; the path ends with the repeated type (`[A, B, A]`). A cycle must never end with a `StackOverflowException`.
7.  **Captive dependencies:** Requesting a scoped service while a singleton is on the chain (also indirectly, e.g. singleton → transient → scoped) throws `CaptiveDependencyException` with `ConsumerType` = the nearest singleton on the chain. Otherwise, requesting a scoped service from the root provider throws `CaptiveDependencyException` with `ConsumerType == null`. A scoped service may depend on singletons.

### Part 3: Attributes, Assembly Scanning and Validation [2 Points]
**Files:** `MiniDI/ServiceCollectionScanningExtensions.cs`, `MiniDI/ServiceCollection.cs` (and your internal types)

1.  **Attribute:** `[Service]` (given) marks a class as a service: `[Service(Lifetime.Scoped, As = typeof(IFoo))]`. The lifetime defaults to `Transient` and `As` defaults to the class itself. It has `AllowMultiple = true` (one registration per attribute) and `Inherited = false`.
2.  **Scanning:** `AddFromAssembly(Assembly assembly, Func<Type, bool>? filter = null)`:
    *   registers every class of the assembly (public or not) that has `[Service]` declared directly on it (the attribute is not inherited) and is accepted by `filter` (if given);
    *   processes the classes **sorted by `Type.FullName`** using ordinal comparison (`StringComparer.Ordinal`; the order of the attributes of one class is unspecified), which makes the order of `IEnumerable<T>` deterministic;
    *   for each attribute adds a type-based registration `As ?? class` → class with the given lifetime; for a generic class it is an open-generic registration (`[Service(As = typeof(IRepository<>))] class Repository<T>`);
    *   lets invalid combinations (an abstract class, `As` not implemented, ...) fail with the `ArgumentException` from `ServiceDescriptor`;
    *   returns the same collection.
3.  **Validation:** `BuildServiceProvider(validateOnBuild: true)` checks the container before returning it:
    *   For every type-based registration with a closed service type, in registration order, simulate the resolution **without creating anything** (no constructor or factory may run): constructor selection, recursively all dependencies (`IEnumerable<T>`: every matching registration), and circular and captive dependency detection, with the same rules, exceptions and paths as in Part 2. A registration is checked as if it were resolved from a scope (for a singleton, the captive rule applies to its dependencies).
    *   Factory, instance and open-generic registrations are not checked themselves (an open generic is checked when a dependency closes it). As dependencies, factory and instance registrations are leaves: they are not inspected, but their lifetime matters (a singleton depending on a scoped factory registration is captive).
    *   Take the first error found for each failing registration and throw a `ContainerValidationException` with these errors in registration order. If there is no error, return the provider.
    *   Without validation (the default), these errors appear only during resolution.

### Part 4: Source Generator [2 Points]
**Files:** `MiniDI.Generator/ServiceRegistrationGenerator.cs` (and your model types), `MiniDI.Demo/Benchmarks.cs`, `README.md`

1.  **Pipeline:** The pipeline is given in `Initialize`: `ForAttributeWithMetadataName("MiniDI.ServiceAttribute")` → `Analyze` for every class → `Collect()` → `Emit`. Keep the step name `ServiceModels` (`WithTrackingName`), which is used by a test.
2.  **Model:** `Analyze` turns the class symbol into an **equatable** model (extend the `ServiceClassModel` record; use records, strings, numbers, `EquatableArray<T>`, `LocationInfo`) that never holds an `ISymbol`, `SyntaxNode`, `Location` or `Compilation`. A test adds a method to a service class and checks that the models are `Cached`/`Unchanged`.
3.  **Generated code:** The generator always (even if there are no services) adds a file containing:
    ```csharp
    namespace MiniDI.Generated
    {
        internal static class GeneratedServices
        {
            public static global::MiniDI.ServiceCollection AddGenerated(this global::MiniDI.ServiceCollection services)
            {
                // one registration per [Service] attribute
                return services;
            }
        }
    }
    ```
    The classes are processed in the same order as in `AddFromAssembly`: sorted by their metadata names using ordinal comparison; a metadata name equals `Type.FullName` (e.g. `Shop.Outer+Inner`, ``Shop.Repository`1``).
4.  **Factories without reflection:** A non-generic class is registered with a factory, e.g. `new ServiceDescriptor(typeof(global::Shop.IClock), static sp => new global::Shop.FixedClock(), Lifetime.Singleton)`. The constructor is chosen at compile time: the `[Inject]` one if there is one, otherwise the only public constructor. Arguments:
    *   a parameter without a default value (also `IEnumerable<T>`): `global::MiniDI.ServiceProviderExtensions.GetRequiredService<T>(sp)`;
    *   a parameter with a default value: the service if `sp.GetService(typeof(T))` returns non-`null`, otherwise the default value. It is enough to support `null`/`default`, `bool`, numeric, `char`, `string` and enum constants (`SymbolDisplay.FormatPrimitive` helps).

    The generated code must not use reflection (`Activator`, `GetConstructor`, `Invoke`, ...); `typeof` is fine. Use fully qualified names (`global::...`) so that the code compiles regardless of the user's `using` directives.
5.  **Fallback to reflection:** A generic class (an open-generic service) and a class with several public constructors and no `[Inject]` (warning MDI005) are registered type-based (e.g. `typeof(global::Shop.Repository<>)`), i.e. activated by reflection at runtime.
6.  **Diagnostics:** Report them with `SourceProductionContext.ReportDiagnostic` using the given `DiagnosticDescriptors`, located at the class name (`ClassDeclarationSyntax.Identifier`); `{0}` is the class name (`ISymbol.ToDisplayString()`). A class with an error gets no registration.

    | Id | Severity | When |
    |---|---|---|
    | MDI001 | Error | the class has no public constructor |
    | MDI002 | Error | more than one public constructor is marked with `[Inject]` |
    | MDI003 | Error | the class does not implement or derive from `As` (`{1}`); for open generics the same rule as in Part 1 |
    | MDI004 | Error | the class is abstract or static, or it or a containing type is neither public nor internal |
    | MDI005 | Warning | a non-generic class has several public constructors and none is marked with `[Inject]` |

7.  **Demo:** `MiniDI.Demo` uses the generated registrations by default and `AddFromAssembly` with the `reflection` argument. Both runs must print the same output (except for the first line):
    ```text
    === MiniDI demo (generated): 9 registrations ===
    -- request for alice --
      [OrderService] request #1: order #1 for alice, discounts: weekend -5%
      Receipt #1 (alice, 2026-01-17 10:30)
          1 x keyboard      249.00 PLN
          2 x mouse         199.00 PLN
        total               425.60 PLN
      OrderService of request #1 disposed asynchronously
      RequestContext #1 disposed
    -- request for bob --
      [OrderService] request #2: order #2 for bob, discounts: bulk -10%, weekend -5%
      Receipt #2 (bob, 2026-01-17 10:30)
         12 x cable         180.00 PLN
        total               153.00 PLN
      OrderService of request #2 disposed asynchronously
      RequestContext #2 disposed
    -- 2 orders in the singleton repository --
    CaptiveDependencyException: Cannot resolve scoped service 'IOrderService' from the root provider. Resolution path: IOrderService.
    ```
8.  **Benchmark:** Implement `Benchmarks.RunAsync` (`Stopwatch` is enough; you may additionally use BenchmarkDotNet). For the given variant (`reflection` or `generated`) measure: (a) registration + `BuildServiceProvider()`, (b) the first resolution of `IOrderService` in a new scope, (c) the mean time of one resolution of `PriceCalculator` from a scope (at least 1,000,000 resolutions after a warm-up). Run each variant in a separate process, in Release. Write a `README.md` in the solution root with the machine and OS, a table of results and a few sentences explaining the differences, including any measurement where the generated variant is **not** faster, and why.

> [!TIP]
> The generator tests run the generator in-process, so you can debug it with breakpoints in the tests. The code generated for the demo is written to `MiniDI.Demo/obj/Debug/net10.0/generated/` (`EmitCompilerGeneratedFiles` is enabled). An IDE may keep an old version of the generator loaded; if the generated code looks outdated, build from the command line or restart the IDE.

### What is evaluated at the defense
*   Explaining and modifying your code: constructor selection, open generics (`MakeGenericType`), lifetimes and ownership of instances, the resolution chain, cycle and captive dependency detection, thread safety of singletons, disposal order.
*   The incremental pipeline: why the models must be equatable and what is (not) re-run after an edit.
*   Interpreting your benchmark results.

## 3. Build

```bash
dotnet build
dotnet test
dotnet run --project MiniDI.Demo                     # generated registrations
dotnet run --project MiniDI.Demo -- reflection       # assembly scanning
dotnet run -c Release --project MiniDI.Demo -- bench reflection
dotnet run -c Release --project MiniDI.Demo -- bench generated
```
