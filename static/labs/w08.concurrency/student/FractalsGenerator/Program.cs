using FractalsGenerator.Generators.MandelbrotSet;
using FractalsGenerator.Generators.MandelbrotSet.Implementations;
using System.Diagnostics;

namespace FractalsGenerator;

public sealed class Program
{
    static void Main(string[] args)
    {
        var maxIterations = args.Length > 0 ? int.Parse(args[0]) : 10_000;
        var width = args.Length > 1 ? int.Parse(args[1]) : 800;
        var height = args.Length > 2 ? int.Parse(args[2]) : 800;

        var generators = new MandelbrotSetGenerator[]
        {
            new SingleThreadGenerator(maxIterations),
            //new MultiThreadGenerator(maxIterations),
            //new TasksGenerator(maxIterations),
            //new ParallelGenerator(maxIterations),
        };

        foreach (var generator in generators)
        {
            var stopwatch = Stopwatch.StartNew();
            generator.Generate(width, height);
            stopwatch.Stop();
            Console.WriteLine($"Elapsed time {stopwatch.ElapsedMilliseconds} ms.");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}