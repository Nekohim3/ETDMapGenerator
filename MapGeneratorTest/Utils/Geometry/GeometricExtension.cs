using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronSoftware.Drawing;
using ReactiveUI;
using SkiaSharp;
using Avalonia.Controls.Shapes;

namespace MapGeneratorTest.Utils.Geometry;

//public enum AngleDirection
//{
//    N = 0,
//    NE = 1,
//    E = 2,
//    SE = 3,
//    S = 4,
//    SW = 5,
//    W = 6,
//    NW = 7
//}

public static class Extensions
{
    #region Rectangle

    public static SKRectI        ExpandLeft(this        SKRectI r, int left)                                 => r with {Left = r.Left     - left};
    public static SKRectI        ExpandTop(this         SKRectI r, int top)                                  => r with {Top = r.Top       - top};
    public static SKRectI        ExpandRight(this       SKRectI r, int right)                                => r with {Right = r.Right   + right};
    public static SKRectI        ExpandBottom(this      SKRectI r, int bottom)                               => r with {Bottom = r.Bottom + bottom};
    public static SKRectI        ExpandWidth(this       SKRectI r, int width)                                => r.ExpandLeft(width).ExpandRight(width);
    public static SKRectI        ExpandHeight(this      SKRectI r, int height)                               => r.ExpandTop(height).ExpandBottom(height);
    public static SKRectI        ExpandAll(this         SKRectI r, int n)                                    => r.ExpandWidth(n).ExpandHeight(n);
    public static SKRectI        Expand(this            SKRectI r, int left, int top, int right, int bottom) => r.ExpandLeft(left).ExpandTop(top).ExpandRight(right).ExpandBottom(bottom);
    public static SKPoint        GetMidPoint(this       SKRectI r)                => new(r.MidX, r.MidY);
    public static float          GetDistanceToRect(this SKRectI r, SKRectI other) => (new SKPoint(r.MidX, r.MidY) - new SKPoint(other.MidX, other.MidY)).Length;
    public static List<SKLineI>  GetRectLines(this      SKRectI r) => new() { new SKLineI(r.Left, r.Top, r.Left, r.Bottom), new SKLineI(r.Left, r.Top, r.Right, r.Top), new SKLineI(r.Right, r.Top, r.Right, r.Bottom), new SKLineI(r.Left, r.Bottom, r.Right, r.Bottom) };
    public static List<SKPointI> GetRectPoints(this     SKRectI r) => new() { new SKPointI(r.Left, r.Top), new SKPointI(r.Right, r.Top), new SKPointI(r.Right, r.Bottom), new SKPointI(r.Left, r.Bottom) };

    public static int GetRectCoord(this SKRectI r, RectDirection d) => r.GetRectCoord((int)d);
    public static int GetRectCoord(this SKRectI r, int d) => d switch
                                                                       {
                                                                           0   => r.Left,
                                                                           1    => r.Top,
                                                                           2  => r.Right,
                                                                           3 => r.Bottom,
                                                                           _                    => throw new ArgumentOutOfRangeException(nameof(d), d, null)
                                                                       };

    #region RoundRectangle

    public static bool IntersectsWith(this SKExpandRoundRectI rr, SKRectI r)
    {
        var rects = new List<SKRectI> { rr.InnerRect.ExpandLeft(rr.ExpandXY), rr.InnerRect.ExpandRight(rr.ExpandXY), rr.InnerRect.ExpandTop(rr.ExpandXY), rr.InnerRect.ExpandBottom(rr.ExpandXY) };
        if (rects.Any(r.IntersectsWith))
            return true;
        var circles = rr.GetCircles();
        return circles.Any(_ => r.GetRectLines().Any(_.IntersectsWith));
    }

    #endregion

    #endregion

    #region Circle


    public static bool IntersectsWith(this SKCircleI c, SKLineI l)
    {
        var lineType = l.GetLineType();
        switch (lineType)
        {
            case LineDirection.Vertical:
                for (var i = l.Start.Y < l.End.Y ? l.Start.Y : l.End.Y; l.Start.Y < l.End.Y ? i < l.End.Y : i < l.Start.Y; i -= l.Start.Y.CompareTo(l.End.Y))
                    if (c.Point.DistanceTo(new SKPointI(l.Start.X, i)) <= c.Radius)
                        return true;
                break;
            case LineDirection.Horizontal:
                for (var i = l.Start.X < l.End.X ? l.Start.X : l.End.X; l.Start.X < l.End.X ? i < l.End.X : i < l.Start.X; i -= l.Start.X.CompareTo(l.End.X))
                    if (c.Point.DistanceTo(new SKPointI(i, l.Start.Y)) <= c.Radius)
                        return true;
                break;
            case LineDirection.Diagonal:
                throw new Exception();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return false;
    }

    #endregion

    #region Line

    public static LineDirection GetLineType(this SKLineI l) => l.Start.X == l.End.X ? LineDirection.Vertical : l.Start.Y == l.End.Y ? LineDirection.Horizontal : LineDirection.Diagonal;
    public static SKPoint IsIntersect(this SKLineI l, SKLineI other, float tolerance = 0.00005f)
    {
        bool IsInsideLine(SKLineI line, SKPoint p, float tol)
        {
            float x = p.X, y = p.Y;

            var leftX = line.Start.X;
            var leftY = line.Start.Y;

            var rightX = line.End.X;
            var rightY = line.End.Y;

            return ((x.IsGreaterThanOrEqual(leftX, tol) && x.IsLessThanOrEqual(rightX, tol))
                    || (x.IsGreaterThanOrEqual(rightX, tol) && x.IsLessThanOrEqual(leftX, tol)))
                   && ((y.IsGreaterThanOrEqual(leftY, tol) && y.IsLessThanOrEqual(rightY, tol))
                       || (y.IsGreaterThanOrEqual(rightY, tol) && y.IsLessThanOrEqual(leftY, tol)));
        }

        if (l.Start == other.Start && l.End == other.End)
            throw new Exception("Both lines are the same.");

        if (l.Start.X.CompareTo(other.Start.X) > 0)
            (l, other) = (other, l);
        else if (l.Start.X.CompareTo(other.Start.X) == 0)
        {
            if (l.Start.Y.CompareTo(other.Start.Y) > 0)
                (l, other) = (other, l);
        }

        float x1 = l.Start.X, y1 = l.Start.Y;
        float x2 = l.End.X, y2 = l.End.Y;
        float x3 = other.Start.X, y3 = other.Start.Y;
        float x4 = other.End.X, y4 = other.End.Y;

        if (x1.IsEqual(x2) && x3.IsEqual(x4) && x1.IsEqual(x3))
        {
            var firstIntersection = new SKPoint(x3, y3);
            if (IsInsideLine(l, firstIntersection, tolerance) &&
                IsInsideLine(other, firstIntersection, tolerance))
                return new SKPoint(x3, y3);
        }

        if (y1.IsEqual(y2) && y3.IsEqual(y4) && y1.IsEqual(y3))
        {
            var firstIntersection = new SKPoint(x3, y3);
            if (IsInsideLine(l, firstIntersection, tolerance) &&
                IsInsideLine(other, firstIntersection, tolerance))
                return new SKPoint(x3, y3);
        }

        if (x1.IsEqual(x2) && x3.IsEqual(x4))
            return default;

        if (y1.IsEqual(y2) && y3.IsEqual(y4))
            return default;

        float x, y;
        if (x1.IsEqual(x2))
        {
            var m2 = (y4 - y3) / (x4 - x3);
            var c2 = -m2 * x3 + y3;
            x = x1;
            y = c2 + m2 * x1;
        }
        else if (x3.IsEqual(x4))
        {
            var m1 = (y2 - y1) / (x2 - x1);
            var c1 = -m1 * x1 + y1;
            x = x3;
            y = c1 + m1 * x3;
        }
        else
        {
            var m1 = (y2 - y1) / (x2 - x1);
            var c1 = -m1 * x1 + y1;
            var m2 = (y4 - y3) / (x4 - x3);
            var c2 = -m2 * x3 + y3;
            x = (c1 - c2) / (m2 - m1);
            y = c2 + m2 * x;

            if (!((-m1 * x + y).IsEqual(c1)
                  && (-m2 * x + y).IsEqual(c2)))
                return default;
        }

        var result = new SKPoint(x, y);

        if (IsInsideLine(l, result, tolerance) &&
            IsInsideLine(other, result, tolerance))
            return result;

        return default;
    }

    #endregion

    #region Point

    public static SKPoint OffsetPoint(this SKPoint  p, float    x, float y) => p with { X = p.X + x, Y = p.Y + y };
    public static float   DistanceTo(this  SKPointI p, SKPointI pp) => new SKLineI(p, pp).Length;

    #endregion

    #region Other

    public static bool IsOpposite(this (RectDirection line, RectDirection side)                       a, (RectDirection line, RectDirection side)                       b) => (int)a.line % 2 == (int)b.line % 2 && a.line != b.line;
    public static bool IsOpposite(this ((RectDirection e, int i) line, (RectDirection e, int i) side) a, ((RectDirection e, int i) line, (RectDirection e, int i) side) b) => a.line.i % 2 == b.line.i % 2 && a.line.i != b.line.i;
    public static ((RectDirection e, int i) line, (RectDirection e, int i) side) GetRectIntersectType(this SKRectI r, SKLineI l)
    {
        var intersections = r.GetRectLines().Select(_ => _.IsIntersect(l)).ToList();
        var point         = intersections.FirstOrDefault(_ => _ != default);
        if (point == default)
            throw new Exception();
        var index = intersections.IndexOf(point);
        return (((RectDirection)index, index), index % 2 == 0 ? r.MidY > point.Y ? (RectDirection.Top, 1) : (RectDirection.Bottom, 3) : r.MidX > point.X ? (RectDirection.Left, 0) : (RectDirection.Right, 2));
    }

    #endregion

    #region Numbers

    #region Float

    public static bool IsLessThan(this float a, float b, float tolerance = 0.00005f) => a - b < -tolerance;
    public static bool IsLessThanOrEqual(this float a, float b, float tolerance = 0.00005f) => a - b < -tolerance || Math.Abs(a - b) < tolerance;
    public static bool IsGreaterThan(this float a, float b, float tolerance = 0.00005f) => a - b > tolerance;
    public static bool IsGreaterThanOrEqual(this float a, float b, float tolerance = 0.00005f) => a - b > tolerance || Math.Abs(a - b) < tolerance;
    public static bool IsEqual(this float a, float b, float tolerance = 0.00005f) => Math.Abs(a - b) < tolerance;

    #endregion

    #region Double

    public static bool IsLessThan(this           double a, double b, double tolerance = 0.00005f) => a - b < -tolerance;
    public static bool IsLessThanOrEqual(this    double a, double b, double tolerance = 0.00005f) => a - b < -tolerance || Math.Abs(a - b) < tolerance;
    public static bool IsGreaterThan(this        double a, double b, double tolerance = 0.00005f) => a - b > tolerance;
    public static bool IsGreaterThanOrEqual(this double a, double b, double tolerance = 0.00005f) => a - b > tolerance || Math.Abs(a - b) < tolerance;
    public static bool IsEqual(this              double a, double b, double tolerance = 0.00005f) => Math.Abs(a - b) < tolerance;

    #endregion

    #endregion
    
}

//public static float GetAngleToRect(this SKRectI  r, SKRectI  other) => GetAngleTo(r.GetMidPoint(), other.GetMidPoint());
//public static float GetAngleTo(this SKPoint p, SKPoint other)
//{
//    var angle           = (float)(Math.Atan2(other.Y - p.Y, other.X - p.Y) * 180.0f / Math.PI);
//    if(angle < 0) angle += 360;
//    return angle;
//}
//public static SKPointI GetMidPointI(this SKRectI r)                   => new(r.MidX, r.MidY);

//public static SKPointI GetXYStepsCount(this   SKRectI r, SKRectI other) => new SKPointI(Math.Abs(r.MidX - other.MidX), Math.Abs(r.MidY - other.MidX));
//public static int      GetStepsCount(this     SKRectI r, SKRectI other) => GetXYStepsCount(r, other).X + GetXYStepsCount(r, other).Y;
//public class CropRectangleExt : CropRectangle
//{
//    public int Left   => X;
//    public int Top    => Y;
//    public int Right  => X + Width;
//    public int Bottom => Y + Height;

//    public CropRectangleExt SetLeft(int left)
//    {
//        Width += Left - left;
//        X     =  left;
//        return this;
//    }

//    public CropRectangleExt SetTop(int top)
//    {
//        Height += Top - top;
//        Y      =  top;
//        return this;
//    }

//    public CropRectangleExt SetRight(int right)
//    {
//        Width += right - Right;
//        return this;
//    }

//    public CropRectangleExt SetBottom(int bottom)
//    {
//        Height += bottom - Bottom;
//        return this;
//    }

//    public CropRectangleExt ExpandLeft(int left)
//    {
//        X     -= left;
//        Width += left;
//        return this;
//    }

//    public CropRectangleExt ExpandTop(int top)
//    {
//        Y      -= top;
//        Height += top;
//        return this;
//    }

//    public CropRectangleExt ExpandRight(int right)
//    {
//        Width += right;
//        return this;
//    }

//    public CropRectangleExt ExpandBottom(int bottom)
//    {
//        Height += bottom;
//        return this;
//    }

//    public CropRectangleExt ExpandAll(int n) => Expand(n, n, n, n);

//    public CropRectangleExt Expand(int left, int top, int right, int bottom)
//    {
//        ExpandLeft(left);
//        ExpandTop(top);
//        ExpandRight(right);
//        ExpandBottom(bottom);
//        return this;
//    }

//    public List<SKPointI> GetAllPoints() => new() {new SKPointI(Left, Top), new SKPointI(Right, Top), new SKPointI(Left, Bottom), new SKPointI(Right, Bottom)};

//    public SKPoint GetMidPoint() => new(X + (float) Width / 2, Y + (float) Height / 2);

//    public CropRectangleExt()
//    {
//    }

//    public CropRectangleExt(CropRectangle r) : base(r.X, r.Y, r.Width, r.Height, r.Units)
//    {
//    }

//    public CropRectangleExt(int x, int y, int width, int height, MeasurementUnits units = MeasurementUnits.Pixels) : base(x, y, width, height, units)
//    {
//    }
//}
//public static class RectangleExtension
//{
//    public static CropRectangleExt SetLeftNew(this   CropRectangleExt r, int left)   => new CropRectangleExt(r).SetLeft(left);
//    public static CropRectangleExt SetTopNew(this    CropRectangleExt r, int top)    => new CropRectangleExt(r).SetTop(top);
//    public static CropRectangleExt SetRightNew(this  CropRectangleExt r, int right)  => new CropRectangleExt(r).SetRight(right);
//    public static CropRectangleExt SetBottomNew(this CropRectangleExt r, int bottom) => new CropRectangleExt(r).SetBottom(bottom);

//    public static CropRectangleExt ExpandLeftNew(this   CropRectangleExt r, int left)                                 => new CropRectangleExt(r).ExpandLeft(left);
//    public static CropRectangleExt ExpandTopNew(this    CropRectangleExt r, int top)                                  => new CropRectangleExt(r).ExpandTop(top);
//    public static CropRectangleExt ExpandRightNew(this  CropRectangleExt r, int right)                                => new CropRectangleExt(r).ExpandRight(right);
//    public static CropRectangleExt ExpandBottomNew(this CropRectangleExt r, int bottom)                               => new CropRectangleExt(r).ExpandBottom(bottom);
//    public static CropRectangleExt ExpandAllNew(this    CropRectangleExt r, int n)                                    => new CropRectangleExt(r).ExpandAll(n);
//    public static CropRectangleExt ExpandNew(this       CropRectangleExt r, int left, int top, int right, int bottom) => new CropRectangleExt(r).Expand(left, top, right, bottom);

//    public static bool IsIntersectWith(this       CropRectangleExt r, CropRectangleExt other) => ((Rectangle) r).IntersectsWith((Rectangle) other);
//    public static bool IsPointInRect(this         CropRectangleExt r, SKPointI         p)     => p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom;
//    public static bool IsPointInRect(this         SKPointI         p, CropRectangleExt r)                      => p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom;
//    public static bool IsPointInRect(this         CropRectangleExt r, SKPoint          p)     => p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom;
//    public static bool IsAllRectPointsInRect(this CropRectangleExt r, CropRectangleExt other) => other.GetAllPoints().All(r.IsPointInRect);
//    public static bool IsAnyRectPointsInRect(this CropRectangleExt r, CropRectangleExt other) => other.GetAllPoints().Any(r.IsPointInRect);

//    public static SKPoint OffsetPoint(this SKPoint p, float x, float y) => p with { X = p.X + x, Y = p.Y + y };
//    public static SKPointI OffsetPoint(this SKPointI p, int x, int y) => p with { X = p.X + x, Y = p.Y + y };

//    //public static CropRectangle SetLeft(this   CropRectangle r, int left) => r with { X = left, Width = r.Width + r.Left   - left };
//    //public static CropRectangle SetTop(this    CropRectangle r, int top) => r with { Y =  top, Height = r.Height + r.Top - top };
//    //public static CropRectangle SetRight(this  CropRectangle r, int right) => r with { Width = r.Width + right - r.Right };
//    //public static CropRectangle SetBottom(this CropRectangle r, int bottom) => r with { Height = r.Height + bottom - r.Bottom };

//    //public static CropRectangle   ExpandLeft(this   CropRectangle r, int x)                      => r with {X = r.X           - x, Width = r.Width   + x};
//    //public static CropRectangle   ExpandTop(this    CropRectangle r, int y)                      => r with {Y = r.Y           - y, Height = r.Height + y};
//    //public static CropRectangle   ExpandRight(this  CropRectangle r, int w)                      => r with {Width = r.Width   + w};
//    //public static CropRectangle   ExpandBottom(this CropRectangle r, int h)                      => r with {Height = r.Height + h};
//    //public static CropRectangle   Expand(this       CropRectangle r, int x)                      => r.Expand(x, x, x, x);
//    //public static CropRectangle   Expand(this CropRectangle r, int x, int y, int w, int h) => r with {X = r.X - x, Y = r.Y - y, Width = r.Width + x + w, Height = r.Height + y + h};
//    //public static List<Point> GetAllPoints(this CropRectangle r) => new() { new Point(r.Left, r.Top), new Point(r.Right, r.Top), new Point(r.Left, r.Bottom), new Point(r.Right, r.Bottom) };
//    //public static PointF      GetMidPoint(this CropRectangle r) => new PointF(r.X + (float)r.Width / 2, r.Y + (float)r.Height / 2);
//}
//public class Rect
//{
//    private int _x;
//    public int X
//    {
//        get => _x;
//        set
//        {
//            _x = value;
//            _left = value;
//        }
//    }
//    private int _y;
//    public int Y
//    {
//        get => _y;
//        set
//        {
//            _y = value;
//            _top = value;
//        }
//    }
//    private int _width;
//    public int Width
//    {
//        get => _width;
//        set
//        {
//            _width = value;
//            _right = _x + value;
//        }
//    }
//    private int _height;
//    public int Height
//    {
//        get => _height;
//        set
//        {
//            _height = value; 
//            _bottom = _y + value;
//        }
//    }
//    private int _left;
//    public int Left
//    {
//        get => _left;
//        set
//        {
//            _width += _left - value;
//            _left  =  value;
//            _x     =  value;
//        }
//    }
//    private int _top;
//    public int Top
//    {
//        get => _top;
//        set
//        {
//            _height += _top - value;
//            _top    =  value;
//            _y      =  value;
//        }
//    }
//    private int _right;
//    public int Right
//    {
//        get => _right;
//        set
//        {
//            _width += value - _right;
//            _right =  value;
//        }
//    }
//    private int _bottom;
//    public int Bottom
//    {
//        get => _bottom;
//        set
//        {
//            _height += value - _bottom;
//            _bottom = value; 
//        }
//    }

//    public Rect(int x, int y, int width, int height)
//    {
//        X      = x;
//        Y      = y;
//        Width  = width;
//        Height = height;
//    }

//    public Rect(Rectangle r)
//    {
//        X      = r.X;
//        Y      = r.Y;
//        Width  = r.Width;
//        Height = r.Height;
//    }

//    public Rect Expand(int l, int t, int r, int b)
//    {
//        var rect = new Rect(X, Y, Width, Height);
//        rect.Left   -= l;
//        rect.Top    -= t;
//        rect.Right  += r;
//        rect.Bottom += b;
//        return rect;
//    }

//    public Rect Expand(int w, int h)
//    {
//        var rect = new Rect(X, Y, Width, Height);
//        rect.Left   -= w;
//        rect.Top    -= h;
//        return rect;
//    }

//    public Rect Expand(int n) => Expand(n, n, n, n);

//    public List<Point> GetAllPoints() => new() { new Point(Left, Top), new Point(Right, Top), new Point(Left, Bottom), new Point(Right, Bottom) };

//    public Rectangle GetRectangle() => new(X, Y, Width, Height);
