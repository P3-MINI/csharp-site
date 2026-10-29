using System.Collections;

namespace MiniDI;

/// <summary>An ordered list of registrations. Later registrations of the same service type win.</summary>
public sealed class ServiceCollection : IReadOnlyList<ServiceDescriptor>
{
    private readonly List<ServiceDescriptor> _descriptors = [];

    public int Count => _descriptors.Count;

    public ServiceDescriptor this[int index] => _descriptors[index];

    public IEnumerator<ServiceDescriptor> GetEnumerator() => _descriptors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ServiceCollection Add(ServiceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _descriptors.Add(descriptor);
        return this;
    }

    // TODO (Part 1): implement the overloads below; each one adds a descriptor and returns this collection.

    // ---- Singleton ----

    public ServiceCollection AddSingleton(Type serviceType, Type implementationType) =>
        throw new NotImplementedException();

    public ServiceCollection AddSingleton(Type serviceType, object instance) =>
        throw new NotImplementedException();

    public ServiceCollection AddSingleton(Type serviceType, Func<IServiceProvider, object> factory) =>
        throw new NotImplementedException();

    public ServiceCollection AddSingleton<TService>() where TService : class =>
        throw new NotImplementedException();

    public ServiceCollection AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService =>
        throw new NotImplementedException();

    public ServiceCollection AddSingleton<TService>(TService instance) where TService : class =>
        throw new NotImplementedException();

    public ServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> factory) where TService : class =>
        throw new NotImplementedException();

    // ---- Scoped ----

    public ServiceCollection AddScoped(Type serviceType, Type implementationType) =>
        throw new NotImplementedException();

    public ServiceCollection AddScoped(Type serviceType, Func<IServiceProvider, object> factory) =>
        throw new NotImplementedException();

    public ServiceCollection AddScoped<TService>() where TService : class =>
        throw new NotImplementedException();

    public ServiceCollection AddScoped<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService =>
        throw new NotImplementedException();

    public ServiceCollection AddScoped<TService>(Func<IServiceProvider, TService> factory) where TService : class =>
        throw new NotImplementedException();

    // ---- Transient ----

    public ServiceCollection AddTransient(Type serviceType, Type implementationType) =>
        throw new NotImplementedException();

    public ServiceCollection AddTransient(Type serviceType, Func<IServiceProvider, object> factory) =>
        throw new NotImplementedException();

    public ServiceCollection AddTransient<TService>() where TService : class =>
        throw new NotImplementedException();

    public ServiceCollection AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService =>
        throw new NotImplementedException();

    public ServiceCollection AddTransient<TService>(Func<IServiceProvider, TService> factory) where TService : class =>
        throw new NotImplementedException();

    // ---- Building ----

    /// <summary>
    /// Creates the root provider from a snapshot of the current registrations.
    /// With <paramref name="validateOnBuild"/> every type-based registration is checked (Part 3).
    /// </summary>
    public ServiceProvider BuildServiceProvider(bool validateOnBuild = false) =>
        throw new NotImplementedException(); // TODO (Part 1, validation: Part 3)
}
