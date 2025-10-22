namespace MiniTestRunner;

/// <summary>
/// Represents a test class containing multiple test methods and optional setup/teardown logic.
/// Responsible for executing all test methods and summarizing the results.
/// </summary>
/// <param name="Type">The <see cref="Type"/> of the test class.</param>
/// <param name="TestMethods">A list of test methods to execute.</param>
/// <param name="BeforeEach">An optional setup method to run before each test.</param>
/// <param name="AfterEach">An optional teardown method to run after each test.</param>
/// <param name="Description">An optional description of the test class.</param>
public sealed record TestClass(
    Type Type,
    List<TestMethod> TestMethods,
    TestSetup? BeforeEach,
    TestSetup? AfterEach,
    string? Description)
{
    /// <summary>
    /// Executes all test methods in the class, applying setup and teardown logic if provided.
    /// Results are printed to the console and summarized at the end.
    /// </summary>
    /// <returns>A <see cref="TestResults"/> object representing the aggregated results of all test executions.</returns>
    public TestResults Run()
    {
        var results = new TestResults();
        var instance = Activator.CreateInstance(Type);
        if (instance is null)
        {
            using var _ = new ConsoleColoring(ConsoleColor.Yellow);
            Console.WriteLine($"Failed to create a class {Type.FullName} instance.");
            return results;
        }

        Console.WriteLine($"Running tests from class {this.Type.FullName}...");

        if (this.Description is not null)
        {
            Console.WriteLine(this.Description);
        }

        foreach (var testMethod in this.TestMethods
                     .OrderBy(tm => tm.Priority)
                     .ThenBy(tm => tm.MethodInfo.Name))
        {
            this.BeforeEach?.Run(instance);
            results += testMethod.Run(instance);
            this.AfterEach?.Run(instance);
        }

        results.Summarize();
        return results;
    }
}