namespace Race;

class Program
{
    static void Main(string[] args)
    {
        List<Vehicle> vehicles = [new Bike("Romet"), new Car("Honda Civic", 1.5f), new Car("Toyota Yaris", 1.0f)];
        
        const float dt = 1.0f;
        for (float time = 0.0f; time < 4.0f; time += dt)
        {
            Console.WriteLine($"====== time: {time,5:F1}s ======");
            foreach (var vehicle in vehicles)
            {
                vehicle.Run(dt);
            }
            foreach (var vehicle in vehicles)
            {
                Console.WriteLine($"Vehicle {vehicle.Name}, Position {vehicle.Position}");
            }
        }
    }
}
