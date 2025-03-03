using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace upeko.ViewModels;

public partial class AddBotItemViewModel : ViewModelBase
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
}