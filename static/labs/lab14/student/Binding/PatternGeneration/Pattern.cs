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
    public class Pattern // : SafeHandleZeroOrMinusOneIsInvalid
    {
        public Image GetImage()
        {
            throw new NotImplementedException();
            // return Image.LoadPixelData<Rgb24>(content, width, height);
        }
    }
}
