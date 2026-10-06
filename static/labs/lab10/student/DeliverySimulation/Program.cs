using DeliverySimulator;
using System.Drawing;

namespace DeliverySimulation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Delivery Simulator!");
            Console.WriteLine();

            Warehouse warehouse = new Warehouse();
            Supplier supplier = new Supplier();

            List<DeliveryMan> deliveryMen = new List<DeliveryMan>();

            for (int i = 0; i < 20; i++)
            {
                deliveryMen.Add(
                    new DeliveryMan(i, new Point(200, 20 + i * 15))
                );
            }

            // Start delivery man threads
            foreach (DeliveryMan dm in deliveryMen)
            {
                Thread t = new Thread(() => dm.Run(warehouse, Log))
                {
                    IsBackground = true
                };

                t.Start();
            }

            // Start supplier thread
            Thread supplierThread = new Thread(() => supplier.Run(warehouse, Log))
            {
                IsBackground = true
            };

            supplierThread.Start();

            // Keep the main thread alive
            Console.ReadLine();
        }
        static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}