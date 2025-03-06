using System;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;
using upeko.Models;
using upeko.Services;

namespace upeko.ViewModels;

public class BotItemViewModel : ViewModelBase, IDisposable
{
    // Internal reference to the bot view model - accessible to BotListViewModel
    internal readonly BotViewModel BotViewModel;

    /// <summary>
    /// Gets the parent list view model
    /// </summary>
    public BotListViewModel Parent => BotViewModel.Parent;

    /// <summary>
    /// Gets the bot icon URL
    /// </summary>
    public string? Icon => BotViewModel.BotIcon ?? "https://cdn.nadeko.bot/other/av_blurred.png";

    /// <summary>
    /// Gets the bot version
    /// </summary>
    public string? Version => BotViewModel.Version;

    /// <summary>
    /// Gets the bot's location on disk
    /// </summary>
    public string? Location => BotViewModel.BotPath;

    /// <summary>
    /// Gets the bot's status
    /// </summary>
    public string Status => BotViewModel.Status;

    /// <summary>
    /// Gets the bot's name
    /// </summary>
    public string Name => BotViewModel.Name;

    /// <summary>
    /// Gets the color representing the bot's status
    /// </summary>
    public IBrush StatusColor => BotViewModel.StatusColor;

    /// <summary>
    /// Gets whether an update is available for this bot
    /// </summary>
    public bool UpdateAvailable => BotViewModel.IsUpdateAvailable;

    /// <summary>
    /// Gets the command to open the bot details view
    /// </summary>
    public ICommand OpenBotCommand { get; }

    /// <summary>
    /// Creates a new instance of the BotItemViewModel
    /// </summary>
    /// <param name="botViewModel">The bot view model to display</param>
    public BotItemViewModel(BotViewModel botViewModel)
    {
        BotViewModel = botViewModel ?? throw new ArgumentNullException(nameof(botViewModel));
        
        // Create the command to open the bot details
        OpenBotCommand = ReactiveCommand.Create(ExecuteOpenBot);
        
        // Subscribe to property changes from the BotViewModel
        BotViewModel.PropertyChanged += BotViewModel_PropertyChanged;
    }

    private void BotViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // When any property changes in the BotViewModel, raise property changed notifications
        // for all exposed properties in this view model
        this.RaisePropertyChanged(nameof(Icon));
        this.RaisePropertyChanged(nameof(Version));
        this.RaisePropertyChanged(nameof(Location));
        this.RaisePropertyChanged(nameof(Status));
        this.RaisePropertyChanged(nameof(Name));
        this.RaisePropertyChanged(nameof(StatusColor));
        this.RaisePropertyChanged(nameof(UpdateAvailable));
    }

    private void ExecuteOpenBot()
    {
        Parent.OpenBotView(BotViewModel);
    }
    
    /// <summary>
    /// Disposes resources used by this view model
    /// </summary>
    public void Dispose()
    {
        // Unsubscribe from events
        BotViewModel.PropertyChanged -= BotViewModel_PropertyChanged;
    }
}