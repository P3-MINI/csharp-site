using System.Diagnostics;

namespace Raytracing;

class Program
{
    static async Task Main(string[] args)
    {
        Scene world = CreateScene();

        Camera cam = new Camera
        {
            ImageWidth = 640,
            SamplesPerPixel = 64,
            Position = new Vector3(0, 0, 1),
            Target = new Vector3(0, 0, -1),
        };

        int threadCount = Environment.ProcessorCount;
        Console.WriteLine($"Rendering {cam.ImageWidth}x{cam.ImageHeight}, {cam.SamplesPerPixel} samples per pixel, {threadCount} logical processors");

        Stopwatch stopwatch = Stopwatch.StartNew();
        Image image = cam.Render(world);
        double sequential = stopwatch.Elapsed.TotalSeconds;
        Report("Sequential", sequential, sequential);

        stopwatch.Restart();
        image = cam.RenderWithThreads(world, threadCount);
        Report($"Threads ({threadCount})", stopwatch.Elapsed.TotalSeconds, sequential);

        stopwatch.Restart();
        image = await cam.RenderWithTasksAsync(world);
        Report("Tasks (one per row)", stopwatch.Elapsed.TotalSeconds, sequential);

        image.Save("image.ppm");
        Console.WriteLine("Saved image.ppm");
    }

    static void Report(string name, double seconds, double baseline)
    {
        Console.WriteLine($"{name,-24}{seconds,6:F2} s   speedup {baseline / seconds,4:F1}x");
    }

    static Scene CreateScene()
    {
        Scene world = new Scene();
        // Add a sky sphere
        world.Add(new Sphere(new Vector3(0, 0, 0), 1000.0f, new EmissiveMaterial(new Vector3(0.5f, 0.7f, 1.0f))));

        // Add 3 spheres
        world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Vector3(1, 0, 0))));
        world.Add(new Sphere(new Vector3(-1.1f, 0, -1), 0.5f, new Lambertian(new Vector3(0, 0, 1))));
        world.Add(new Sphere(new Vector3(1.1f, 0, -1), 0.5f, new Lambertian(new Vector3(1, 1, 0))));

        // Add a green ground plane
        world.Add(new Plane(new Vector3(0, -0.5f, 0), new Vector3(0, 1, 0), new Lambertian(new Vector3(0.2f, 0.8f, 0.2f))));

        return world;
    }
}
