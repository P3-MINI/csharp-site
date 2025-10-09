namespace Race;

public class Bike : Vehicle
{    
    public Bike(string name) : base(name) {}
    public override float Run(float dt)
    {
        Console.WriteLine($"Bike.Run({dt})"); // We can skip implementation if not for the output.
        return base.Run(dt);
    }
}