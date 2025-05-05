using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ContextCreator.Models
{
    /// <summary>
    /// Represents a saved context configuration
    /// </summary>
    public class ContextConfiguration
    {
        public string Name { get; set; } = "Unnamed Configuration";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public string RootFolder { get; set; } = "";
        public List<string> SelectedPaths { get; set; } = new List<string>();

        /// <summary>
        /// Saves a context configuration to a file
        /// </summary>
        /// <param name="filePath">Path where to save the configuration</param>
        public void SaveToFile(string filePath)
        {
            ModifiedDate = DateTime.Now;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a context configuration from a file
        /// </summary>
        /// <param name="filePath">Path to the configuration file</param>
        /// <returns>The loaded configuration</returns>
        public static ContextConfiguration LoadFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ContextConfiguration>(json) ?? new ContextConfiguration();
        }
    }
}