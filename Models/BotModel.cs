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

        [ObservableProperty]
        private bool _autoStart;

        public BotModel()
        {
            _guid = Guid.NewGuid();
            _name = "New Bot";
            _iconUri = null;
            _version = null;
            _pathUri = null;
            _autoStart = false;
        }

        /// <summary>
        /// Moves the bot and all files from the current location to a new location
        /// Data in the new location will be overwritten
        /// </summary>
        /// <param name="location"></param>
        public void Move(string location)
        {
            if (!Uri.TryCreate(location, UriKind.Absolute, out var uri))
                return;

            if (Directory.Exists(location))
            {
                Directory.Delete(location, true);
            }

            if (PathUri is not null && Directory.Exists(PathUri.ToString()))
            {
                Directory.Move(PathUri.ToString(), location);
            }

            PathUri = uri;
        }
    }
}