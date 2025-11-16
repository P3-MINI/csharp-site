using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Benchmark;

public static class Program
{
    public static void Main()
    {
        var methods = typeof(Program).GetMethods(BindingFlags.Public | BindingFlags.Static);
        
        foreach (var method in methods)
        {
            IEnumerable<BenchmarkAttribute> attributes = method
                .GetCustomAttributes<BenchmarkAttribute>();

            foreach (var attribute in attributes)
            {
                Action? action = (Action?)Delegate.CreateDelegate(typeof(Action), method, false);
                if (action is null)
                {
                    Console.WriteLine($"Method {method.Name} needs to take no parameters, and return void.");
                    continue;
                }

                uint rep = attribute.Repetitions;
                Console.WriteLine($"Found benchmark: {method.Name}");
                Console.WriteLine($"Calling it {attribute.Repetitions} times");
                Stopwatch sw = Stopwatch.StartNew();
                for (uint i = 0; i < rep; i++)
                {
                    action();
                }
                sw.Stop();
                double micro = sw.Elapsed.TotalNanoseconds / rep / 1000;
                Console.WriteLine($"{method.Name} time: {micro:0}μs");
            }
        }
    }

    [Benchmark]
    public static void StringAdd()
    {
        string _ = "";
        for (int i = 0; i < 10_000; i++)
        {
            _ += 'a';
        }
    }
    
    [Benchmark(10000)]
    public static void StringBuilder()
    {
        StringBuilder a = new StringBuilder();
        for (int i = 0; i < 10_000; i++)
        {
            a.Append('a');
        }

        string _ = a.ToString();
    }
    
    [Benchmark()]
    public static void StringJoin()
    {
        string _ = string.Join(string.Empty, Enumerable.Repeat('a', 10_000));
    }
}