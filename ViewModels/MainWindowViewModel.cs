using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using ReactiveUI;
using upeko.Views;
using System.Threading.Tasks;
using AsyncImageLoader.Loaders;
using upeko.Services;
using AsyncImageLoader;
using System;

namespace upeko.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly FfmpegDepViewModel _ffmpegViewModel;
    private readonly YtdlDepViewModel _ytdlpViewModel;
    private readonly IAsyncImageLoader _imageLoaderService;
    private const string ChangelogUrl = "https://github.com/nadeko-bot/nadekobot/blob/v6/CHANGELOG.md";
    private bool _isDarkTheme;

    public BotListViewModel Bots { get; } = new();

    public FfmpegDepViewModel FfmpegViewModel
    {
        get => _ffmpegViewModel;
    }

    public YtdlDepViewModel YtDlpViewModel
    {
        get => _ytdlpViewModel;
    }
    
    public IAsyncImageLoader ImageLoader => _imageLoaderService;

    public ICommand OpenChangelogCommand { get; }
    public ICommand ToggleThemeCommand { get; }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set => this.RaiseAndSetIfChanged(ref _isDarkTheme, value);
    }

    public string ThemeButtonText => IsDarkTheme ? "Light Theme" : "Dark Theme";

    public MainWindowViewModel()
    {
        // Get the ImageLoaderService from the dependency injection container
        _imageLoaderService = App.Services.GetService(typeof(IAsyncImageLoader)) as IAsyncImageLoader
                              ?? throw new System.InvalidOperationException("Failed to resolve IAsyncImageLoader");
                              
        // Initialize view models
        _ffmpegViewModel = new FfmpegDepViewModel();
        _ytdlpViewModel = new YtdlDepViewModel();

        // Initialize the theme state (default to light theme)
        _isDarkTheme = Application.Current!.RequestedThemeVariant == ThemeVariant.Dark;

        // Initialize commands
        OpenChangelogCommand = ReactiveCommand.Create(OpenChangelog);
        ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);

        // Run the check methods for ffmpeg and ytdlp view models when the app starts
        // Use Dispatcher to ensure UI updates happen on the UI thread
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Add a small delay to ensure the UI is fully loaded
            await Task.Delay(1000);

            // Run the checks
            await _ffmpegViewModel.CheckAsync();
            await _ytdlpViewModel.CheckAsync();
        });

        // Set up property changed notification for ThemeButtonText when IsDarkTheme changes
        this.WhenAnyValue(x => x.IsDarkTheme)
            .Subscribe(darkTheme => this.RaisePropertyChanged(nameof(ThemeButtonText)));
    }

    private void OpenChangelog()
    {
        var psi = new ProcessStartInfo
        {
            FileName = ChangelogUrl,
            UseShellExecute = true
        };
        
        Process.Start(psi);
    }

    private void ToggleTheme()
    {
        // Toggle the theme state
        IsDarkTheme = !IsDarkTheme;

        // Apply the theme change at the application level
        var newTheme = IsDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
        Application.Current!.RequestedThemeVariant = newTheme;
    }
}