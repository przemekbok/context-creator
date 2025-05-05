using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ContextCreator.Models
{
    /// <summary>
    /// Represents a file in the file explorer
    /// </summary>
    public class FileItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isMatch;

        public string Name { get; }
        public string FullPath { get; }
        public DateTime LastModified { get; }
        public long Size { get; }
        public FolderItem Parent { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
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

        public FileItem(string fullPath, FolderItem parent)
        {
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);
            Parent = parent;
            
            var fileInfo = new FileInfo(fullPath);
            LastModified = fileInfo.LastWriteTime;
            Size = fileInfo.Length;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}