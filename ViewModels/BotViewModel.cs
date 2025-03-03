using System.Windows.Input;
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
        private string _platform;
        private string _architecture;
        private string _location;
        private string _status;
        
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
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }
        
        public ICommand BackCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand UpdateCommand { get; }
        
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
            
            // Initialize commands
            BackCommand = ReactiveCommand.Create(ExecuteBack);
            StartCommand = ReactiveCommand.Create(ExecuteStart);
            StopCommand = ReactiveCommand.Create(ExecuteStop);
            UpdateCommand = ReactiveCommand.Create(ExecuteUpdate);
        }
        
        private void ExecuteBack()
        {
            _parent.NavigateBack();
        }
        
        private void ExecuteStart()
        {
            // Logic to start the bot
            Status = "Running";
            _botItem.Status = "Running";
        }
        
        private void ExecuteStop()
        {
            // Logic to stop the bot
            Status = "Stopped";
            _botItem.Status = "Stopped";
        }
        
        private void ExecuteUpdate()
        {
            // Logic to update the bot
            Status = "Updating...";
            _botItem.Status = "Updating...";
            
            // Simulate update process
            // In a real application, you would use async/await here
            Status = "Stopped";
            _botItem.Status = "Stopped";
            Version = "1.0.1";
            _botItem.Version = "1.0.1";
        }
    }
}
