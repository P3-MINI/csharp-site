namespace MiniTest;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class DescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}