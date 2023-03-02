using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapGeneratorTest.Utils;
using MapGeneratorTest.ViewModels;
using ReactiveUI;
using SkiaSharp;

namespace MapGeneratorTest.MapGenerator
{
    public enum PassType
    {
        Straight,
        Oblique,
        SingleBend,
        TwoBend,
    }
    public class Pass : ViewModelBase
    {
        private Room _startRoom;
        public Room StartRoom
        {
            get => _startRoom;
            set => this.RaiseAndSetIfChanged(ref _startRoom, value);
        }

        private Room _endRoom;
        public Room EndRoom
        {
            get => _endRoom;
            set => this.RaiseAndSetIfChanged(ref _endRoom, value);
        }

        private SKPointI _start;
        public SKPointI Start
        {
            get => _start;
            set => this.RaiseAndSetIfChanged(ref _start, value);
        }

        private SKPointI _end;
        public SKPointI End
        {
            get => _end;
            set => this.RaiseAndSetIfChanged(ref _end, value);
        }

        private PassType _type;
        public PassType Type
        {
            get => _type;
            set => this.RaiseAndSetIfChanged(ref _type, value);
        }

        //public Pass(Room startRoom, Room endRoom)
        //{
        //    _startRoom = startRoom;
        //    _endRoom   = endRoom;
        //}

        public Pass(Room startRoom, Room endRoom, params SKLineI[] lineList)
        {
            _startRoom = startRoom;
            _endRoom   = endRoom;
        }
    }
}
