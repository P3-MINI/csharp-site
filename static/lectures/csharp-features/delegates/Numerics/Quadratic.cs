namespace Delegates;

public class Quadratic
{
    public double A { get; }
    public double B { get; }
    public double C { get; }
    
    public Quadratic(double a, double b, double c)
    {
        A = a;
        B = b;
        C = c;
    }
    
    public double Function(double x) => A * x * x + B * x + C;
    
    public double Derivative(double x) => 2 * A * x + B;
    
    public override string ToString() => $"f(x) = {A}x^2 + {B}x + {C}";
}