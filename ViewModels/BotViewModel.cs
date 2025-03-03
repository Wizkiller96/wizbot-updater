using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using upeko.Models;

namespace upeko.ViewModels
{
    public class BotViewModel : ViewModelBase
    {
        private readonly BotListViewModel _parent;
        private readonly BotItemViewModel _botItem;
        private readonly BotModel _model;
        
        private string _consoleOutput;
        
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
        
        public Bitmap IconSource
        {
            get => _model.Icon;
            set
            {
                if (_model.Icon != value)
                {
                    _model.Icon = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        
        public string Version
        {
            get => _model.Version;
            set
            {
                if (_model.Version != value)
                {
                    _model.Version = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        
        public string Location
        {
            get => _model.Location;
            set
            {
                if (_model.Location != value)
                {
                    _model.Location = value;
                    this.RaisePropertyChanged();
                }
            }
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
            _model = botItem.Model;
            
            // Initialize console output
            _consoleOutput = "Welcome to Bot Console\n\n" +
                             "> Bot initialized\n" +
                             "> Version: " + _model.Version + "\n" +
                             "> Status: " + _botItem.Status + "\n\n" +
                             "Type 'help' for available commands.\n";
            
            // Subscribe to model property changes
            _model.PropertyChanged += (sender, args) =>
            {
                // When the model changes, raise property changed for the corresponding property
                this.RaisePropertyChanged(args.PropertyName);
            };
            
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
            }
            else
            {
                // Start the bot
                Status = "Running";
                ConsoleOutput += "\n> Bot started at " + DateTime.Now.ToString("HH:mm:ss") + "\n";
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
            ConsoleOutput += "> Update completed. New version: " + Version + "\n";
        }
        
        private void ExecuteOpenFolder()
        {
            try
            {
                // Open the folder in explorer
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = _model.Location,
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
