using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace upeko.Views
{
    public partial class BotListItem : UserControl
    {
        public BotListItem()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
