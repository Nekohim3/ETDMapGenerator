using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using DynamicData;
using IronSoftware.Drawing;
using MapGeneratorTest.Utils;
using MapGeneratorTest.ViewModels;
using ReactiveUI;
using SkiaSharp;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Color = System.Drawing.Color;

namespace MapGeneratorTest.MapGenerator;

public class Map : ViewModelBase
{
    private ObservableCollectionWithSelectedItem<Room> _rooms = new();
    public ObservableCollectionWithSelectedItem<Room> Rooms
    {
        get => _rooms;
        set { this.RaiseAndSetIfChanged(ref _rooms, value); }
    }

    private ObservableCollectionWithSelectedItem<Pass> _passes = new();
    public ObservableCollectionWithSelectedItem<Pass> Passes
    {
        get => _passes;
        set => this.RaiseAndSetIfChanged(ref _passes, value);
    }

    private Bitmap _image;
    public Bitmap Image
    {
        get => _image;
        set => this.RaiseAndSetIfChanged(ref _image, value);
    }

    private int _roomMinW;
    public int RoomMinW
    {
        get => _roomMinW;
        set { this.RaiseAndSetIfChanged(ref _roomMinW, value); GenerateMap(); }
    }

    private int _roomMinH;
    public int RoomMinH
    {
        get => _roomMinH;
        set { this.RaiseAndSetIfChanged(ref _roomMinH, value); GenerateMap(); }
    }

    private int _roomMaxW;
    public int RoomMaxW
    {
        get => _roomMaxW;
        set { this.RaiseAndSetIfChanged(ref _roomMaxW, value); GenerateMap(); }
    }

    private int _roomMaxH;
    public int RoomMaxH
    {
        get => _roomMaxH;
        set { this.RaiseAndSetIfChanged(ref _roomMaxH, value); GenerateMap(); }
    }

    private int _minRoomCount;
    public int MinRoomCount
    {
        get => _minRoomCount;
        set { this.RaiseAndSetIfChanged(ref _minRoomCount, value); GenerateMap(); }
    }

    private int _maxRoomCount;
    public int MaxRoomCount
    {
        get => _maxRoomCount;
        set { this.RaiseAndSetIfChanged(ref _maxRoomCount, value); GenerateMap(); }
    }

    private int _minDistanceBetweenRooms;
    public int MinDistanceBetweenRooms
    {
        get => _minDistanceBetweenRooms;
        set { this.RaiseAndSetIfChanged(ref _minDistanceBetweenRooms, value); GenerateMap(); }
    }

    private int _maxDistanceBetweenRooms;
    public int MaxDistanceBetweenRooms
    {
        get => _maxDistanceBetweenRooms;
        set { this.RaiseAndSetIfChanged(ref _maxDistanceBetweenRooms, value); GenerateMap(); }
    }

    private int _minDistanceFromCornerToCorridor;
    public int MinDistanceFromCornerToCorridor
    {
        get => _minDistanceFromCornerToCorridor;
        set { this.RaiseAndSetIfChanged(ref _minDistanceFromCornerToCorridor, value); GenerateMap(); }
    }

    private int _maxDistanceFromCornerToCorridor;
    public int MaxDistanceFromCornerToCorridor
    {
        get => _maxDistanceFromCornerToCorridor;
        set { this.RaiseAndSetIfChanged(ref _maxDistanceFromCornerToCorridor, value); GenerateMap(); }
    }

    private int? _seed;
    public int? Seed
    {
        get => _seed;
        set { this.RaiseAndSetIfChanged(ref _seed, value); GenerateMap(); }
    }

    private Random _rand;
    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    private void FindIntersection(
        SKPoint     p1,              SKPoint  p2, SKPoint p3, SKPoint p4,
        out bool    lines_intersect, out bool segments_intersect,
        out SKPoint intersection,
        out SKPoint close_p1, out SKPoint close_p2)
    {
        // Get the segments' parameters.
        float dx12 = p2.X - p1.X;
        float dy12 = p2.Y - p1.Y;
        float dx34 = p4.X - p3.X;
        float dy34 = p4.Y - p3.Y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1 =
            ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
            / denominator;
        if (float.IsInfinity(t1))
        {
            // The lines are parallel (or close enough to it).
            lines_intersect    = false;
            segments_intersect = false;
            intersection       = new SKPoint(float.NaN, float.NaN);
            close_p1           = new SKPoint(float.NaN, float.NaN);
            close_p2           = new SKPoint(float.NaN, float.NaN);
            return;
        }
        lines_intersect = true;

        float t2 =
            ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
            / -denominator;

        // Find the point of intersection.
        intersection = new SKPoint(p1.X + dx12 * t1, p1.Y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        segments_intersect =
            ((t1 >= 0) && (t1 <= 1) &&
             (t2 >= 0) && (t2 <= 1));

        // Find the closest points on the segments.
        if (t1 < 0)
        {
            t1 = 0;
        }
        else if (t1 > 1)
        {
            t1 = 1;
        }

        if (t2 < 0)
        {
            t2 = 0;
        }
        else if (t2 > 1)
        {
            t2 = 1;
        }

        close_p1 = new SKPoint(p1.X + dx12 * t1, p1.Y + dy12 * t1);
        close_p2 = new SKPoint(p3.X + dx34 * t2, p3.Y + dy34 * t2);
    }
    public Map(int roomMinW, int roomMinH, int roomMaxW, int roomMaxH, int minRoomCount, int maxRoomCount, int minDistanceBetweenRooms, int maxDistanceBetweenRooms, int? seed = null)
    {
        //var l1   = new SKLine(new SKPoint(0,  0), new SKPoint(10,        10));
        //var l2   = new SKLine(new SKPoint(10, 0), new SKPoint(5.000001f, 5));
        //var q    = l1.IsIntersect(l2);
        //var qq   = new SKRectI();
        //var qqq  = qq.GetTopLine();
        //var qqq1 = qq.GetTopLine();
        //var q1   = qqq.IsIntersect(qqq1);
        //indIntersection(l1.Start, l1.End, l2.Start, l2.End, out var intersect, out var sInterssect, out var q1, out var q2, out var q3);
        _roomMinW                       = roomMinW;
        _roomMinH                       = roomMinH;
        _roomMaxW                       = roomMaxW;
        _roomMaxH                       = roomMaxH;
        _minRoomCount                   = minRoomCount;
        _maxRoomCount                   = maxRoomCount;
        _minDistanceBetweenRooms        = minDistanceBetweenRooms;
        _maxDistanceBetweenRooms        = maxDistanceBetweenRooms;
        MinDistanceFromCornerToCorridor = 2;
        MaxDistanceFromCornerToCorridor = 2;
        _seed                           = seed;
        GenerateMap();
    }

    public void GenerateMap()
    {
        _rand = Seed.HasValue ? Seed == -1 ? new Random() : new Random(Seed.Value) : new Random();
        if (GenerateRooms())
        {
            GeneratePasses();
            Image = GetMapImage().GetBitmap;
            GetMapImage().Crop(GetArea()).Save("C:\\Config\\test.png");
        }
        else
        {
            Image = GetErrorMapImage().GetBitmap;
        }
        
    }

    public bool GenerateRooms()
    {
        Rooms.Clear();
        var roomCount = GetRand(MinRoomCount, MaxRoomCount);
        while (Rooms.Count < roomCount)
        {
            if (Rooms.Count == 0)
                Rooms.Add(new Room(0, 0, GetRand(RoomMinW, RoomMaxW), GetRand(RoomMinH, RoomMaxH), (Rooms.Count + 1).ToString()));
            else
            {
                var room = RepeatableCode.RepeatResult(() =>
                                                       {
                                                           var room = GenerateRoom((Rooms.Count + 1).ToString());
                                                           return CheckRoom(room) ? room : null;
                                                       }, 100000);

                if (room == null)
                    return false;

                Rooms.Add(room);
            }

            NormalizeRooms();
        }

        return true;
    }

    public bool GeneratePasses()
    {
        Passes.Clear();
        foreach (var x in Rooms)
        {
            foreach (var c in GetNearestRooms(x))
            {
                if(PassExist(x, c)) continue;
                if (c.Rect.Right  > x.Rect.Left + 2 && c.Rect.Left < x.Rect.Right  - 2)
                {
                    var passX = GetRand(Math.Max(x.Rect.Left, c.Rect.Left) + 1, Math.Min(x.Rect.Right, c.Rect.Right) - 1);
                    Passes.Add(x.Rect.Bottom < c.Rect.Top
                                   ? new Pass(x, c, new SKLineI(passX, x.Rect.Bottom, passX, c.Rect.Top))
                                   : new Pass(x, c, new SKLineI(passX, x.Rect.Top,    passX, c.Rect.Bottom)));
                }
                else if (c.Rect.Bottom > x.Rect.Top + 2 && c.Rect.Top < x.Rect.Bottom - 2)
                {
                    var passY = GetRand(Math.Max(x.Rect.Top, c.Rect.Top) + 1, Math.Min(x.Rect.Bottom, c.Rect.Bottom) - 1);
                    Passes.Add(x.Rect.Right < c.Rect.Left
                                   ? new Pass(x, c, new SKLineI(x.Rect.Right, passY, c.Rect.Left,  passY))
                                   : new Pass(x, c, new SKLineI(x.Rect.Left,  passY, c.Rect.Right, passY)));

                }
                else
                {
                    if(MaxDistanceFromCornerToCorridor == 7) continue;
                    var line  = new SKLineI(x.Rect.MidX, x.Rect.MidY, c.Rect.MidX, c.Rect.MidY);
                    var xp    = x.Rect.GetRectIntersectType(line);
                    var cp    = c.Rect.GetRectIntersectType(line);
                    var lines = new List<SKLineI>();
                    if (xp.IsOpposite(cp))
                    {
                        switch (xp)
                        {
                            case RectIntersectType.LeftTop:
                            {
                                var output = GetRand(x.Rect.Top,    x.Rect.MidY);
                                var input  = GetRand(c.Rect.Bottom, c.Rect.MidY);
                                var br     = (x.Rect.Left + c.Rect.Right) / 2; //GetRand(x.Rect.Left,   c.Rect.Right);
                                lines.Add(new SKLineI(x.Rect.Left,  output, br, output));
                                lines.Add(new SKLineI(c.Rect.Right, input,  br, input));
                                lines.Add(new SKLineI(br,           output, br, input));
                                }
                                break;
                            case RectIntersectType.LeftBottom:
                            {
                                var output = GetRand(x.Rect.Bottom, x.Rect.MidY);
                                var input  = GetRand(c.Rect.Top,    c.Rect.MidY);
                                var br     = (x.Rect.Left + c.Rect.Right) / 2; //GetRand(x.Rect.Left,   c.Rect.Right);
                                lines.Add(new SKLineI(x.Rect.Left,  output, br, output));
                                lines.Add(new SKLineI(c.Rect.Right, input,  br, input));
                                lines.Add(new SKLineI(br,           output, br, input + 1));
                                }
                                break;
                            case RectIntersectType.TopLeft:
                            {
                                var output = GetRand(x.Rect.Left,    x.Rect.MidX);
                                var input  = GetRand(c.Rect.Right, c.Rect.MidX);
                                var br     = GetRand(x.Rect.Top,   c.Rect.Bottom);
                                lines.Add(new SKLineI(output, x.Rect.Left,  output, br));
                                lines.Add(new SKLineI(input,  c.Rect.Right, output, br));
                                lines.Add(new SKLineI(output, br,           input,  br));
                            }
                                break;
                            case RectIntersectType.TopRight:
                            {

                                var output = GetRand(x.Rect.Right,  x.Rect.MidY);
                                var input  = GetRand(c.Rect.Left, c.Rect.MidY);
                                var br     = GetRand(x.Rect.Top,   c.Rect.Bottom);
                                lines.Add(new SKLineI(output, x.Rect.Left,  output, br));
                                lines.Add(new SKLineI(input,  c.Rect.Right, output, br));
                                lines.Add(new SKLineI(output, br,           input,  br));
                                }
                                break;
                            case RectIntersectType.RightTop:
                            {
                                var output = GetRand(x.Rect.Top,    x.Rect.MidY);
                                var input  = GetRand(c.Rect.Bottom, c.Rect.MidY);
                                var br     = GetRand(x.Rect.Right,   c.Rect.Left);
                                lines.Add(new SKLineI(x.Rect.Left,  output, br, output));
                                lines.Add(new SKLineI(c.Rect.Right, input,  br, input));
                                lines.Add(new SKLineI(br,           output, br, input));
                                }
                                break;
                            case RectIntersectType.RightBottom:
                            {
                                var output = GetRand(x.Rect.Bottom, x.Rect.MidY);
                                var input  = GetRand(c.Rect.Top,    c.Rect.MidY);
                                var br     = GetRand(x.Rect.Right,   c.Rect.Left);
                                lines.Add(new SKLineI(x.Rect.Left,  output, br, output));
                                lines.Add(new SKLineI(c.Rect.Right, input,  br, input));
                                lines.Add(new SKLineI(br,           output, br, input + 1));
                                }
                                break;
                            case RectIntersectType.BottomLeft:
                            {
                                var output = GetRand(x.Rect.Left,  x.Rect.MidY);
                                var input  = GetRand(c.Rect.Right, c.Rect.MidY);
                                var br     = GetRand(x.Rect.Bottom,   c.Rect.Top);
                                lines.Add(new SKLineI(output, x.Rect.Left,  output, br));
                                lines.Add(new SKLineI(input,  c.Rect.Right, output, br));
                                lines.Add(new SKLineI(output, br,           input,  br));
                                }
                                break;
                            case RectIntersectType.BottomRight:
                            {

                                var output = GetRand(x.Rect.Right, x.Rect.MidX);
                                var input  = GetRand(c.Rect.Left,  c.Rect.MidX);
                                var br     = (x.Rect.Top + c.Rect.Bottom) / 2 + 1;
                                lines.Add(new SKLineI(output, x.Rect.Left + 1,  output, br));
                                lines.Add(new SKLineI(input,  c.Rect.Right + 3, input, br));
                                lines.Add(new SKLineI(output, br,           input,  br));
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        
                    }
                    Passes.Add(new Pass(x, c, lines.ToArray()));
                    //Passes.Add(new Pass(x, c, new SKLineI(x.Rect.MidX, x.Rect.MidY, c.Rect.MidX, x.Rect.MidY), 
                    //                    new SKLineI(c.Rect.MidX, c.Rect.MidY, c.Rect.MidX, x.Rect.MidY)));
                    //var xNearest = GetNearestRooms(x).Where(_ => _ != c).ToList();
                    //var cNearest = GetNearestRooms(c).Where(_ => _ != x).ToList();
                    //var nearest  = xNearest.Intersect(cNearest).ToList();
                    //if (nearest.Count == 0)
                    //{
                    //    Passes.Add(new Pass(x, c, new SKLineI(x.Rect.MidX - x.Rect.Left, x.Rect.MidY - x.Rect.Top, c.Rect.MidX - c.Rect.Left, c.Rect.MidY - c.Rect.Top)));
                    //}
                    //else if (nearest.Count == 1)
                    //{
                    //    Passes.Add(new Pass(x, c, new SKLineI(x.Rect.MidX - x.Rect.Left, x.Rect.MidY - x.Rect.Top, c.Rect.MidX - c.Rect.Left, c.Rect.MidY - c.Rect.Top)));
                    //}
                    //else if (nearest.Count == 2)
                    //{

                    //}
                    //else
                    //{

                    //}
                }

            }
        }
        
        return false;
    }

    public bool PassExist(Room r1, Room r2) => Passes.Count(_ => _.StartRoom == r1 && _.EndRoom == r2 || _.StartRoom == r2 && _.EndRoom == r1) != 0;
    

    private void NormalizeRooms()
    {
        if (!Rooms.Any(_ => _.Rect.Left < 0 || _.Rect.Top < 0))
            return;
        
        var minX = Rooms.Min(_ => _.Rect.Left);
        var minY = Rooms.Min(_ => _.Rect.Top);
        foreach (var x in Rooms)
            x.Rect.Offset(-minX, -minY);
    }

    public Sbmp GetErrorMapImage()
    {
        var sbmp = new Sbmp(100, 100);
        sbmp.Fill(SKColors.Red);
        return sbmp;
    }

    public Sbmp GetMapImage()
    {
        var sbmp = new Sbmp(Rooms.Max(_ => _.Rect.Right), Rooms.Max(_ => _.Rect.Bottom));
        sbmp.Fill(SKColors.Gray);
        //foreach (var x in Rooms)
        //    sbmp.DrawOutlinedRectangle(x.Rect.ExpandAll(MinDistanceBetweenRooms), new SKColor(0, 150, 50, 255), 1);
        //foreach (var x in Rooms)
        //{
        //    var rect = x.Rect.ExpandAll(MaxDistanceBetweenRooms);
        //    sbmp.DrawLine(new SKPoint(rect.Left,  rect.Top),    new SKPoint(x.Rect.Left,  x.Rect.Top),    new SKColor(150, 50, 50, 255), 1);
        //    sbmp.DrawLine(new SKPoint(rect.Right, rect.Top),    new SKPoint(x.Rect.Right, x.Rect.Top),    new SKColor(150, 50, 50, 255), 1);
        //    sbmp.DrawLine(new SKPoint(rect.Left,  rect.Bottom), new SKPoint(x.Rect.Left,  x.Rect.Bottom), new SKColor(150, 50, 50, 255), 1);
        //    sbmp.DrawLine(new SKPoint(rect.Right, rect.Bottom), new SKPoint(x.Rect.Right, x.Rect.Bottom), new SKColor(150, 50, 50, 255), 1);
        //    sbmp.DrawOutlinedRectangle(rect, new SKColor(150,                                                              50, 50, 255), 1);
        //}

        foreach (var x in Rooms)
            sbmp.DrawFillRectangle(x.Rect, new SKColor(0, 50, 255, 255));


        //var lst = new List<(Room r1, Room r2)>();
        //foreach (var x in Rooms)
        //{
        //    foreach (var c in GetNearestRooms(x).Where(c => lst.Count(_ => _.r1 == x && _.r2 == c || _.r1 == c && _.r2 == x) == 0))
        //    {
        //        sbmp.DrawLine(x.Rect.GetMidPoint(), c.Rect.GetMidPoint(), new SKColor(0, 0, 0, 150), 1);
        //        lst.Add((x, c));
        //    }
        //}
        foreach (var x in Passes)
        {
            foreach (var c in x.LineList)
            {
                sbmp.DrawLine(new SKPointI(c.Start.X, c.Start.Y), new SKPointI(c.End.X, c.End.Y), new SKColor(255, 20, 25, 255), 1);
            }
            //sbmp.DrawLine(new SKPointI(x.StartRoom.Rect.Left + x.Start.X, x.StartRoom.Rect.Top + x.Start.Y), new SKPointI(x.EndRoom.Rect.Left + x.End.X, x.EndRoom.Rect.Top + x.End.Y), new SKColor(0, 50, 255, 255), 1);
        }

        for (var i = 0; i < Rooms.Count; i++)
            sbmp.DrawText(Rooms[i].Name, Rooms[i].Rect.GetMidPoint().OffsetPoint(-3.5f, 3.5f), SKColors.White, 10);

        return sbmp;
    }


    private SKRectI GetGenerateArea()
    {
        var r = Rooms[0].Rect;
        foreach (var x in Rooms)
            r.Union(x.Rect);
        return r.ExpandAll(MaxDistanceBetweenRooms).ExpandLeft(RoomMaxW).ExpandTop(RoomMaxH);
    }


    public SKRectI GetArea()
    {
        var r = Rooms[0].Rect;
        foreach (var x in Rooms)
            r.Union(x.Rect);
        return r;
    }


    public List<Room> GetNearestRooms(Room room) => Rooms.Where(x => room != x).Where(x => room.Rect.ExpandAll(MaxDistanceBetweenRooms).IntersectsWith(x.Rect)).ToList();
    private Room GenerateRoom(string name)
    {
        var area = GetGenerateArea();
        var r    = new SKRectI {Left = GetRand(area.Left, area.Right), Top = GetRand(area.Top, area.Bottom)};
        r.Right  = r.Left + GetRand(RoomMinW, RoomMaxW);
        r.Bottom = r.Top  + GetRand(RoomMinH, RoomMaxH);
        return new Room(r, name);
    }
    
    private bool CheckRoom(Room r)
    {
        return !Rooms.Any(_ => _.Rect.ExpandAll(MinDistanceBetweenRooms).IntersectsWith(r.Rect)) && Rooms.Any(_ => _.Rect.ExpandAll(MaxDistanceBetweenRooms).IntersectsWith(r.Rect)) && (Rooms.Count < 3 || GetNearestRooms(r).Count > 1);
    }

    private int GetRand(int min, int max) => min <= max ? _rand.Next(min, max) : _rand.Next(max, min);
}
