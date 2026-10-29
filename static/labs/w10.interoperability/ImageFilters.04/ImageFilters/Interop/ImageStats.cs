using System.Runtime.InteropServices;

namespace ImageFilters.Interop;

/// <summary>Managed counterpart of <c>imgf_stats</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct ImageStats
{
    public byte Min;
    public byte Max;
    // 6 bytes of padding here - the same as in C, because double is 8-byte aligned
    public double Mean;

    public override readonly string ToString() => $"min = {Min}, max = {Max}, mean = {Mean:F2}";
}
