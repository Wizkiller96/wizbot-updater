using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;

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
        _items = new(new()
        {
            new()
            {
                Name = "Bot 1",
                IconSource = null, // Default icon will be used
                Version = "1.0.0",
                Location = "C:\\Bots\\Bot1"
            },
            new()
            {
                Name = "Bot 2",
                IconSource = null, // Default icon will be used
                Version = "1.0.0",
                Location = "C:\\Bots\\Bot2"
            },
            new()
            {
                Name = "Bot 3",
                IconSource = null, // Default icon will be used
                Version = "1.0.0",
                Location = "C:\\Bots\\Bot3"
            }
        });
        
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
        
        var newBot = new BotItemViewModel
        {
            Name = botName,
            IconSource = null, // Default icon will be used
            Version = "1.0.0",
            Location = $"C:\\Bots\\{botName.Replace(" ", "")}"
        };
        
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
    private string _name = string.Empty;
    private Bitmap _iconSource;
    private string _version = string.Empty;
    private string _platform = string.Empty;
    private string _architecture = string.Empty;
    private string _location = string.Empty;
    private string _status = "Stopped";

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
    
    public ICommand OpenBotCommand => ReactiveCommand.Create(ExecuteOpenBot);
    
    private BotListViewModel _parent;
    
    public BotItemViewModel()
    {
        // This constructor is used for design-time data
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