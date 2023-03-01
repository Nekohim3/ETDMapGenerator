using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace MapGeneratorTest.Utils
{
    public struct SKLine
    {
        public SKPoint Start { get; set; }
        public SKPoint End { get; set; }

        public double Length => Math.Sqrt(Math.Pow(Start.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2));

        public SKLine(SKPoint start, SKPoint end)
        {
            Start = start;
            End   = end;
        }
        public SKLine(SKPointI start, SKPointI end)
        {
            Start = start;
            End   = end;
        }
    }
    public struct SKLineI
    {
        public SKPointI Start { get; set; }
        public SKPointI End   { get; set; }

        public double Length => Math.Sqrt(Math.Pow(Start.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2));

        public SKLineI(SKPointI start, SKPointI end)
        {
            Start = start;
            End   = end;
        }
    }
}
