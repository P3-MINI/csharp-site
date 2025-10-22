namespace Hiding;

public class Base
{
    public int Member = 0;
    public string Method() => "Base.Method";
    public virtual string VirtualMethod() => "Base.VirtualMethod";
}

public class Hider : Base
{
    public int Member = 1;
    public string Method() => "Hider.Method";
    public string VirtualMethod() => "Hider.VirtualMethod";
}

public class Overrider : Base
{
    public override string VirtualMethod() => "Overrider.VirtualMethod";
}

class Program
{
    static void Main(string[] args)
    {
        Hider hider = new Hider();
        Base baseHider = hider;
        
        Overrider overrider = new Overrider();
        Base baseOverrider = overrider;
        
        Console.WriteLine(hider.Method()); // Hider.Method
        Console.WriteLine(hider.VirtualMethod()); // Hider.VirtualMethod
        Console.WriteLine(baseHider.Method()); // Base.Method
        Console.WriteLine(baseHider.VirtualMethod()); // Base.VirtualMethod
        
        Console.WriteLine(overrider.Method()); // Base.Method
        Console.WriteLine(overrider.VirtualMethod()); // Overrider.VirtualMethod
        Console.WriteLine(baseOverrider.Method()); // Base.Method
        Console.WriteLine(baseOverrider.VirtualMethod()); // Overrider.VirtualMethod
    }
}
