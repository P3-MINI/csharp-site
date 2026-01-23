using PatternGeneration;
using SixLabors.ImageSharp.Formats.Png;

namespace PatternGenerationDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Pattern p = new(800, 600);
            var pts = new PatternGeneration.Point[10];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = new(Random.Shared.NextDouble(), Random.Shared.NextDouble());
            p.Populate(pts);
            {
                var s = File.OpenWrite("Image1.png");
                p.GetImage().Save(s, new PngEncoder());
            }

            StripeSettings stripe = new StripeSettings();
            stripe.a = new(255, 0, 255);
            stripe.b = new(0, 0, 0);
            stripe.slope = 0.5;
            stripe.stripe_a_width = 0.1;
            stripe.stripe_b_width = 0.1;

            p.Enstripen(stripe);
            {
                var s = File.OpenWrite("Image2.png");
                p.GetImage().Save(s, new PngEncoder());
            }
        }
    }
}
