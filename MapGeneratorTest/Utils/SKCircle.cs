using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace MapGeneratorTest.Utils
{
    public struct SKCircleI
    {
        public int MidX   { get; set; }
        public int MidY   { get; set; }
        public int Radius { get; set; }

        public SKCircleI(int midX, int midY, int radius)
        {
            MidX   = midX;
            MidY   = midY;
            Radius = radius;
        }
    }

    public static class CircleExtension
    {
        public static bool IntersectsWith(this SKCircleI c, SKPointI p)
        {

        }
        public static bool IntersectsWith(this SKCircleI c, SKLineI l)
        {

        }

    }
}
