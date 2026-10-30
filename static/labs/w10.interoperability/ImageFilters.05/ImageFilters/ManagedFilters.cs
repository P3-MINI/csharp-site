using System.Runtime.InteropServices;

namespace ImageFilters;

/// <summary>Filters implemented in C# - the same algorithms as in imgfilter.c.</summary>
public static class ManagedFilters
{
    // 1. Safe code: array indexing, every access is bounds-checked
    public static void GrayscaleSafe(byte[] pixels)
    {
        for (int i = 0; i + 2 < pixels.Length; i += 3)
        {
            byte gray = (byte)((77 * pixels[i] + 150 * pixels[i + 1] + 29 * pixels[i + 2]) >> 8);
            pixels[i] = gray;
            pixels[i + 1] = gray;
            pixels[i + 2] = gray;
        }
    }

    // 2. Span<T>: bytes reinterpreted as RGB pixels and modified by reference
    public static void GrayscaleSpan(Span<byte> pixels)
    {
        foreach (ref Rgb24 p in MemoryMarshal.Cast<byte, Rgb24>(pixels))
        {
            byte gray = (byte)((77 * p.R + 150 * p.G + 29 * p.B) >> 8);
            p = new Rgb24(gray, gray, gray);
        }
    }

    // 3. Unsafe code: pointer arithmetic, no bounds checks
    public static unsafe void GrayscaleUnsafe(Span<byte> pixels)
    {
        fixed (byte* start = pixels)
        {
            byte* end = start + pixels.Length - pixels.Length % 3;
            for (byte* p = start; p < end; p += 3)
            {
                byte gray = (byte)((77 * p[0] + 150 * p[1] + 29 * p[2]) >> 8);
                p[0] = gray;
                p[1] = gray;
                p[2] = gray;
            }
        }
    }

    public static Image BoxBlur(Image source, bool useUnsafe)
    {
        // Small, short-lived buffer: allocated on the stack, not on the GC heap
        Span<int> kernel = stackalloc int[9];
        kernel.Fill(1);

        var result = new Image(source.Width, source.Height);
        if (useUnsafe)
            Convolve3x3Unsafe(source, result, kernel, 9);
        else
            Convolve3x3Safe(source, result, kernel, 9);
        return result;
    }

    public static void Convolve3x3Safe(Image source, Image destination, ReadOnlySpan<int> kernel, int divisor)
    {
        Validate(source, destination, kernel, divisor);
        int w = source.Width, h = source.Height, stride = w * Image.Channels;
        byte[] src = source.Pixels, dst = destination.Pixels;

        for (int y = 0; y < h; y++)
        {
            // Offsets of the neighbouring rows, clamped to the image edges
            int above = Math.Max(y - 1, 0) * stride, row = y * stride, below = Math.Min(y + 1, h - 1) * stride;
            for (int x = 0; x < w; x++)
            {
                // Offsets of the neighbouring columns, clamped to the image edges
                int left = Math.Max(x - 1, 0) * 3, center = x * 3, right = Math.Min(x + 1, w - 1) * 3;
                for (int c = 0; c < 3; c++)
                {
                    int sum =
                        kernel[0] * src[above + left + c] + kernel[1] * src[above + center + c] + kernel[2] * src[above + right + c] +
                        kernel[3] * src[row + left + c] + kernel[4] * src[row + center + c] + kernel[5] * src[row + right + c] +
                        kernel[6] * src[below + left + c] + kernel[7] * src[below + center + c] + kernel[8] * src[below + right + c];
                    dst[row + center + c] = (byte)Math.Clamp(sum / divisor, 0, 255);
                }
            }
        }
    }

    public static unsafe void Convolve3x3Unsafe(Image source, Image destination, ReadOnlySpan<int> kernel, int divisor)
    {
        // No bounds checks below - validate everything up front
        Validate(source, destination, kernel, divisor);
        int w = source.Width, h = source.Height, stride = w * Image.Channels;

        fixed (byte* src = source.Pixels)
        fixed (byte* dst = destination.Pixels)
        fixed (int* k = kernel)
        {
            for (int y = 0; y < h; y++)
            {
                byte* above = src + Math.Max(y - 1, 0) * stride;
                byte* row = src + y * stride;
                byte* below = src + Math.Min(y + 1, h - 1) * stride;
                byte* output = dst + y * stride;
                for (int x = 0; x < w; x++)
                {
                    int left = Math.Max(x - 1, 0) * 3, center = x * 3, right = Math.Min(x + 1, w - 1) * 3;
                    for (int c = 0; c < 3; c++)
                    {
                        int sum =
                            k[0] * above[left + c] + k[1] * above[center + c] + k[2] * above[right + c] +
                            k[3] * row[left + c] + k[4] * row[center + c] + k[5] * row[right + c] +
                            k[6] * below[left + c] + k[7] * below[center + c] + k[8] * below[right + c];
                        output[center + c] = (byte)Math.Clamp(sum / divisor, 0, 255);
                    }
                }
            }
        }
    }

    private static void Validate(Image source, Image destination, ReadOnlySpan<int> kernel, int divisor)
    {
        if (source.Width != destination.Width || source.Height != destination.Height)
            throw new ArgumentException("Source and destination images must have the same size");
        ArgumentOutOfRangeException.ThrowIfNotEqual(kernel.Length, 9);
        ArgumentOutOfRangeException.ThrowIfZero(divisor);
    }
}
