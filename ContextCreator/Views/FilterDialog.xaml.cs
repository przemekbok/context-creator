using System.Windows;
using ContextCreator.ViewModels;

namespace ContextCreator.Views
{
    /// <summary>
    /// Interaction logic for FilterDialog.xaml
    /// </summary>
    public partial class FilterDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the FilterDialog class
        /// </summary>
        public FilterDialog(FilterViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}