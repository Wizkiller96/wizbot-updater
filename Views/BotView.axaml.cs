using Avalonia.Controls;
using Avalonia.Interactivity;
using upeko.ViewModels;

namespace upeko.Views
{
    public partial class BotView : UserControl
    {
        public BotView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Event handler for when the bot name is clicked
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event arguments</param>
        private void OnNameTapped(object? sender, RoutedEventArgs e)
        {
            if (DataContext is BotViewModel viewModel)
            {
                viewModel.EditNameCommand.Execute(null);
            }
        }
    }
}
