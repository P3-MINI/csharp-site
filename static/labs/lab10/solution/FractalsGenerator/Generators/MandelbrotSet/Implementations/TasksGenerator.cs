using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FractalsGenerator.Generators.MandelbrotSet.Implementations;

public sealed class TasksGenerator(int maxIterations)
    : MandelbrotSetGenerator(maxIterations)
{
    protected override void PopulateImage(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;

        var taskCount = Environment.ProcessorCount;
        var rowsPerTask = height / taskCount;

        var tasks = new Task[taskCount];
        for (var t = 0; t < taskCount; t++)
        {
            var startRow = t * rowsPerTask;
            var endRow = (t == taskCount - 1) ? height : startRow + rowsPerTask;

            tasks[t] = Task.Run(() =>
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
        }

        Task.WaitAll(tasks);
    }
}