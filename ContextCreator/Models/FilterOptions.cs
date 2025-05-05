namespace ContextCreator.Models
{
    /// <summary>
    /// Represents options for filtering files and folders
    /// </summary>
    public class FilterOptions
    {
        /// <summary>
        /// Gets or sets the filter type
        /// </summary>
        public FilterType Type { get; set; } = FilterType.Content;

        /// <summary>
        /// Gets or sets the filter expression
        /// </summary>
        public string Expression { get; set; } = "";

        /// <summary>
        /// Gets or sets whether the filter is case sensitive
        /// </summary>
        public bool IsCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the filter uses regular expressions
        /// </summary>
        public bool IsRegex { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the filter includes or excludes matching files
        /// </summary>
        public FilterAction Action { get; set; } = FilterAction.Include;

        /// <summary>
        /// Gets or sets whether the filter applies only to selected folders
        /// </summary>
        public bool ApplyToSelectedFoldersOnly { get; set; } = false;
    }

    /// <summary>
    /// Defines the type of filter to apply
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// Filter based on file content
        /// </summary>
        Content,

        /// <summary>
        /// Filter based on file name
        /// </summary>
        FileName
    }

    /// <summary>
    /// Defines the action to take for matching files
    /// </summary>
    public enum FilterAction
    {
        /// <summary>
        /// Include matching files in the result
        /// </summary>
        Include,

        /// <summary>
        /// Exclude matching files from the result
        /// </summary>
        Exclude
    }
}