using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media.Imaging;
using MapGeneratorTest.Utils;
using MapGeneratorTest.Utils.Geometry;
using MapGeneratorTest.ViewModels;
using ReactiveUI;
using SkiaSharp;

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
        set
        {
            this.RaiseAndSetIfChanged(ref _roomMinW, value);
            GenerateMap();
        }
    }

    private int _roomMinH;
    public int RoomMinH
    {
        get => _roomMinH;
        set
        {
            this.RaiseAndSetIfChanged(ref _roomMinH, value);
            GenerateMap();
        }
    }

    private int _roomMaxW;
    public int RoomMaxW
    {
        get => _roomMaxW;
        set
        {
            this.RaiseAndSetIfChanged(ref _roomMaxW, value);
            GenerateMap();
        }
    }

    private int _roomMaxH;
    public int RoomMaxH
    {
        get => _roomMaxH;
        set
        {
            this.RaiseAndSetIfChanged(ref _roomMaxH, value);
            GenerateMap();
        }
    }

    private int _minRoomCount;
    public int MinRoomCount
    {
        get => _minRoomCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _minRoomCount, value);
            GenerateMap();
        }
    }

    private int _maxRoomCount;
    public int MaxRoomCount
    {
        get => _maxRoomCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxRoomCount, value);
            GenerateMap();
        }
    }

    private int _minDistanceBetweenRooms;
    public int MinDistanceBetweenRooms
    {
        get => _minDistanceBetweenRooms;
        set
        {
            this.RaiseAndSetIfChanged(ref _minDistanceBetweenRooms, value);
            GenerateMap();
        }
    }

    private int _maxDistanceBetweenRooms;
    public int MaxDistanceBetweenRooms
    {
        get => _maxDistanceBetweenRooms;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxDistanceBetweenRooms, value);
            GenerateMap();
        }
    }

    private int _minDistanceFromCornerToCorridor;
    public int MinDistanceFromCornerToCorridor
    {
        get => _minDistanceFromCornerToCorridor;
        set
        {
            this.RaiseAndSetIfChanged(ref _minDistanceFromCornerToCorridor, value);
            GenerateMap();
        }
    }

    private int _maxDistanceFromCornerToCorridor;
    public int MaxDistanceFromCornerToCorridor
    {
        get => _maxDistanceFromCornerToCorridor;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxDistanceFromCornerToCorridor, value);
            GenerateMap();
        }
    }

    private int _seed;
    public int Seed
    {
        get => _seed;
        set
        {
            this.RaiseAndSetIfChanged(ref _seed, value);
            GenerateMap();
        }
    }

    private Random _rand;

    public Map(int roomMinW, int roomMinH, int roomMaxW, int roomMaxH, int minRoomCount, int maxRoomCount, int minDistanceBetweenRooms, int maxDistanceBetweenRooms, int seed = 0)
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
        GenerateRandMap();
    }

    public void GenerateMap()
    {
        if (Seed == -1)
            Seed = new Random().Next(0, int.MaxValue);
        _rand = new Random(Seed);
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

    public void GenerateRandMap()
    {
        Seed  = new Random().Next(0, int.MaxValue);
        _rand = new Random(Seed);
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
                //if (x.Rect.Bottom == c.Rect.Top || c.Rect.Bottom == x.Rect.Top
                //                                &&
                //    x.Rect.Right == c.Rect.Left || c.Rect.Right == x.Rect.Left)
                //    continue;
                if (PassExist(x, c))
                    continue;
                if (c.Rect.Right > x.Rect.Left + 2 && c.Rect.Left < x.Rect.Right - 2)// straight pass vertical
                {
                    var passX = GetRand(Math.Max(x.Rect.Left, c.Rect.Left) + 1, Math.Min(x.Rect.Right, c.Rect.Right) - 1);
                    Passes.Add(x.Rect.Bottom < c.Rect.Top
                                   ? new Pass(x, c, new SKLineI(passX, x.Rect.Bottom, passX, c.Rect.Top - 1))
                                   : new Pass(x, c, new SKLineI(passX, x.Rect.Top - 1, passX, c.Rect.Bottom)));
                }
                else if (c.Rect.Bottom > x.Rect.Top + 2 && c.Rect.Top < x.Rect.Bottom - 2) // straight pass horizontal
                {
                    var passY = GetRand(Math.Max(x.Rect.Top, c.Rect.Top) + 1, Math.Min(x.Rect.Bottom, c.Rect.Bottom) - 1);
                    Passes.Add(x.Rect.Right < c.Rect.Left
                                   ? new Pass(x, c, new SKLineI(x.Rect.Right, passY, c.Rect.Left - 1, passY))
                                   : new Pass(x, c, new SKLineI(x.Rect.Left - 1, passY, c.Rect.Right, passY)));
                }
                else // breaked pass
                {
                    var line  = new SKLineI(x.Rect.MidX, x.Rect.MidY, c.Rect.MidX, c.Rect.MidY);
                    var xp    = x.Rect.GetRectIntersectType(line);
                    var cp    = c.Rect.GetRectIntersectType(line);
                    var lines = new List<SKLineI>();
                    if (xp.IsOpposite(cp))
                    {
                        var output = GetRand(x.Rect.GetRectCoord(xp.side) - ((float)xp.side).CompareTo(1.1f), (int)xp.line % 2 == 0 ? x.Rect.MidX : x.Rect.MidY);
                        var br     = GetRand();
                        var input  = GetRand(x.Rect.GetRectCoord(xp.side + ((int)xp.side <= 1 ? 2 : -2)) - ((float)xp.side + ((int)xp.side <= 1 ? 2 : -2)).CompareTo(1.1f), (int)xp.line % 2 == 0 ? x.Rect.MidX : x.Rect.MidY);
                    }
                    else
                    {
                        
                    }
                }
            }
        }

        //foreach (var x in Passes)
        //{
        //    if (x.LineList.Count > 0)
        //    {
        //        var lt = x.LineList[0].GetLineType();
        //        var lineList = new List<SKLineI>();
        //        if (x.LineList.Count == 1)
        //        {
        //            switch (lt)
        //            {
        //                case LineDirection.Diagonal:
        //                    break;
        //                case LineDirection.Vertical:
        //                    lineList.Add(_rand.Next(int.MinValue, int.MaxValue) % 2 == 0
        //                                     ? new SKLineI(x.LineList[0].Start.X - 1, x.LineList[0].Start.Y, x.LineList[0].End.X - 1, x.LineList[0].End.Y)
        //                                     : new SKLineI(x.LineList[0].Start.X + 1, x.LineList[0].Start.Y, x.LineList[0].End.X + 1, x.LineList[0].End.Y));
        //                    break;
        //                case LineDirection.Horizontal:
        //                    lineList.Add(_rand.Next(int.MinValue, int.MaxValue) % 2 == 0
        //                                     ? new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y - 1, x.LineList[0].End.X, x.LineList[0].End.Y - 1)
        //                                     : new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y + 1, x.LineList[0].End.X, x.LineList[0].End.Y + 1));
        //                    break;
        //                default:
        //                    throw new ArgumentOutOfRangeException();
        //            }

        //        }
        //        else
        //        {
        //            if (x.LineList.Count == 2)
        //            {
        //                switch (lt)
        //                {
        //                    case LineDirection.Diagonal:
        //                        break;
        //                    case LineDirection.Vertical:
        //                        if (x.LineList[1].Start.X > x.LineList[0].Start.X)
        //                        {
        //                            lineList.Add(new SKLineI(x.LineList[0].Start.X + 1, x.LineList[0].Start.Y, x.LineList[0].End.X + 1, x.LineList[0].End.Y));
        //                            lineList.Add(new SKLineI(x.LineList[1].Start.X, x.LineList[1].Start.Y + 1, x.LineList[1].End.X, x.LineList[1].End.Y + 1));
        //                        }
        //                        else
        //                        {
        //                            lineList.Add(new SKLineI(x.LineList[0].Start.X - 1, x.LineList[0].Start.Y, x.LineList[0].End.X - 1, x.LineList[0].End.Y));
        //                            lineList.Add(new SKLineI(x.LineList[1].Start.X, x.LineList[1].Start.Y - 1, x.LineList[1].End.X, x.LineList[1].End.Y - 1));
        //                        }
        //                        break;
        //                    case LineDirection.Horizontal:
        //                        if (x.LineList[1].Start.Y > x.LineList[0].Start.Y)
        //                        {
        //                            lineList.Add(new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y + 1, x.LineList[0].End.X, x.LineList[0].End.Y + 1));
        //                            lineList.Add(new SKLineI(x.LineList[1].Start.X + 1, x.LineList[1].Start.Y, x.LineList[1].End.X + 1, x.LineList[1].End.Y));
        //                        }
        //                        else
        //                        {
        //                            lineList.Add(new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y - 1, x.LineList[0].End.X, x.LineList[0].End.Y - 1));
        //                            lineList.Add(new SKLineI(x.LineList[1].Start.X - 1, x.LineList[1].Start.Y, x.LineList[1].End.X - 1, x.LineList[1].End.Y));
        //                        }
        //                        break;
        //                    default:
        //                        throw new ArgumentOutOfRangeException();
        //                }
        //            }
        //            else if (x.LineList.Count == 3)
        //            {
        //                switch (lt)
        //                {
        //                    case LineDirection.Diagonal:
        //                        break;
        //                    case LineDirection.Vertical:
        //                        if (x.LineList[0].Start.X < x.LineList[2].Start.X)
        //                        {
        //                            lineList.Add(new SKLineI(x.LineList[0].Start.X + 1, x.LineList[0].Start.Y, x.LineList[0].End.X + 1, x.LineList[0].End.Y));
        //                            lineList.Add(new SKLineI(x.LineList[2].Start.X - 1, x.LineList[2].Start.Y, x.LineList[2].End.X - 1, x.LineList[2].End.Y));
        //                        }
        //                        else
        //                        {
        //                            lineList.Add(new SKLineI(x.LineList[0].Start.X - 1, x.LineList[0].Start.Y, x.LineList[0].End.X - 1, x.LineList[0].End.Y));
        //                            lineList.Add(new SKLineI(x.LineList[2].Start.X + 1, x.LineList[2].Start.Y, x.LineList[2].End.X + 1, x.LineList[2].End.Y));
        //                        }

        //                        lineList.Add(_rand.Next(int.MinValue, int.MaxValue) % 2 == 0
        //                                         ? new SKLineI(x.LineList[1].Start.X, x.LineList[1].Start.Y - 1, x.LineList[1].End.X, x.LineList[1].End.Y - 1)
        //                                         : new SKLineI(x.LineList[1].Start.X, x.LineList[1].Start.Y + 1, x.LineList[1].End.X, x.LineList[1].End.Y + 1));
        //                        break;
        //                    case LineDirection.Horizontal:
        //                        if (x.LineList[0].Start.Y < x.LineList[2].Start.Y)
        //                        {
        //                            lineList.Add(new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y + 1, x.LineList[0].End.X, x.LineList[0].End.Y + 1));
        //                            lineList.Add(new SKLineI(x.LineList[2].Start.X, x.LineList[2].Start.Y - 1, x.LineList[2].End.X, x.LineList[2].End.Y - 1));
        //                        }
        //                        else
        //                        {
        //                            lineList.Add(new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y - 1, x.LineList[0].End.X, x.LineList[0].End.Y - 1));
        //                            lineList.Add(new SKLineI(x.LineList[2].Start.X, x.LineList[2].Start.Y + 1, x.LineList[2].End.X, x.LineList[2].End.Y + 1));
        //                        }

        //                        lineList.Add(_rand.Next(int.MinValue, int.MaxValue) % 2 == 0
        //                                         ? new SKLineI(x.LineList[1].Start.X - 1, x.LineList[1].Start.Y, x.LineList[1].End.X - 1, x.LineList[1].End.Y)
        //                                         : new SKLineI(x.LineList[1].Start.X + 1, x.LineList[1].Start.Y, x.LineList[1].End.X + 1, x.LineList[1].End.Y));
        //                        break;
        //                    default:
        //                        throw new ArgumentOutOfRangeException();
        //                }
        //            }
        //            else
        //            {
        //                throw new Exception();
        //            }
        //        }
        //        x.LineList.AddRange(lineList);
        //    }
        //}

        return false;
    }
    //public bool GeneratePasses()
    //{
    //    Passes.Clear();
    //    foreach (var x in Rooms)
    //    {
    //        foreach (var c in GetNearestRooms(x))
    //        {
    //            if (x.Rect.Bottom == c.Rect.Top || c.Rect.Bottom == x.Rect.Top
    //                                            &&
    //                x.Rect.Right == c.Rect.Left || c.Rect.Right == x.Rect.Left)
    //                continue;
    //            if (PassExist(x, c))
    //                continue;
    //            if (c.Rect.Right > x.Rect.Left + 2 && c.Rect.Left < x.Rect.Right - 2)
    //            {
    //                var passX = GetRand(Math.Max(x.Rect.Left, c.Rect.Left) + 1, Math.Min(x.Rect.Right, c.Rect.Right) - 1);
    //                Passes.Add(x.Rect.Bottom < c.Rect.Top
    //                               ? new Pass(x, c, new SKLineI(passX, x.Rect.Bottom, passX, c.Rect.Top - 1))
    //                               : new Pass(x, c, new SKLineI(passX, x.Rect.Top - 1, passX, c.Rect.Bottom)));
    //            }
    //            else if (c.Rect.Bottom > x.Rect.Top + 2 && c.Rect.Top < x.Rect.Bottom - 2)
    //            {
    //                var passY = GetRand(Math.Max(x.Rect.Top, c.Rect.Top) + 1, Math.Min(x.Rect.Bottom, c.Rect.Bottom) - 1);
    //                Passes.Add(x.Rect.Right < c.Rect.Left
    //                               ? new Pass(x, c, new SKLineI(x.Rect.Right, passY, c.Rect.Left - 1, passY))
    //                               : new Pass(x, c, new SKLineI(x.Rect.Left - 1, passY, c.Rect.Right, passY)));
    //            }
    //            else
    //            {
    //                var line = new SKLineI(x.Rect.MidX, x.Rect.MidY, c.Rect.MidX, c.Rect.MidY);
    //                var xp = x.Rect.GetRectIntersectType(line);
    //                var cp = c.Rect.GetRectIntersectType(line);
    //                if (xp != RectIntersectType.None && cp != RectIntersectType.None)
    //                {
    //                    var lines = new List<SKLineI>();
    //                    if (xp.IsOpposite(cp))
    //                    {
    //                        switch (xp)
    //                        {
    //                            case RectIntersectType.LeftTop:
    //                                {
    //                                    var output = GetRand(x.Rect.Top + 1, x.Rect.MidY);
    //                                    var input = GetRand(c.Rect.Bottom - 1, c.Rect.MidY);
    //                                    var br = GetRand(c.Rect.Right + (x.Rect.Left - c.Rect.Right) * (1 / (float)3), c.Rect.Right + (x.Rect.Left - c.Rect.Right) * (2 / (float)3));
    //                                    lines.Add(new SKLineI(x.Rect.Left - 1, output, br, output));
    //                                    lines.Add(new SKLineI(br, output, br, input));
    //                                    lines.Add(new SKLineI(br, input, c.Rect.Right, input));
    //                                }
    //                                break;
    //                            case RectIntersectType.LeftBottom:
    //                                {
    //                                    var output = GetRand(x.Rect.Bottom - 1, x.Rect.MidY);
    //                                    var input = GetRand(c.Rect.Top + 1, c.Rect.MidY);
    //                                    var br = GetRand(c.Rect.Right + (x.Rect.Left - c.Rect.Right) * (1 / (float)3), c.Rect.Right + (x.Rect.Left - c.Rect.Right) * (2 / (float)3));
    //                                    lines.Add(new SKLineI(x.Rect.Left - 1, output, br, output));
    //                                    lines.Add(new SKLineI(br, output, br, input));
    //                                    lines.Add(new SKLineI(br, input, c.Rect.Right, input));
    //                                }
    //                                break;
    //                            case RectIntersectType.TopLeft:
    //                                {
    //                                    var output = GetRand(x.Rect.Left + 1, x.Rect.MidX);
    //                                    var input = GetRand(c.Rect.Right - 1, c.Rect.MidX);
    //                                    var br = GetRand(c.Rect.Bottom + (x.Rect.Top - c.Rect.Bottom) * (1 / (float)3), c.Rect.Bottom + (x.Rect.Top - c.Rect.Bottom) * (2 / (float)3));
    //                                    lines.Add(new SKLineI(output, x.Rect.Top - 1, output, br));
    //                                    lines.Add(new SKLineI(output, br, input, br));
    //                                    lines.Add(new SKLineI(input, br, input, c.Rect.Bottom));
    //                                }
    //                                break;
    //                            case RectIntersectType.TopRight:
    //                                {
    //                                    var output = GetRand(x.Rect.Right - 1, x.Rect.MidX);
    //                                    var input = GetRand(c.Rect.Left + 1, c.Rect.MidX);
    //                                    var br = GetRand(c.Rect.Bottom + (x.Rect.Top - c.Rect.Bottom) * (1 / (float)3), c.Rect.Bottom + (x.Rect.Top - c.Rect.Bottom) * (2 / (float)3));
    //                                    lines.Add(new SKLineI(output, x.Rect.Top - 1, output, br));
    //                                    lines.Add(new SKLineI(output, br, input, br));
    //                                    lines.Add(new SKLineI(input, br, input, c.Rect.Bottom));
    //                                }
    //                                break;
    //                            case RectIntersectType.RightTop:
    //                                {
    //                                    var output = GetRand(x.Rect.Top + 1, x.Rect.MidY);
    //                                    var input = GetRand(c.Rect.Bottom - 1, c.Rect.MidY);
    //                                    var br = GetRand(x.Rect.Right + (c.Rect.Left - x.Rect.Right) * (1 / (float)3), x.Rect.Right + (c.Rect.Left - x.Rect.Right) * (2 / (float)3));
    //                                    lines.Add(new SKLineI(x.Rect.Right, output, br, output));
    //                                    lines.Add(new SKLineI(br, output, br, input));
    //                                    lines.Add(new SKLineI(br, input, c.Rect.Left - 1, input));
    //                                }
    //                                break;
    //                            case RectIntersectType.RightBottom:
    //                                {
    //                                    var output = GetRand(x.Rect.Bottom - 1, x.Rect.MidY);
    //                                    var input = GetRand(c.Rect.Top + 1, c.Rect.MidY);
    //                                    var br = GetRand(x.Rect.Right + (c.Rect.Left - x.Rect.Right) * (1 / (float)3), x.Rect.Right + (c.Rect.Left - x.Rect.Right) * (2 / (float)3));
    //                                    lines.Add(new SKLineI(x.Rect.Right, output, br, output));
    //                                    lines.Add(new SKLineI(br, output, br, input));
    //                                    lines.Add(new SKLineI(br, input, c.Rect.Left - 1, input));
    //                                }
    //                                break;
    //                            case RectIntersectType.BottomLeft:
    //                                {
    //                                    var output = GetRand(x.Rect.Left + 1, x.Rect.MidX);
    //                                    var input = GetRand(c.Rect.Right - 1, c.Rect.MidX);
    //                                    var br = GetRand(x.Rect.Bottom + (c.Rect.Top - x.Rect.Bottom) * (1 / (float)3), x.Rect.Bottom + (c.Rect.Top - x.Rect.Bottom) * (2 / (float)3)) + 1;
    //                                    lines.Add(new SKLineI(output, x.Rect.Bottom, output, br));
    //                                    lines.Add(new SKLineI(output, br, input, br));
    //                                    lines.Add(new SKLineI(input, br, input, c.Rect.Top - 1));
    //                                }
    //                                break;
    //                            case RectIntersectType.BottomRight:
    //                                {
    //                                    var output = GetRand(x.Rect.Right - 1, x.Rect.MidX);
    //                                    var input = GetRand(c.Rect.Left + 1, c.Rect.MidX);
    //                                    var br = GetRand(x.Rect.Bottom + (c.Rect.Top - x.Rect.Bottom) * (1 / (float)3), x.Rect.Bottom + (c.Rect.Top - x.Rect.Bottom) * (2 / (float)3));
    //                                    lines.Add(new SKLineI(output, x.Rect.Bottom, output, br));
    //                                    lines.Add(new SKLineI(output, br, input, br));
    //                                    lines.Add(new SKLineI(input, br, input, c.Rect.Top - 1));
    //                                }
    //                                break;
    //                            default:
    //                                throw new ArgumentOutOfRangeException();
    //                        }
    //                    }
    //                    else
    //                    {
    //                        switch (xp)
    //                        {
    //                            case RectIntersectType.LeftTop:
    //                                {
    //                                    var output = GetRand(x.Rect.Top + 1, x.Rect.MidY);
    //                                    var input = GetRand(c.Rect.Right - 1, c.Rect.MidX);
    //                                    lines.Add(new SKLineI(x.Rect.Left - 1, output, input, output));
    //                                    lines.Add(new SKLineI(input, c.Rect.Bottom, input, output));
    //                                }
    //                                break;
    //                            case RectIntersectType.LeftBottom:
    //                                {
    //                                    var output = GetRand(x.Rect.Bottom - 1, x.Rect.MidY);
    //                                    var input = GetRand(c.Rect.Right - 1, c.Rect.MidX);
    //                                    lines.Add(new SKLineI(x.Rect.Left - 1, output, input, output));
    //                                    lines.Add(new SKLineI(input, c.Rect.Top - 1, input, output));
    //                                }
    //                                break;
    //                            case RectIntersectType.TopLeft:
    //                                {
    //                                    var output = GetRand(x.Rect.Left + 1, x.Rect.MidX);
    //                                    var input = GetRand(c.Rect.Bottom - 1, c.Rect.MidY);
    //                                    lines.Add(new SKLineI(output, x.Rect.Top - 1, output, input));
    //                                    lines.Add(new SKLineI(c.Rect.Right, input, output, input));
    //                                }
    //                                break;
    //                            case RectIntersectType.TopRight:
    //                                {
    //                                    var output = GetRand(x.Rect.Right - 1, x.Rect.MidX);
    //                                    var input = GetRand(c.Rect.Bottom - 1, c.Rect.MidY);
    //                                    lines.Add(new SKLineI(output, x.Rect.Top - 1, output, input));
    //                                    lines.Add(new SKLineI(c.Rect.Left - 1, input, output, input));
    //                                }
    //                                break;
    //                            case RectIntersectType.RightTop:
    //                                {
    //                                    var output = GetRand(x.Rect.Top + 1, x.Rect.MidY);
    //                                    var input = GetRand(c.Rect.Left + 1, c.Rect.MidX);
    //                                    lines.Add(new SKLineI(x.Rect.Right, output, input, output));
    //                                    lines.Add(new SKLineI(input, c.Rect.Bottom, input, output));
    //                                }
    //                                break;
    //                            case RectIntersectType.RightBottom:
    //                                {
    //                                    var output = GetRand(x.Rect.Bottom - 1, x.Rect.MidY);
    //                                    var input = GetRand(c.Rect.Left + 1, c.Rect.MidX);
    //                                    lines.Add(new SKLineI(x.Rect.Right, output, input, output));
    //                                    lines.Add(new SKLineI(input, c.Rect.Top - 1, input, output));
    //                                }
    //                                break;
    //                            case RectIntersectType.BottomLeft:
    //                                {
    //                                    var output = GetRand(x.Rect.Left + 1, x.Rect.MidX);
    //                                    var input = GetRand(c.Rect.Top + 1, c.Rect.MidY);
    //                                    lines.Add(new SKLineI(output, x.Rect.Bottom, output, input));
    //                                    lines.Add(new SKLineI(c.Rect.Right, input, output, input));
    //                                }
    //                                break;
    //                            case RectIntersectType.BottomRight:
    //                                {
    //                                    var output = GetRand(x.Rect.Right - 1, x.Rect.MidX);
    //                                    var input = GetRand(c.Rect.Top + 1, c.Rect.MidY);
    //                                    lines.Add(new SKLineI(output, x.Rect.Bottom, output, input));
    //                                    lines.Add(new SKLineI(c.Rect.Left - 1, input, output, input));
    //                                }
    //                                break;
    //                            default:
    //                                throw new ArgumentOutOfRangeException();
    //                        }
    //                    }

    //                    Passes.Add(new Pass(x, c, lines.ToArray()));
    //                }
    //            }
    //        }
    //    }

    //    foreach (var x in Passes)
    //    {
    //        if (x.LineList.Count > 0)
    //        {
    //            var lt = x.LineList[0].GetLineType();
    //            var lineList = new List<SKLineI>();
    //            if (x.LineList.Count == 1)
    //            {
    //                switch (lt)
    //                {
    //                    case LineDirection.Diagonal:
    //                        break;
    //                    case LineDirection.Vertical:
    //                        lineList.Add(_rand.Next(int.MinValue, int.MaxValue) % 2 == 0
    //                                         ? new SKLineI(x.LineList[0].Start.X - 1, x.LineList[0].Start.Y, x.LineList[0].End.X - 1, x.LineList[0].End.Y)
    //                                         : new SKLineI(x.LineList[0].Start.X + 1, x.LineList[0].Start.Y, x.LineList[0].End.X + 1, x.LineList[0].End.Y));
    //                        break;
    //                    case LineDirection.Horizontal:
    //                        lineList.Add(_rand.Next(int.MinValue, int.MaxValue) % 2 == 0
    //                                         ? new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y - 1, x.LineList[0].End.X, x.LineList[0].End.Y - 1)
    //                                         : new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y + 1, x.LineList[0].End.X, x.LineList[0].End.Y + 1));
    //                        break;
    //                    default:
    //                        throw new ArgumentOutOfRangeException();
    //                }

    //            }
    //            else
    //            {
    //                if (x.LineList.Count == 2)
    //                {
    //                    switch (lt)
    //                    {
    //                        case LineDirection.Diagonal:
    //                            break;
    //                        case LineDirection.Vertical:
    //                            if (x.LineList[1].Start.X > x.LineList[0].Start.X)
    //                            {
    //                                lineList.Add(new SKLineI(x.LineList[0].Start.X + 1, x.LineList[0].Start.Y, x.LineList[0].End.X + 1, x.LineList[0].End.Y));
    //                                lineList.Add(new SKLineI(x.LineList[1].Start.X, x.LineList[1].Start.Y + 1, x.LineList[1].End.X, x.LineList[1].End.Y + 1));
    //                            }
    //                            else
    //                            {
    //                                lineList.Add(new SKLineI(x.LineList[0].Start.X - 1, x.LineList[0].Start.Y, x.LineList[0].End.X - 1, x.LineList[0].End.Y));
    //                                lineList.Add(new SKLineI(x.LineList[1].Start.X, x.LineList[1].Start.Y - 1, x.LineList[1].End.X, x.LineList[1].End.Y - 1));
    //                            }
    //                            break;
    //                        case LineDirection.Horizontal:
    //                            if (x.LineList[1].Start.Y > x.LineList[0].Start.Y)
    //                            {
    //                                lineList.Add(new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y + 1, x.LineList[0].End.X, x.LineList[0].End.Y + 1));
    //                                lineList.Add(new SKLineI(x.LineList[1].Start.X + 1, x.LineList[1].Start.Y, x.LineList[1].End.X + 1, x.LineList[1].End.Y));
    //                            }
    //                            else
    //                            {
    //                                lineList.Add(new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y - 1, x.LineList[0].End.X, x.LineList[0].End.Y - 1));
    //                                lineList.Add(new SKLineI(x.LineList[1].Start.X - 1, x.LineList[1].Start.Y, x.LineList[1].End.X - 1, x.LineList[1].End.Y));
    //                            }
    //                            break;
    //                        default:
    //                            throw new ArgumentOutOfRangeException();
    //                    }
    //                }
    //                else if (x.LineList.Count == 3)
    //                {
    //                    switch (lt)
    //                    {
    //                        case LineDirection.Diagonal:
    //                            break;
    //                        case LineDirection.Vertical:
    //                            if (x.LineList[0].Start.X < x.LineList[2].Start.X)
    //                            {
    //                                lineList.Add(new SKLineI(x.LineList[0].Start.X + 1, x.LineList[0].Start.Y, x.LineList[0].End.X + 1, x.LineList[0].End.Y));
    //                                lineList.Add(new SKLineI(x.LineList[2].Start.X - 1, x.LineList[2].Start.Y, x.LineList[2].End.X - 1, x.LineList[2].End.Y));
    //                            }
    //                            else
    //                            {
    //                                lineList.Add(new SKLineI(x.LineList[0].Start.X - 1, x.LineList[0].Start.Y, x.LineList[0].End.X - 1, x.LineList[0].End.Y));
    //                                lineList.Add(new SKLineI(x.LineList[2].Start.X + 1, x.LineList[2].Start.Y, x.LineList[2].End.X + 1, x.LineList[2].End.Y));
    //                            }

    //                            lineList.Add(_rand.Next(int.MinValue, int.MaxValue) % 2 == 0
    //                                             ? new SKLineI(x.LineList[1].Start.X, x.LineList[1].Start.Y - 1, x.LineList[1].End.X, x.LineList[1].End.Y - 1)
    //                                             : new SKLineI(x.LineList[1].Start.X, x.LineList[1].Start.Y + 1, x.LineList[1].End.X, x.LineList[1].End.Y + 1));
    //                            break;
    //                        case LineDirection.Horizontal:
    //                            if (x.LineList[0].Start.Y < x.LineList[2].Start.Y)
    //                            {
    //                                lineList.Add(new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y + 1, x.LineList[0].End.X, x.LineList[0].End.Y + 1));
    //                                lineList.Add(new SKLineI(x.LineList[2].Start.X, x.LineList[2].Start.Y - 1, x.LineList[2].End.X, x.LineList[2].End.Y - 1));
    //                            }
    //                            else
    //                            {
    //                                lineList.Add(new SKLineI(x.LineList[0].Start.X, x.LineList[0].Start.Y - 1, x.LineList[0].End.X, x.LineList[0].End.Y - 1));
    //                                lineList.Add(new SKLineI(x.LineList[2].Start.X, x.LineList[2].Start.Y + 1, x.LineList[2].End.X, x.LineList[2].End.Y + 1));
    //                            }

    //                            lineList.Add(_rand.Next(int.MinValue, int.MaxValue) % 2 == 0
    //                                             ? new SKLineI(x.LineList[1].Start.X - 1, x.LineList[1].Start.Y, x.LineList[1].End.X - 1, x.LineList[1].End.Y)
    //                                             : new SKLineI(x.LineList[1].Start.X + 1, x.LineList[1].Start.Y, x.LineList[1].End.X + 1, x.LineList[1].End.Y));
    //                            break;
    //                        default:
    //                            throw new ArgumentOutOfRangeException();
    //                    }
    //                }
    //                else
    //                {
    //                    throw new Exception();
    //                }
    //            }
    //            x.LineList.AddRange(lineList);
    //        }
    //    }

    //    return false;
    //}

    public bool PassExist(Room r1, Room r2) => Passes.Count(_ => (_.StartRoom == r1 && _.EndRoom == r2) || (_.StartRoom == r2 && _.EndRoom == r1)) != 0;

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
        sbmp.Fill(new SKColor(0x11,0x11,0x11,0xff));
        //foreach (var x in Rooms)
        //    sbmp.DrawOutlinedRectangle(x.Rect.ExpandAll(MinDistanceBetweenRooms), new SKColor(0, 150, 50, 255), 1);
        //foreach (var x in Rooms)
        //{
        //    var rect = x.Rect.ExpandAll(MaxDistanceBetweenRooms);
        //    sbmp.DrawLine(new SKPoint(rect.Left, rect.Top), new SKPoint(x.Rect.Left, x.Rect.Top), new SKColor(150, 50, 50, 255), 1);
        //    sbmp.DrawLine(new SKPoint(rect.Right, rect.Top), new SKPoint(x.Rect.Right, x.Rect.Top), new SKColor(150, 50, 50, 255), 1);
        //    sbmp.DrawLine(new SKPoint(rect.Left, rect.Bottom), new SKPoint(x.Rect.Left, x.Rect.Bottom), new SKColor(150, 50, 50, 255), 1);
        //    sbmp.DrawLine(new SKPoint(rect.Right, rect.Bottom), new SKPoint(x.Rect.Right, x.Rect.Bottom), new SKColor(150, 50, 50, 255), 1);
        //    sbmp.DrawOutlinedRectangle(rect, new SKColor(150, 50, 50, 255), 1);
        //}

        foreach (var x in Rooms)
            sbmp.DrawFillRectangle(x.Rect, new SKColor(0x55,0x55,0x55,0xff));

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
                sbmp.DrawLine(new SKPointI(c.Start.X, c.Start.Y), new SKPointI(c.End.X, c.End.Y), new SKColor(0x55,0x55,0x55,0xff), 1);
            }
            //sbmp.DrawLine(new SKPointI(x.StartRoom.Rect.Left + x.Start.X, x.StartRoom.Rect.Top + x.Start.Y), new SKPointI(x.EndRoom.Rect.Left + x.End.X, x.EndRoom.Rect.Top + x.End.Y), new SKColor(0, 50, 255, 255), 1);
        }

        //for (var i = 0; i < Rooms.Count; i++)
            //sbmp.DrawText(Rooms[i].Name, Rooms[i].Rect.GetMidPoint().OffsetPoint(-3.5f, 3.5f), SKColors.White, 10);

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

    //public List<Room> GetNearestRooms(Room room) => Rooms.Where(x => room != x).Where(x => room.Rect.ExpandAll(MaxDistanceBetweenRooms).IntersectsWith(x.Rect)).ToList();
    public List<Room> GetNearestRooms(Room r) => Rooms.Where(_ => r != _)
                                                      .Where(_ => r.Rect.GetDistanceToRect(_.Rect) <= MaxDistanceBetweenRooms).ToList();

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
        return Rooms.All(_ => _.Rect.GetDistanceToRect(r.Rect) >= MinDistanceBetweenRooms) &&
               Rooms.Any(_ => _.Rect.GetDistanceToRect(r.Rect) <= MaxDistanceBetweenRooms) &&
               Rooms.All(_ => !_.Rect.IntersectsWith(r.Rect))                              && (Rooms.Count < 3 || GetNearestRooms(r).Count > 1);
        //return !Rooms.Any(_ => _.Rect.ExpandAll(MinDistanceBetweenRooms).IntersectsWith(r.Rect)) && Rooms.Any(_ => _.Rect.ExpandAll(MaxDistanceBetweenRooms).IntersectsWith(r.Rect)) && (Rooms.Count < 3 || GetNearestRooms(r).Count > 1);
    }

    private int GetRand(int    min, int   max) => min <= max ? _rand.Next(min,       max) : _rand.Next(max,             min);
    private int GetRand(float  min, float max) => min <= max ? _rand.Next((int) min, (int) max) : _rand.Next((int) max, (int) min);
    private int GetRand1(int   min, int   max) => min <= max ? new Random().Next(min,       max) : new Random().Next(max,             min);
    private int GetRand1(float min, float max) => min <= max ? new Random().Next((int) min, (int) max) : new Random().Next((int) max, (int) min);
}
