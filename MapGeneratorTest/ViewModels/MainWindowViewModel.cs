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
        public ReactiveCommand<Unit, Unit> RegenerateRandMapCmd { get; }



        public MainWindowViewModel()
        {
            RegenerateMapCmd     = ReactiveCommand.Create(OnRegenerateMap);
            RegenerateRandMapCmd = ReactiveCommand.Create(OnRegenerateRandMap);
            Map                  = new Map(10, 10, 30, 30, 4, 8, 6, 20);
        }

        private void OnRegenerateMap()
        {
            
            Map.GenerateMap();
        }
        private void OnRegenerateRandMap()
        {
            Map.GenerateRandMap();
        }
    }
}
