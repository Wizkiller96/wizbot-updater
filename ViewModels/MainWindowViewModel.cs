using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using ReactiveUI;
using upeko.Views;
using System.Threading.Tasks;
using AsyncImageLoader.Loaders;
using upeko.Services;
using AsyncImageLoader;

namespace upeko.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly FfmpegDepViewModel _ffmpegViewModel;
    private readonly YtdlDepViewModel _ytdlpViewModel;
    private readonly IAsyncImageLoader _imageLoaderService;

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

    public MainWindowViewModel()
    {
        // Get the ImageLoaderService from the dependency injection container
        _imageLoaderService = App.Services.GetService(typeof(IAsyncImageLoader)) as IAsyncImageLoader
                              ?? throw new System.InvalidOperationException("Failed to resolve IAsyncImageLoader");
                              
        // Initialize view models
        _ffmpegViewModel = new FfmpegDepViewModel();
        _ytdlpViewModel = new YtdlDepViewModel();

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
    }
}