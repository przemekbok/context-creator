using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ContextCreator.Models;

namespace ContextCreator.Services
{
    /// <summary>
    /// Provides filtering functionality for files and folders
    /// </summary>
    public class FilterService
    {
        /// <summary>
        /// Applies a filter to a folder hierarchy
        /// </summary>
        /// <param name="rootFolder">The root folder to filter</param>
        /// <param name="options">The filter options</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ApplyFilterAsync(FolderItem rootFolder, FilterOptions options)
        {
            // Reset all matches first
            ResetMatches(rootFolder);

            // Apply the filter based on the type
            if (options.Type == FilterType.FileName)
            {
                await ApplyFileNameFilterAsync(rootFolder, options);
            }
            else // Content filter
            {
                await ApplyContentFilterAsync(rootFolder, options);
            }
            
            // Ensure content is loaded for all folders for proper display
            EnsureContentLoaded(rootFolder);
        }

        private void ResetMatches(FolderItem folder)
        {
            folder.IsMatch = false;
            
            foreach (var file in folder.Files)
            {
                file.IsMatch = false;
            }

            foreach (var subFolder in folder.Folders)
            {
                ResetMatches(subFolder);
            }
        }
        
        private void EnsureContentLoaded(FolderItem folder)
        {
            // Load content for this folder
            folder.EnsureContentLoaded();
            
            // Recursively ensure content is loaded for all subfolders
            foreach (var subFolder in folder.Folders)
            {
                EnsureContentLoaded(subFolder);
            }
        }

        private async Task ApplyFileNameFilterAsync(FolderItem folder, FilterOptions options)
        {
            // Always load the content to ensure we can filter properly
            folder.EnsureContentLoaded();
            
            if (options.ApplyToSelectedFoldersOnly && !folder.IsSelected)
            {
                return;
            }

            // Check files in this folder
            foreach (var file in folder.Files)
            {
                bool isMatch;
                
                if (options.IsRegex)
                {
                    try
                    {
                        var regex = new Regex(options.Expression, 
                            options.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                        isMatch = regex.IsMatch(file.Name);
                    }
                    catch
                    {
                        // Invalid regex
                        isMatch = false;
                    }
                }
                else
                {
                    isMatch = options.IsCaseSensitive 
                        ? file.Name.Contains(options.Expression) 
                        : file.Name.Contains(options.Expression, StringComparison.OrdinalIgnoreCase);
                }

                // Apply the action (include/exclude)
                file.IsMatch = options.Action == FilterAction.Include ? isMatch : !isMatch;
                
                // If a file matches, the parent folder should also match
                if (file.IsMatch)
                {
                    folder.IsMatch = true;
                    
                    // Propagate match status up to parent folders
                    PropagateMatchToParents(folder);
                }
            }

            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                await ApplyFileNameFilterAsync(subFolder, options);
                
                // If a subfolder matches, this folder should also match
                if (subFolder.IsMatch)
                {
                    folder.IsMatch = true;
                    
                    // Propagate match status up to parent folders
                    PropagateMatchToParents(folder);
                }
            }

            // Use Task.CompletedTask to make this method awaitable
            await Task.CompletedTask;
        }

        private async Task ApplyContentFilterAsync(FolderItem folder, FilterOptions options)
        {
            // Always load the content to ensure we can filter properly
            folder.EnsureContentLoaded();
            
            if (options.ApplyToSelectedFoldersOnly && !folder.IsSelected)
            {
                return;
            }

            // Check files in this folder
            foreach (var file in folder.Files)
            {
                try
                {
                    // Skip binary files or very large files
                    if (IsBinaryFile(file.FullPath) || file.Size > 10 * 1024 * 1024)
                    {
                        continue;
                    }

                    // Read file content
                    string content = await File.ReadAllTextAsync(file.FullPath);
                    bool isMatch;

                    if (options.IsRegex)
                    {
                        try
                        {
                            var regex = new Regex(options.Expression,
                                options.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                            isMatch = regex.IsMatch(content);
                        }
                        catch
                        {
                            // Invalid regex
                            isMatch = false;
                        }
                    }
                    else
                    {
                        isMatch = options.IsCaseSensitive
                            ? content.Contains(options.Expression)
                            : content.Contains(options.Expression, StringComparison.OrdinalIgnoreCase);
                    }

                    // Apply the action (include/exclude)
                    file.IsMatch = options.Action == FilterAction.Include ? isMatch : !isMatch;

                    // If a file matches, the parent folder should also match
                    if (file.IsMatch)
                    {
                        folder.IsMatch = true;
                        
                        // Propagate match status up to parent folders
                        PropagateMatchToParents(folder);
                    }
                }
                catch (Exception ex)
                {
                    // Handle file access exceptions
                    System.Diagnostics.Debug.WriteLine($"Error reading file {file.FullPath}: {ex.Message}");
                }
            }

            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                await ApplyContentFilterAsync(subFolder, options);

                // If a subfolder matches, this folder should also match
                if (subFolder.IsMatch)
                {
                    folder.IsMatch = true;
                    
                    // Propagate match status up to parent folders
                    PropagateMatchToParents(folder);
                }
            }
        }
        
        private void PropagateMatchToParents(FolderItem folder)
        {
            var parent = folder.Parent;
            while (parent != null)
            {
                parent.IsMatch = true;
                parent = parent.Parent;
            }
        }

        private bool IsBinaryFile(string filePath)
        {
            // Simple check for binary files - better implementations exist
            var extension = Path.GetExtension(filePath).ToLower();
            var binaryExtensions = new[] { ".exe", ".dll", ".pdb", ".zip", ".rar", ".7z", ".png", ".jpg", ".jpeg", ".gif", ".pdf" };
            return binaryExtensions.Contains(extension);
        }
    }
}