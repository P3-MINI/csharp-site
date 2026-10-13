using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeliverySimulator
{
    internal class Supplier
    {
        public void Run(Warehouse warehouse, Action<string> log)
        {
            Random random = new Random();

            while (true)
            {
                Thread.Sleep(random.Next(1000, 8000));

                int packages = random.Next(5, 16);
                warehouse.AddPackages(packages);

                log(
                    $"Supplier: delivered {packages} packages. " +
                    $"Warehouse: {warehouse.PackageCount}"
                );
            }
        }
    }
}
