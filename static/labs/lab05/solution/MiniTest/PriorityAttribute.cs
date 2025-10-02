namespace MiniTest;

[AttributeUsage(AttributeTargets.Method)]
public sealed class PriorityAttribute(int priority) : Attribute
{
    public int Priority { get; } = priority;
}