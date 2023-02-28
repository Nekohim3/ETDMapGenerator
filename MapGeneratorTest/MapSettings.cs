using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGeneratorTest
{
    public static class MapSettings
    {
        public static int RoomMinW                        { get; set; }
        public static int RoomMinH                        { get; set; }
        public static int RoomMaxW                        { get; set; }
        public static int RoomMaxH                        { get; set; }
        public static int MinRoomCount                    { get; set; }
        public static int MaxRoomCount                    { get; set; }
        public static int MinDistanceBetweenRooms         { get; set; }
        public static int MaxDistanceBetweenRooms         { get; set; }
        public static int MinDistanceFromCornerToCorridor { get; set; }
        public static int MaxDistanceFromCornerToCorridor { get; set; }
    }
}
