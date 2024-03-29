﻿using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronSoftware.Drawing;
using ReactiveUI;
using SkiaSharp;

namespace MapGeneratorTest.Utils;

public enum AngleDirection
{
    N,
    NE,
    E,
    SE,
    S,
    SW,
    W,
    NW
}
public static class RectangleExtension
{
    public static SKRectI ExpandLeft(this   SKRectI r, int left)   => r with { Left = r.Left    - left };
    public static SKRectI ExpandTop(this    SKRectI r, int top)    => r with { Top = r.Top      - top };
    public static SKRectI ExpandRight(this  SKRectI r, int right)  => r with { Right = r.Right  + right};
    public static SKRectI ExpandBottom(this SKRectI r, int bottom) => r with {Bottom = r.Bottom + bottom};

    public static SKRectI ExpandWidth(this  SKRectI r, int width)                                => r.ExpandLeft(width).ExpandRight(width);
    public static SKRectI ExpandHeight(this SKRectI r, int height)                               => r.ExpandTop(height).ExpandBottom(height);
    public static SKRectI ExpandAll(this    SKRectI r, int n)                                    => r.ExpandWidth(n).ExpandHeight(n);
    public static SKRectI Expand(this       SKRectI r, int left, int top, int right, int bottom) => r.ExpandLeft(left).ExpandTop(top).ExpandRight(right).ExpandBottom(bottom);

    public static SKLineI GetTopLine(this SKRectI r) => new(new SKPointI(r.Left, r.Top), new SKPointI(r.Right, r.Top));

    public static SKRectI Multiple(this SKRectI r, int n) => r with {Left = r.Left * n, Top = r.Top * n, Right = r.Right * n, Bottom = r.Bottom * n};

    public static SKPoint GetMidPoint(this SKRectI r)                   => new(r.MidX, r.MidY);
    public static SKPointI GetMidPointI(this SKRectI r)                   => new(r.MidX, r.MidY);
    public static SKPoint OffsetPoint(this SKPoint p, float x, float y) => p with {X = p.X + x, Y = p.Y + y};

    public static SKPointI GetXYStepsCount(this SKRectI r, SKRectI other) => new SKPointI(Math.Abs(r.MidX - other.MidX), Math.Abs(r.MidY - other.MidX));
    public static int      GetStepsCount(this   SKRectI r, SKRectI other) => GetXYStepsCount(r, other).X + GetXYStepsCount(r, other).Y; 

    public static double GetAngleToRect(this SKRectI  r, SKRectI  other) => GetAngleTo(r.GetMidPoint(), other.GetMidPoint());
    public static double GetAngleTo(this     SKPoint p, SKPoint other)
    {
        var angle = Math.Atan2(other.Y - p.Y, other.X - p.Y) * 180.0f / Math.PI;
        if(angle < 0) angle += 360;
        return angle;
    }

    public static SKPoint IsIntersect(this SKLine l, SKLine other)
    {
        var dx12 = l.End.X     - l.Start.X;
        var dy12 = l.End.Y     - l.Start.Y;
        var dx34 = other.End.X - other.Start.X;
        var dy34 = other.End.Y - other.Start.Y;

        var denominator = (dy12 * dx34 - dx12 * dy34);

        var t1 = ((l.Start.X - other.Start.X) * dy34 + (other.Start.Y - l.Start.Y) * dx34) / denominator;
        if (float.IsInfinity(t1))
            return default;

        var t2           = ((other.Start.X - l.Start.X) * dy12 + (l.Start.Y - other.Start.Y) * dx12) / -denominator;
        var intersection = new SKPoint(l.Start.X + dx12 * t1, l.Start.Y + dy12 * t1);
        if ((t1 is >= 0 and <= 1 && t2 is >= 0 and <= 1))
            return intersection;

        return default;
    }
    public static SKPoint IsIntersect(this SKLineI l, SKLineI other)
    {
        var dx12 = l.End.X     - l.Start.X;
        var dy12 = l.End.Y     - l.Start.Y;
        var dx34 = other.End.X - other.Start.X;
        var dy34 = other.End.Y - other.Start.Y;

        var denominator = (dy12 * dx34 - dx12 * dy34);

        var t1 = ((l.Start.X - other.Start.X) * dy34 + (other.Start.Y - l.Start.Y) * dx34) / denominator;
        if (float.IsInfinity(t1))
            return default;

        var t2           = ((other.Start.X - l.Start.X) * dy12 + (l.Start.Y - other.Start.Y) * dx12) / -denominator;
        var intersection = new SKPoint(l.Start.X + dx12 * t1, l.Start.Y + dy12 * t1);
        if ((t1 is >= 0 and <= 1 && t2 is >= 0 and <= 1))
            return intersection;

        return default;
    }

    //public static SKPoint IsIntersect(this SKLineI l, SKLineI other) => IsIntersect(l, other);

    public static AngleDirection GetDirection(double angle)
    {
        const double half = (double)45 / 2;
        return angle switch
               {
                   >= 270 - half and < 270 + half => AngleDirection.N,
                   >= 315 - half and < 315 + half => AngleDirection.NE,
                   >= 360 - half or < 0    + half => AngleDirection.E,
                   >= 45  - half and < 45  + half => AngleDirection.SE,
                   >= 90  - half and < 90  + half => AngleDirection.S,
                   >= 135 - half and < 135 + half => AngleDirection.SW,
                   >= 180 - half and < 180 + half => AngleDirection.W,
                   >= 225 - half and < 225 + half => AngleDirection.NW,
                   _                              => throw new Exception()
               };
    }

    public static AngleDirection GetDirectionToRect(this SKRectI r, SKRectI other) => GetDirectionTo(r.GetMidPoint(), other.GetMidPoint());

    public static AngleDirection GetDirectionTo(this SKPoint p, SKPoint other) => GetDirection(p.GetAngleTo(other));


}


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
