using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FractalsGenerator.Generators.MandelbrotSet.Implementations;

public sealed class ParallelGenerator(int maxIterations)
    : MandelbrotSetGenerator(maxIterations)
{
    protected override void PopulateImage(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;

        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; x++)
            {
                var a = (x - width / 2.0) * 4.0 / width;
                var b = (y - height / 2.0) * 4.0 / height;
                var iterations = Calculate(a, b);
                image[x, y] = GetColor(iterations);
            }
        });
    }
}