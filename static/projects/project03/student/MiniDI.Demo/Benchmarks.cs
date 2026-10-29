namespace MiniDI.Demo;

/// <summary>
/// Compares reflection-based (<c>AddFromAssembly</c>) and generated (<c>AddGenerated</c>) registrations.
/// Run each variant in its own process, in Release:
/// <code>
/// dotnet run -c Release --project MiniDI.Demo -- bench reflection
/// dotnet run -c Release --project MiniDI.Demo -- bench generated
/// </code>
/// </summary>
public static class Benchmarks
{
    public static Task RunAsync(string variant)
    {
        // TODO (Part 4): for the given variant ("reflection" or "generated") measure and print:
        //  (a) the time of registration + BuildServiceProvider(),
        //  (b) the time of the first resolution of IOrderService in a new scope,
        //  (c) the mean time of one resolution of PriceCalculator from a scope
        //      (at least 1,000,000 resolutions, after a warm-up).
        throw new NotImplementedException();
    }
}
