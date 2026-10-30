using System.Runtime.InteropServices;

namespace ImageFilters;

/// <summary>One RGB pixel - 3 bytes, no padding.</summary>
[StructLayout(LayoutKind.Sequential)]
public record struct Rgb24(byte R, byte G, byte B);
