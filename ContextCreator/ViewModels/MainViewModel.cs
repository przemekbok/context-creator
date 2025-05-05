using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ContextCreator.Models;
using ContextCreator.Services;
using ContextCreator.Commands;
using Microsoft.Win32;

namespace ContextCreator.ViewModels
{
    /// <summary>
    /// Main view model for the application
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly FilterService _filterService;
        private readonly ContextExportService _exportService;
        private FolderItem? _rootFolder;
        private string _statusMessage = "Ready";
        private int _selectedFileCount;
        private long _selectedFileSize;
        private int _estimatedTokenCount;
        private ContextConfiguration _currentConfiguration;

        /// <summary>
        /// Gets the root folder
        /// </summary>
        public FolderItem? RootFolder
        {
            get => _rootFolder;
            private set => SetProperty(ref _rootFolder, value);
        }

        /// <summary>
        /// Gets the status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets the number of selected files
        /// </summary>
        public int SelectedFileCount
        {
            get => _selectedFileCount;
            set => SetProperty(ref _selectedFileCount, value);
        }

        /// <summary>
        /// Gets the total size of selected files
        /// </summary>
        public long SelectedFileSize
        {
            get => _selectedFileSize;
            set => SetProperty(ref _selectedFileSize, value);
        }

        /// <summary>
        /// Gets the estimated token count
        /// </summary>
        public int EstimatedTokenCount
        {
            get => _estimatedTokenCount;
            set => SetProperty(ref _estimatedTokenCount, value);
        }

        /// <summary>
        /// Gets the selected file size as a formatted string
        /// </summary>
        public string FormattedSelectedFileSize => FormatFileSize(SelectedFileSize);

        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public ContextConfiguration CurrentConfiguration
        {
            get => _currentConfiguration;
            set => SetProperty(ref _currentConfiguration, value);
        }

        /// <summary>
        /// Gets a list of recent folders
        /// </summary>
        public ObservableCollection<string> RecentFolders { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets a list of recent configurations
        /// </summary>
        public ObservableCollection<string> RecentConfigurations { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets the command to open a folder
        /// </summary>
        public ICommand OpenFolderCommand { get; }

        /// <summary>
        /// Gets the command to save the current configuration
        /// </summary>
        public ICommand SaveConfigurationCommand { get; }

        /// <summary>
        /// Gets the command to load a configuration
        /// </summary>
        public ICommand LoadConfigurationCommand { get; }

        /// <summary>
        /// Gets the command to export the context
        /// </summary>
        public ICommand ExportContextCommand { get; }

        /// <summary>
        /// Gets the command to apply a filter
        /// </summary>
        public ICommand ApplyFilterCommand { get; }

        /// <summary>
        /// Gets the command to clear filters
        /// </summary>
        public ICommand ClearFiltersCommand { get; }

        /// <summary>
        /// Gets the command to estimate token count
        /// </summary>
        public ICommand EstimateTokenCountCommand { get; }

        /// <summary>
        /// Gets the command to select all files
        /// </summary>
        public ICommand SelectAllCommand { get; }

        /// <summary>
        /// Gets the command to deselect all files
        /// </summary>
        public ICommand DeselectAllCommand { get; }

        /// <summary>
        /// Gets the command to invert selection
        /// </summary>
        public ICommand InvertSelectionCommand { get; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class
        /// </summary>
        public MainViewModel()
        {
            _filterService = new FilterService();
            _exportService = new ContextExportService();
            _currentConfiguration = new ContextConfiguration();

            // Initialize commands
            OpenFolderCommand = new RelayCommand(ExecuteOpenFolder);
            SaveConfigurationCommand = new RelayCommand(ExecuteSaveConfiguration);
            LoadConfigurationCommand = new RelayCommand(ExecuteLoadConfiguration);
            ExportContextCommand = new RelayCommand(ExecuteExportContext);
            ApplyFilterCommand = new RelayCommand<FilterOptions>(ExecuteApplyFilter);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
            EstimateTokenCountCommand = new RelayCommand(ExecuteEstimateTokenCount);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);
            DeselectAllCommand = new RelayCommand(ExecuteDeselectAll);
            InvertSelectionCommand = new RelayCommand(ExecuteInvertSelection);

            // Load recent folders and configurations
            LoadRecentItems();
        }

        private void ExecuteOpenFolder(object? parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder to open",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenFolder(dialog.SelectedPath);
            }
        }

        private void OpenFolder(string folderPath)
        {
            try
            {
                StatusMessage = $"Opening folder: {folderPath}";
                RootFolder = new FolderItem(folderPath);
                RootFolder.IsExpanded = true; // Expand root folder by default
                
                CurrentConfiguration = new ContextConfiguration
                {
                    RootFolder = folderPath
                };
                
                // Add to recent folders
                if (!RecentFolders.Contains(folderPath))
                {
                    RecentFolders.Insert(0, folderPath);
                    if (RecentFolders.Count > 10)
                    {
                        RecentFolders.RemoveAt(RecentFolders.Count - 1);
                    }
                }
                
                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening folder: {ex.Message}";
            }
        }

        private void ExecuteSaveConfiguration(object? parameter)
        {
            if (RootFolder == null)
            {
                StatusMessage = "No folder loaded";
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Context Configuration (*.ctx)|*.ctx",
                Title = "Save Configuration",
                DefaultExt = "ctx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                SaveConfiguration(saveDialog.FileName);
            }
        }

        private void SaveConfiguration(string filePath)
        {
            try
            {
                StatusMessage = "Saving configuration...";
                
                // Collect selected files
                CurrentConfiguration.SelectedPaths.Clear();
                if (RootFolder != null)
                {
                    CollectSelectedPaths(RootFolder, CurrentConfiguration.SelectedPaths);
                }
                
                // Save to file
                CurrentConfiguration.SaveToFile(filePath);
                
                // Add to recent configurations
                if (!RecentConfigurations.Contains(filePath))
                {
                    RecentConfigurations.Insert(0, filePath);
                    if (RecentConfigurations.Count > 10)
                    {
                        RecentConfigurations.RemoveAt(RecentConfigurations.Count - 1);
                    }
                }
                
                StatusMessage = "Configuration saved successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving configuration: {ex.Message}";
            }
        }

        private void ExecuteLoadConfiguration(object? parameter)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Context Configuration (*.ctx)|*.ctx",
                Title = "Load Configuration"
            };

            if (openDialog.ShowDialog() == true)
            {
                LoadConfiguration(openDialog.FileName);
            }
        }

        private void LoadConfiguration(string filePath)
        {
            try
            {
                StatusMessage = "Loading configuration...";
                
                // Load from file
                CurrentConfiguration = ContextConfiguration.LoadFromFile(filePath);
                
                // Open the root folder
                if (!string.IsNullOrEmpty(CurrentConfiguration.RootFolder) && Directory.Exists(CurrentConfiguration.RootFolder))
                {
                    OpenFolder(CurrentConfiguration.RootFolder);
                    
                    // Apply selection
                    if (RootFolder != null && CurrentConfiguration.SelectedPaths.Any())
                    {
                        ApplySelection(RootFolder, CurrentConfiguration.SelectedPaths);
                    }
                }
                
                // Add to recent configurations
                if (!RecentConfigurations.Contains(filePath))
                {
                    RecentConfigurations.Insert(0, filePath);
                    if (RecentConfigurations.Count > 10)
                    {
                        RecentConfigurations.RemoveAt(RecentConfigurations.Count - 1);
                    }
                }
                
                StatusMessage = "Configuration loaded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading configuration: {ex.Message}";
            }
        }

        private void ExecuteExportContext(object? parameter)
        {
            if (RootFolder == null)
            {
                StatusMessage = "No folder loaded";
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Markdown (*.md)|*.md|JSON (*.json)|*.json|Text (*.txt)|*.txt",
                Title = "Export Context",
                DefaultExt = "md"
            };

            if (saveDialog.ShowDialog() == true)
            {
                ExportContextAsync(saveDialog.FileName, GetExportFormatFromExtension(Path.GetExtension(saveDialog.FileName)));
            }
        }

        private async void ExportContextAsync(string filePath, ExportFormat format)
        {
            try
            {
                StatusMessage = "Exporting context...";
                
                // Collect selected files
                var selectedFiles = new List<string>();
                if (RootFolder != null)
                {
                    CollectSelectedPaths(RootFolder, selectedFiles);
                }
                
                if (!selectedFiles.Any())
                {
                    StatusMessage = "No files selected for export";
                    return;
                }
                
                // Export context
                bool success = await _exportService.ExportContextAsync(selectedFiles, filePath, format);
                
                StatusMessage = success ? "Context exported successfully" : "Error exporting context";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting context: {ex.Message}";
            }
        }

        private async void ExecuteApplyFilter(FilterOptions? options)
        {
            if (RootFolder == null || options == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Applying filter...";
                await _filterService.ApplyFilterAsync(RootFolder, options);
                StatusMessage = "Filter applied";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error applying filter: {ex.Message}";
            }
        }

        private void ExecuteClearFilters(object? parameter)
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Clearing filters...";
                ClearFilters(RootFolder);
                StatusMessage = "Filters cleared";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error clearing filters: {ex.Message}";
            }
        }

        private async void ExecuteEstimateTokenCount(object? parameter)
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Estimating token count...";
                
                // Collect selected files
                var selectedFiles = new List<string>();
                CollectSelectedPaths(RootFolder, selectedFiles);
                
                if (!selectedFiles.Any())
                {
                    StatusMessage = "No files selected";
                    EstimatedTokenCount = 0;
                    return;
                }
                
                // Update selected file count and size
                SelectedFileCount = selectedFiles.Count;
                SelectedFileSize = selectedFiles.Sum(file => new FileInfo(file).Length);
                
                // Estimate token count
                EstimatedTokenCount = await _exportService.EstimateTokenCountAsync(selectedFiles);
                
                StatusMessage = "Token estimation complete";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error estimating tokens: {ex.Message}";
            }
        }

        private void ExecuteSelectAll(object? parameter)
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Selecting all files...";
                RootFolder.IsSelected = true;
                ExecuteEstimateTokenCount(null);
                StatusMessage = "All files selected";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error selecting files: {ex.Message}";
            }
        }

        private void ExecuteDeselectAll(object? parameter)
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Deselecting all files...";
                RootFolder.IsSelected = false;
                ExecuteEstimateTokenCount(null);
                StatusMessage = "All files deselected";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deselecting files: {ex.Message}";
            }
        }

        private void ExecuteInvertSelection(object? parameter)
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Inverting selection...";
                InvertSelection(RootFolder);
                ExecuteEstimateTokenCount(null);
                StatusMessage = "Selection inverted";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error inverting selection: {ex.Message}";
            }
        }

        private void InvertSelection(FolderItem folder)
        {
            // Invert selection for files
            foreach (var file in folder.Files)
            {
                file.IsSelected = !file.IsSelected;
            }
            
            // Recursively invert for subfolders
            foreach (var subFolder in folder.Folders)
            {
                InvertSelection(subFolder);
            }
            
            // Update folder selection status
            folder.IsSelected = folder.Files.All(f => f.IsSelected) && folder.Folders.All(f => f.IsSelected);
        }

        private void ClearFilters(FolderItem folder)
        {
            folder.IsMatch = false;
            
            foreach (var file in folder.Files)
            {
                file.IsMatch = false;
            }
            
            foreach (var subFolder in folder.Folders)
            {
                ClearFilters(subFolder);
            }
        }

        private void CollectSelectedPaths(FolderItem folder, List<string> selectedPaths)
        {
            // Add selected files
            foreach (var file in folder.Files)
            {
                if (file.IsSelected)
                {
                    selectedPaths.Add(file.FullPath);
                }
            }
            
            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                CollectSelectedPaths(subFolder, selectedPaths);
            }
        }

        private void ApplySelection(FolderItem folder, List<string> selectedPaths)
        {
            // Set selection for files
            foreach (var file in folder.Files)
            {
                file.IsSelected = selectedPaths.Contains(file.FullPath);
            }
            
            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                ApplySelection(subFolder, selectedPaths);
            }
            
            // Update folder selection status
            folder.IsSelected = folder.Files.All(f => f.IsSelected) && folder.Folders.All(f => f.IsSelected);
        }

        private ExportFormat GetExportFormatFromExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".md" => ExportFormat.Markdown,
                ".json" => ExportFormat.Json,
                _ => ExportFormat.Plain
            };
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }

        private void LoadRecentItems()
        {
            // This would typically load from settings/registry
            // For simplicity, we'll leave this as a stub
        }
    }
}