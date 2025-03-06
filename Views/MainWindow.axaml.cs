using Avalonia.ReactiveUI;
using upeko.ViewModels;

namespace upeko.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }

}