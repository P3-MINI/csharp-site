using System.Runtime.InteropServices;
using ImageFilters.Interop;

namespace ImageFilters;

/// <summary>Managed API over the imgfilter native library.</summary>
public static class ImgFilter
{
    public static int Add(int a, int b) => ImgFilterNative.Add(a, b);

    // The library owns the string: copy it, but do not free it
    public static string Version => Marshal.PtrToStringUTF8(ImgFilterNative.Version())!;

    // We own the string: copy it and give the memory back to the library
    public static string BuildInfo
    {
        get
        {
            nint ptr = ImgFilterNative.BuildInfo();
            if (ptr == 0)
                throw new OutOfMemoryException();
            try
            {
                return Marshal.PtrToStringUTF8(ptr)!;
            }
            finally
            {
                ImgFilterNative.Free(ptr);
            }
        }
    }

    public static string ErrorMessage(int code) => Marshal.PtrToStringUTF8(ImgFilterNative.ErrorMessage(code))!;

    public static void Invert(Image image) =>
        Check(ImgFilterNative.Invert(image.Pixels, (nuint)image.Pixels.Length, image.Info));

    public static void Grayscale(Image image) =>
        Check(ImgFilterNative.Grayscale(image.Pixels, (nuint)image.Pixels.Length, image.Info));

    public static ImageStats ComputeStats(Image image)
    {
        Check(ImgFilterNative.ComputeStats(image.Pixels, (nuint)image.Pixels.Length, image.Info, out ImageStats stats));
        return stats;
    }

    public static int LiveImages => ImgFilterNative.LiveImages();

    // Converts an error code returned by the library into an exception
    internal static void Check(int code)
    {
        if (code != 0)
            throw new InvalidOperationException($"imgfilter error {code}: {ErrorMessage(code)}");
    }
}
