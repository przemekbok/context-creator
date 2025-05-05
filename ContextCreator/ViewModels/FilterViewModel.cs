using System;
using System.Windows.Input;
using ContextCreator.Commands;
using ContextCreator.Models;

namespace ContextCreator.ViewModels
{
    /// <summary>
    /// View model for the filter dialog
    /// </summary>
    public class FilterViewModel : BaseViewModel
    {
        private FilterType _filterType = FilterType.FileName;
        private string _filterExpression = "";
        private bool _isCaseSensitive;
        private bool _isRegex;
        private FilterAction _filterAction = FilterAction.Include;
        private bool _applyToSelectedFoldersOnly;

        /// <summary>
        /// Gets or sets the filter type
        /// </summary>
        public FilterType FilterType
        {
            get => _filterType;
            set => SetProperty(ref _filterType, value);
        }

        /// <summary>
        /// Gets or sets the filter expression
        /// </summary>
        public string FilterExpression
        {
            get => _filterExpression;
            set => SetProperty(ref _filterExpression, value);
        }

        /// <summary>
        /// Gets or sets whether the filter is case sensitive
        /// </summary>
        public bool IsCaseSensitive
        {
            get => _isCaseSensitive;
            set => SetProperty(ref _isCaseSensitive, value);
        }

        /// <summary>
        /// Gets or sets whether the filter uses regular expressions
        /// </summary>
        public bool IsRegex
        {
            get => _isRegex;
            set => SetProperty(ref _isRegex, value);
        }

        /// <summary>
        /// Gets or sets whether the filter includes or excludes matching files
        /// </summary>
        public FilterAction FilterAction
        {
            get => _filterAction;
            set => SetProperty(ref _filterAction, value);
        }

        /// <summary>
        /// Gets or sets whether the filter applies only to selected folders
        /// </summary>
        public bool ApplyToSelectedFoldersOnly
        {
            get => _applyToSelectedFoldersOnly;
            set => SetProperty(ref _applyToSelectedFoldersOnly, value);
        }

        /// <summary>
        /// Gets the command to apply the filter
        /// </summary>
        public ICommand ApplyCommand { get; }

        /// <summary>
        /// Gets the command to cancel the filter operation
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Event raised when the filter is applied
        /// </summary>
        public event EventHandler<FilterOptions>? FilterApplied;

        /// <summary>
        /// Event raised when the filter operation is canceled
        /// </summary>
        public event EventHandler? FilterCanceled;

        /// <summary>
        /// Initializes a new instance of the FilterViewModel class
        /// </summary>
        public FilterViewModel()
        {
            ApplyCommand = new RelayCommand(ExecuteApply);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void ExecuteApply(object? parameter)
        {
            var options = new FilterOptions
            {
                Type = FilterType,
                Expression = FilterExpression,
                IsCaseSensitive = IsCaseSensitive,
                IsRegex = IsRegex,
                Action = FilterAction,
                ApplyToSelectedFoldersOnly = ApplyToSelectedFoldersOnly
            };

            FilterApplied?.Invoke(this, options);
        }

        private void ExecuteCancel(object? parameter)
        {
            FilterCanceled?.Invoke(this, EventArgs.Empty);
        }
    }
}