namespace MiniTest.Attributes;

/// <summary>
/// Specifies a set of data values to be passed to a parameterized test method.
/// Allows multiple instances on the same method to provide different sets of input data.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class DataRowAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataRowAttribute"/> class with the specified data values.
    /// </summary>
    /// <param name="data">An array of objects representing the data to be passed to the test method.</param>
    public DataRowAttribute(params object?[]? data)
    {
        Data = data ?? [null];
    }

    /// <summary>
    /// Gets the data values associated with this attribute.
    /// </summary>
    public object?[] Data { get; }

    /// <summary>
    /// Gets or sets an optional description for the data row.
    /// </summary>
    public string? Description { get; set; }
}