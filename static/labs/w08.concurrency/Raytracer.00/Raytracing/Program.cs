using System.Diagnostics;

namespace Raytracing;

class Program
{
    static void Main(string[] args)
    {
        Scene world = CreateScene();

        Camera cam = new Camera
        {
            ImageWidth = 640,
            SamplesPerPixel = 64,
            Position = new Vector3(0, 0, 1),
            Target = new Vector3(0, 0, -1),
        };

        Console.WriteLine($"Rendering {cam.ImageWidth}x{cam.ImageHeight}, {cam.SamplesPerPixel} samples per pixel");

        Stopwatch stopwatch = Stopwatch.StartNew();
        Image image = cam.Render(world);
        stopwatch.Stop();
        Console.WriteLine($"Sequential: {stopwatch.Elapsed.TotalSeconds:F2} s");

        image.Save("image.ppm");
        Console.WriteLine("Saved image.ppm");
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
