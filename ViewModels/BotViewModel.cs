using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace upeko.ViewModels
{
    public class BotViewModel : ViewModelBase
    {
        private readonly BotListViewModel _parent;
        private readonly BotItemViewModel _botItem;
        
        private string _name;
        private Bitmap _iconSource;
        private string _version;
        private string _location;
        private string _status;
        private string _consoleOutput;
        private string _lastUpdated;
        private bool _autoStart;
        
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        
        public Bitmap IconSource
        {
            get => _iconSource;
            set => this.RaiseAndSetIfChanged(ref _iconSource, value);
        }
        
        public string Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }
        
        public string Location
        {
            get => _location;
            set => this.RaiseAndSetIfChanged(ref _location, value);
        }
        
        public string Status
        {
            get => _status;
            set
            {
                this.RaiseAndSetIfChanged(ref _status, value);
                // When status changes, update the bot item status
                if (_botItem != null)
                {
                    _botItem.Status = value;
                    // Notify that StatusColor has changed
                    this.RaisePropertyChanged(nameof(StatusColor));
                }
            }
        }
        
        public IBrush StatusColor
        {
            get
            {
                return Status switch
                {
                    "Running" => new SolidColorBrush(Color.Parse("#107C10")), // Green
                    "Stopped" => new SolidColorBrush(Color.Parse("#797775")), // Gray
                    "Error" => new SolidColorBrush(Color.Parse("#D83B01")),   // Red
                    "Updating..." => new SolidColorBrush(Color.Parse("#0078D7")), // Blue
                    _ => new SolidColorBrush(Color.Parse("#797775"))          // Default gray
                };
            }
        }
        
        public string ConsoleOutput
        {
            get => _consoleOutput;
            set => this.RaiseAndSetIfChanged(ref _consoleOutput, value);
        }
        
        public string LastUpdated
        {
            get => _lastUpdated;
            set => this.RaiseAndSetIfChanged(ref _lastUpdated, value);
        }
        
        public bool AutoStart
        {
            get => _autoStart;
            set => this.RaiseAndSetIfChanged(ref _autoStart, value);
        }
        
        public string RunButtonText => Status == "Running" ? "Stop" : "Start";
        
        public IBrush RunButtonBackground => Status == "Running" 
            ? new SolidColorBrush(Color.Parse("#D83B01")) // Red for stop
            : new SolidColorBrush(Color.Parse("#107C10")); // Green for start
        
        public IBrush RunButtonForeground => new SolidColorBrush(Colors.White);
        
        public string RunButtonIcon => Status == "Running" 
            ? "M14,19H18V5H14M6,19H10V5H6V19Z" // Stop icon
            : "M8,5.14V19.14L19,12.14L8,5.14Z"; // Play icon
        
        public ICommand BackCommand { get; }
        public ICommand ToggleRunningCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand OpenFolderCommand { get; }
        
        public BotViewModel(BotListViewModel parent, BotItemViewModel botItem)
        {
            _parent = parent;
            _botItem = botItem;
            
            // Copy properties from the bot item
            _name = botItem.Name;
            _iconSource = botItem.IconSource;
            _version = botItem.Version;
            _location = botItem.Location;
            _status = botItem.Status;
            
            // Initialize default values
            _lastUpdated = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss");
            _consoleOutput = "Welcome to Bot Console\n\n" +
                             "> Bot initialized\n" +
                             "> Version: " + _version + "\n" +
                             "> Status: " + _status + "\n\n" +
                             "Type 'help' for available commands.\n";
            _autoStart = false;
            
            // Initialize commands
            BackCommand = ReactiveCommand.Create(ExecuteBack);
            ToggleRunningCommand = ReactiveCommand.Create(ExecuteToggleRunning);
            UpdateCommand = ReactiveCommand.Create(ExecuteUpdate);
            OpenFolderCommand = ReactiveCommand.Create(ExecuteOpenFolder);
        }
        
        private void ExecuteBack()
        {
            _parent.NavigateBack();
        }
        
        private void ExecuteToggleRunning()
        {
            if (Status == "Running")
            {
                // Stop the bot
                Status = "Stopped";
                ConsoleOutput += "\n> Bot stopped at " + DateTime.Now.ToString("HH:mm:ss") + "\n";
                this.RaisePropertyChanged(nameof(RunButtonText));
                this.RaisePropertyChanged(nameof(RunButtonBackground));
                this.RaisePropertyChanged(nameof(RunButtonIcon));
            }
            else
            {
                // Start the bot
                Status = "Running";
                ConsoleOutput += "\n> Bot started at " + DateTime.Now.ToString("HH:mm:ss") + "\n";
                this.RaisePropertyChanged(nameof(RunButtonText));
                this.RaisePropertyChanged(nameof(RunButtonBackground));
                this.RaisePropertyChanged(nameof(RunButtonIcon));
            }
        }
        
        private void ExecuteUpdate()
        {
            // Logic to update the bot
            Status = "Updating...";
            ConsoleOutput += "\n> Updating bot...\n";
            
            // Simulate update process
            // In a real application, you would use async/await here
            Status = "Stopped";
            Version = "1.0.1";
            _botItem.Version = "1.0.1";
            LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ConsoleOutput += "> Update completed. New version: " + Version + "\n";
            this.RaisePropertyChanged(nameof(RunButtonText));
            this.RaisePropertyChanged(nameof(RunButtonBackground));
            this.RaisePropertyChanged(nameof(RunButtonIcon));
        }
        
        private void ExecuteOpenFolder()
        {
            try
            {
                // Open the folder in explorer
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = _location,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ConsoleOutput += "\n> Error opening folder: " + ex.Message + "\n";
            }
        }
    }
}
