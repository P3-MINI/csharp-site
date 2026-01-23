using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatternGeneration
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StripeSettings
    {
        public Rgb24 a, b;
        public double stripe_a_width, stripe_b_width;
        public double slope;
    }
}
