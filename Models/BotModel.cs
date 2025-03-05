using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace upeko.Models
{
    [INotifyPropertyChanged]
    public partial class BotModel
    {
        [ObservableProperty]
        private Guid _guid;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private Uri? _iconUri;

        [ObservableProperty]
        private string? _version;

        [ObservableProperty]
        private Uri? _pathUri;

        public BotModel()
        {
            _guid = Guid.NewGuid();
            _name = "New Bot";
            _iconUri = null;
            _version = null;
            _pathUri = null;
        }
    }
}