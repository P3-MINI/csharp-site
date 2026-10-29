namespace MiniDI;

/// <summary>
/// One registration: a service type, a lifetime and exactly one way of creating the instance
/// (implementation type, ready instance or factory).
/// </summary>
public sealed class ServiceDescriptor
{
    /// <summary>Type-based registration (the container calls a constructor of <paramref name="implementationType"/>).</summary>
    public ServiceDescriptor(Type serviceType, Type implementationType, Lifetime lifetime)
    {
        // TODO (Part 1): validate the arguments (closed types and open generics) and set the properties.
        throw new NotImplementedException();
    }

    /// <summary>Instance registration (always <see cref="MiniDI.Lifetime.Singleton"/>).</summary>
    public ServiceDescriptor(Type serviceType, object instance)
    {
        // TODO (Part 1)
        throw new NotImplementedException();
    }

    /// <summary>Factory registration.</summary>
    public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, Lifetime lifetime)
    {
        // TODO (Part 1)
        throw new NotImplementedException();
    }

    public Type ServiceType { get; }

    public Lifetime Lifetime { get; }

    public Type? ImplementationType { get; }

    public object? ImplementationInstance { get; }

    public Func<IServiceProvider, object>? ImplementationFactory { get; }
}
