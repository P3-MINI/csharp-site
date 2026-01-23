using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatternGeneration
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Point(double x, double y)
    {
        public double x = x, y = y;
    }
}
