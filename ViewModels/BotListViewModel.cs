using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using upeko.Models;
using upeko.Services;
using System.IO;
using AsyncImageLoader.Loaders;
using AsyncImageLoader;

namespace upeko.ViewModels;

public class BotListViewModel : ViewModelBase
{
    private readonly IBotRepository _botRepository;
    private ObservableCollection<BotViewModel?> _items;

    public ObservableCollection<BotViewModel?> Items
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

    private readonly IAsyncImageLoader _imageLoaderService;

    public IAsyncImageLoader ImageLoader
        => _imageLoaderService;


    public BotListViewModel()
    {
        // Get the bot repository from the service provider
        _botRepository = App.Services.GetService(typeof(IBotRepository)) as IBotRepository
                         ?? throw new InvalidOperationException("Failed to resolve IBotRepository service");

        // Get the ImageLoaderService from the dependency injection container
        _imageLoaderService = App.Services.GetService(typeof(IAsyncImageLoader)) as IAsyncImageLoader
                              ?? throw new InvalidOperationException("Failed to resolve IImageLoaderService");

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
            var botViewModel = new BotViewModel(this, botModel);
            _items.Add(botViewModel);
            
            // Create a BotItemViewModel that wraps the BotViewModel
            var botItemViewModel = new BotItemViewModel(botViewModel);
            _allItems.Add(botItemViewModel);
        }

        // Add the button as the last item
        _allItems.Add(new AddButtonViewModel(this));
    }

    public void AddNewBot()
    {
        var guid = Guid.NewGuid();
        var first5 = guid.ToString().Substring(0, 5);
        var botName = $"bot-{first5}";

        // Create a default path in the user's documents folder
        string defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "upeko",
            botName);

        // Create a new bot model
        var botModel = new BotModel()
        {
            Guid = guid,
            Name = botName,
            PathUri = new Uri(defaultPath, UriKind.Absolute)
        };

        // Add the bot to the repository
        _botRepository.AddBot(botModel);

        // Create a view model from the model
        var botViewModel = new BotViewModel(this, botModel);
        _items.Add(botViewModel);
        
        // Create a BotItemViewModel that wraps the BotViewModel
        var botItemViewModel = new BotItemViewModel(botViewModel);
        _allItems.Insert(_allItems.Count - 1, botItemViewModel);
    }

    public void OpenBotView(BotViewModel botViewModel)
    {
        // Set it as the current page
        CurrentPage = botViewModel;
    }

    public void NavigateBack()
    {
        // Navigate back to the bot list
        CurrentPage = this;
    }

    public void UpdateBot(BotModel bot)
    {
        // Update the bot in the repository
        _botRepository.UpdateBot(bot);
    }

    public void RemoveBot(BotViewModel botViewModel)
    {
        // Remove the bot from the repository
        _botRepository.RemoveBot(botViewModel.Bot);

        // Remove from the collections
        _items.Remove(botViewModel);
        
        // Find and remove the corresponding BotItemViewModel
        var itemToRemove = _allItems.OfType<BotItemViewModel>()
            .FirstOrDefault(x => ReferenceEquals(x.BotViewModel, botViewModel));
            
        if (itemToRemove != null)
        {
            _allItems.Remove(itemToRemove);
            
            // Dispose the item view model to clean up subscriptions
            if (itemToRemove is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        CurrentPage = this;
    }
}