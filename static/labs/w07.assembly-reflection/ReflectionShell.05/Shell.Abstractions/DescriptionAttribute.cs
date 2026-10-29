namespace Shell.Abstractions;

// Human-readable description displayed by the help command
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
public sealed class DescriptionAttribute(string text) : Attribute
{
    public string Text { get; } = text;
}
