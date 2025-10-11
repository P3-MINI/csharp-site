using System.Reflection;

namespace MiniTestRunner;

/// <summary>
/// Represents a test method with optional parameters, description, and priority.
/// Provides functionality to execute the test and report results.
/// </summary>
/// <param name="MethodInfo">The reflection metadata for the test method.</param>
/// <param name="Parameters">Optional list of parameter sets for data-driven tests.</param>
/// <param name="Description">An optional description of the test method.</param>
/// <param name="Priority">The priority value used to order test execution.</param>
public sealed record TestMethod(
    MethodInfo MethodInfo,
    List<TestParameters>? Parameters,
    string? Description,
    int Priority = int.MinValue)
{
    /// <summary>
    /// Executes the test method on the given instance.
    /// If parameters are provided, runs the method for each parameter set.
    /// </summary>
    /// <param name="instance">The instance of the test class on which to invoke the method.</param>
    /// <returns>A <see cref="TestResults"/> object containing the outcome of the test execution.</returns>
    public TestResults Run(object instance)
    {
        return this.Parameters is { Count: > 0 } 
            ? this.RunParameterizedTest(instance) 
            : this.RunTest(instance);
    }

    /// <summary>
    /// Executes a non-parameterized test method and prints the result to the console.
    /// </summary>
    /// <param name="instance">The instance of the test class.</param>
    /// <returns>A <see cref="TestResults"/> object representing the result of the test.</returns>
    private TestResults RunTest(object instance)
    {
        Console.Write($"{this.MethodInfo.Name,-60}: ");
        TestResults results;
        try
        {
            using var _ = new ConsoleColoring(ConsoleColor.Green);
            this.MethodInfo.Invoke(instance, []);
            Console.WriteLine("PASSED");
            results = new TestResults(1, 1);
        }
        catch (Exception e)
        {
            using var _ = new ConsoleColoring(ConsoleColor.Red);
            Console.WriteLine("FAILED");
            Console.WriteLine(e.InnerException?.Message);
            results = new TestResults(0, 1);
        }

        if (this.Description is not null)
        {
            Console.WriteLine(this.Description);
        }
        return results;
    }

    /// <summary>
    /// Executes a parameterized test method for each set of parameters and prints the results.
    /// </summary>
    /// <param name="obj">The instance of the test class.</param>
    /// <returns>A <see cref="TestResults"/> object representing the aggregated results of all parameterized test runs.</returns>
    private TestResults RunParameterizedTest(object obj)
    {
        var results = new TestResults();
        Console.WriteLine(this.MethodInfo.Name);
        foreach (var param in this.Parameters!)
        {
            Console.Write($" - {param.Description,-57}: ");
            try
            {
                using var _ = new ConsoleColoring(ConsoleColor.Green);
                this.MethodInfo.Invoke(obj, param.Parameters);
                Console.WriteLine("PASSED");
                results += new TestResults(1, 1);
            }
            catch (Exception e)
            {
                using var _ = new ConsoleColoring(ConsoleColor.Red);
                Console.WriteLine("FAILED");
                Console.WriteLine(e.InnerException?.Message);
                results += new TestResults(0, 1);
            }
        }

        if (this.Description is not null)
        {
            Console.WriteLine(this.Description);
        }
        return results;
    }
}