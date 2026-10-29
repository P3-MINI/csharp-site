namespace Shell.Abstractions;

// Marks a method as a shell command. Without a name, the lower-case method name is used.
[AttributeUsage(AttributeTargets.Method)]
public sealed class CommandAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}
