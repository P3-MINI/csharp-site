using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FractalsGenerator.Generators.MandelbrotSet.Implementations;

public sealed class MultiThreadGenerator(int maxIterations)
    : MandelbrotSetGenerator(maxIterations)
{
    protected override void PopulateImage(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;
        var threadCount = Environment.ProcessorCount;
        var threads = new Thread[threadCount];
        var rowsPerThread = height / threadCount;

        for (var t = 0; t < threadCount; t++)
        {
            var startRow = t * rowsPerThread;
            var endRow = (t == threadCount - 1) ? height : startRow + rowsPerThread;

            threads[t] = new Thread(() =>
            {
                for (var y = startRow; y < endRow; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var a = (x - width / 2.0) * 4.0 / width;
                        var b = (y - height / 2.0) * 4.0 / height;
                        var iterations = Calculate(a, b);
                        image[x, y] = GetColor(iterations);
                    }
                }
            });
            threads[t].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
}