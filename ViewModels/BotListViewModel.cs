using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;
using upeko.Models;
using upeko.Services;

namespace upeko.ViewModels;

public class BotListViewModel : ViewModelBase
{
    private readonly IBotRepository _botRepository;
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

    public FfmpegDepViewModel FfmpegViewModel { get; } = new();
    public YtdlDepViewModel YtdlpViewModel { get; } = new();

    public BotListViewModel()
    {
        // Get the bot repository from the service provider
        _botRepository = App.Services.GetService(typeof(IBotRepository)) as IBotRepository
                         ?? throw new InvalidOperationException("Failed to resolve IBotRepository service");

        // Initialize collections
        _items = new();
        _allItems = new();

        // Load bots from the repository
        LoadBots();

        // Set the current page to this list view
        _currentPage = this;
    }

    private void LoadBots()
    {
        // Clear existing items
        _items.Clear();
        _allItems.Clear();

        // Get bots from the repository
        var botModels = _botRepository.GetBots();

        // Create view models for each bot model
        foreach (var botModel in botModels)
        {
            var botItemViewModel = new BotItemViewModel(botModel, this);
            _items.Add(botItemViewModel);
            _allItems.Add(botItemViewModel);
        }

        // Add the button as the last ite
        _allItems.Add(new AddButtonViewModel(this));
    }

    public void AddNewBot()
    {
        var botName = $"Bot {Guid.NewGuid().ToString().Substring(0, 5)}";

        // Create a new bot model
        var botModel = new BotModel()
        {
            Name = botName
        };

        // Add the bot to the repository
        _botRepository.AddBot(botModel);

        // Create a view model from the model
        var newBot = new BotItemViewModel(botModel, this);

        // Add to the items collection
        _items.Add(newBot);

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

    public void UpdateBot(BotItemViewModel botItem)
    {
        // Update the bot in the repository
        _botRepository.UpdateBot(botItem.Model);
    }

    public void RemoveBot(BotItemViewModel botItem)
    {
        // Remove the bot from the repository
        _botRepository.RemoveBot(botItem.Model);

        // Remove from the collections
        _items.Remove(botItem);
        _allItems.Remove(botItem);

        CurrentPage = this;
    }
}

public class BotItemViewModel : ViewModelBase
{
    private readonly BotModel _model;
    private BotListViewModel _parent;
    private string _status = "Stopped";

    public BotModel Model
        => _model;

    public string Name
    {
        get => _model.Name;
        set
        {
            if (_model.Name != value)
            {
                _model.Name = value;
                this.RaisePropertyChanged();
                _parent.UpdateBot(this);
            }
        }
    }

    public string? Icon
    {
        get => _model.IconUri?.ToString() ?? "https://cdn.nadeko.bot/other/av_blurred.png";
        set
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri != _model.IconUri)
            {
                _model.IconUri = uri;
                this.RaisePropertyChanged();
                _parent.UpdateBot(this);
            }
        }
    }

    public string? Version
    {
        get => _model.Version;
        set
        {
            if (_model.Version != value)
            {
                _model.Version = value;
                this.RaisePropertyChanged();
                _parent.UpdateBot(this);
            }
        }
    }

    public string? Location
    {
        get => _model.PathUri?.ToString();
        set
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && _model.PathUri != uri)
            {
                _model.PathUri = uri;
                this.RaisePropertyChanged();
                _parent.UpdateBot(this);
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
                "Error" => new SolidColorBrush(Color.Parse("#D83B01")), // Red
                "Updating..." => new SolidColorBrush(Color.Parse("#0078D7")), // Blue
                _ => new SolidColorBrush(Color.Parse("#797775")) // Default gray
            };
        }
    }

    public ICommand OpenBotCommand
        => ReactiveCommand.Create(ExecuteOpenBot);

    public BotItemViewModel(BotModel model, BotListViewModel parent)
    {
        _model = model;
        _parent = parent;

        // Subscribe to model property changes
        _model.PropertyChanged += (sender, args) =>
        {
            // When the model changes, raise property changed for the corresponding property
            this.RaisePropertyChanged(args.PropertyName);
        };
    }

    private void ExecuteOpenBot()
    {
        _parent.OpenBotView(this);
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