using Avalonia.ReactiveUI;
using wizbotupdater.ViewModels;

namespace wizbotupdater.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }

}