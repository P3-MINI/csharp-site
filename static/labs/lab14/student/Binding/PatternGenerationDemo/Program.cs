using PatternGeneration;
using SixLabors.ImageSharp.Formats.Png;

namespace PatternGenerationDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Pattern p = new Pattern();

            //p.Populate(points);
            {
                var s = File.OpenWrite("Image1.png");
                p.GetImage().Save(s, new PngEncoder());
            }

            //p.Enstripen(stripe);
            {
                var s = File.OpenWrite("Image2.png");
                p.GetImage().Save(s, new PngEncoder());
            }
        }
    }
}
