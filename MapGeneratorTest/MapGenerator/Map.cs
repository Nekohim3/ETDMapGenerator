using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
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

    public Map(int roomMinW, int roomMinH, int roomMaxW, int roomMaxH, int minRoomCount, int maxRoomCount, int minDistanceBetweenRooms, int maxDistanceBetweenRooms, int? seed = null)
    {
        var p0 = new SKPoint(0,  0);

        var p1 = new SKPoint(-1, -1);
        var p2 = new SKPoint(0,  -1);
        var p3 = new SKPoint(1,  -1);
        var p4 = new SKPoint(1,  0);
        var p5 = new SKPoint(1,  1);
        var p6 = new SKPoint(0,  1);
        var p7 = new SKPoint(-1, 1);
        var p8 = new SKPoint(-1, 0);

        var q1 = p0.GetAngleTo(p1);
        var q2 = p0.GetAngleTo(p2);
        var q3 = p0.GetAngleTo(p3);
        var q4 = p0.GetAngleTo(p4);
        var q5 = p0.GetAngleTo(p5);
        var q6 = p0.GetAngleTo(p6);
        var q7 = p0.GetAngleTo(p7);
        var q8 = p0.GetAngleTo(p8);

        var w1 = p0.GetDirectionTo(p1);
        var w2 = p0.GetDirectionTo(p2);
        var w3 = p0.GetDirectionTo(p3);
        var w4 = p0.GetDirectionTo(p4);
        var w5 = p0.GetDirectionTo(p5);
        var w6 = p0.GetDirectionTo(p6);
        var w7 = p0.GetDirectionTo(p7);
        var w8 = p0.GetDirectionTo(p8);

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
            Image = GetMapImage().Crop(GetArea()).GetBitmap;
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
                    var passX      = (Math.Max(x.Rect.Left, c.Rect.Left) + Math.Min(x.Rect.Right, c.Rect.Right)) / 2;
                    Passes.Add(x.Rect.Bottom < c.Rect.Top
                                   ? new Pass(x, c, new SKPointI(passX - x.Rect.Left, x.Rect.Height), new SKPointI(passX - c.Rect.Left, 0))
                                   : new Pass(x, c, new SKPointI(passX - x.Rect.Left, 0),             new SKPointI(passX - c.Rect.Left, c.Rect.Height)));
                }
                else if (c.Rect.Bottom > x.Rect.Top + 2 && c.Rect.Top < x.Rect.Bottom - 2)
                {
                    var passY      = (Math.Max(x.Rect.Top, c.Rect.Top) + Math.Min(x.Rect.Bottom, c.Rect.Bottom)) / 2;

                    Passes.Add(x.Rect.Right < c.Rect.Left
                                   ? new Pass(x, c, new SKPointI(x.Rect.Width, passY - x.Rect.Top), new SKPointI(0, passY - c.Rect.Top))
                                   : new Pass(x, c, new SKPointI(0,passY - x.Rect.Top),             new SKPointI(c.Rect.Width, passY - c.Rect.Top)));
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
        var sbmp = new Sbmp(1000, 1000);
        sbmp.Fill(SKColors.Gray);
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
            sbmp.DrawLine(new SKPointI(x.StartRoom.Rect.Left + x.Start.X, x.StartRoom.Rect.Top + x.Start.Y), new SKPointI(x.EndRoom.Rect.Left + x.End.X, x.EndRoom.Rect.Top + x.End.Y), new SKColor(0, 50, 255, 255), 3);
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

    private int GetRand(int min, int max) => _rand.Next(min, max);
}
