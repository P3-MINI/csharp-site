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
}
