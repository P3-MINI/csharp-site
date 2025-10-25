namespace MiniTest.Attributes;

/// <summary>
/// Specifies a human-readable description for a test method or test class.
/// Useful for providing context or additional information about the purpose of a test.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class DescriptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class with the specified description.
    /// </summary>
    /// <param name="description">The description text associated with the test method or class.</param>
    public DescriptionAttribute(string description)
    {
        Description = description;
    }

    /// <summary>
    /// Gets the description text associated with the test method or class.
    /// </summary>
    public string Description { get; }
}