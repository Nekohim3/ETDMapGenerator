using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using Avalonia.Controls;
using MapGeneratorTest.MapGenerator;
using ReactiveUI;

namespace MapGeneratorTest.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Map _map;
        public Map Map
        {
            get => _map;
            set => this.RaiseAndSetIfChanged(ref _map, value);
        }

        public ReactiveCommand<Unit, Unit> RegenerateMapCmd { get; }



        public MainWindowViewModel()
        {
            RegenerateMapCmd = ReactiveCommand.Create(OnRegenerateMap);
            Map              = new Map(10, 10, 30, 30, 4, 8, 15, 30);
        }

        private void OnRegenerateMap()
        {
            Map.GenerateMap();
        }
    }
}
