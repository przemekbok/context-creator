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
        private MatchType _matchType = MatchType.None;

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
                    if (value)
                    {
                        MatchType = MatchType.Direct;
                    }
                    else
                    {
                        MatchType = MatchType.None;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public MatchType MatchType
        {
            get => _matchType;
            set
            {
                if (_matchType != value)
                {
                    _matchType = value;
                    _isMatch = value != MatchType.None;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsMatch));
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

    /// <summary>
    /// Defines the type of match for highlighting purposes
    /// </summary>
    public enum MatchType
    {
        /// <summary>
        /// Not a match
        /// </summary>
        None,

        /// <summary>
        /// Direct match (the file/folder itself matches the filter)
        /// </summary>
        Direct,

        /// <summary>
        /// Ancestor match (parent/grandparent folder leading to a match)
        /// </summary>
        Ancestor
    }
}