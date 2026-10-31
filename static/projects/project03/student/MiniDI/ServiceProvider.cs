namespace MiniDI;

/// <summary>The root provider: owns singletons (and transients resolved directly from it).</summary>
public sealed class ServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable, IAsyncDisposable
{
    // TODO (Parts 1-3): implement the provider. You may change this constructor and add any
    // private/internal members and types (e.g. an engine shared by ServiceProvider and ServiceScope).
    internal ServiceProvider(IReadOnlyList<ServiceDescriptor> descriptors, bool validateOnBuild)
    {
        throw new NotImplementedException();
    }

    /// <summary>Returns the service, or <c>null</c> if no registration matches.</summary>
    public object? GetService(Type serviceType) => throw new NotImplementedException();

    /// <summary>Returns the service or throws a <see cref="ResolutionException"/>.</summary>
    public object GetRequiredService(Type serviceType) => throw new NotImplementedException();

    /// <summary>Creates a new scope. Scoped services can only be resolved from a scope.</summary>
    public ServiceScope CreateScope() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();

    public ValueTask DisposeAsync() => throw new NotImplementedException();
}
