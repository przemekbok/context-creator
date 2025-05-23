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
using MatchType = ContextCreator.Models.MatchType;

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
        private FilterOptions? _currentFilterOptions;
        private bool _hasActiveFilter = false;
        private ObservableCollection<string> _matchingPaths = new ObservableCollection<string>();
        private int _matchCount = 0;

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
        /// Gets if there is an active filter
        /// </summary>
        public bool HasActiveFilter
        {
            get => _hasActiveFilter;
            private set => SetProperty(ref _hasActiveFilter, value);
        }

        /// <summary>
        /// Gets the collection of matching file and folder paths
        /// </summary>
        public ObservableCollection<string> MatchingPaths
        {
            get => _matchingPaths;
            set => SetProperty(ref _matchingPaths, value);
        }

        /// <summary>
        /// Gets the count of matching items
        /// </summary>
        public int MatchCount
        {
            get => _matchCount;
            set => SetProperty(ref _matchCount, value);
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
        /// Gets the command to select matching items
        /// </summary>
        public ICommand SelectMatchingCommand { get; }

        /// <summary>
        /// Gets the command to deselect matching items
        /// </summary>
        public ICommand DeselectMatchingCommand { get; }
        
        /// <summary>
        /// Gets the command to expand matching paths
        /// </summary>
        public ICommand ExpandMatchingCommand { get; }
        
        /// <summary>
        /// Gets the command to collapse all folders
        /// </summary>
        public ICommand CollapseAllCommand { get; }

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
        /// Gets the command to show matching paths
        /// </summary>
        public ICommand ShowMatchingPathsCommand { get; }

        /// <summary>
        /// Gets the command to create a new folder with the current setup
        /// </summary>
        public ICommand CreateFolderWithSetupCommand { get; }

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
            SelectMatchingCommand = new RelayCommand(_ => ExecuteSelectMatching(), _ => HasActiveFilter);
            DeselectMatchingCommand = new RelayCommand(_ => ExecuteDeselectMatching(), _ => HasActiveFilter);
            ExpandMatchingCommand = new RelayCommand(_ => ExecuteExpandMatching(), _ => HasActiveFilter);
            CollapseAllCommand = new RelayCommand(_ => ExecuteCollapseAll());
            EstimateTokenCountCommand = new RelayCommand(_ => ExecuteEstimateTokenCount());
            SelectAllCommand = new RelayCommand(_ => ExecuteSelectAll());
            DeselectAllCommand = new RelayCommand(_ => ExecuteDeselectAll());
            InvertSelectionCommand = new RelayCommand(_ => ExecuteInvertSelection());
            ShowMatchingPathsCommand = new RelayCommand(_ => ExecuteShowMatchingPaths(), _ => HasActiveFilter);
            CreateFolderWithSetupCommand = new RelayCommand(_ => ExecuteCreateFolderWithSetup(), _ => RootFolder != null);

            // Load recent folders and configurations
            LoadRecentItems();
        }

        /// <summary>
        /// Creates a new folder with the current selected file structure
        /// </summary>
        private void ExecuteCreateFolderWithSetup()
        {
            if (RootFolder == null)
            {
                _dialogService.ShowMessageBox("No folder loaded", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            // Ask for a destination folder
            var destFolderPath = _dialogService.ShowFolderBrowserDialog("Select a destination folder");
            if (string.IsNullOrEmpty(destFolderPath))
            {
                return;
            }

            try
            {
                StatusMessage = "Creating folder with selected files...";

                // Get the name of the root folder
                string rootFolderName = Path.GetFileName(RootFolder.FullPath);
                if (string.IsNullOrEmpty(rootFolderName))
                {
                    // For drive roots (C:\), use a default name
                    rootFolderName = "Context";
                }

                // Create the target folder
                string newFolderPath = Path.Combine(destFolderPath, rootFolderName);

                // If the folder already exists, add a suffix
                int suffix = 1;
                string originalPath = newFolderPath;
                while (Directory.Exists(newFolderPath))
                {
                    newFolderPath = $"{originalPath}_{suffix}";
                    suffix++;
                }

                // Create the directory
                Directory.CreateDirectory(newFolderPath);

                // Collect selected files
                var selectedFiles = new List<string>();
                CollectSelectedPaths(RootFolder, selectedFiles);

                if (!selectedFiles.Any())
                {
                    StatusMessage = "No files selected";
                    return;
                }

                int copiedFiles = 0;
                long totalSize = 0;

                foreach (var filePath in selectedFiles)
                {
                    // Calculate the relative path from the root folder
                    string relativePath = GetRelativePath(RootFolder.FullPath, filePath);
                    
                    // Determine the target path
                    string targetPath = Path.Combine(newFolderPath, relativePath);
                    
                    // Create the directory structure
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    
                    // Copy the file
                    File.Copy(filePath, targetPath);
                    
                    // Update statistics
                    copiedFiles++;
                    totalSize += new FileInfo(filePath).Length;
                }

                // Show success message
                StatusMessage = $"Created folder with {copiedFiles} files ({FormatFileSize(totalSize)})";

                // Ask if the user wants to open the new folder
                var result = _dialogService.ShowMessageBox(
                    $"Successfully created folder with {copiedFiles} files.\n\nDo you want to open the new folder?",
                    "Folder Created",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Information);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    OpenFolder(newFolderPath);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating folder: {ex.Message}";
                _dialogService.ShowMessageBox($"Error creating folder: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the relative path from a base directory to a full path
        /// </summary>
        private string GetRelativePath(string basePath, string fullPath)
        {
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                basePath += Path.DirectorySeparatorChar;
            }

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // Replace forward slashes with the platform-specific directory separator
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            return relativePath;
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
                        string matchStatus = "";
                        if (HasActiveFilter)
                        {
                            if (subfolder.MatchType == MatchType.Direct)
                                matchStatus = " [MATCH]";
                            else if (subfolder.MatchType == MatchType.Ancestor)
                                matchStatus = " [CONTAINS MATCHES]";
                        }
                        
                        folderInfo.AppendLine($"- {subfolder.Name}{matchStatus}");
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
                        string matchStatus = "";
                        if (HasActiveFilter && file.MatchType == MatchType.Direct)
                            matchStatus = " [MATCH]";
                            
                        folderInfo.AppendLine($"- {file.Name} ({FormatFileSize(file.Size)}){matchStatus}");
                    }
                }
                
                PreviewContent = folderInfo.ToString();
            }
            else
            {
                PreviewContent = string.Empty;
            }
        }

        /// <summary>
        /// Shows a list of all paths that match the current filter
        /// </summary>
        private void ExecuteShowMatchingPaths()
        {
            if (RootFolder == null || !HasActiveFilter)
            {
                return;
            }

            try
            {
                // Clear the previous list
                MatchingPaths.Clear();
                MatchCount = 0;
                
                // Build a string with all matching paths
                var matchingPathsList = new List<string>();
                CollectMatchingPaths(RootFolder, matchingPathsList);
                
                // Sort the paths for better readability
                matchingPathsList.Sort();
                
                // Add paths to the observable collection
                foreach (var path in matchingPathsList)
                {
                    MatchingPaths.Add(path);
                }
                
                MatchCount = MatchingPaths.Count;
                
                // Build the preview content
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Filter Results: {MatchCount} matches found");
                sb.AppendLine();
                
                if (_currentFilterOptions != null)
                {
                    string filterType = _currentFilterOptions.Type == FilterType.Content ? "Content" : "Filename";
                    string matchType = _currentFilterOptions.IsExactMatch ? "exact match" : "contains";
                    string caseSensitive = _currentFilterOptions.IsCaseSensitive ? "case-sensitive" : "case-insensitive";
                    string action = _currentFilterOptions.Action == FilterAction.Include ? "Include" : "Exclude";
                    string regex = _currentFilterOptions.IsRegex ? "using regex" : "";
                    
                    sb.AppendLine($"Filter: {filterType} filter, {action} items that {matchType} \"{_currentFilterOptions.Expression}\" ({caseSensitive}) {regex}");
                    sb.AppendLine();
                }
                
                sb.AppendLine("Matching Paths:");
                sb.AppendLine("---------------");
                
                foreach (var path in MatchingPaths)
                {
                    sb.AppendLine(path);
                }
                
                PreviewContent = sb.ToString();
                StatusMessage = $"Found {MatchCount} matching items";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error showing matching paths: {ex.Message}";
            }
        }

        /// <summary>
        /// Collects paths of all items that match the current filter
        /// </summary>
        private void CollectMatchingPaths(FolderItem folder, List<string> matchingPaths)
        {
            // Add matching files
            foreach (var file in folder.Files)
            {
                if (file.MatchType == MatchType.Direct)
                {
                    matchingPaths.Add(file.FullPath);
                }
            }
            
            // Add the folder if it directly matches
            if (folder.MatchType == MatchType.Direct)
            {
                matchingPaths.Add($"{folder.FullPath}/");
            }
            
            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                CollectMatchingPaths(subFolder, matchingPaths);
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

        private void OpenFolder(string folderPath, bool createNewConfiguration = true)
        {
            try
            {
                StatusMessage = $"Opening folder: {folderPath}";
                RootFolder = new FolderItem(folderPath);
                RootFolder.IsExpanded = true; // Expand root folder by default
                
                if (createNewConfiguration)
                {
                    RootFolder.IsSelected = true; // Select all files by default
                    CurrentConfiguration = new ContextConfiguration
                    {
                        RootFolder = folderPath
                    };
                }
                
                // Add to recent folders
                if (!RecentFolders.Contains(folderPath))
                {
                    RecentFolders.Insert(0, folderPath);
                    if (RecentFolders.Count > 10)
                    {
                        RecentFolders.RemoveAt(RecentFolders.Count - 1);
                    }
                }

                // Update file count and token estimation
                if (createNewConfiguration)
                {
                    ExecuteEstimateTokenCount();
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

        /// <summary>
        /// Ensures all folders and subfolders are loaded
        /// </summary>
        private void EnsureAllFoldersLoaded(FolderItem folder)
        {
            // Ensure content is loaded for this folder
            folder.EnsureContentLoaded();
            
            // Recursively load all subfolders
            foreach (var subFolder in folder.Folders)
            {
                EnsureAllFoldersLoaded(subFolder);
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
                    OpenFolder(CurrentConfiguration.RootFolder, false); // Don't create new configuration
                    
                    // Ensure all folders are loaded before applying selection
                    if (RootFolder != null)
                    {
                        // Show loading message
                        StatusMessage = "Loading folder structure...";
                        
                        // Ensure all folders are loaded
                        EnsureAllFoldersLoaded(RootFolder);
                        
                        // Apply selection
                        if (CurrentConfiguration.SelectedPaths.Any())
                        {
                            ApplySelection(RootFolder, CurrentConfiguration.SelectedPaths);
                        }
                        
                        // Update token estimation after applying selection
                        ExecuteEstimateTokenCount();
                    }
                }
                else
                {
                    StatusMessage = "Error: Root folder from configuration does not exist";
                    _dialogService.ShowMessageBox(
                        "The folder specified in the configuration does not exist: " + CurrentConfiguration.RootFolder,
                        "Error Loading Configuration", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                    return;
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
                _dialogService.ShowMessageBox(
                    $"Error loading configuration:\n{ex.Message}", 
                    "Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void ApplySelection(FolderItem folder, List<string> selectedPaths)
        {
            // Ensure content is loaded
            folder.EnsureContentLoaded();
            
            // Set selection for files
            foreach (var file in folder.Files)
            {
                // Use a more robust path comparison
                file.IsSelected = selectedPaths.Any(p => 
                    Path.GetFullPath(p).Equals(Path.GetFullPath(file.FullPath), 
                    StringComparison.OrdinalIgnoreCase));
            }
            
            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                ApplySelection(subFolder, selectedPaths);
            }
            
            // Update folder selection status based on its children
            UpdateFolderSelectionStatus(folder);
        }

        private void UpdateFolderSelectionStatus(FolderItem folder)
        {
            // A folder is selected if ALL its files and subfolders are selected
            bool allFilesSelected = folder.Files.Count > 0 && folder.Files.All(f => f.IsSelected);
            bool allFoldersSelected = folder.Folders.Count > 0 && folder.Folders.All(f => f.IsSelected);
            
            // If there are no files or folders, don't change selection state
            if (folder.Files.Count == 0 && folder.Folders.Count == 0)
                return;
            
            // If there are only files but no folders
            if (folder.Files.Count > 0 && folder.Folders.Count == 0)
                folder.IsSelected = allFilesSelected;
            
            // If there are only folders but no files
            else if (folder.Files.Count == 0 && folder.Folders.Count > 0)
                folder.IsSelected = allFoldersSelected;
            
            // If there are both files and folders
            else
                folder.IsSelected = allFilesSelected && allFoldersSelected;
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
                _currentFilterOptions = options;
                await _filterService.ApplyFilterAsync(RootFolder, options);
                HasActiveFilter = true;
                
                // Show the matching paths after applying the filter
                ExecuteShowMatchingPaths();
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
                _currentFilterOptions = null;
                HasActiveFilter = false;
                MatchingPaths.Clear();
                MatchCount = 0;
                StatusMessage = "Filters cleared";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error clearing filters: {ex.Message}";
            }
        }

        private void ExecuteSelectMatching()
        {
            if (RootFolder == null || !HasActiveFilter)
            {
                return;
            }

            try
            {
                StatusMessage = "Selecting matching items...";
                SelectMatchingItems(RootFolder, true);
                ExecuteEstimateTokenCount();
                StatusMessage = "Matching items selected";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error selecting matching items: {ex.Message}";
            }
        }

        private void ExecuteDeselectMatching()
        {
            if (RootFolder == null || !HasActiveFilter)
            {
                return;
            }

            try
            {
                StatusMessage = "Deselecting matching items...";
                SelectMatchingItems(RootFolder, false);
                ExecuteEstimateTokenCount();
                StatusMessage = "Matching items deselected";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deselecting matching items: {ex.Message}";
            }
        }
        
        private void ExecuteExpandMatching()
        {
            if (RootFolder == null || !HasActiveFilter)
            {
                return;
            }

            try
            {
                StatusMessage = "Expanding to matching items...";
                RootFolder.ExpandToMatches();
                StatusMessage = "Expanded to show matching items";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error expanding to matches: {ex.Message}";
            }
        }
        
        private void ExecuteCollapseAll()
        {
            if (RootFolder == null)
            {
                return;
            }

            try
            {
                StatusMessage = "Collapsing all folders...";
                CollapseAllFolders(RootFolder);
                StatusMessage = "All folders collapsed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error collapsing folders: {ex.Message}";
            }
        }
        
        private void CollapseAllFolders(FolderItem folder)
        {
            // First collapse all subfolders recursively
            foreach (var subFolder in folder.Folders)
            {
                CollapseAllFolders(subFolder);
            }
            
            // Then collapse this folder (except the root folder)
            if (folder != RootFolder)
            {
                folder.IsExpanded = false;
            }
        }

        private void SelectMatchingItems(FolderItem folder, bool select)
        {
            // Set selection for matching files
            foreach (var file in folder.Files)
            {
                if (file.MatchType == MatchType.Direct)
                {
                    file.IsSelected = select;
                }
            }
            
            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                if (subFolder.MatchType == MatchType.Direct)
                {
                    // Only select/deselect the folder if it matches directly
                    subFolder.IsSelected = select;
                }
                else
                {
                    // Process the subfolder's contents without selecting the folder itself
                    SelectMatchingItems(subFolder, select);
                }
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
            folder.MatchType = MatchType.None;
            
            foreach (var file in folder.Files)
            {
                file.MatchType = MatchType.None;
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