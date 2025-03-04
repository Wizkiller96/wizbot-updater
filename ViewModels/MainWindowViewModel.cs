using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using upeko.Views;

namespace upeko.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public BotListViewModel Bots { get; } = new();

    public MainWindowViewModel()
    {
    }
}