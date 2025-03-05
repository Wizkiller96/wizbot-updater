using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;
using upeko.Models;
using upeko.Services;

namespace upeko.ViewModels
{
    public class BotViewModel : ViewModelBase
    {
        private readonly BotListViewModel _parent;
        private readonly BotItemViewModel _botItem;
        private readonly BotModel _model;
        private readonly UpdateChecker _updateChecker;
        private readonly IDialogService _dialogService;

        private string _consoleOutput;
        private bool _isDownloading;
        private string _downloadStatus;
        private double _downloadProgress;

        public enum MainActivityState
        {
            Running,
            Runnable,
            Updatable,
            Downloadable
        }

        public string Name
        {
            get => _model.Name;
            set
            {
                if (_model.Name != value)
                {
                    _model.Name = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public string Icon
        {
            get => _botItem.Icon;
        }

        public string Version
        {
            get => _model.Version ?? "None";
            set
            {
                if (_model.Version != value)
                {
                    _model.Version = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(IsUpdateAvailable));
                    this.RaisePropertyChanged(nameof(IsBotDownloaded));
                    this.RaisePropertyChanged(nameof(UpdateButtonText));
                    this.RaisePropertyChanged(nameof(CanStartBot));
                    this.RaisePropertyChanged(nameof(State));
                }
            }
        }

        public bool IsUpdateAvailable
        {
            get => _updateChecker.IsUpdateAvailable(_model.Version);
        }

        public bool IsBotDownloaded
        {
            get => _model.Version != null;
        }

        public bool CanStartBot
        {
            get => State != MainActivityState.Running && !IsDownloading;
        }

        public string UpdateButtonText
        {
            get => IsBotDownloaded ? "Update" : "Download";
        }

        public bool UpdateAvailable
        {
            get => _botItem.UpdateAvailable;
            set
            {
                if (_botItem.UpdateAvailable != value)
                {
                    _botItem.UpdateAvailable = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public Uri? BotPathUri
        {
            get => _model.PathUri;
        }

        public string? BotPath
        {
            get => _model.PathUri?.ToString();
            set
            {
                if (_model.PathUri?.ToString() != value
                    && Uri.TryCreate(value, UriKind.Absolute, out var uri))
                {
                    _model.PathUri = uri;
                    this.RaisePropertyChanged();
                }
            }
        }

        public string? ExecutablePath
        {
            get => _model.PathUri != null ? System.IO.Path.Combine(_model.PathUri.LocalPath, "upeko") : null;
        }

        public string Status
        {
            get => _botItem.Status;
            set
            {
                if (_botItem.Status != value)
                {
                    _botItem.Status = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(StatusColor));
                    this.RaisePropertyChanged(nameof(RunButtonText));
                    this.RaisePropertyChanged(nameof(RunButtonBackground));
                    this.RaisePropertyChanged(nameof(RunButtonIcon));
                    this.RaisePropertyChanged(nameof(CanStartBot));
                    this.RaisePropertyChanged(nameof(State));
                }
            }
        }

        public MainActivityState State
        {
            get
            {
                // If the bot is running, it's in the Running state
                if (Status == "Running")
                    return MainActivityState.Running;
                
                // If the bot is not downloaded (version is null), it's in the Downloadable state
                if (_model.Version == null)
                    return MainActivityState.Downloadable;
                
                // If an update is available, it's in the Updatable state, otherwise it's Runnable
                return IsUpdateAvailable ? MainActivityState.Updatable : MainActivityState.Runnable;
            }
        }

        public IBrush StatusColor
        {
            get
            {
                return Status switch
                {
                    "Running" => new SolidColorBrush(Color.Parse("#107C10")), // Green
                    "Stopped" => new SolidColorBrush(Color.Parse("#FFB900")), // Yellow
                    "Error" => new SolidColorBrush(Color.Parse("#D83B01")), // Red
                    "Updating..." => new SolidColorBrush(Color.Parse("#0078D7")), // Blue
                    "Downloading..." => new SolidColorBrush(Color.Parse("#0078D7")), // Blue
                    _ => new SolidColorBrush(Color.Parse("#797775")) // Default gray
                };
            }
        }

        public string ConsoleOutput
        {
            get => _consoleOutput;
            set => this.RaiseAndSetIfChanged(ref _consoleOutput, value);
        }

        public bool AutoStart
        {
            get => _model.AutoStart;
            set
            {
                if (_model.AutoStart != value)
                {
                    _model.AutoStart = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public string RunButtonText
            => State == MainActivityState.Running ? "Stop" : "Start";

        public IBrush RunButtonBackground
            => State == MainActivityState.Running
                ? new SolidColorBrush(Color.Parse("#D83B01")) // Red for stop
                : new SolidColorBrush(Color.Parse("#107C10")); // Green for start

        public IBrush RunButtonForeground
            => new SolidColorBrush(Colors.White);
        

        public string RunButtonIcon
            => State == MainActivityState.Running
                ? "M14,19H18V5H14M6,19H10V5H6V19Z" // Stop icon
                : "M8,5.14V19.14L19,12.14L8,5.14Z"; // Play icon
        
        private bool _deleteConfirm = false;
        public bool DeleteConfirm
        {
            get => _deleteConfirm;
            set => this.RaiseAndSetIfChanged(ref _deleteConfirm, value);
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            private set
            {
                this.RaiseAndSetIfChanged(ref _isDownloading, value);
                this.RaisePropertyChanged(nameof(IsNotDownloading));
                this.RaisePropertyChanged(nameof(CanStartBot));
            }
        }

        public bool IsNotDownloading => !IsDownloading;

        public string DownloadStatus
        {
            get => _downloadStatus;
            private set => this.RaiseAndSetIfChanged(ref _downloadStatus, value);
        }

        public double DownloadProgress
        {
            get => _downloadProgress;
            private set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
        }

        public ICommand BackCommand { get; }
        public ICommand ToggleRunningCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand SelectBotPathCommand { get; }
        public ICommand DeleteIntentCommand { get; }
        public ICommand DeleteBotCommand { get; }
        public ICommand DeleteCancelCommand { get; }
        public ICommand CheckForUpdatesCommand { get; }
        public ICommand OpenCredsFileCommand { get; }
        public ICommand OpenDataFolderCommand { get; }

        public ReleaseModel? LatestRelease { get; private set; }

        public BotViewModel(BotListViewModel parent, BotItemViewModel botItem)
        {
            _parent = parent;
            _botItem = botItem;
            _model = botItem.Model;
            _updateChecker = UpdateChecker.Instance;
            _dialogService = App.Services.GetService(typeof(IDialogService)) as IDialogService
                           ?? throw new InvalidOperationException("Failed to resolve IDialogService");

            // Subscribe to update notifications
            _updateChecker.OnNewVersionFound += OnNewVersionFound;
            _updateChecker.OnDownloadProgress += OnDownloadProgress;
            _updateChecker.OnDownloadComplete += OnDownloadComplete;

            // Initialize console output
            _consoleOutput = "Welcome to NadekoBot\n\n" +
                             "> Version: " + (_model.Version ?? "Not installed") + "\n" +
                             "> Status: " + _botItem.Status + "\n\n";

            // Subscribe to model property changes
            _model.PropertyChanged += (_, args) =>
            {
                // When the model changes, raise property changed for the corresponding property
                this.RaisePropertyChanged(args.PropertyName);
            };

            // Initialize commands
            BackCommand = ReactiveCommand.Create(ExecuteBack);
            ToggleRunningCommand = ReactiveCommand.Create(ExecuteToggleRunning);
            UpdateCommand = ReactiveCommand.Create(() => _ = Update());
            SelectBotPathCommand = ReactiveCommand.CreateFromTask(ExecuteSelectBotPath);
            DeleteBotCommand = ReactiveCommand.Create(ExecuteDeleteBot);
            DeleteIntentCommand = ReactiveCommand.Create(ExecuteDeleteIntent);
            DeleteCancelCommand = ReactiveCommand.Create(ExecuteCancelDelete);
            CheckForUpdatesCommand = ReactiveCommand.CreateFromTask(ExecuteCheckForUpdates);
            OpenCredsFileCommand = ReactiveCommand.Create(ExecuteOpenCredsFile);
            OpenDataFolderCommand = ReactiveCommand.Create(ExecuteOpenDataFolder);

            // Check for updates when the view model is created
            _ = ExecuteCheckForUpdates();
        }

        private void OnNewVersionFound(ReleaseModel release)
        {
            // Update the UI to show that a new version is available
            UpdateAvailable = true;
            
            // Set the latest release
            LatestRelease = release;
            
            // Force UI update
            this.RaisePropertyChanged(nameof(IsUpdateAvailable));
            this.RaisePropertyChanged(nameof(UpdateButtonText));
            this.RaisePropertyChanged(nameof(State));
        }

        private void OnDownloadProgress(double progress, string status)
        {
            DownloadProgress = progress;
            DownloadStatus = status;
        }

        private void OnDownloadComplete(bool success, string message)
        {
            if (success)
            {
                Version = message; // Update the version with the downloaded version
                Status = "Stopped";
                UpdateAvailable = false; // Ensure the bot is in Runnable state
            }
            else
            {
                Status = "Error";
            }

            IsDownloading = false;
            this.RaisePropertyChanged(nameof(State));
            this.RaisePropertyChanged(nameof(IsUpdateAvailable));
        }
        
        private void ExecuteCancelDelete()
        {
            DeleteConfirm = false;
        }
        
        private void ExecuteDeleteIntent()
        {
            DeleteConfirm = true;
        }
        
        private void ExecuteDeleteBot()
        {
            _parent.RemoveBot(_botItem);
        }
        
        private void ExecuteBack()
        {
            _parent.NavigateBack();
        }

        private void ExecuteToggleRunning()
        {
            switch (State)
            {
                case MainActivityState.Running:
                    Stop();
                    break;
                case MainActivityState.Runnable:
                    Run();
                    break;
                case MainActivityState.Updatable:
                    Run();
                    break;
                case MainActivityState.Downloadable:
                    _ = Download();
                    break;
            }
        }

        private void Run()
        {
            // Start the bot
            Status = "Running";
        }

        private void Stop()
        {
            // Stop the bot
            Status = "Stopped";
        }

        private async Task ExecuteCheckForUpdates()
        {
            if (IsDownloading)
            {
                return; // Don't check for updates while downloading
            }
            
            string? error = await _updateChecker.CheckForUpdatesAsync();
            
            // If there's an update available, the OnNewVersionFound event will handle the UI update
        }

        private async Task ExecuteUpdate()
        {
            if (IsDownloading)
            {
                return; // Prevent multiple downloads
            }

            // Make sure the bot is stopped before updating
            if (State == MainActivityState.Running)
            {
                Stop(); // Stop the bot
            }

            await Update();
        }

        private async Task Update()
        {
            IsDownloading = true;
            Status = "Updating...";

            // Get the bot path from the model
            string? botPathStr = _model.PathUri?.LocalPath;
            if (string.IsNullOrEmpty(botPathStr))
            {
                IsDownloading = false;
                Status = "Error";
                return;
            }

            // Start the download and installation process
            string? error = await _updateChecker.DownloadAndInstallBotAsync(_model.Name, botPathStr);
            
            if (error != null)
            {
                Status = "Error";
                IsDownloading = false;
            }
            // The OnDownloadComplete event will handle the UI update when download is complete
        }

        private async Task Download()
        {
            IsDownloading = true;
            Status = "Downloading...";

            // Get the bot path from the model
            string? botPathStr = _model.PathUri?.LocalPath;
            if (string.IsNullOrEmpty(botPathStr))
            {
                IsDownloading = false;
                Status = "Error";
                return;
            }

            // Start the download and installation process
            string? error = await _updateChecker.DownloadAndInstallBotAsync(_model.Name, botPathStr);
            
            if (error != null)
            {
                Status = "Error";
                IsDownloading = false;
            }
            // The OnDownloadComplete event will handle the UI update when download is complete
        }

        private async Task ExecuteSelectBotPath()
        {
            try
            {
                // Get the current path or default to user documents folder
                string initialDirectory = _model.PathUri?.LocalPath ?? 
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "upeko");
                
                // Ensure the directory exists for the dialog
                System.IO.Directory.CreateDirectory(initialDirectory);
                
                // Show folder picker dialog
                string? selectedPath = await _dialogService.ShowFolderPickerAsync("Select Bot Location", initialDirectory);
                
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Update the bot path
                    BotPath = selectedPath;
                }
            }
            catch (Exception ex)
            {
                // Handle error silently
            }
        }

        private void ExecuteOpenCredsFile()
        {
            try
            {
                var credsFile = System.IO.Path.Combine(ExecutablePath ?? "", "data", "creds.yml");
                var credsExampleFile = System.IO.Path.Combine(ExecutablePath ?? "", "data", "creds_example.yml");
                
                if (!System.IO.File.Exists(credsFile))
                {
                    if (!System.IO.File.Exists(credsExampleFile))
                    {
                        return;
                    }

                    System.IO.File.Copy(credsExampleFile, credsFile);
                }

                // Open the file in the default editor
                Process.Start(new ProcessStartInfo
                {
                    FileName = PlatformSpecific.GetNameOfFileExplorer(),
                    Arguments = System.IO.Path.GetFullPath(credsFile),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Handle error silently
            }
        }

        private void ExecuteOpenDataFolder()
        {
            try
            {
                var dataFolder = System.IO.Path.Combine(_model.PathUri?.LocalPath ?? "", "data");
                
                if (!System.IO.Directory.Exists(dataFolder))
                {
                    return;
                }
                
                // Open the folder in explorer
                Process.Start(new ProcessStartInfo
                {
                    FileName = PlatformSpecific.GetNameOfFileExplorer(),
                    Arguments = System.IO.Path.GetFullPath(dataFolder),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
            }
        }
    }
}

public static class PlatformSpecific
{
    public static string GetNameOfFileExplorer()
    {
        return Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => "explorer.exe",
            PlatformID.Unix => "xdg-open",
            PlatformID.MacOSX => "open",
            _ => throw new PlatformNotSupportedException("Unsupported platform: " + Environment.OSVersion.Platform)
        };
    }
}