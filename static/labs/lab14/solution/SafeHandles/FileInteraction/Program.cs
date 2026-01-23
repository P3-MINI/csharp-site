using System.Drawing;
using System.Text;

namespace FileInteraction
{
    internal class Program
    {
        static void Main()
        {
            // Prepare
            string name1 = "text.txt";
            {
                using var fs = System.IO.File.Create(name1);
                fs.Write(Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet!"));
            }
            {
                var f = new MyFile(name1);
                string? output;
                do
                {
                    output = f.Read(1);
                    Console.Write(output);
                } while (!string.IsNullOrEmpty(output));
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("====================");
            Console.WriteLine();

            // Prepare
            string name2 = "values.csv";
            {
                using var fs = System.IO.File.Create(name2);
                for (int i = 0; i < 30; i++)
                {
                    double value = Random.Shared.NextDouble();
                    fs.Write(Encoding.UTF8.GetBytes($"{value:0.00000};"));
                }
            }
            {
                int n = 4;
                Console.WriteLine($"First {n} values of the file {name2}:");
                var f = new MyFile(name2);
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine(f.Read(7));
                    f.Read(1);
                }
            }
        }
    }
}
