namespace MiniTestRunner;

/// <summary>
/// Represents the results of a test run, including passed and total test counts.
/// Provides summary output and supports aggregation via the '+' operator.
/// </summary>
/// <param name="Passed">The number of tests that passed.</param>
/// <param name="Total">The total number of tests executed.</param>
public readonly record struct TestResults(int Passed, int Total)
{
    /// <summary>
    /// Gets the number of failed tests.
    /// </summary>
    public int Failed => this.Total - this.Passed;

    /// <summary>
    /// Adds two <see cref="TestResults"/> instances together, combining their passed and total test counts.
    /// </summary>
    /// <param name="left">The first test result.</param>
    /// <param name="right">The second test result.</param>
    /// <returns>A new <see cref="TestResults"/> instance representing the combined results.</returns>
    public static TestResults operator +(TestResults left, TestResults right)
    {
        var passed = left.Passed + right.Passed;
        var run = left.Total + right.Total;
        return new TestResults(passed, run);
    }

    /// <summary>
    /// Prints a formatted summary of the test results to the console.
    /// </summary>
    public void Summarize()
    {
        Console.WriteLine("******************************");
        Console.WriteLine($"* Test passed: {this.Passed,5} / {this.Total,-5} *");
        Console.WriteLine($"* Failed:      {this.Failed,5}         *");
        Console.WriteLine("******************************");
    }
}