namespace VariantStack;

class Program
{
    static void Main(string[] args)
    {
        // Covariance
        var carStack = new Stack<Car>();
        carStack.Push(new Car());
        carStack.Push(new Car());
        IPoppable<Car> vehiclePoppable = carStack;
        WashVehicles(vehiclePoppable);
        
        void WashVehicles(IPoppable<Vehicle> vehicles)
        {
            while (vehicles.Count > 0)
            {
                Vehicle vehicle = vehicles.Pop();
                Console.WriteLine($"Washing {vehicle}");
            }
        }
        
        // Contravariance
        var vehiclesStack = new Stack<Vehicle>();
        vehiclesStack.Push(new Car());
        vehiclesStack.Push(new Bike());
        IPushable<Vehicle> carPushable = vehiclesStack;
        DeliverCars(carPushable, 2);

        void DeliverCars(IPushable<Car> cars, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine("Adding car to IPushable");
                cars.Push(new Car());
            }
        }
    }
}