using System.Reflection;

namespace MiniDI;

/// <summary>Base class of all errors detected while resolving (or validating) a service.</summary>
public abstract class ResolutionException : Exception
{
    protected ResolutionException(string message, IReadOnlyList<Type> resolutionPath)
        : base($"{message} Resolution path: {TypeNameFormatter.FormatPath(resolutionPath)}.")
    {
        if (resolutionPath.Count == 0)
            throw new ArgumentException("The resolution path must not be empty.", nameof(resolutionPath));
        ResolutionPath = resolutionPath;
    }

    /// <summary>
    /// Requested service types, from the outermost request to the service whose resolution failed
    /// (inclusive), e.g. <c>[IOrderService, PriceCalculator, IDiscountRule]</c>.
    /// </summary>
    public IReadOnlyList<Type> ResolutionPath { get; }

    /// <summary>The service type whose resolution failed (the last element of <see cref="ResolutionPath"/>).</summary>
    public Type ServiceType => ResolutionPath[^1];
}

/// <summary>A required service has no matching registration.</summary>
public sealed class ServiceNotRegisteredException(IReadOnlyList<Type> resolutionPath)
    : ResolutionException(
        $"No service for type '{TypeNameFormatter.Format(resolutionPath[^1])}' has been registered.",
        resolutionPath);

/// <summary>No candidate constructor of the implementation type can be satisfied.</summary>
public sealed class NoSuitableConstructorException(Type implementationType, IReadOnlyList<Type> resolutionPath)
    : ResolutionException(
        $"No suitable constructor found for '{TypeNameFormatter.Format(implementationType)}': " +
        "it has no public constructor whose parameters can all be resolved.",
        resolutionPath)
{
    public Type ImplementationType { get; } = implementationType;
}

/// <summary>The container cannot decide which constructor to use.</summary>
public sealed class AmbiguousConstructorException(
    Type implementationType,
    IReadOnlyList<ConstructorInfo> candidates,
    IReadOnlyList<Type> resolutionPath)
    : ResolutionException(
        $"Cannot choose a constructor of '{TypeNameFormatter.Format(implementationType)}' between: " +
        $"{string.Join(", ", candidates.Select(TypeNameFormatter.Format))}.",
        resolutionPath)
{
    public Type ImplementationType { get; } = implementationType;
    public IReadOnlyList<ConstructorInfo> Candidates { get; } = candidates;
}

/// <summary>
/// A service (directly or indirectly) depends on itself. The path ends with the repeated
/// service type, e.g. <c>[A, B, A]</c>.
/// </summary>
public sealed class CircularDependencyException(IReadOnlyList<Type> resolutionPath)
    : ResolutionException("Circular dependency detected.", resolutionPath);

/// <summary>
/// A scoped service (the last element of the path) would be captured by a singleton
/// (<see cref="ConsumerType"/>) or resolved from the root provider (<see cref="ConsumerType"/> is <c>null</c>).
/// </summary>
public sealed class CaptiveDependencyException(Type? consumerType, IReadOnlyList<Type> resolutionPath)
    : ResolutionException(
        consumerType is null
            ? $"Cannot resolve scoped service '{TypeNameFormatter.Format(resolutionPath[^1])}' from the root provider."
            : $"Cannot consume scoped service '{TypeNameFormatter.Format(resolutionPath[^1])}' " +
              $"from singleton '{TypeNameFormatter.Format(consumerType)}'.",
        resolutionPath)
{
    /// <summary>The singleton service that would capture the scoped one, or <c>null</c> for the root provider.</summary>
    public Type? ConsumerType { get; } = consumerType;
}

/// <summary>Thrown by <c>BuildServiceProvider(validateOnBuild: true)</c>; contains one error per invalid registration.</summary>
public sealed class ContainerValidationException(IReadOnlyList<ResolutionException> errors)
    : AggregateException("Some services cannot be constructed.", errors)
{
    public IReadOnlyList<ResolutionException> Errors { get; } = errors;
}
