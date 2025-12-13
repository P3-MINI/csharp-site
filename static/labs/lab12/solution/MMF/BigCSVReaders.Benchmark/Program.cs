using System.Diagnostics;
using BigCsvReaders;

public sealed class Program
{
    static void Main(string[] args)
    {
        int rowsCnt = 100_000;
        int colsCnt = 100;

        string path = "benchmark.csv";

        try {
            GenerateLargeCsv(path, rowsCnt, colsCnt);
            Console.WriteLine("File is ready");

            using (var streamReader = new StreamBigCsvReader(path))
            {
                Console.WriteLine("StreamReader");
                Benchmark(streamReader, rowsCnt, colsCnt);
            }
                
            
            using (var streamReader = new MmfBigCsvReader(path))
            {
                Console.WriteLine("MmfReader");
                Benchmark(streamReader, rowsCnt, colsCnt);
            }   
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }


    static void Benchmark(BigCsvReader reader, int rowsCnt, int colsCnt)
    {
        var rnd = new Random(12345);

        var stopwatch = Stopwatch.StartNew();

        for (int i=0; i < rowsCnt; i++)
        {
            int row = rnd.Next(rowsCnt);
            int col = rnd.Next(colsCnt);

            var _ = reader[row, col];
        }

        stopwatch.Stop();
        Console.WriteLine($"Elapsed time {stopwatch.ElapsedMilliseconds} ms.");
    }


    static void GenerateLargeCsv(string path, int rows, int cols, int maxRndValue=1000)
    {
        using var writer = new StreamWriter(path);
        var rnd = new Random(12345);

        for (int row=0; row < rows; row++)
        {
            for (int col=0; col < cols-1; col++)
            {
                writer.Write($"{rnd.Next(maxRndValue)},");
            }

            writer.WriteLine(rnd.Next(maxRndValue).ToString());
        }
    }
}