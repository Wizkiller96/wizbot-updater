using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using ReactiveUI;
using System.Threading.Tasks;
using AsyncImageLoader;
using System;
using System.Reflection;
using wizbotupdater.Services;
using System.Net.Http.Json;
using wizbotupdater.Models;

namespace wizbotupdater.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly FfmpegDepViewModel _ffmpegViewModel;
    private readonly YtdlDepViewModel _ytdlpViewModel;
    private readonly IAsyncImageLoader _imageLoaderService;
    private const string ChangelogUrl = "https://github.com/Wizkiller96/WizBot/blob/v6/CHANGELOG.md";
    private const string WizBotUpdaterReleaseUrl = "https://github.com/Wizkiller96/wizbotupdater/releases";
    private bool _isDarkTheme;
    private bool _isWizBotUpdaterUpdateAvailable;
    private string _currentVersion;

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
    public ICommand OpenDiscordCommand { get; }
    public ICommand OpenWizBotUpdaterReleaseCommand { get; }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set => this.RaiseAndSetIfChanged(ref _isDarkTheme, value);
    }

    public string ThemeButtonText => IsDarkTheme ? "Light Theme" : "Dark Theme";

    public bool IsWizBotUpdaterUpdateAvailable
    {
        get => _isWizBotUpdaterUpdateAvailable;
        set => this.RaiseAndSetIfChanged(ref _isWizBotUpdaterUpdateAvailable, value);
    }

    public string CurrentVersion => _currentVersion;

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

        // Get current version from assembly
        _currentVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0.0";

        // Initialize commands
        OpenChangelogCommand = ReactiveCommand.Create(OpenChangelog);
        ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);
        OpenDiscordCommand = ReactiveCommand.Create(OpenDiscord);
        OpenWizBotUpdaterReleaseCommand = ReactiveCommand.Create(OpenWizBotUpdaterReleasePage);

        // Run the check methods for ffmpeg and ytdlp view models when the app starts
        // Use Dispatcher to ensure UI updates happen on the UI thread
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Add a small delay to ensure the UI is fully loaded
            await Task.Delay(1000);

            // Run the checks
            await _ffmpegViewModel.CheckAsync();
            await _ytdlpViewModel.CheckAsync();
            
            // Check for WizBot Updater updates
            await CheckForWizBotUpdaterUpdatesAsync();
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

    private void OpenDiscord()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "https://discord.gg/0YNaDOYuD5QOpeNI",
            UseShellExecute = true
        };
        
        Process.Start(psi);
    }

    private void OpenWizBotUpdaterReleasePage()
    {
        var psi = new ProcessStartInfo
        {
            FileName = WizBotUpdaterReleaseUrl,
            UseShellExecute = true
        };
        
        Process.Start(psi);
    }

    private async Task CheckForWizBotUpdaterUpdatesAsync()
    {
        try
        {
            // Create a custom UpdateChecker for WizBot Updater
            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "WizBot-Updater-Update-Checker");
            
            var response = await httpClient.GetAsync("https://api.github.com/repos/Wizkiller96/wizbot-updater/releases/latest");
            response.EnsureSuccessStatusCode();
            
            var newRelease = await response.Content.ReadFromJsonAsync(SourceJsonSerializer.Default.ReleaseModel);
            
            if (newRelease != null)
            {
                var latestVersion = newRelease.TagName?.TrimStart('v') ?? "1.0.0.0";
                IsWizBotUpdaterUpdateAvailable = CompareVersions(_currentVersion, latestVersion) < 0;
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't display it to the user
            Debug.WriteLine($"Error checking for updates: {ex.Message}");
            IsWizBotUpdaterUpdateAvailable = false;
        }
    }

    private int CompareVersions(string version1, string version2)
    {
        // Parse the version strings into Version objects
        if (Version.TryParse(version1, out var v1) && Version.TryParse(version2, out var v2))
        {
            // Use the built-in comparison
            return v1.CompareTo(v2);
        }

        // If parsing fails, fall back to string comparison
        return string.Compare(version1, version2, StringComparison.Ordinal);
    }
}