namespace Delegates;

public static class Program
{
    public static void Main()
    {
        var quadratic = new Quadratic(1.0, -7.0, 10.0);
        
        Numerics.Function function = quadratic.Function;
        Numerics.Function derivative = quadratic.Derivative;
        
        double root = Numerics.NewtonRootFinding(function, derivative);
        
        Console.WriteLine($"Root of {quadratic}: {root:F2}");
    }
}