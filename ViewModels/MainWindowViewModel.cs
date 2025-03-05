using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using ReactiveUI;
using upeko.Views;
using System.Threading.Tasks;

namespace upeko.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly FfmpegDepViewModel _ffmpegViewModel;
    private readonly YtdlDepViewModel _ytdlpViewModel;
    
    public BotListViewModel Bots { get; } = new();
    
    public FfmpegDepViewModel FfmpegViewModel 
    {
        get => _ffmpegViewModel;
    }
    
    public YtdlDepViewModel YtDlpViewModel 
    {
        get => _ytdlpViewModel;
    }

    public MainWindowViewModel()
    {
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