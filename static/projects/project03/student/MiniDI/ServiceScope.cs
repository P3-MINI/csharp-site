namespace MiniDI;

/// <summary>A scope (e.g. one web request): owns its scoped services and the transients resolved from it.</summary>
public sealed class ServiceScope : IServiceProvider, ISupportRequiredService, IDisposable, IAsyncDisposable
{
    // TODO (Parts 1-2): implement the scope. You may change this constructor.
    internal ServiceScope(ServiceProvider root)
    {
        throw new NotImplementedException();
    }

    /// <summary>Returns the service, or <c>null</c> if no registration matches.</summary>
    public object? GetService(Type serviceType) => throw new NotImplementedException();

    /// <summary>Returns the service or throws a <see cref="ResolutionException"/>.</summary>
    public object GetRequiredService(Type serviceType) => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();

    public ValueTask DisposeAsync() => throw new NotImplementedException();
}
