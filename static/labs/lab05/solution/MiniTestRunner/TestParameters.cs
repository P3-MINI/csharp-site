namespace MiniTestRunner;

/// <summary>
/// Represents a set of parameters and an optional description for a parameterized test method.
/// </summary>
/// <param name="Description">An optional description of the test case.</param>
/// <param name="Parameters">An array of parameters to be passed to the test method.</param>
public sealed record TestParameters(
    string? Description,
    params object?[] Parameters
);