namespace MiniTestRunner;

public sealed record TestParameters(
    string? Description, 
    params object?[] Parameters
);