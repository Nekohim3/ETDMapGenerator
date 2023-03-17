using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace MapGeneratorTest.Utils.Geometry
{
    public struct SKLine
    {
        public SKPoint Start { get; set; }
        public SKPoint End { get; set; }

        public float Length => (float)Math.Sqrt(Math.Pow(End.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2));

        public SKLine(SKPoint start, SKPoint end)
        {
            Start = start;
            End = end;
        }

        public SKLine(float sx, float sy, float ex, float ey)
        {
            Start = new SKPoint(sx, sy);
            End = new SKPoint(ex, ey);
        }

        public static implicit operator SKLine(SKLineI l) => new SKLine(new SKPoint(l.Start.X, l.Start.Y), new SKPoint(l.End.X, l.End.Y));
    }
    public struct SKLineI
    {
        public SKPointI Start { get; set; }
        public SKPointI End { get; set; }

        public float Length => (float)Math.Sqrt(Math.Pow(End.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2));

        public SKLineI(SKPointI start, SKPointI end)
        {
            Start = start;
            End = end;
        }

        public SKLineI(int sx, int sy, int ex, int ey)
        {
            Start = new SKPointI(sx, sy);
            End = new SKPointI(ex, ey);
        }

        public static implicit operator SKLine(SKLineI l) => new SKLine(l.Start.X, l.Start.Y, l.End.X, l.End.Y);
    }
}
