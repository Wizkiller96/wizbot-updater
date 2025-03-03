using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace upeko.Models
{
    [INotifyPropertyChanged]
    public partial class BotModel
    {
        private string Id { get; init; }

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private Bitmap _icon;

        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private string _location;

        [ObservableProperty]
        private bool _autoStart;

        public BotModel()
        {
            Id = Guid.NewGuid().ToString();
            _name = "New Bot";
            _icon = null;
            _version = "1.0.0";
            _location = string.Empty;
            _autoStart = false;
        }

        public BotModel(string name, string? location = null)
        {
            Id = Guid.NewGuid().ToString();
            _name = name;
            _icon = null;
            _version = "1.0.0";
            _location = location ?? $"C:\\Bots\\{name.Replace(" ", "")}";
            _autoStart = false;
        }
    }
}