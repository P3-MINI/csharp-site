using System.Runtime.InteropServices;

namespace ImageFilters.Interop;

/// <summary>Raw declarations of the functions exported by the imgfilter native library.</summary>
internal static unsafe partial class ImgFilterNative
{
    // imgfilter.dll (Windows), libimgfilter.so (Linux), libimgfilter.dylib (macOS)
    private const string Lib = "imgfilter";

    [LibraryImport(Lib, EntryPoint = "imgf_add")]
    internal static partial int Add(int a, int b);

    // Static string owned by the library - returned as a raw pointer, NOT as string
    [LibraryImport(Lib, EntryPoint = "imgf_version")]
    internal static partial nint Version();

    // Allocated with malloc - must be released with imgf_free
    [LibraryImport(Lib, EntryPoint = "imgf_build_info")]
    internal static partial nint BuildInfo();

    [LibraryImport(Lib, EntryPoint = "imgf_free")]
    internal static partial void Free(nint ptr);

    [LibraryImport(Lib, EntryPoint = "imgf_error_message")]
    internal static partial nint ErrorMessage(int code);

    [LibraryImport(Lib, EntryPoint = "imgf_invert")]
    internal static partial int Invert(Span<byte> pixels, nuint length, in ImageInfo info);

    [LibraryImport(Lib, EntryPoint = "imgf_grayscale")]
    internal static partial int Grayscale(Span<byte> pixels, nuint length, in ImageInfo info);

    [LibraryImport(Lib, EntryPoint = "imgf_compute_stats")]
    internal static partial int ComputeStats(ReadOnlySpan<byte> pixels, nuint length, in ImageInfo info,
        out ImageStats stats);

    [LibraryImport(Lib, EntryPoint = "imgf_image_create")]
    internal static partial SafeImageHandle ImageCreate(int width, int height, int channels);

    // Called only from SafeImageHandle.ReleaseHandle, hence the raw handle
    [LibraryImport(Lib, EntryPoint = "imgf_image_destroy")]
    internal static partial void ImageDestroy(nint image);

    [LibraryImport(Lib, EntryPoint = "imgf_image_info")]
    internal static partial ImageInfo ImageGetInfo(SafeImageHandle image);

    [LibraryImport(Lib, EntryPoint = "imgf_image_pixels")]
    internal static partial byte* ImageGetPixels(SafeImageHandle image);

    [LibraryImport(Lib, EntryPoint = "imgf_image_convolve3x3")]
    internal static partial int ImageConvolve3x3(SafeImageHandle image, ReadOnlySpan<int> kernel, int divisor,
        delegate* unmanaged[Cdecl]<int, int, void> progress);

    [LibraryImport(Lib, EntryPoint = "imgf_live_images")]
    internal static partial int LiveImages();
}
