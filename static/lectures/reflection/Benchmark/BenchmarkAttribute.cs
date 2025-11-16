namespace Benchmark;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class BenchmarkAttribute : Attribute
{
    public uint Repetitions { get; }

    public BenchmarkAttribute(uint repetitions = 100)
    {
        Repetitions = repetitions;
    }
}