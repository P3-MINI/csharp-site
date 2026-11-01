using System.Text;
using ImageFilters.Interop;

namespace ImageFilters;

/// <summary>
/// 8-bit RGB image. Pixels are stored row by row, 3 bytes per pixel: R, G, B, R, G, B, ...
/// </summary>
public sealed class Image
{
    public const int Channels = 3;

    public int Width { get; }
    public int Height { get; }
    public byte[] Pixels { get; }

    public ImageInfo Info => new(Width, Height, Channels);

    public Image(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Width = width;
        Height = height;
        Pixels = new byte[width * height * Channels];
    }

    public Image Clone()
    {
        var copy = new Image(Width, Height);
        Pixels.CopyTo(copy.Pixels, 0);
        return copy;
    }

    /// <summary>Loads a PPM image: text (P3) or binary (P6) with maximum value 255.</summary>
    public static Image Load(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        int position = 0;

        string magic = NextToken(data, ref position);
        if (magic is not ("P3" or "P6"))
            throw new InvalidDataException($"Unsupported format '{magic}', expected P3 or P6");
        int width = int.Parse(NextToken(data, ref position));
        int height = int.Parse(NextToken(data, ref position));
        int maxValue = int.Parse(NextToken(data, ref position));
        if (maxValue != 255)
            throw new InvalidDataException("Only images with maximum value 255 are supported");

        var image = new Image(width, height);
        if (magic == "P6")
        {
            // Binary data starts after a single whitespace character
            position++;
            data.AsSpan(position, image.Pixels.Length).CopyTo(image.Pixels);
        }
        else
        {
            for (int i = 0; i < image.Pixels.Length; i++)
                image.Pixels[i] = byte.Parse(NextToken(data, ref position));
        }
        return image;
    }

    /// <summary>Saves the image in the binary PPM format (P6).</summary>
    public void Save(string path)
    {
        using FileStream stream = File.Create(path);
        stream.Write(Encoding.ASCII.GetBytes($"P6\n{Width} {Height}\n255\n"));
        stream.Write(Pixels);
    }

    /// <summary>Generates a colorful test image with smooth gradients and sharp edges.</summary>
    public static Image CreateTestPattern(int width, int height)
    {
        var image = new Image(width, height);
        float radius = 0.3f * height;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / width, v = (float)y / height;

                // Background: hue changes from left to right, brightness from top to bottom
                (float r, float g, float b) = Hue(u);
                float brightness = 1.0f - 0.6f * v;
                (r, g, b) = (r * brightness, g * brightness, b * brightness);

                // Left: white disc with a black ring
                float d1 = Distance(x, y, 0.27f * width, 0.42f * height);
                if (d1 < radius)
                    (r, g, b) = d1 > radius - 6 ? (0f, 0f, 0f) : (1f, 1f, 1f);

                // Right: red and blue concentric rings
                float d2 = Distance(x, y, 0.73f * width, 0.42f * height);
                if (d2 < radius)
                    (r, g, b) = (int)(d2 / 8) % 2 == 0 ? (0.9f, 0.1f, 0.1f) : (0.1f, 0.2f, 0.9f);

                // Bottom: black and white checkerboard
                if (v > 0.85f)
                    (r, g, b) = (x / 8 + y / 8) % 2 == 0 ? (0f, 0f, 0f) : (1f, 1f, 1f);

                int i = (y * width + x) * Channels;
                image.Pixels[i] = (byte)(255 * r);
                image.Pixels[i + 1] = (byte)(255 * g);
                image.Pixels[i + 2] = (byte)(255 * b);
            }
        }
        return image;
    }

    private static float Distance(float x1, float y1, float x2, float y2) =>
        MathF.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

    private static (float R, float G, float B) Hue(float h) => (
        Math.Clamp(MathF.Abs(h * 6 - 3) - 1, 0, 1),
        Math.Clamp(2 - MathF.Abs(h * 6 - 2), 0, 1),
        Math.Clamp(2 - MathF.Abs(h * 6 - 4), 0, 1));

    private static string NextToken(byte[] data, ref int position)
    {
        // Skip whitespace and comments (from '#' to the end of the line)
        while (position < data.Length)
        {
            if (data[position] == '#')
                while (position < data.Length && data[position] != '\n')
                    position++;
            else if (char.IsWhiteSpace((char)data[position]))
                position++;
            else
                break;
        }

        int start = position;
        while (position < data.Length && !char.IsWhiteSpace((char)data[position]))
            position++;
        if (start == position)
            throw new EndOfStreamException("Unexpected end of the PPM file");
        return Encoding.ASCII.GetString(data, start, position - start);
    }
}
