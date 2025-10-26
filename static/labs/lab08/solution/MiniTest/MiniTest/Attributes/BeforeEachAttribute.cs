namespace MiniTest.Attributes;

/// <summary>
/// Indicates that a method should be executed before each test method in a test class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class BeforeEachAttribute : Attribute;
