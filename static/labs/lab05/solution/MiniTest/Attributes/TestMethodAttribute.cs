namespace MiniTest.Attributes;

/// <summary>
/// Identifies a method as a test method to be executed by the MiniTest framework.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class TestMethodAttribute : Attribute;