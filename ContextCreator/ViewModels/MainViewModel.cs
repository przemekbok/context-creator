using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ContextCreator.Commands;
using ContextCreator.Models;
using ContextCreator.Services;

namespace ContextCreator.ViewModels
{
    /// <summary>
    /// Main view model for the application
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly FilterService _filterService;
        private readonly ContextExportService _exportService;
        private readonly DialogService _dialogService;
        private FolderItem? _rootFolder;
        private string _statusMessage = "Ready";
        private int _selectedFileCount;
        private long _selectedFileSize;
        private int _estimatedTokenCount;
        private ContextConfiguration _currentConfiguration;
        private string _previewContent = "";

        /// <summary>
        /// Gets the root folder
        /// </summary>
        public FolderItem? RootFolder
        {
            get => _rootFolder;
            private set
            {
                if (SetProperty(ref _rootFolder, value))
                {
                    OnPropertyChanged(nameof(RootFolderCollection));
                }
            }
        }

        /// <summary>
        /// Gets the collection containing the root folder for TreeView binding
        /// </summary>
        public ObservableCollection<FolderItem> RootFolderCollection
        {
            get
            {
                ObservableCollection<FolderItem> collection = new ObservableCollection<FolderItem>();
                if (RootFolder != null)
                {
                    collection.Add(RootFolder);
                }
                return collection;
            }
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
            set
            {
                if (SetProperty(ref _selectedFileSize, value))
                {
                    OnPropertyChanged(nameof(FormattedSelectedFileSize));
                }
            }
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
        /// Gets or sets the preview content for the selected file
        /// </summary>
        public string PreviewContent
        {
            get => _previewContent;
            set => SetProperty(ref _previewContent, value);
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
        /// Gets the command to open a recent folder
        /// </summary>
        public ICommand OpenRecentFolderCommand { get; }

        /// <summary>
        /// Gets the command to save the current configuration
        /// </summary>
        public ICommand SaveConfigurationCommand { get; }

        /// <summary>
        /// Gets the command to load a configuration
        /// </summary>
        public ICommand LoadConfigurationCommand { get; }

        /// <summary>
        /// Gets the command to load a recent configuration
        /// </summary>
        public ICommand LoadRecentConfigurationCommand { get; }

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
            _dialogService = new DialogService();
            _currentConfiguration = new ContextConfiguration();

            // Initialize commands
            OpenFolderCommand = new RelayCommand(_ => ExecuteOpenFolder());
            OpenRecentFolderCommand = new RelayCommand(ExecuteOpenRecentFolder);
            SaveConfigurationCommand = new RelayCommand(_ => ExecuteSaveConfiguration());
            LoadConfigurationCommand = new RelayCommand(_ => ExecuteLoadConfiguration());
            LoadRecentConfigurationCommand = new RelayCommand(ExecuteLoadRecentConfiguration);
            ExportContextCommand = new RelayCommand(_ => ExecuteExportContext());
            ApplyFilterCommand = new RelayCommand<FilterOptions>(ExecuteApplyFilter);
            ClearFiltersCommand = new RelayCommand(_ => ExecuteClearFilters());
            EstimateTokenCountCommand = new RelayCommand(_ => ExecuteEstimateTokenCount());
            SelectAllCommand = new RelayCommand(_ => ExecuteSelectAll());
            DeselectAllCommand = new RelayCommand(_ => ExecuteDeselectAll());
            InvertSelectionCommand = new RelayCommand(_ => ExecuteInvertSelection());

            // Load recent folders and configurations
            LoadRecentItems();
        }

        /// <summary>
        /// Updates the file preview
        /// </summary>
        /// <param name="item">The item to preview</param>
        public void UpdatePreview(object item)
        {
            if (item is FileItem fileItem)
            {
                try
                {
                    PreviewContent = File.ReadAllText(fileItem.FullPath);
                }
                catch (Exception ex)
                {
                    PreviewContent = $"Error loading file: {ex.Message}";
                }
            }
            else if (item is FolderItem folderItem)
            {
                // Ensure content is loaded when a folder is selected for preview
                folderItem.EnsureContentLoaded();
                
                // Build folder info - include more detailed information
                var folderInfo = new System.Text.StringBuilder();
                folderInfo.AppendLine($"Folder: {folderItem.FullPath}");
                folderInfo.AppendLine();
                folderInfo.AppendLine($"Subfolders: {folderItem.Folders.Count}");
                
                // List the subfolder names
                if (folderItem.Folders.Count > 0)
                {
                    folderInfo.AppendLine();
                    folderInfo.AppendLine("Subfolder List:");
                    foreach (var subfolder in folderItem.Folders)
                    {
                        folderInfo.AppendLine($"- {subfolder.Name}");
                    }
                }
                
                folderInfo.AppendLine();
                folderInfo.AppendLine($"Files: {folderItem.Files.Count}");
                
                // List the file names
                if (folderItem.Files.Count > 0)
                {
                    folderInfo.AppendLine();
                    folderInfo.AppendLine("File List:");
                    foreach (var file in folderItem.Files)
                    {
                        folderInfo.AppendLine($"- {file.Name} ({FormatFileSize(file.Size)})");
                    }
                }
                
                PreviewContent = folderInfo.ToString();
            }
            else
            {
                PreviewContent = string.Empty;
            }
        }

        private void ExecuteOpenFolder()
        {
            var folderPath = _dialogService.ShowFolderBrowserDialog("Select a folder to open");
            if (!string.IsNullOrEmpty(folderPath))
            {
                OpenFolder(folderPath);
            }
        }

        private void ExecuteOpenRecentFolder(object? parameter)
        {
            if (parameter is string folderPath && Directory.Exists(folderPath))
            {
                OpenFolder(folderPath);
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

        private void ExecuteSaveConfiguration()
        {
            if (RootFolder == null)
            {
                _dialogService.ShowMessageBox("No folder loaded", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var filePath = _dialogService.ShowSaveFileDialog("Save Configuration", "Context Configuration (*.ctx)|*.ctx", "ctx");
            if (!string.IsNullOrEmpty(filePath))
            {
                SaveConfiguration(filePath);
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

        private void ExecuteLoadConfiguration()
        {
            var filePath = _dialogService.ShowOpenFileDialog("Load Configuration", "Context Configuration (*.ctx)|*.ctx");
            if (!string.IsNullOrEmpty(filePath))
            {
                LoadConfiguration(filePath);
            }
        }

        private void ExecuteLoadRecentConfiguration(object? parameter)
        {
            if (parameter is string filePath && File.Exists(filePath))
            {
                LoadConfiguration(filePath);
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

        private void ExecuteExportContext()
        {
            if (RootFolder == null)
            {
                _dialogService.ShowMessageBox("No folder loaded", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var filePath = _dialogService.ShowSaveFileDialog(
                "Export Context", 
                "Markdown (*.md)|*.md|JSON (*.json)|*.json|Text (*.txt)|*.txt", 
                "md");
                
            if (!string.IsNullOrEmpty(filePath))
            {
                ExportContextAsync(filePath, GetExportFormatFromExtension(Path.GetExtension(filePath)));
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

        private void ExecuteClearFilters()
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

        private async void ExecuteEstimateTokenCount()
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

        private void ExecuteSelectAll()
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Selecting all files...";
                RootFolder.IsSelected = true;
                ExecuteEstimateTokenCount();
                StatusMessage = "All files selected";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error selecting files: {ex.Message}";
            }
        }

        private void ExecuteDeselectAll()
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Deselecting all files...";
                RootFolder.IsSelected = false;
                ExecuteEstimateTokenCount();
                StatusMessage = "All files deselected";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deselecting files: {ex.Message}";
            }
        }

        private void ExecuteInvertSelection()
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Inverting selection...";
                InvertSelection(RootFolder);
                ExecuteEstimateTokenCount();
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