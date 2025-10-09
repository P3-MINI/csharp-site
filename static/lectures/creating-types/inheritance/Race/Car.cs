namespace Race;

public class Car : Vehicle
{
    public override float Speed { get; protected set; } = 0.0f;
    public virtual float Acceleration { get; }
    
    public Car(string name, float acceleration) : base(name) => Acceleration = acceleration;
    public override float Run(float dt)
    {
        Console.WriteLine($"Car.Run({dt})");
        Position += dt * Speed;
        Speed += dt * Acceleration;
        return Position;
    }
}