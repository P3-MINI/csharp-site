using System.Text;

namespace Raytracing;

public class Image
{
    public int Width { get; }
    public int Height { get; }
    private Vector3[] Pixels { get; }

    public Vector3 this[int x, int y]
    {
        get => Pixels[y * Width + x];
        set => Pixels[y * Width + x] = value;
    }
    
    public Image(int width, int height)
    {
        Width = width;
        Height = height;
        Pixels = new Vector3[width * height];
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        result.AppendLine("P3");
        result.AppendLine($"{Width} {Height}");
        result.AppendLine("255");
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                var color = this[j, i];
                int r = Math.Clamp((int)(color.X * 255.99), 0, 255);
                int g = Math.Clamp((int)(color.Y * 255.99), 0, 255);
                int b = Math.Clamp((int)(color.Z * 255.99), 0, 255);
                result.AppendLine($"{r} {g} {b}");
            }
        }
        return result.ToString();
    }
}