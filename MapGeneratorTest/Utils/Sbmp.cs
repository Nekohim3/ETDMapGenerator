using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Avalonia.Media;
using IronSoftware.Drawing;
using JetBrains.Annotations;
using SkiaSharp;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Color = System.Drawing.Color;

namespace MapGeneratorTest.Utils;

public class Sbmp : IDisposable
{
    private SKImageInfo _imageInfo;
    private SKSurface   _surface;
    private SKCanvas    _canvas => _surface.Canvas;
    public  int         Width   => _imageInfo.Width;
    public  int         Height  => _imageInfo.Height;

    public Sbmp(int width, int height)
    {
        _imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        _surface   = SKSurface.Create(_imageInfo);
    }

    public Sbmp(SKImage image)
    {
        _imageInfo = new SKImageInfo(image.Width, image.Height);
        _surface = SKSurface.Create(image.PeekPixels());
    }

    #region DrawFuncs

    public void Fill(SKColor color) => DrawFillRectangle(new SKRectI(0, 0, Width, Height), color);

    #region Rectangles

    public void DrawRectangle(SKRectI rect, SKColor fillColor, SKColor strokeColor, int strokeThickness, int gapX = 0, int gapY = 0)
    {
        var fillRect  = new SKRectI(rect.Left + gapX + strokeThickness, rect.Top + gapY + strokeThickness, rect.Right - gapX - strokeThickness, rect.Bottom - gapY - strokeThickness);
        var paintFill = new SKPaint() {Style = SKPaintStyle.Fill, Color = fillColor};
        _canvas.DrawRect(fillRect, paintFill);

        if (strokeThickness == 0)
            return;

        var strokeRect  = new SKRectI(rect.Left + strokeThickness / 2, rect.Top + strokeThickness / 2, rect.Right - 1 - strokeThickness / 2, rect.Bottom - 1 - strokeThickness / 2);
        var paintStroke = new SKPaint() {Style = SKPaintStyle.Stroke, Color = strokeColor, StrokeWidth = strokeThickness};
        _canvas.DrawRect(strokeRect, paintStroke);
    }

    public void DrawRectangle(int left, int top, int right, int bottom, SKColor fillColor, SKColor strokeColor, int strokeThickness, int gapX = 0, int gapY = 0) =>
        DrawRectangle(new SKRectI(left, top, right, bottom), fillColor, strokeColor, strokeThickness, gapX, gapY);

    public void DrawFillRectangle(SKRectI rect, SKColor fillColor)
    {
        var fillRect  = new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
        var paintFill = new SKPaint() {Style = SKPaintStyle.Fill, Color = fillColor};
        _canvas.DrawRect(fillRect, paintFill);
    }

    public void DrawFillRectangle(int left, int top, int right, int bottom, SKColor fillColor) =>
        DrawFillRectangle(new SKRectI(left, top, right, bottom), fillColor);

    public void DrawOutlinedRectangle(SKRectI rect, SKColor strokeColor, int strokeThickness)
    {
        if (strokeThickness == 0)
            return;

        var strokeRect  = new SKRectI(rect.Left + strokeThickness / 2, rect.Top + strokeThickness / 2, rect.Right - 1 - strokeThickness / 2, rect.Bottom - 1 - strokeThickness / 2);
        var paintStroke = new SKPaint() {Style = SKPaintStyle.Stroke, Color = strokeColor, StrokeWidth = strokeThickness};
        _canvas.DrawRect(strokeRect, paintStroke);
    }

    public void DrawOutlinedRectangle(int left, int top, int right, int bottom, SKColor strokeColor, int strokeThickness) =>
        DrawOutlinedRectangle(new SKRectI(left, top, right, bottom), strokeColor, strokeThickness);

    #endregion

    #region Line

    public void DrawLine(SKPoint start, SKPoint end, SKColor color, int strokeThickness)
    {
        var paintStroke = new SKPaint() {Color = color, Style = SKPaintStyle.Stroke, StrokeWidth = strokeThickness, StrokeCap = SKStrokeCap.Square};
        _canvas.DrawLine(start, end, paintStroke);
    }

    #endregion

    #region Text

    public void DrawText(string text, SKPoint pos, SKColor color, int size, SKFont? font = null)
    {
        font ??= new SKFont(SKTypeface.FromFamilyName("verdana"));
        _canvas.DrawText(text, pos, new SKPaint(font) { Color = color, TextSize = size});
    }
    #endregion

    #endregion

    public Sbmp Crop(SKRectI rect)
    {
        if (rect.Left < 0)
            rect.Left = 0;
        if (rect.Top < 0)
            rect.Top = 0;
        if (rect.Right > Width)
            rect.Right = Width;
        if (rect.Bottom > Height)
            rect.Bottom = Height;
        return new Sbmp(_surface.Snapshot(rect));
    }

    public SKImage      GetImage() => _surface.Snapshot();
    public SKData       GetData()  => GetImage().Encode();
    public byte[]       GetBytes() => GetData().ToArray();
    public MemoryStream GetStream  => new(GetBytes());
    public Bitmap       GetBitmap  => new(GetStream);

    public void Save(string path)
    {
        using var fs  = new FileStream(path, FileMode.Create, FileAccess.Write);
        var       buf = GetBytes();
        fs.Write(buf, 0, buf.Length);
    }

    public void Dispose()
    {
    }
}
//public class Sbmp : IDisposable
//{
//    public string fname = "";
//    public byte[] Buf;
//    public int    Height;
//    public int    Width;

//    public Sbmp(byte[] b, int w, int h)
//    {
//        Buf    = b;
//        Width  = w;
//        Height = h;
//    }

//    public Sbmp(Bitmap source)
//    {
//        if (source.PixelFormat != PixelFormat.Format32bppArgb)
//            source = Convert32(source);
//        Width  = source.Width;
//        Height = source.Height;
//        Buf    = new byte[Width * Height * 4];
//        var rect       = new Rectangle(0, 0, Width, Height);
//        var bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);
//        Marshal.Copy(bitmapData.Scan0, Buf, 0, Buf.Length);
//        bitmapData = null;
//        source.Dispose();
//    }

//    public Sbmp(string str)
//    {
//        Image image;
//        using (var myStream = new FileStream(str, FileMode.Open, FileAccess.Read))
//            image = Image.FromStream(myStream);
//        var source = (Bitmap) image;
//        if (source.PixelFormat != PixelFormat.Format32bppArgb)
//            source = Convert32(source);
//        Width  = source.Width;
//        Height = source.Height;
//        Buf    = new byte[Width * Height * 4];
//        var rect       = new Rectangle(0, 0, Width, Height);
//        var bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);
//        Marshal.Copy(bitmapData.Scan0, Buf, 0, Buf.Length);
//        bitmapData = null;
//        source.Dispose();
//        fname = str;
//    }

//    public Sbmp(int w, int h)
//    {
//        Width  = w;
//        Height = h;
//        Buf    = new byte[Width * Height * 4];
//    }

//    public static Sbmp[] PackLoad(string[] str)
//    {
//        var lst = new List<Sbmp>();
//        foreach (var q in str)
//        {
//            var source = new Bitmap(q);
//            if (source.PixelFormat != PixelFormat.Format32bppArgb)
//                source = Convert32(source);
//            var buf        = new byte[source.Width * source.Height * 4];
//            var rect       = new Rectangle(0, 0, source.Width, source.Height);
//            var bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);
//            Marshal.Copy(bitmapData.Scan0, buf, 0, buf.Length);
//            bitmapData = null;
//            lst.Add(new Sbmp(buf, source.Width, source.Height));
//            source.Dispose();
//        }

//        return lst.ToArray();
//    }

//    public Sbmp()
//    {
//    }

//    public Bitmap Resize(int w, int h)
//    {
//        var newImage = new Bitmap(w, h);
//        using (var gr = Graphics.FromImage(newImage))
//        {
//            gr.SmoothingMode     = SmoothingMode.HighQuality;
//            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
//            gr.PixelOffsetMode   = PixelOffsetMode.HighQuality;
//            gr.DrawImage(GetBmp(), new Rectangle(0, 0, w, h));
//        }

//        return newImage;
//    }

//    public static Bitmap Resize(string str, int w, int h)
//    {
//        var newImage = new Bitmap(w, h);
//        using (var gr = Graphics.FromImage(newImage))
//        {
//            gr.SmoothingMode     = SmoothingMode.HighQuality;
//            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
//            gr.PixelOffsetMode   = PixelOffsetMode.HighQuality;
//            gr.DrawImage(new Bitmap(str), new Rectangle(0, 0, w, h));
//        }

//        return newImage;
//    }

//    public static Bitmap Convert32(Bitmap orig)
//    {
//        var clone = new Bitmap(orig.Width, orig.Height, PixelFormat.Format32bppPArgb);
//        using (var gr = Graphics.FromImage(clone))
//            gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
//        return clone;
//    }

//    public Color GetPixel(int x, int y)
//    {
//        if (x < 0 || y < 0)
//            return Color.FromArgb(0, 0, 0, 0);
//        var i = (y * Width + x) * 4;
//        if (i > Buf.Length - 4)
//            return Color.FromArgb(0, 0, 0, 0);
//        var b = Buf[i];
//        var g = Buf[i + 1];
//        var r = Buf[i + 2];
//        var a = Buf[i + 3];
//        return Color.FromArgb(a, r, g, b);
//    }

//    public void SetPixel(int x, int y, Color color)
//    {
//        var i = (y * Width + x) * 4;
//        Buf[i]     = color.B;
//        Buf[i + 1] = color.G;
//        Buf[i + 2] = color.R;
//        Buf[i + 3] = color.A;
//    }

//    public void MixPixel(int x, int y, Color c)
//    {
//        var i = (y * Width + x) * 4;
//        var p = GetPixel(x, y);

//        var aB = p.A;
//        var rB = p.R;
//        var gB = p.G;
//        var bB = p.B;
//        var aA = c.A;
//        var rA = c.R;
//        var gA = c.G;
//        var bA = c.B;

//        var aOut = aA + aB * (255 - aA) / 255;
//        var rOut = (rA * aA + rB * aB * (255 - aA) / 255) / aOut;
//        var gOut = (gA * aA + gB * aB * (255 - aA) / 255) / aOut;
//        var bOut = (bA * aA + bB * aB * (255 - aA) / 255) / aOut;

//        Buf[i]     = (byte) rOut;
//        Buf[i + 1] = (byte) gOut;
//        Buf[i + 2] = (byte) bOut;
//        Buf[i + 3] = (byte) aOut;
//    }

//    public void Fill(Color color, bool mix = false)
//    {
//        for (var y = 0; y < Height; y++)
//        for (var x = 0; x < Width; x++)
//        {
//            if (mix)
//                MixPixel(x, y, color);
//            else
//                SetPixel(x, y, color);
//        }
//    }

//    public void DrawLine(int x1, int y1, int x2, int y2, Color color, bool mix = false)
//    {
//        for (var y = y1; y > 0 && y < Height && y <= y2; y++)
//        for (var x = x1; x > 0 && x < Width && x <= x2; x++)
//        {
//            if (mix)
//                MixPixel(x, y, color);
//            else
//                SetPixel(x, y, color);
//        }
//    }

//    public void DrawLine(Point p1, Point p2, Color color, bool mix = false) => DrawLine(p1.X, p1.Y, p2.X, p2.Y, color, mix);

//    public void DrawRectangleOutline(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, Color color, bool mix = false)
//    {
//        for (var y = y1; y > 0 && y < Height && y <= y2; y++)
//        for (var x = x1; x > 0 && x < Width && x <= x2; x++)
//        {
//            if (mix)
//                MixPixel(x, y, color);
//            else
//                SetPixel(x, y, color);
//        }

//        for (var y = y2; y > 0 && y < Height && y <= y4; y++)
//        for (var x = x2; x > 0 && x < Width && x <= x4; x++)
//        {
//            if (mix)
//                MixPixel(x, y, color);
//            else
//                SetPixel(x, y, color);
//        }

//        for (var y = y1; y > 0 && y < Height && y <= y3; y++)
//        for (var x = x1; x > 0 && x < Width && x <= x3; x++)
//        {
//            if (mix)
//                MixPixel(x, y, color);
//            else
//                SetPixel(x, y, color);
//        }

//        for (var y = y3; y > 0 && y < Height && y <= y4; y++)
//        for (var x = x3; x > 0 && x < Width && x <= x4; x++)
//        {
//            if (mix)
//                MixPixel(x, y, color);
//            else
//                SetPixel(x, y, color);
//        }
//    }

//    public void DrawRectangleOutline(Rectangle r, Color color, bool mix = false) => DrawRectangleOutline(r.Left, r.Top, r.Right, r.Top, r.Left, r.Bottom, r.Right, r.Bottom, color, mix);

//    public void DrawRectangleFill(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, Color color, bool mix = false)
//    {
//        for (var y = y1; y > 0 && y < Height && y <= y4; y++)
//        for (var x = x1; x > 0 && x < Width && x <= x4; x++)
//        {
//            if (mix)
//                MixPixel(x, y, color);
//            else
//                SetPixel(x, y, color);
//        }
//    }

//    public void DrawRectangleFill(Rectangle r, Color color, bool mix = false) => DrawRectangleFill(r.Left, r.Top, r.Right, r.Top, r.Left, r.Bottom, r.Right, r.Bottom, color, mix);

//    public Sbmp Copy() => new(Buf.ToArray(), Width, Height);

//    public Sbmp Crop(Rectangle r)
//    {
//        var bbuf = new byte[r.Width * r.Height * 4];
//        for (int i = r.X, ii = 0; i < r.X + r.Width; i++, ii++)
//        {
//            for (int j = r.Y, jj = 0; j < r.Y + r.Height; j++, jj++)
//            {
//                var x     = (jj * r.Width + ii) * 4;
//                var color = GetPixel(i, j);
//                bbuf[x]     = color.B;
//                bbuf[x + 1] = color.G;
//                bbuf[x + 2] = color.R;
//                bbuf[x + 3] = color.A;
//            }
//        }

//        return new Sbmp(bbuf, r.Width, r.Height);
//    }

//    public Bitmap GetBmp()
//    {
//        var bmp  = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
//        var bd   = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
//        var iptr = bd.Scan0;
//        Marshal.Copy(Buf, 0, iptr, Buf.Length);
//        bmp.UnlockBits(bd);
//        return bmp;
//    }

//    public void Save(string fileName) => GetBmp().Save(fileName);

//    public bool Check(Sbmp b, Point p)
//    {
//        for (var i = 0; i < b.Width; i++)
//        for (var j = 0; j < b.Height; j++)
//            if (GetPixel(p.X + i, p.Y + j) != b.GetPixel(i, j))
//                return false;
//        return true;
//    }

//    public bool Check(Sbmp b, Point p, int thr, int npc)
//    {
//        var counter = 0;
//        for (var i = 0; i < b.Width; i++)
//        {
//            for (var j = 0; j < b.Height; j++)
//            {
//                var c = GetPixel(p.X + i, p.Y + j);
//                var e = b.GetPixel(i, j);
//                if (Math.Abs(c.R - e.R) <= thr && Math.Abs(c.G - e.G) <= thr &&
//                    Math.Abs(c.B - e.B) <= thr)
//                    counter++;
//            }
//        }

//        double all  = b.Width       * b.Height;
//        var    proc = counter * 100 / all;
//        return proc >= npc;
//    }

//    public bool Check(Sbmp b, int x, int y, int thr, int npc)
//    {
//        var counter = 0;
//        for (var i = 0; i < b.Width; i++)
//        {
//            for (var j = 0; j < b.Height; j++)
//            {
//                var c = GetPixel(x + i, y + j);
//                var e = b.GetPixel(i, j);
//                if (Math.Abs(c.R - e.R) <= thr && Math.Abs(c.G - e.G) <= thr &&
//                    Math.Abs(c.B - e.B) <= thr)
//                    counter++;
//            }
//        }

//        double all  = b.Width       * b.Height;
//        var    proc = counter * 100 / all;
//        return proc >= npc;
//    }

//    public bool Find(Sbmp b, Point p, int rad)
//    {
//        for (var i = p.X - rad; i < p.X + rad; i++)
//        for (var j = p.Y - rad; j < p.Y + rad; j++)
//            if (Check(b, new Point(i, j)))
//                return true;
//        return false;
//    }

//    public bool Find(Sbmp b, Point p, int radx, int rady)
//    {
//        for (var i = p.X - radx; i < p.X + radx; i++)
//        for (var j = p.Y - rady; j < p.Y + rady; j++)
//            if (Check(b, new Point(i, j)))
//                return true;
//        return false;
//    }

//    public bool Find(Sbmp b, Rectangle rad)
//    {
//        for (var i = rad.X; i < rad.Width; i++)
//        for (var j = rad.Y; j < rad.Height; j++)
//            if (Check(b, new Point(i, j)))
//                return true;
//        return false;
//    }

//    public bool Find(Sbmp b, Point p, int rad, int thr, int npc)
//    {
//        for (var i = p.X - rad; i < p.X + rad; i++)
//        for (var j = p.Y - rad; j < p.Y + rad; j++)
//            if (Check(b, new Point(i, j), thr, npc))
//                return true;
//        return false;
//    }

//    public bool Find(Sbmp b, int x, int y, int rad, int thr, int npc)
//    {
//        for (var i = x - rad; i < x + rad; i++)
//        for (var j = y - rad; j < y + rad; j++)
//            if (Check(b, new Point(i, j), thr, npc))
//                return true;
//        return false;
//    }

//    public bool Find(Sbmp b, Rectangle rad, int thr, int npc)
//    {
//        for (var i = rad.X; i < rad.Width; i++)
//        for (var j = rad.Y; j < rad.Height; j++)
//            if (Check(b, new Point(i, j), thr, npc))
//                return true;
//        return false;
//    }

//    public bool Find(Sbmp b, Point p, int radx, int rady, int thr, int npc)
//    {
//        for (var i = p.X - radx; i < p.X + radx; i++)
//        for (var j = p.Y - rady; j < p.Y + rady; j++)
//            if (Check(b, new Point(i, j), thr, npc))
//                return true;
//        return false;
//    }

//    public void Dispose()
//    {
//        Buf = null;
//        if (fname != "")
//            File.Delete(fname);
//    }

//    public void Thr(int t)
//    {
//        for (var i = 0; i < Width; i++)
//        {
//            for (var j = 0; j < Height; j++)
//            {
//                var p  = GetPixel(i, j);
//                var gs = (int) (p.R * 0.3f + p.G * 0.59f + p.B * 0.11f);
//                SetPixel(i, j, gs < t ? Color.FromArgb(255, 0, 0, 0) : Color.FromArgb(255, 255, 255, 255));
//            }
//        }
//    }
//}
//public class Mask
//{
//    public Sbmp        Pic     { get; set; }
//    public List<Color> Palette { get; set; }

//    public Mask(string f) => Init(new Sbmp(f));

//    public Mask(Bitmap b) => Init(new Sbmp(b));

//    public void Init(Sbmp s)
//    {
//        Pic     = s;
//        Palette = new List<Color>();
//        for (var i = 0; i < Pic.Width; i++)
//        for (var j = 0; j < Pic.Height; j++)
//        {
//            var c = Pic.GetPixel(i, j);
//            if (c.A == 255)
//                Palette.Add(c);
//        }

//        Palette = Palette.Distinct().ToList();
//    }

//    public bool Compare(Sbmp b, int x, int y)
//    {
//        if (x + Pic.Width > b.Width || y + Pic.Height > b.Height)
//            return false;
//        for (int i = x, ii = 0; ii < Pic.Width; i++, ii++)
//        {
//            for (int j = y, jj = 0; jj < Pic.Height; j++, jj++)
//            {
//                var cb = b.GetPixel(i, j);
//                var cp = Pic.GetPixel(ii, jj);
//                if (cp.A == 0) //Вообще похеру
//                {
//                }

//                if (cp.A == 1) //Цвет есть в палитре
//                    if (Palette.Any(xx => xx == cb))
//                        return false;
//                if (cp.A == 2) //Данного цвета нет в палитре
//                    if (Palette.All(xx => xx != cb))
//                        return false;

//                if (cp.A == 255)
//                    if (cb != cp)
//                        return false;
//            }
//        }

//        return true;
//    }

//    public bool Compare(Sbmp b, int x, int y, int npc)
//    {
//        if (x + Pic.Width > b.Width || y + Pic.Height > b.Height)
//            return false;
//        var cnt = 0;
//        for (int i = x, ii = 0; ii < Pic.Width; i++, ii++)
//        {
//            for (int j = y, jj = 0; jj < Pic.Height; j++, jj++)
//            {
//                var cb = b.GetPixel(i, j);
//                var cp = Pic.GetPixel(ii, jj);
//                if (cp.A == 0)
//                    cnt++;

//                if (cp.A == 1)
//                    if (Palette.All(xx => xx != cb))
//                        cnt++;

//                if (cp.A == 2)
//                    if (Palette.Any(xx => xx == cb))
//                        cnt++;

//                if (cp.A == 255)
//                    if (cb == cp)
//                        cnt++;
//            }
//        }

//        double all  = Pic.Width * Pic.Height;
//        var    proc = cnt * 100 / all;
//        return proc >= npc;
//    }

//    public bool Compare1(Sbmp b, int x, int y)
//    {
//        if (x + Pic.Width > b.Width || y + Pic.Height > b.Height)
//            return false;
//        for (int i = x, ii = 0; ii < Pic.Width; i++, ii++)
//        {
//            for (int j = y, jj = 0; jj < Pic.Height; j++, jj++)
//            {
//                var cb = b.GetPixel(i, j);
//                var cp = Pic.GetPixel(ii, jj);
//                if (cp.A == 0) //Вообще похеру
//                {
//                }
//                else
//                    //if (cp.A == 1) //Цвет есть в палитре
//                if (cb.R < 220 || cb.G < 200 || cb.B < 200)
//                    //if (Palette.Any(xx => xx == cb))
//                    return false;
//                //if (cp.A == 2) //Данного цвета нет в палитре
//                //    if (Palette.All(xx => xx != cb))
//                //        return false;

//                //if (cp.A == 255)
//                //    if (cb != cp)
//                //        return false;
//            }
//        }

//        return true;
//    }

//    public bool Compare1(Sbmp b, int x, int y, int npc)
//    {
//        if (x + Pic.Width > b.Width || y + Pic.Height > b.Height)
//            return false;
//        var cnt = 0;
//        for (int i = x, ii = 0; ii < Pic.Width; i++, ii++)
//        {
//            for (int j = y, jj = 0; jj < Pic.Height; j++, jj++)
//            {
//                var cb = b.GetPixel(i, j);
//                var cp = Pic.GetPixel(ii, jj);
//                if (cp.A == 0)
//                    cnt++;
//                else if (cb.R >= 220 && cb.G >= 200 && cb.B >= 200)
//                    cnt++;

//                //if (cp.A == 1)
//                //    if (Palette.All(xx => xx != cb))
//                //        cnt++;

//                //if (cp.A == 2)
//                //    if (Palette.Any(xx => xx == cb))
//                //        cnt++;

//                //if (cp.A == 255)
//                //    if (cb == cp)
//                //        cnt++;
//            }
//        }

//        double all  = Pic.Width * Pic.Height;
//        var    proc = cnt * 100 / all;
//        return proc >= npc;
//    }
//}
