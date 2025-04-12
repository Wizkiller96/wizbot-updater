using System.Windows.Input;
using ReactiveUI;

namespace wizbotupdater.ViewModels;

public class AddButtonViewModel : ViewModelBase
{
    private readonly BotListViewModel _botListViewModel;
    public ICommand AddCommand { get; }

    public AddButtonViewModel(BotListViewModel botListViewModel)
    {
        _botListViewModel = botListViewModel;
        AddCommand = ReactiveCommand.Create(ExecuteAdd);
    }

    private void ExecuteAdd()
    {
        // Add a new bot to the items list
        _botListViewModel.AddNewBot();
    }
}