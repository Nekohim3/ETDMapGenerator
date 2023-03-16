using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace MapGeneratorTest.Utils.Geometry
{
    public enum CirclePos
    {
        LeftTop,
        RightTop,
        TopRight,
        BottomRight,
    }
    public class SKExpandRoundRectI
    {
        public SKRectI InnerRect { get; set; }
        public int ExpandXY { get; set; }

        public SKExpandRoundRectI(SKRectI innerRect, int expandXY)
        {
            InnerRect = innerRect;
            ExpandXY = expandXY;
        }

        public SKCircleI GetCircle(CirclePos cp) => cp switch
                                                    {
                                                        CirclePos.LeftTop     => new SKCircleI(InnerRect.Left,  InnerRect.Top,    ExpandXY),
                                                        CirclePos.RightTop    => new SKCircleI(InnerRect.Right, InnerRect.Top,    ExpandXY),
                                                        CirclePos.TopRight    => new SKCircleI(InnerRect.Left,  InnerRect.Bottom, ExpandXY),
                                                        CirclePos.BottomRight => new SKCircleI(InnerRect.Right, InnerRect.Bottom, ExpandXY),
                                                        _                     => throw new ArgumentOutOfRangeException(nameof(cp), cp, null)
                                                    };

        public List<SKCircleI> GetCircles() => new() {new(InnerRect.Left, InnerRect.Top, ExpandXY), new(InnerRect.Right, InnerRect.Top, ExpandXY), new(InnerRect.Right, InnerRect.Bottom, ExpandXY), new(InnerRect.Left, InnerRect.Bottom, ExpandXY)};
    }

    //public static class SKExpandRoundRectExtension
    //{
    //    public static bool IntersectsWith(this SKExpandRoundRectI rr, SKRectI r)
    //    {
    //        var rects = new List<SKRectI> { rr.InnerRect.ExpandLeft(rr.ExpandXY), rr.InnerRect.ExpandRight(rr.ExpandXY), rr.InnerRect.ExpandTop(rr.ExpandXY), rr.InnerRect.ExpandBottom(rr.ExpandXY) };
    //        if (rects.Any(r.IntersectsWith))
    //            return true;
    //        var circles
    //    }
    //}
}
