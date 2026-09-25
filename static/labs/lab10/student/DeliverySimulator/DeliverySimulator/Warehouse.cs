using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeliverySimulator
{
    internal class Warehouse
    {
        public int PackageCount { get; private set; }
        public void AddPackages(int count)
        {
            PackageCount += count;
        }
        public bool TakePackage()
        {
            if (PackageCount > 0)
            {
                Thread.Sleep(100); // Simulate the time taken to take a package WARNING: Do not remove this line,
                // removing it will reduce the chance of a race condition, but it will not eliminate it completely.
                PackageCount--;
                return true;
            }
            return false;
        }
        public Warehouse()
        {
            PackageCount = 0;
        }
    }
}
