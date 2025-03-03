using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using upeko.Models;

namespace upeko.ViewModels;

public class BotListViewModel : ViewModelBase
{
    private ObservableCollection<BotItemViewModel?> _items;

    public ObservableCollection<BotItemViewModel?> Items
    {
        get => _items;
        set => this.RaiseAndSetIfChanged(ref _items, value);
    }

    private ObservableCollection<ViewModelBase> _allItems;
    
    public ObservableCollection<ViewModelBase> AllItems
    {
        get => _allItems;
        set => this.RaiseAndSetIfChanged(ref _allItems, value);
    }

    public ObservableCollection<bool> ButtonVisible { get; } = new(new() { true, false, false });
    
    private ViewModelBase _currentPage;
    
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public BotListViewModel()
    {
        // Create sample bot models
        var bot1 = new BotModel("Bot 1", "C:\\Bots\\Bot1");
        var bot2 = new BotModel("Bot 2", "C:\\Bots\\Bot2");
        var bot3 = new BotModel("Bot 3", "C:\\Bots\\Bot3");
        
        // Create view models from the models
        _items = new ObservableCollection<BotItemViewModel?>
        {
            new BotItemViewModel(bot1),
            new BotItemViewModel(bot2) { Status = "Running" },
            new BotItemViewModel(bot3)
        };
        
        // Create the combined collection with bot items and the add button
        _allItems = new ObservableCollection<ViewModelBase>();
        foreach (var item in _items)
        {
            if (item != null)
            {
                item.SetParent(this);
                _allItems.Add(item);
            }
        }
        
        // Add the button as the last item
        _allItems.Add(new AddButtonViewModel(this));
        
        // Set the current page to this list view
        _currentPage = this;
    }
    
    public void AddNewBot(string name = "")
    {
        // Create a new bot with an auto-generated name if none provided
        var botCount = _items.Count + 1;
        var botName = string.IsNullOrEmpty(name) ? $"Bot {botCount}" : name;
        
        // Create a new bot model
        var botModel = new BotModel(botName);
        
        // Create a view model from the model
        var newBot = new BotItemViewModel(botModel);
        
        // Add to the items collection
        _items.Add(newBot);
        
        // Insert into the AllItems collection before the add button
        // (which should be the last item)
        newBot.SetParent(this);
        _allItems.Insert(_allItems.Count - 1, newBot);
    }
    
    public void OpenBotView(BotItemViewModel botItem)
    {
        // Create a new BotViewModel and set it as the current page
        var botViewModel = new BotViewModel(this, botItem);
        CurrentPage = botViewModel;
    }
    
    public void NavigateBack()
    {
        // Navigate back to the bot list
        CurrentPage = this;
    }
}

public class BotItemViewModel : ViewModelBase
{
    private readonly BotModel _model;
    private BotListViewModel _parent;
    private string _status = "Stopped";
    
    public BotModel Model => _model;
    
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
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                this.RaisePropertyChanged();
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
    
    public ICommand OpenBotCommand => ReactiveCommand.Create(ExecuteOpenBot);
    
    public BotItemViewModel(BotModel model)
    {
        _model = model;
        
        // Subscribe to model property changes
        _model.PropertyChanged += (sender, args) =>
        {
            // When the model changes, raise property changed for the corresponding property
            this.RaisePropertyChanged(args.PropertyName);
        };
    }
    
    private void ExecuteOpenBot()
    {
        // Get the parent BotListViewModel
        if (_parent != null)
        {
            _parent.OpenBotView(this);
        }
    }
    
    // Called by the parent to set the reference
    public void SetParent(BotListViewModel parent)
    {
        _parent = parent;
    }
}

public class AddButtonViewModel : ViewModelBase
{
    private readonly BotListViewModel _botListViewModel;
    public ICommand AddCommand { get; }
    
    public AddButtonViewModel(BotListViewModel botListViewModel)
    {
        _botListViewModel = botListViewModel;
        AddCommand = ReactiveCommand.Create(ExecuteAdd);
    }
    
    private void ExecuteAdd()
    {
        // Add a new bot to the items list
        _botListViewModel.AddNewBot();
    }
}