namespace MiniDI;

/// <summary>Specifies how long a resolved service instance lives.</summary>
public enum Lifetime
{
    /// <summary>One instance per root provider (and registration), shared by all scopes.</summary>
    Singleton,

    /// <summary>One instance per scope (and registration).</summary>
    Scoped,

    /// <summary>A new instance on every request.</summary>
    Transient,
}
