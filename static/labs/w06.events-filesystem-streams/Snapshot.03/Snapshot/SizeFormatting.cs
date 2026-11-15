using System.Globalization;

namespace Snapshot;

public static class SizeFormatting
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    /// <summary>
    /// Formats a number of bytes in a human-readable way, e.g. 1536 -> "1.5 KB".
    /// </summary>
    public static string ToHumanSize(this long bytes)
    {
        double size = bytes;
        int unit = 0;
        while (size >= 1024 && unit < Units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        // Invariant culture: always a dot as the decimal separator, regardless of system settings
        return unit == 0
            ? $"{bytes} B"
            : string.Create(CultureInfo.InvariantCulture, $"{size:0.0} {Units[unit]}");
    }
}
