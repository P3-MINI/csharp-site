using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeliverySimulator
{
    enum DeliveryStatus
    {
        Waiting,
        Loading,
        Delivering
    }
    internal class DeliveryMan
    {
        public int Id { get; }
        public DeliveryStatus Status { get; private set; }
        public Point Position { get; private set; }

        private readonly Random _random;

        public DeliveryMan(int id, Point startPosition)
        {
            Id = id;
            Position = startPosition;
            Status = DeliveryStatus.Waiting;

            _random = new Random(id);
        }

        public void Run(Warehouse warehouse)
        {
            while (true)
            {
                Status = DeliveryStatus.Waiting;

                if (!warehouse.TakePackage())
                {
                    Thread.Sleep(100);
                    continue;
                }

                Status = DeliveryStatus.Loading;
                Thread.Sleep(200);

                Status = DeliveryStatus.Delivering;

                int targetX = 400;
                int speed = _random.Next(2, 6);

                while (Position.X < targetX)
                {
                    Position = new Point(
                        Position.X + speed,
                        Position.Y
                    );

                    Thread.Sleep(30);
                }

                Thread.Sleep(_random.Next(500, 1500));

                while (Position.X > 200)
                {
                    Position = new Point(
                        Position.X - speed,
                        Position.Y
                    );

                    Thread.Sleep(30);
                }
            }
        }
        private static int nextId = 1;
    }
}
