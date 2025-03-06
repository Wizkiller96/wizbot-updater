using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Interactivity;
using ReactiveUI;
using upeko.Services;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using AsyncImageLoader;
using upeko.Models;

namespace upeko.ViewModels
{
    /// <summary>
    /// ViewModel for bot management and interaction
    /// </summary>
    public partial class BotViewModel : ViewModelBase
    {
        #region Properties

        private BotModel _bot;

        public BotModel Bot
            => _bot;

        public string? BotIcon
        {
            get => _bot.IconUri?.ToString();
            set
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                {
                    _bot.IconUri = uri;
                    Parent.UpdateBot(_bot);

                    this.RaisePropertyChanged();
                }
            }
        }

        public string BotPath
        {
            get => Bot.PathUri?.LocalPath ?? string.Empty;
            set
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                {
                    _bot.PathUri = uri;
                    Parent.UpdateBot(_bot);

                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(ExecutablePath));
                }
            }
        }

        public string Name
            => Bot.Name;

        public string ExecutablePath
            => Path.Combine(BotPath, PlatformSpecific.GetExecutableName());

        private ReleaseModel _latestRelease;

        public ReleaseModel LatestReleaseModel
        {
            get => _latestRelease;
            set => this.RaiseAndSetIfChanged(ref _latestRelease, value);
        }

        private MainActivityState _state;

        public MainActivityState State
        {
            get => _state;
            set
            {
                this.RaiseAndSetIfChanged(ref _state, value);
                this.RaisePropertyChanged(nameof(IsRunning));
                this.RaisePropertyChanged(nameof(IsBotDownloaded));
                this.RaisePropertyChanged(nameof(IsNotDownloading));
                this.RaisePropertyChanged(nameof(IsDownloading));
                this.RaisePropertyChanged(nameof(UpdateButtonText));
                this.RaisePropertyChanged(nameof(IsUpdateAvailable));
                this.RaisePropertyChanged(nameof(CanStart));
                this.RaisePropertyChanged(nameof(CanStop));
            }
        }

        public BotListViewModel Parent { get; }

        private Process? _process;

        public bool IsRunning
            => _process != null;

        public bool CanStart
            => !IsRunning && (State == MainActivityState.Runnable || State == MainActivityState.Updatable);

        public bool CanStop
            => IsRunning && State == MainActivityState.Running;

        public bool IsUpdateAvailable
            => UpdateChecker.Instance.IsUpdateAvailable(Bot?.Version);

        public bool IsBotDownloaded
            => !string.IsNullOrWhiteSpace(Bot?.Version);

        public string UpdateButtonText
            => IsBotDownloaded ? "Update" : "Download";

        private bool _isDownloading;

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                this.RaiseAndSetIfChanged(ref _isDownloading, value);
                this.RaisePropertyChanged(nameof(IsNotDownloading));
            }
        }

        public bool IsNotDownloading
            => !IsDownloading;

        private double _downloadProgress;

        public double DownloadProgress
        {
            get => _downloadProgress;
            set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
        }

        private string _downloadStatus = string.Empty;

        public string DownloadStatus
        {
            get => _downloadStatus;
            set => this.RaiseAndSetIfChanged(ref _downloadStatus, value);
        }

        private string _consoleOutput = string.Empty;

        public string ConsoleOutput
        {
            get => _consoleOutput;
            set => this.RaiseAndSetIfChanged(ref _consoleOutput, value);
        }

        private bool _deleteConfirm;

        public bool DeleteConfirm
        {
            get => _deleteConfirm;
            set => this.RaiseAndSetIfChanged(ref _deleteConfirm, value);
        }

        public string RunButtonText
            => IsRunning ? "Stop" : "Start";

        public string RunButtonIcon
            => IsRunning
                ? "M14,19H18V5H14M6,19H10V5H6V19Z" // Stop icon
                : "M8,5.14V19.14L19,12.14L8,5.14Z"; // Play icon

        public IBrush RunButtonBackground
            => IsRunning
                ? new SolidColorBrush(Color.Parse("#E74C3C")) // Red for stop
                : new SolidColorBrush(Color.Parse("#2ECC71")); // Green for start


        public string Status
            => State switch
            {
                MainActivityState.Running => "Running",
                MainActivityState.Runnable => "Ready",
                MainActivityState.Updatable => "Update Available",
                MainActivityState.Downloadable => "Not Downloaded",
                _ => "Unknown"
            };

        public IBrush StatusColor
            => State switch
            {
                MainActivityState.Running => new SolidColorBrush(Color.Parse("#2ECC71")), // Green
                MainActivityState.Runnable => new SolidColorBrush(Color.Parse("#3498DB")), // Blue
                MainActivityState.Updatable => new SolidColorBrush(Color.Parse("#F39C12")), // Orange
                MainActivityState.Downloadable => new SolidColorBrush(Color.Parse("#95A5A6")), // Gray
                _ => new SolidColorBrush(Color.Parse("#95A5A6")) // Gray
            };

        public string? Version
            => Bot?.Version;

        public IAsyncImageLoader ImageLoader { get; }

        #endregion

        #region Commands

        public ICommand BackCommand { get; }
        public ICommand SelectBotPathCommand { get; }
        public ICommand DeleteIntentCommand { get; }
        public ICommand DeleteBotCommand { get; }
        public ICommand DeleteCancelCommand { get; }
        public ICommand OpenDataFolderCommand { get; }
        public ICommand OpenCredsFileCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand CheckForUpdatesCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand SelectAvatarCommand { get; }

        #endregion

        #region Constructor

        public BotViewModel()
        {
            ImageLoader = App.Services.GetService(typeof(IAsyncImageLoader)) as IAsyncImageLoader
                          ?? throw new InvalidOperationException("Failed to resolve IImageLoaderService");

            DeleteIntentCommand = ReactiveCommand.Create(ExecuteDeleteIntentCommand);
            DeleteBotCommand = ReactiveCommand.Create(ExecuteDeleteBotCommand);
            DeleteCancelCommand = ReactiveCommand.Create(ExecuteDeleteCancelCommand);
            OpenDataFolderCommand = ReactiveCommand.Create(OpenDataClick);
            OpenCredsFileCommand = ReactiveCommand.Create(OpenCredsClick);
            StartCommand = ReactiveCommand.Create(ExecuteStartCommand);
            StopCommand = ReactiveCommand.Create(ExecuteStopCommand);
            CheckForUpdatesCommand = ReactiveCommand.Create(ExecuteCheckForUpdatesCommand);
            UpdateCommand = ReactiveCommand.Create(UpdateButtonClick);
            BackCommand = ReactiveCommand.Create(ExecuteBackCommand);
            SelectBotPathCommand = ReactiveCommand.Create(ExecuteSelectBotPathCommand);
            SelectAvatarCommand = ReactiveCommand.Create(ExecuteSelectAvatarCommand);


            UpdateChecker.Instance.OnNewVersionFound += OnNewVersionFound;
            UpdateChecker.Instance.OnDownloadProgress += OnDownloadProgress;
            UpdateChecker.Instance.OnDownloadComplete += OnDownloadComplete;
        }

        public BotViewModel(BotListViewModel parent, BotModel model) : this()
        {
            _bot = model;
            Parent = parent;
            ReloadVersionFromPath();
            UpdateCurrentActivity();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks whether there's a valid Bot file inside the selected folder, and updates the version if there is
        /// </summary>
        public void ReloadVersionFromPath()
        {
            if (string.IsNullOrWhiteSpace(BotPath))
                return;

            if (!File.Exists(ExecutablePath))
            {
                Bot.Version = null;
                return;
            }

            try
            {
                Debug.WriteLine(ExecutablePath);
                using var p = Process.Start(new ProcessStartInfo()
                {
                    FileName = ExecutablePath,
                    Arguments = "--version",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                });

                var info = p.StandardOutput.ReadToEnd();
                Bot.Version = info.Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Bot.Version = null;
                return;
            }

            UpdateCurrentActivity();
        }

        /// <summary>
        /// Updates the current activity state based on the bot's status
        /// </summary>
        private void UpdateCurrentActivity()
        {
            if (!IsBotDownloaded)
                State = MainActivityState.Downloadable;
            else if (IsRunning)
                State = MainActivityState.Running;
            else if (IsUpdateAvailable)
                State = MainActivityState.Updatable;
            else
                State = MainActivityState.Runnable;
        }

        /// <summary>
        /// Called when a new version is found by the UpdateChecker
        /// </summary>
        private void OnNewVersionFound(ReleaseModel newVer)
        {
            LatestReleaseModel = newVer;
            UpdateCurrentActivity();
        }

        /// <summary>
        /// Downloads the latest version and installs it
        /// </summary>
        private async Task DownloadAndInstallBotAsync()
        {
            try
            {
                // Reset download state
                IsDownloading = true;
                DownloadProgress = 0;
                DownloadStatus = "Preparing to download...";

                // Start the download
                await UpdateChecker.Instance.DownloadAndInstallBotAsync(
                    Bot.Name,
                    BotPath
                );

                IsDownloading = false;
                ReloadVersionFromPath();
                UpdateCurrentActivity();

                // Success will be handled by the OnDownloadComplete handler
            }
            catch (Exception ex)
            {
                // Handle failure directly if exception occurs
                IsDownloading = false;
                DownloadStatus = $"Error: {ex.Message}";

                // Log the exception
                Console.WriteLine($"Error downloading installer: {ex}");
            }
        }

        /// <summary>
        /// Called to delete this bot from the updater
        /// </summary>
        /// <param name="wipe">Whether to wipe the folder from the disk too.</param>
        public void Delete(bool wipe)
        {
            if (string.IsNullOrWhiteSpace(BotPath))
                return;

            Parent.RemoveBot(this);
        }

        /// <summary>
        /// Download the bot, if able
        /// </summary>
        public async Task Download()
        {
            if (State != MainActivityState.Downloadable && !IsUpdateAvailable)
            {
                throw new InvalidOperationException("Bot is already downloaded");
            }

            UpdateCurrentActivity();
            await DownloadAndInstallBotAsync();
        }

        /// <summary>
        /// Update the bot, if able
        /// </summary>
        public async Task Update()
        {
            if (State != MainActivityState.Updatable && State != MainActivityState.Downloadable)
            {
                throw new InvalidOperationException("No update is available.");
            }

            UpdateCurrentActivity();
            await DownloadAndInstallBotAsync();
        }

        /// <summary>
        /// Run the bot, if able
        /// </summary>
        public void Run()
        {
            if (State != MainActivityState.Runnable && State != MainActivityState.Updatable)
            {
                throw new InvalidOperationException(
                    "You can't run this bot. You either haven't downloaded it, or it's already running.");
            }

            if (string.IsNullOrEmpty(ExecutablePath) || !File.Exists(ExecutablePath))
            {
                throw new InvalidOperationException("Executable path is invalid or does not exist.");
            }

            _process = Process.Start(new ProcessStartInfo
            {
                FileName = ExecutablePath,
                WorkingDirectory = BotPath,
            });

            var p = _process;
            _ = Task.Run(async () =>
            {
                await p.WaitForExitAsync();
                // Use dispatcher to update UI thread
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _process = null;
                    UpdateCurrentActivity();
                });

                p.Dispose();
            });

            UpdateCurrentActivity();
        }

        /// <summary>
        /// Stop the bot, if running
        /// </summary>
        public void Stop()
        {
            if (State != MainActivityState.Running)
            {
                throw new InvalidOperationException("You can't stop the bot, it's not running.");
            }

            using var p = _process;
            _process = null;
            try
            {
                p.Kill();
            }
            catch
            {
            }

            UpdateCurrentActivity();
        }

        #endregion

        #region UI Event Handlers

        private void UpdateButtonClick()
        {
            if (IsUpdateAvailable)
            {
                _ = Update();
            }
            else if (!IsBotDownloaded)
            {
                _ = Download();
            }
        }

        private void ExecuteStartCommand()
        {
            Run();
        }

        private void ExecuteStopCommand()
        {
            Stop();
        }

        private async Task OnUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            // This method appears to be referencing UI elements that aren't defined in the ViewModel
            // Keeping the method signature for compatibility, but implementation should be adjusted
            await UpdateChecker.Instance.CheckForUpdatesAsync();
        }

        private void OpenCredsClick()
        {
            if (string.IsNullOrWhiteSpace(BotPath))
                return;

            var credsFile = Path.Combine(BotPath, "data", "creds.yml");
            var credsExampleFile = Path.Combine(BotPath, "data", "creds_example.yml");
            if (!File.Exists(credsFile))
            {
                if (!File.Exists(credsExampleFile))
                {
                    return;
                }

                File.Copy(credsExampleFile, credsFile);
            }

            Process.Start(PlatformSpecific.GetNameOfFileExplorer(), Path.GetFullPath(credsFile));
        }

        private void OpenDataClick()
        {
            if (string.IsNullOrWhiteSpace(BotPath))
                return;

            var dataFolder = Path.Combine(BotPath, "data");
            if (!Directory.Exists(dataFolder))
            {
                return;
            }

            Process.Start(PlatformSpecific.GetNameOfFileExplorer(), dataFolder);
        }

        private void ExecuteBackCommand()
        {
            Parent?.NavigateBack();
        }

        private async Task ExecuteSelectBotPathCommand()
        {
            var dialog = new OpenFolderDialog()
            {
                Directory = BotPath
            };

            var result = await dialog.ShowAsync(App.MainWindow);

            if (result == null)
                return;

            BotPath = result;
        }

        private void ExecuteDeleteIntentCommand()
        {
            DeleteConfirm = true;
        }

        private void ExecuteDeleteBotCommand()
        {
            Delete(true);
            DeleteConfirm = false;
        }

        private void ExecuteDeleteCancelCommand()
        {
            DeleteConfirm = false;
        }

        private bool _canCheckForUpdates = true;

        public bool CanCheckForUpdates
        {
            get => _canCheckForUpdates;
            set => this.RaiseAndSetIfChanged(ref _canCheckForUpdates, value);
        }

        private async Task ExecuteCheckForUpdatesCommand()
        {
            CanCheckForUpdates = false;
            try
            {
                await UpdateChecker.Instance.CheckForUpdatesAsync();
            }
            finally
            {
                CanCheckForUpdates = true;
            }
        }

        private void OnDownloadProgress(double progress, string status)
        {
            DownloadProgress = progress;
            DownloadStatus = status;
            IsDownloading = true;
        }

        private void OnDownloadComplete(bool success, string version)
        {
            IsDownloading = false;

            if (success)
            {
                // Reload the version information
                ReloadVersionFromPath();
            }
            else
            {
                // Handle download failure
                DownloadStatus = $"Download failed: {version}";
            }
        }

        private async void ExecuteSelectAvatarCommand()
        {
            var dialogService = new DialogService();
            var selectedImage = await dialogService.ShowImagePickerAsync("Select Avatar Image");

            if (!string.IsNullOrEmpty(selectedImage))
            {
                // Convert local file path to a file:// URI
                var fileUri = new Uri(selectedImage).ToString();
                BotIcon = fileUri;

                // Save the changes to persist the new icon path
                Parent?.UpdateBot(Bot);
            }
        }

        #endregion
    }
}