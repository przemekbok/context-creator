using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContextCreator.Models;
using ContextCreator.Services;
using ContextCreator.ViewModels;
using ContextCreator.Views;

namespace ContextCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly DialogService _dialogService;
        private object _lastSelectedItem;

        public MainWindow()
        {
            InitializeComponent();
            
            // Create services
            _dialogService = new DialogService();
            
            // Create view model
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Add keyboard shortcuts
            InitializeKeyboardShortcuts();
        }

        private void InitializeKeyboardShortcuts()
        {
            // Create keyboard shortcuts
            var commands = new[]
            {
                new { Key = Key.O, Modifiers = ModifierKeys.Control, Command = _viewModel.OpenFolderCommand },
                new { Key = Key.S, Modifiers = ModifierKeys.Control, Command = _viewModel.SaveConfigurationCommand },
                new { Key = Key.L, Modifiers = ModifierKeys.Control, Command = _viewModel.LoadConfigurationCommand },
                new { Key = Key.E, Modifiers = ModifierKeys.Control, Command = _viewModel.ExportContextCommand },
                new { Key = Key.F, Modifiers = ModifierKeys.Control, Command = null, Action = () => ContentFilterMenuItem_Click(this, new RoutedEventArgs()) },
                new { Key = Key.F, Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Command = null, Action = () => FilenameFilterMenuItem_Click(this, new RoutedEventArgs()) },
                new { Key = Key.Escape, Modifiers = ModifierKeys.None, Command = _viewModel.ClearFiltersCommand },
                new { Key = Key.M, Modifiers = ModifierKeys.Control, Command = _viewModel.SelectMatchingCommand },
                new { Key = Key.M, Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Command = _viewModel.DeselectMatchingCommand },
                new { Key = Key.E, Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Command = _viewModel.CollapseAllCommand },
                new { Key = Key.P, Modifiers = ModifierKeys.Control, Command = _viewModel.ShowMatchingPathsCommand },
                new { Key = Key.T, Modifiers = ModifierKeys.Control, Command = _viewModel.EstimateTokenCountCommand },
                new { Key = Key.A, Modifiers = ModifierKeys.Control, Command = _viewModel.SelectAllCommand },
                new { Key = Key.D, Modifiers = ModifierKeys.Control, Command = _viewModel.DeselectAllCommand },
                new { Key = Key.I, Modifiers = ModifierKeys.Control, Command = _viewModel.InvertSelectionCommand },
                new { Key = Key.N, Modifiers = ModifierKeys.Control, Command = _viewModel.CreateFolderWithSetupCommand }
            };

            // Add key bindings to the window
            foreach (var command in commands)
            {
                var binding = new KeyBinding
                {
                    Key = command.Key,
                    Modifiers = command.Modifiers
                };

                if (command.Command != null)
                {
                    binding.Command = command.Command;
                }
                else if (command.Action != null)
                {
                    binding.Command = new RelayCommand(command.Action);
                }

                InputBindings.Add(binding);
            }
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                _lastSelectedItem = e.NewValue;
                
                // Update preview content
                _viewModel.UpdatePreview(e.NewValue);
            }
        }
        
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem)
            {
                // Prevent the event from bubbling up to parent TreeViewItems
                e.Handled = true;
                
                // Force the selection to stay with this item
                treeViewItem.IsSelected = true;
                
                // Get the actual data item bound to this TreeViewItem
                var item = treeViewItem.DataContext;
                
                // Update the preview with the actual item
                if (item != null)
                {
                    _viewModel.UpdatePreview(item);
                }
            }
        }
        
        private void TreeViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem)
            {
                // Make sure this item gets selected
                treeViewItem.IsSelected = true;
                
                // Get the actual data item bound to this TreeViewItem
                var item = treeViewItem.DataContext;
                
                // Update the preview with the actual item
                if (item != null)
                {
                    _viewModel.UpdatePreview(item);
                }
                
                // Mark event as handled to prevent it from bubbling up
                e.Handled = true;
            }
        }

        private void ContentFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var filterViewModel = new FilterViewModel
            {
                FilterType = FilterType.Content
            };
            
            var filterDialog = new FilterDialog(filterViewModel);
            filterViewModel.FilterApplied += (s, options) =>
            {
                filterDialog.Close();
                _viewModel.ApplyFilterCommand.Execute(options);
            };
            
            filterViewModel.FilterCanceled += (s, args) => filterDialog.Close();
            filterDialog.Owner = this;
            filterDialog.ShowDialog();
        }

        private void FilenameFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var filterViewModel = new FilterViewModel
            {
                FilterType = FilterType.FileName
            };
            
            var filterDialog = new FilterDialog(filterViewModel);
            filterViewModel.FilterApplied += (s, options) =>
            {
                filterDialog.Close();
                _viewModel.ApplyFilterCommand.Execute(options);
            };
            
            filterViewModel.FilterCanceled += (s, args) => filterDialog.Close();
            filterDialog.Owner = this;
            filterDialog.ShowDialog();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Open settings dialog (not implemented)
            _dialogService.ShowMessageBox("Settings dialog will be implemented in a future version.", "Settings");
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Display about dialog
            _dialogService.ShowMessageBox(
                "LLM Context Preparation Tool\r\nVersion 1.0\r\n\r\nDesigned to help users prepare and manage context files for Large Language Models.", 
                "About");
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Close application
            Close();
        }
    }

    /// <summary>
    /// Helper class for keyboard shortcuts
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute ?? throw new System.ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}