using Microsoft.CodeAnalysis;

namespace MiniDI.Generator;

/// <summary>Diagnostics reported by the generator (Part 4). Do not change the ids, severities or messages.</summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "MiniDI";

    /// <summary>{0} = the class (e.g. <c>Shop.OrderService</c>).</summary>
    public static readonly DiagnosticDescriptor NoPublicConstructor = new(
        id: "MDI001",
        title: "Service has no public constructor",
        messageFormat: "Service '{0}' has no public constructor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>{0} = the class.</summary>
    public static readonly DiagnosticDescriptor MultipleInjectConstructors = new(
        id: "MDI002",
        title: "Several constructors are marked with [Inject]",
        messageFormat: "Service '{0}' has more than one public constructor marked with [Inject]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>{0} = the class, {1} = the type given in <c>As</c>.</summary>
    public static readonly DiagnosticDescriptor NotAssignableToServiceType = new(
        id: "MDI003",
        title: "Service does not implement the registered service type",
        messageFormat: "Service '{0}' cannot be registered as '{1}' because it does not implement or derive from it",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>{0} = the class.</summary>
    public static readonly DiagnosticDescriptor NotInstantiable = new(
        id: "MDI004",
        title: "Service class cannot be instantiated by generated code",
        messageFormat: "Service '{0}' must be a non-abstract, non-static class accessible from generated code (public or internal)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>{0} = the class.</summary>
    public static readonly DiagnosticDescriptor ConstructorChosenAtRuntime = new(
        id: "MDI005",
        title: "Constructor will be chosen at runtime",
        messageFormat: "Service '{0}' has several public constructors and none is marked with [Inject]; it is registered for reflection-based activation",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
