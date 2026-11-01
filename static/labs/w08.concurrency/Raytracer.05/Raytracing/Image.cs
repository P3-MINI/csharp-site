using System.Text;

namespace Raytracing;

public class Image
{
    public int Width { get; }
    public int Height { get; }
    private Vector3[,] Pixels;

    public Vector3 this[int x, int y]
    {
        get => Pixels[x, y];
        set => Pixels[x, y] = value;
    }
    
    public Image(int width, int height)
    {
        Width = width;
        Height = height;
        Pixels = new Vector3[width, height];
    }

    public void SetRow(int y, Vector3[] row)
    {
        for (int x = 0; x < Width; x++)
        {
            Pixels[x, y] = row[x];
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        result.AppendLine("P3");
        result.AppendLine($"{Width} {Height}");
        result.AppendLine("255");
        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                var color = this[i, j];
                int r = Math.Clamp((int)(color.X * 255.99), 0, 255);
                int g = Math.Clamp((int)(color.Y * 255.99), 0, 255);
                int b = Math.Clamp((int)(color.Z * 255.99), 0, 255);
                result.AppendLine($"{r} {g} {b}");
            }
        }
        return result.ToString();
    }

    public void Save(string path)
    {
        File.WriteAllText(path, ToString());
    }

    public async Task SaveAsync(string path, CancellationToken token = default)
    {
        await File.WriteAllTextAsync(path, ToString(), token);
    }
}
