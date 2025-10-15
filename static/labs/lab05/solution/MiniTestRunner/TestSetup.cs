using System.Reflection;

namespace MiniTestRunner;

/// <summary>
/// Represents a setup method bound to a specific test instance.
/// Used to invoke setup logic before or after test execution.
/// </summary>
/// <param name="MethodInfo">The method information representing the setup method.</param>
public sealed record TestSetup(MethodInfo MethodInfo)
{
    /// <summary>
    /// Gets or sets the bound delegate for invoking the setup method.
    /// </summary>
    private Action? Method { get; set; }

    /// <summary>
    /// Binds the setup method to the specified test class instance.
    /// </summary>
    /// <param name="instance">The instance of the test class to bind the method to.</param>
    /// <returns>An <see cref="Action"/> delegate that can invoke the setup method.</returns>
    private Action Bind(object instance)
    {
        return (Action)Delegate.CreateDelegate(typeof(Action), instance, MethodInfo);
    }

    /// <summary>
    /// Executes the setup method on the specified test class instance.
    /// </summary>
    /// <param name="instance">The instance of the test class on which to run the setup method.</param>
    public void Run(object instance)
    {
        this.Method ??= this.Bind(instance);
        this.Method();
    }
}