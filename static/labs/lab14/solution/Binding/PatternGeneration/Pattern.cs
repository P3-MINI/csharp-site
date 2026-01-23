using Microsoft.Win32.SafeHandles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatternGeneration
{
    public class Pattern : SafeHandleZeroOrMinusOneIsInvalid
    {
        public Pattern(int width, int height) : base(true)
        {
            handle = NativeMethods.pattern_init(width, height);
        }

        public void Populate(PatternGeneration.Point[] points)
        {
            NativeMethods.pattern_populate(this, points, points.Length);
        }
        public void Enstripen(StripeSettings settings)
        {
            NativeMethods.pattern_enstripen(this, settings);
        }
        public Image GetImage()
        {
            var dimensions = new int[2];
            Marshal.Copy(handle, dimensions, 0, dimensions.Length);
            var content = new byte[dimensions[0]*dimensions[1]*3];
            Marshal.Copy(handle + sizeof(int) * 2, content, 0, content.Length);
            return Image.LoadPixelData<Rgb24>(content, dimensions[0], dimensions[1]);
        }
        protected override bool ReleaseHandle()
        {
            NativeMethods.pattern_destroy(this);
            return true;
        }
    }
}
