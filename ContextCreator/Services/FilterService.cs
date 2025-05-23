using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ContextCreator.Models;
using MatchType = ContextCreator.Models.MatchType;

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
            
            // Mark all folders in paths to matches with the Ancestor match type
            MarkAncestorFolders(rootFolder);
            
            // Expand all folders that lead to matches
            rootFolder.ExpandToMatches();
        }

        private void ResetMatches(FolderItem folder)
        {
            folder.MatchType = MatchType.None;
            
            foreach (var file in folder.Files)
            {
                file.MatchType = MatchType.None;
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
        
        private void MarkAncestorFolders(FolderItem folder)
        {
            bool hasDirectMatch = folder.MatchType == MatchType.Direct ||
                                  folder.Files.Any(f => f.MatchType == MatchType.Direct);
            
            bool hasMatchingChildren = folder.Folders.Any(f => 
                f.MatchType == MatchType.Direct || 
                f.MatchType == MatchType.Ancestor);
            
            // If folder has direct matches or matching children, mark parents as ancestors
            if (hasDirectMatch || hasMatchingChildren)
            {
                MarkParentsAsAncestors(folder);
            }
            
            // Recursively process all subfolders
            foreach (var subFolder in folder.Folders)
            {
                MarkAncestorFolders(subFolder);
            }
        }
        
        private void MarkParentsAsAncestors(FolderItem folder)
        {
            var parent = folder.Parent;
            while (parent != null)
            {
                // Only change if it's not already a direct match
                if (parent.MatchType != MatchType.Direct)
                {
                    parent.MatchType = MatchType.Ancestor;
                }
                
                parent = parent.Parent;
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
            bool hasMatchingFiles = false;
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
                else if (options.IsExactMatch)
                {
                    // Exact match comparison
                    isMatch = options.IsCaseSensitive 
                        ? file.Name.Equals(options.Expression) 
                        : file.Name.Equals(options.Expression, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    // Contains match comparison (default behavior)
                    isMatch = options.IsCaseSensitive 
                        ? file.Name.Contains(options.Expression) 
                        : file.Name.Contains(options.Expression, StringComparison.OrdinalIgnoreCase);
                }

                // Apply the action (include/exclude)
                if (options.Action == FilterAction.Include)
                {
                    file.MatchType = isMatch ? MatchType.Direct : MatchType.None;
                }
                else
                {
                    file.MatchType = !isMatch ? MatchType.Direct : MatchType.None;
                }
                
                // Track if this folder contains any matching files
                if (file.MatchType == MatchType.Direct)
                {
                    hasMatchingFiles = true;
                }
            }
            
            // Check if the folder name itself matches
            bool folderNameMatches = false;
            if (options.IsRegex)
            {
                try
                {
                    var regex = new Regex(options.Expression,
                        options.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    folderNameMatches = regex.IsMatch(folder.Name);
                }
                catch
                {
                    // Invalid regex
                    folderNameMatches = false;
                }
            }
            else if (options.IsExactMatch)
            {
                // Exact match for folder name
                folderNameMatches = options.IsCaseSensitive
                    ? folder.Name.Equals(options.Expression)
                    : folder.Name.Equals(options.Expression, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Contains match for folder name
                folderNameMatches = options.IsCaseSensitive
                    ? folder.Name.Contains(options.Expression)
                    : folder.Name.Contains(options.Expression, StringComparison.OrdinalIgnoreCase);
            }
            
            // Apply folder name match - only mark the folder as a direct match if its name matches
            if (options.Action == FilterAction.Include && folderNameMatches)
            {
                folder.MatchType = MatchType.Direct;
            }
            else if (options.Action == FilterAction.Exclude && !folderNameMatches)
            {
                folder.MatchType = MatchType.Direct;
            }
            // If the folder has matching files but its name doesn't match, mark it as an ancestor
            else if (hasMatchingFiles && folder.MatchType != MatchType.Direct)
            {
                folder.MatchType = MatchType.Ancestor;
            }

            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                await ApplyFileNameFilterAsync(subFolder, options);
                
                // If a subfolder is a direct match or contains matches, update this folder's status if needed
                if ((subFolder.MatchType == MatchType.Direct || subFolder.MatchType == MatchType.Ancestor) && 
                    folder.MatchType == MatchType.None)
                {
                    folder.MatchType = MatchType.Ancestor;
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
            bool hasMatchingFiles = false;
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
                    if (options.Action == FilterAction.Include)
                    {
                        file.MatchType = isMatch ? MatchType.Direct : MatchType.None;
                    }
                    else
                    {
                        file.MatchType = !isMatch ? MatchType.Direct : MatchType.None;
                    }

                    // Track if this folder contains any matching files
                    if (file.MatchType == MatchType.Direct)
                    {
                        hasMatchingFiles = true;
                    }
                }
                catch (Exception ex)
                {
                    // Handle file access exceptions
                    System.Diagnostics.Debug.WriteLine($"Error reading file {file.FullPath}: {ex.Message}");
                }
            }

            // If the folder has matching files, mark it as an ancestor (unless it's already a direct match)
            if (hasMatchingFiles && folder.MatchType != MatchType.Direct)
            {
                folder.MatchType = MatchType.Ancestor;
            }

            // Recursively process subfolders
            foreach (var subFolder in folder.Folders)
            {
                await ApplyContentFilterAsync(subFolder, options);

                // If a subfolder is a direct match or contains matches, update this folder's status if needed
                if ((subFolder.MatchType == MatchType.Direct || subFolder.MatchType == MatchType.Ancestor) && 
                    folder.MatchType == MatchType.None)
                {
                    folder.MatchType = MatchType.Ancestor;
                }
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