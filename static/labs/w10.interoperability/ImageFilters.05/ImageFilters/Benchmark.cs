using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ImageFilters;

public static class Benchmark
{
    private static int s_sink;

    public static void Run()
    {
        Console.WriteLine("=== Benchmark ===");
#if DEBUG
        Console.WriteLine("Warning: Debug build - use 'dotnet run -c Release' for meaningful results");
#endif
        Image image = Image.CreateTestPattern(1920, 1080);
        Console.WriteLine($"Image: {image.Width}x{image.Height}");

        // Grayscale of a gray image costs the same, so the same buffer can be reused
        Image gray = image.Clone();
        Measure("Grayscale: safe (byte[])", 20, () => ManagedFilters.GrayscaleSafe(gray.Pixels));
        Measure("Grayscale: Span<Rgb24>", 20, () => ManagedFilters.GrayscaleSpan(gray.Pixels));
        Measure("Grayscale: unsafe (pointers)", 20, () => ManagedFilters.GrayscaleUnsafe(gray.Pixels));
        Measure("Grayscale: native (C)", 20, () => ImgFilter.Grayscale(gray));

        Measure("Box blur: safe (byte[])", 5, () => ManagedFilters.BoxBlur(image, useUnsafe: false));
        Measure("Box blur: unsafe (pointers)", 5, () => ManagedFilters.BoxBlur(image, useUnsafe: true));
        using (NativeImage nativeImage = NativeImage.FromImage(image))
        {
            Measure("Box blur: native (C)", 5, () => nativeImage.Convolve3x3([1, 1, 1, 1, 1, 1, 1, 1, 1], 9));
        }

        // The cost of the call itself: a managed method vs P/Invoke into native code
        const int calls = 10_000_000;
        double managedMs = Measure("10M calls: managed Add", 3, () =>
        {
            int sum = 0;
            for (int i = 0; i < calls; i++)
                sum = ManagedAdd(sum, 1);
            s_sink = sum;
        });
        double nativeMs = Measure("10M calls: native imgf_add", 3, () =>
        {
            int sum = 0;
            for (int i = 0; i < calls; i++)
                sum = ImgFilter.Add(sum, 1);
            s_sink = sum;
        });
        Console.WriteLine($"One call: managed {managedMs * 1e6 / calls:F2} ns, native {nativeMs * 1e6 / calls:F2} ns");
    }

    /// <summary>Returns the average time of one iteration in milliseconds.</summary>
    private static double Measure(string name, int iterations, Action action)
    {
        // Warm-up: JIT compilation, loading the native library, filling CPU caches
        action();

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            action();
        stopwatch.Stop();

        double milliseconds = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Console.WriteLine($"{name,-30} {milliseconds,8:F2} ms");
        return milliseconds;
    }

    // NoInlining: we want to measure a real call, as with the native function
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ManagedAdd(int a, int b) => a + b;
}
