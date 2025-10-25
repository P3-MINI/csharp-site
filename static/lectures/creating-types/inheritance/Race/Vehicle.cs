namespace Race;

public class Vehicle
{
    public float Position { get; protected set; } = 0;
    public virtual float Speed { get; protected set; } = 1.0f;
    public string Name { get; }
    
    public Vehicle(string name) => Name = name;
    public virtual float Run(float dt)
    {
        Console.WriteLine($"Vehicle.Run({dt})");
        return (Position = Position + dt * Speed);
    }
}