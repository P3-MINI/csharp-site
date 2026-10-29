namespace MiniDI;

/// <summary>
/// Marks a class as a service. Such classes are registered by
/// <see cref="ServiceCollectionScanningExtensions.AddFromAssembly"/> (reflection, Part 3)
/// and by the source generator (Part 4).
/// </summary>
/// <param name="lifetime">Lifetime of the registration.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ServiceAttribute(Lifetime lifetime = Lifetime.Transient) : Attribute
{
    public Lifetime Lifetime { get; } = lifetime;

    /// <summary>
    /// The service type the class is registered as (e.g. <c>typeof(IClock)</c> or
    /// <c>typeof(IRepository&lt;&gt;)</c>). <c>null</c> means the class itself.
    /// </summary>
    public Type? As { get; set; }
}

/// <summary>Marks the constructor that the container must use to create the service.</summary>
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class InjectAttribute : Attribute;
