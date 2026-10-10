namespace Raytracing;

class Program
{
    static void Main(string[] args)
    {
        Image image = new Image(1920, 1080);
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                float r = (float) i / image.Width;
                float g = (float) j / image.Height;
                float b = MathF.Sin(2 * MathF.PI * ((float) i * j / image.Width / image.Height));
                image[i, j] = new Vector3(r, g, b);
            }
        }

        Console.WriteLine(image);
    }
}