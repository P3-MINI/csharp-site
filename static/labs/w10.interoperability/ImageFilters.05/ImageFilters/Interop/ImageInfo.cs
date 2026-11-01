using System.Runtime.InteropServices;

namespace ImageFilters.Interop;

/// <summary>Managed counterpart of <c>imgf_info</c> - fields in the same order and of the same sizes.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ImageInfo(int width, int height, int channels)
{
    public readonly int Width = width;
    public readonly int Height = height;
    public readonly int Channels = channels;

    public override string ToString() => $"{Width}x{Height}x{Channels}";
}
