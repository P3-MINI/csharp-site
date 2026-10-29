namespace Shell.Abstractions;

// An alternative name of a command - a command may have many aliases
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class AliasAttribute(string alias) : Attribute
{
    public string Alias { get; } = alias;
}
