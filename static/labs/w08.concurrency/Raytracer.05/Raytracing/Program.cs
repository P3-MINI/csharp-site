using System.Diagnostics;

namespace Raytracing;

class Program
{
    static async Task Main(string[] args)
    {
        Scene world = CreateScene();

        if (args is ["benchmark"])
        {
            await BenchmarkAsync(world);
            return;
        }

        using CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // do not terminate the process, stop the rendering gracefully
            cts.Cancel();
        };

        if (args is ["animation"])
        {
            await RenderAnimationAsync(world, cts.Token);
        }
        else
        {
            RenderImage(world, cts.Token);
        }
    }

    static void RenderImage(Scene world, CancellationToken token)
    {
        Camera cam = new Camera
        {
            ImageWidth = 1280,
            SamplesPerPixel = 100,
            Position = new Vector3(0, 0, 1),
            Target = new Vector3(0, 0, -1),
        };

        Console.WriteLine($"Rendering {cam.ImageWidth}x{cam.ImageHeight}, {cam.SamplesPerPixel} samples per pixel. Press Ctrl+C to cancel.");

        Image image = new Image(cam.ImageWidth, cam.ImageHeight);
        ParallelOptions options = new ParallelOptions { CancellationToken = token };
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            cam.RenderParallel(world, image, options, new ProgressBar(cam.ImageHeight));
            Console.WriteLine($"Rendered in {stopwatch.Elapsed.TotalSeconds:F2} s");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine();
            Console.WriteLine($"Cancelled after {stopwatch.Elapsed.TotalSeconds:F2} s, saving the partial image");
        }

        image.Save("image.ppm");
        Console.WriteLine("Saved image.ppm");
    }

    static async Task RenderAnimationAsync(Scene world, CancellationToken token)
    {
        const int frameCount = 36;
        Directory.CreateDirectory("frames");
        Console.WriteLine($"Rendering {frameCount} frames. Press Ctrl+C to cancel.");
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Start all frames at once - the thread pool runs as many of them in parallel as it can
        Task<Frame>[] renderTasks = new Task<Frame>[frameCount];
        for (int k = 0; k < frameCount; k++)
        {
            int index = k;
            renderTasks[k] = Task.Run(() => RenderFrame(world, index, frameCount, token), token);
        }

        int saved = 0;
        try
        {
            // Frames come in the order in which they finish, not in the order they were started
            await foreach (Task<Frame> finished in Task.WhenEach(renderTasks))
            {
                Frame frame = await finished; // already completed: gives the result or rethrows
                string path = Path.Combine("frames", $"frame_{frame.Index:D3}.ppm");

                // No token here: never leave a half-written file behind
                await frame.Image.SaveAsync(path);
                saved++;
                Console.WriteLine($"[{stopwatch.Elapsed.TotalSeconds,5:F2} s] saved {path} ({saved}/{frameCount})");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Cancelled, saved {saved} of {frameCount} frames");
        }
    }

    static Frame RenderFrame(Scene world, int index, int frameCount, CancellationToken token)
    {
        // Every frame needs its own camera - Camera is mutable and must not be shared
        float angle = 2 * MathF.PI * index / frameCount;
        Vector3 target = new Vector3(0, 0, -1);
        Camera cam = new Camera
        {
            ImageWidth = 480,
            SamplesPerPixel = 32,
            Fov = 60,
            Target = target,
            Position = target + new Vector3(2.5f * MathF.Sin(angle), 0.5f, 2.5f * MathF.Cos(angle)),
        };

        return new Frame(index, cam.Render(world, token));
    }

    static async Task BenchmarkAsync(Scene world)
    {
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

        int[] degrees = [1, 2, 4, 8, threadCount];
        foreach (int degree in degrees)
        {
            stopwatch.Restart();
            image = cam.RenderParallel(world, new ParallelOptions { MaxDegreeOfParallelism = degree });
            Report($"Parallel.For ({degree})", stopwatch.Elapsed.TotalSeconds, sequential);
        }

        stopwatch.Restart();
        image = cam.RenderParallel(world, new ParallelOptions(), new ProgressBar(cam.ImageHeight));
        Report("Parallel.For + progress", stopwatch.Elapsed.TotalSeconds, sequential);

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
