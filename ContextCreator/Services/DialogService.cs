using System;
using System.Windows;
using Microsoft.Win32;

namespace ContextCreator.Services
{
    /// <summary>
    /// Service for showing dialogs
    /// </summary>
    public class DialogService
    {
        /// <summary>
        /// Shows a folder browser dialog
        /// </summary>
        /// <param name="description">Description text for the dialog</param>
        /// <returns>The selected folder path or null if canceled</returns>
        public string? ShowFolderBrowserDialog(string description)
        {
            // In a WPF application, we would typically use the System.Windows.Forms.FolderBrowserDialog
            // but since we want a pure WPF solution, we'll use the OpenFileDialog with FileName picking instead
            
            // Windows Vista and higher support a better folder picker with the CommonOpenFileDialog from Windows API CodePack
            // but for simplicity and to avoid extra dependencies, we'll use this approach
            var dialog = new OpenFileDialog
            {
                Title = description,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection", // This is a placeholder
                ValidateNames = false,
                DereferenceLinks = true
            };

            if (dialog.ShowDialog() == true)
            {
                // Get the folder path by removing the filename part
                return System.IO.Path.GetDirectoryName(dialog.FileName);
            }

            return null;
        }

        /// <summary>
        /// Shows a save file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="filter">File filter</param>
        /// <param name="defaultExt">Default extension</param>
        /// <returns>The selected file path or null if canceled</returns>
        public string? ShowSaveFileDialog(string title, string filter, string defaultExt)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExt
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Shows an open file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="filter">File filter</param>
        /// <returns>The selected file path or null if canceled</returns>
        public string? ShowOpenFileDialog(string title, string filter)
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Shows a message box
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="title">Dialog title</param>
        /// <param name="button">Message box button</param>
        /// <param name="icon">Message box icon</param>
        /// <returns>Result of the message box</returns>
        public MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            return MessageBox.Show(message, title, button, icon);
        }
    }
}