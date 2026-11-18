namespace MiniDI;

/// <summary>Generic helpers for <see cref="IServiceProvider"/> (used by factories and by the generated code).</summary>
public static class ServiceProviderExtensions
{
    /// <summary>Returns the service, or <c>default</c> if it is not registered.</summary>
    public static T? GetService<T>(this IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        return provider.GetService(typeof(T)) is T service ? service : default;
    }

    /// <summary>Returns the service or throws a <see cref="ResolutionException"/>.</summary>
    public static T GetRequiredService<T>(this IServiceProvider provider) where T : notnull =>
        (T)provider.GetRequiredService(typeof(T));

    /// <summary>Returns the service or throws a <see cref="ResolutionException"/>.</summary>
    public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(serviceType);
        if (provider is ISupportRequiredService required)
            return required.GetRequiredService(serviceType);
        return provider.GetService(serviceType) ?? throw new ServiceNotRegisteredException([serviceType]);
    }
}
