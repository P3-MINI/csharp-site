using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FractalsGenerator.Generators.MandelbrotSet;

public abstract class MandelbrotSetGenerator(int maxIterations)
{
    public int MaxIterations { get; } = maxIterations;

    public void Generate(int width, int height, string? path = null)
    {
        using var image = new Image<Rgba32>(width, height);

        PopulateImage(image);

        var filename = $"{GetType().Name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        path = path is null
            ? filename
            : Path.Combine(path, filename);

        Console.WriteLine($"Saving file to: {path}");
        image.Save(path);
    }

    protected abstract void PopulateImage(Image<Rgba32> image);

    protected Color GetColor(int iterations)
    {
        if (iterations == MaxIterations)
            return Color.Black;

        var c = (byte)(255 - iterations * 255 / MaxIterations);
        return Color.FromRgb(c, c, c);
    }

    protected int Calculate(double a, double b)
    {
        double x = 0, y = 0;
        var iterations = 0;

        while (x * x + y * y <= 4 && iterations < MaxIterations)
        {
            var tempX = x * x - y * y + a;
            y = 2 * x * y + b;
            x = tempX;
            iterations++;
        }

        return iterations;
    }
}
