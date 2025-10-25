namespace Delegates;

public static class Numerics
{
    public delegate double Function(double x);
    
    public static double NewtonRootFinding(Function f, Function df, double x0 = 0, double eps = 1e-6)
    {
        double x;
        double xn = x0;
        
        do
        {
            x = xn;
            xn = x - f(x) / df(x);
        } while (Math.Abs(x - xn) >= eps);
        
        return xn;
    }
}