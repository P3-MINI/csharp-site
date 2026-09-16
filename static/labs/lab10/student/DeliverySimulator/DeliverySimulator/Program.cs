using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeliverySimulator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
                Thread t = new Thread(() => dm.Run(warehouse))
                {
                    IsBackground = true
                };

                t.Start();
            }

            // Start supplier thread
            Thread supplierThread = new Thread(() => supplier.Run(warehouse))
            {
                IsBackground = true
            };
            supplierThread.Start();

            Application.Run(new Form1(warehouse, supplier, deliveryMen));

        }
    }
}
