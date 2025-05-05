using System.Windows;
using ContextCreator.Models;
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

        public MainWindow()
        {
            InitializeComponent();
            
            // Create view model
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            
            // Subscribe to tree view events
            FolderTreeView.SelectedItemChanged += FolderTreeView_SelectedItemChanged;
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Handle item selection for content preview
            if (e.NewValue is FileItem fileItem)
            {
                // Show file content preview
                try
                {
                    string fileContent = System.IO.File.ReadAllText(fileItem.FullPath);
                    ContentPreviewTextBox.Text = fileContent;
                }
                catch (System.Exception ex)
                {
                    ContentPreviewTextBox.Text = $"Error loading file: {ex.Message}";
                }
            }
            else if (e.NewValue is FolderItem folderItem)
            {
                // Show folder info
                ContentPreviewTextBox.Text = $"Folder: {folderItem.FullPath}\r\n\r\nFiles: {folderItem.Files.Count}\r\nSubfolders: {folderItem.Folders.Count}";
            }
            else
            {
                ContentPreviewTextBox.Text = string.Empty;
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
            MessageBox.Show("Settings dialog will be implemented in a future version.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Display about dialog
            MessageBox.Show("LLM Context Preparation Tool\r\nVersion 1.0\r\n\r\nDesigned to help users prepare and manage context files for Large Language Models.", 
                "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Close application
            Close();
        }
    }
}