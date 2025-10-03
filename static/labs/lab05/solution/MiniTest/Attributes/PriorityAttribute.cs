namespace MiniTest.Attributes;

/// <summary>
/// Specifies the priority level of a test method.
/// Useful for controlling the order in which tests are executed or for categorizing their importance.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class PriorityAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityAttribute"/> class with the specified priority value.
    /// </summary>
    /// <param name="priority">An integer representing the priority level of the test.</param>
    public PriorityAttribute(int priority)
    {
        Priority = priority;
    }

    /// <summary>
    /// Gets the priority level assigned to the test method.
    /// </summary>
    public int Priority { get; }
}