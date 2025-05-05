using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ContextCreator.Models
{
    /// <summary>
    /// Represents a folder in the file explorer
    /// </summary>
    public class FolderItem : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isLoaded;
        private bool _isMatch;

        public string Name { get; }
        public string FullPath { get; }
        public FolderItem? Parent { get; }
        public ObservableCollection<FolderItem> Folders { get; } = new ObservableCollection<FolderItem>();
        public ObservableCollection<FileItem> Files { get; } = new ObservableCollection<FileItem>();

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    if (_isExpanded && !_isLoaded)
                    {
                        LoadContent();
                    }
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    
                    // Update selection status for all children
                    foreach (var folder in Folders)
                    {
                        folder.IsSelected = value;
                    }
                    
                    foreach (var file in Files)
                    {
                        file.IsSelected = value;
                    }
                    
                    // Update parent selection status
                    UpdateParentSelectionStatus();
                }
            }
        }

        public bool IsMatch
        {
            get => _isMatch;
            set
            {
                if (_isMatch != value)
                {
                    _isMatch = value;
                    OnPropertyChanged();
                }
            }
        }

        public FolderItem(string fullPath, FolderItem? parent = null)
        {
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);
            // If root directory, use directory name
            if (string.IsNullOrEmpty(Name))
            {
                Name = fullPath;
            }
            Parent = parent;
        }

        public void LoadContent()
        {
            if (_isLoaded) return;

            try
            {
                // Load subfolders
                var directories = Directory.GetDirectories(FullPath);
                foreach (var directory in directories)
                {
                    Folders.Add(new FolderItem(directory, this));
                }

                // Load files
                var files = Directory.GetFiles(FullPath);
                foreach (var file in files)
                {
                    Files.Add(new FileItem(file, this));
                }

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                // Handle exceptions (access denied, etc.)
                System.Diagnostics.Debug.WriteLine($"Error loading folder {FullPath}: {ex.Message}");
            }
        }

        private void UpdateParentSelectionStatus()
        {
            if (Parent == null) return;

            // If all siblings are selected, parent should be selected
            // If none are selected, parent should be unselected
            // If some are selected, parent should be in indeterminate state (not handled here, would require a third state)
            
            var allSiblingsSelected = Parent.Folders.All(f => f.IsSelected) && Parent.Files.All(f => f.IsSelected);
            var noneSiblingsSelected = Parent.Folders.All(f => !f.IsSelected) && Parent.Files.All(f => !f.IsSelected);
            
            if (allSiblingsSelected)
            {
                Parent.IsSelected = true;
            }
            else if (noneSiblingsSelected)
            {
                Parent.IsSelected = false;
            }
            // For partial selection, we would need a third state
            
            // Propagate up the tree
            Parent.UpdateParentSelectionStatus();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}