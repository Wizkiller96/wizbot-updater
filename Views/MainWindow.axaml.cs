using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using upeko.ViewModels;

namespace upeko.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(action =>
            action(ViewModel!.ShowAddBotDialog.RegisterHandler(DoShowAddBotDialogAsync)));
    }

    private async Task DoShowAddBotDialogAsync(IInteractionContext<AddBotViewModel, AddBotOutputViewModel?> interaction)
    {
        var dialog = new AddBotWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<AddBotOutputViewModel>(this);
        interaction.SetOutput(result);
    }
}