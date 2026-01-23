using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using PatternGeneration;

internal static partial class NativeMethods
{
    private const string LibName = "pg";
    [LibraryImport(LibName)]
    public static partial nint pattern_init(int width, int height);
    [LibraryImport(LibName)]
    public static partial void pattern_populate(Pattern pattern, [In] PatternGeneration.Point[] points, int n);
    [LibraryImport(LibName)]
    public static partial void pattern_enstripen(Pattern pattern, StripeSettings settings);
    [LibraryImport(LibName)]
    public static partial void pattern_destroy(Pattern pattern);
}
