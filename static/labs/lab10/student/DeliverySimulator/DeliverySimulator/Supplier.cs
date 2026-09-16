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
        public void Run(Warehouse warehouse)
        {
            Random random = new Random();

            while (true)
            {
                Thread.Sleep(random.Next(1000, 7000));

                int packages = random.Next(5, 21);
                warehouse.AddPackages(packages);
            }
        }
    }
}
