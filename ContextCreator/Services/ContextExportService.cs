using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContextCreator.Models;

namespace ContextCreator.Services
{
    /// <summary>
    /// Service for exporting context information
    /// </summary>
    public class ContextExportService
    {
        /// <summary>
        /// Estimates the token count for the selected files
        /// </summary>
        /// <param name="selectedFiles">List of selected file paths</param>
        /// <returns>Estimated token count</returns>
        public async Task<int> EstimateTokenCountAsync(List<string> selectedFiles)
        {
            int totalTokens = 0;
            
            foreach (var filePath in selectedFiles)
            {
                try
                {
                    // Skip binary files
                    if (IsBinaryFile(filePath))
                    {
                        continue;
                    }
                    
                    // Read file content
                    string content = await File.ReadAllTextAsync(filePath);
                    
                    // Rough estimation: 1 token ≈ 4 characters
                    totalTokens += content.Length / 4;
                }
                catch (Exception ex)
                {
                    // Handle file access exceptions
                    System.Diagnostics.Debug.WriteLine($"Error estimating tokens for {filePath}: {ex.Message}");
                }
            }
            
            return totalTokens;
        }
        
        /// <summary>
        /// Exports the selected files to a context file
        /// </summary>
        /// <param name="selectedFiles">List of selected file paths</param>
        /// <param name="outputPath">Path to export the context file</param>
        /// <param name="format">Format of the exported file</param>
        /// <returns>True if export was successful, false otherwise</returns>
        public async Task<bool> ExportContextAsync(List<string> selectedFiles, string outputPath, ExportFormat format)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                
                // Add header based on format
                switch (format)
                {
                    case ExportFormat.Markdown:
                        sb.AppendLine("# LLM Context");
                        sb.AppendLine();
                        break;
                    case ExportFormat.Json:
                        sb.AppendLine("{");
                        sb.AppendLine("  \"files\": [");
                        break;
                    case ExportFormat.Plain:
                    default:
                        sb.AppendLine("LLM Context");
                        sb.AppendLine(new string('-', 50));
                        break;
                }
                
                // Process each file
                for (int i = 0; i < selectedFiles.Count; i++)
                {
                    var filePath = selectedFiles[i];
                    
                    // Skip binary files
                    if (IsBinaryFile(filePath))
                    {
                        continue;
                    }
                    
                    try
                    {
                        string content = await File.ReadAllTextAsync(filePath);
                        string fileName = Path.GetFileName(filePath);
                        
                        // Format the content based on the selected format
                        switch (format)
                        {
                            case ExportFormat.Markdown:
                                sb.AppendLine($"## {fileName}");
                                sb.AppendLine();
                                sb.AppendLine("```");
                                sb.AppendLine(content);
                                sb.AppendLine("```");
                                sb.AppendLine();
                                break;
                            case ExportFormat.Json:
                                string escapedContent = content.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
                                sb.AppendLine("    {");
                                sb.AppendLine($"      \"name\": \"{fileName}\",");
                                sb.AppendLine($"      \"content\": \"{escapedContent}\"");
                                sb.AppendLine(i == selectedFiles.Count - 1 ? "    }" : "    },");
                                break;
                            case ExportFormat.Plain:
                            default:
                                sb.AppendLine($"File: {fileName}");
                                sb.AppendLine(new string('-', 50));
                                sb.AppendLine(content);
                                sb.AppendLine(new string('-', 50));
                                sb.AppendLine();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle file access exceptions
                        System.Diagnostics.Debug.WriteLine($"Error exporting {filePath}: {ex.Message}");
                    }
                }
                
                // Add footer based on format
                if (format == ExportFormat.Json)
                {
                    sb.AppendLine("  ]");
                    sb.AppendLine("}");
                }
                
                // Write to output file
                await File.WriteAllTextAsync(outputPath, sb.ToString());
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during context export: {ex.Message}");
                return false;
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
    
    /// <summary>
    /// Defines the format of the exported context file
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>
        /// Plain text format
        /// </summary>
        Plain,
        
        /// <summary>
        /// Markdown format
        /// </summary>
        Markdown,
        
        /// <summary>
        /// JSON format
        /// </summary>
        Json
    }
}