using System.Runtime.InteropServices;

namespace ImageFilters.Interop;

/// <summary>Raw declarations of the functions exported by the imgfilter native library.</summary>
internal static partial class ImgFilterNative
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
}
