using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using SkiaSharp;

namespace MapGeneratorTest.Utils.Geometry
{
    public class SKCircleI
    {
        private int _midX;
        public int MidX
        {
            get => _midX;
            set
            {
                _midX = value;
                _point = _point with { X = value };
            }
        }

        private int _midY;
        public int MidY
        {
            get => _midY;
            set
            {
                _midY = value;
                _point = _point with { Y = value };
            }
        }

        private SKPointI _point;
        public SKPointI Point
        {
            get => _point;
            set
            {
                _point = value;
                MidX = value.X;
                MidY = value.Y;
            }
        }

        public int Radius { get; set; }


        public SKCircleI(int midX, int midY, int radius)
        {
            MidX = midX;
            MidY = midY;
            Radius = radius;
        }

        public SKCircleI(SKPointI p, int radius)
        {
            Point = p;
            Radius = radius;
        }
    }
}
