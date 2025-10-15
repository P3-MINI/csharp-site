namespace MiniTest.Attributes;

/// <summary>
/// Identifies a class that contains test methods for the MiniTest framework.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TestClassAttribute : Attribute;