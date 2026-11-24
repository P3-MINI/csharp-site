using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer
{
    public sealed class RenderFrame
    {
        public ColorRGB[,] Pixels { get; }
        public int SamplesPerPixel { get; }

        public RenderFrame(ColorRGB[,] pixels, int samplesPerPixel)
        {
            Pixels = pixels;
            SamplesPerPixel = samplesPerPixel;
        }
    }
}
