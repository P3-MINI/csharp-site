namespace MiniDI;

/// <summary>
/// Implemented by providers that can produce a detailed error (with the resolution path)
/// for a missing required service.
/// </summary>
public interface ISupportRequiredService
{
    /// <summary>Resolves <paramref name="serviceType"/> or throws a <see cref="ResolutionException"/>.</summary>
    object GetRequiredService(Type serviceType);
}
