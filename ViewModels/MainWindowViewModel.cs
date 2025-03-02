using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace upeko.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ICommand AddBotCommand { get; }
    
    public Interaction<AddBotViewModel, AddBotOutputViewModel?> ShowAddBotDialog { get; }

    public MainWindowViewModel()
    {
        ShowAddBotDialog = new();
        AddBotCommand = ReactiveCommand.Create(async () =>
        {
            var result = await ShowAddBotDialog.Handle(new AddBotViewModel());
            Debug.WriteLine(result);
        });
    }
}
