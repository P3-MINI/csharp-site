using System.Drawing;
using System.Drawing.Imaging;

namespace tasks;

public sealed class Task02 : IExecutable
{
    public void Execute(string[] args)
    {
        var desktopPath = Environment
            .GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        var width = 1000;
        var height = 800;
        var rows = 3;
        var cols = 4;

        var stars = CreateStarryGrid(width, height, rows, cols);

        stars.DrawAndSaveOnBitmap(
            background: Color.White,
            foreground: Color.Blue,
            width, height,
            Path.Combine(desktopPath, $"starry_grid_{rows}_by_{cols}.png")
        );

        var starCount = 200;

        stars = CreateStarryNight(width, height, starCount);

        stars.DrawAndSaveOnBitmap(
            background: Color.Black,
            foreground: Color.White,
            width, height,
            Path.Combine(desktopPath, $"starry_night_{starCount}.png")
        );
    }

    public static List<Star> CreateStarryGrid(int width, int height, int rows, int cols)
    {
        var minPoints = 3;

        var outerRadius = Math.Min(width / cols, height / rows) * 0.4f;
        var innerRadius = outerRadius * 0.5f;

        var stars = new List<Star>(capacity: rows * cols);

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                var index = row * cols + col;
                var points = minPoints + index;

                var cellWidth = width / (float)cols;
                var cellHeight = height / (float)rows;
                var center = new PointF(
                    x: col * cellWidth + cellWidth / 2,
                    y: row * cellHeight + cellHeight / 2
                );

                var star = new Star(center, outerRadius, innerRadius, points);
                stars.Add(star);
            }
        }

        return stars;
    }

    public static List<Star> CreateStarryNight(int width, int height, int starCount)
    {
        var stars = new List<Star>(capacity: starCount);
        var random = new Random(Seed: 2137);

        for (var i = 0; i < starCount; i++)
        {
            var center = new PointF(random.Next(width), random.Next(height));
            var outerR = random.Next(8, 16);
            var innerR = outerR * 0.5f;
            var points = random.Next(3, 8);
            var star = new Star(center, outerR, innerR, points);

            stars.Add(star);
        }

        return stars;
    }
}

public record Star(
    PointF Center,
    float OuterRadius,
    float InnerRadius,
    int Points
);

public static class StarExtensions
{
    public static void DrawAndSaveOnBitmap(
        this IEnumerable<Star> stars,
        Color background,
        Color foreground,
        int width,
        int height,
        string outputPath)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        using var pen = new Pen(foreground);

        graphics.Clear(background);

        foreach (var star in stars)
        {
            graphics.DrawStar(pen, star);
        }

        bitmap.Save(outputPath, ImageFormat.Png);

        Console.WriteLine($"The file {outputPath} has been saved...");
    }
}

static class GraphicsExtensions
{
    public static void DrawStar(this Graphics g, Pen pen, Star star)
    {
        var theta = -Math.PI / 2.0;
        var step = Math.PI / star.Points;
        var pts = new PointF[star.Points * 2 + 1];

        for (var i = 0; i < star.Points * 2 + 1; i++)
        {
            var r = i % 2 == 0
                ? star.OuterRadius
                : star.InnerRadius;

            pts[i] = new PointF(
                star.Center.X + (float)(r * Math.Cos(theta)),
                star.Center.Y + (float)(r * Math.Sin(theta))
            );

            theta += step;
        }

        g.DrawPolygon(pen, pts);
    }
}