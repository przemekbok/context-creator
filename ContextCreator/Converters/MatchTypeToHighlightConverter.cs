using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ContextCreator.Models;

namespace ContextCreator.Converters
{
    /// <summary>
    /// Converts a MatchType value to a highlight color
    /// </summary>
    public class MatchTypeToHighlightConverter : IValueConverter
    {
        /// <summary>
        /// Converts a MatchType value to a highlight brush
        /// </summary>
        /// <param name="value">The source data</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>A brush for highlighting</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MatchType matchType)
            {
                return matchType switch
                {
                    MatchType.Direct => new SolidColorBrush(Colors.Yellow),      // Bright yellow for direct matches
                    MatchType.Ancestor => new SolidColorBrush(Color.FromRgb(255, 249, 196)), // Light yellow for ancestors
                    MatchType.None => new SolidColorBrush(Colors.Transparent),   // No highlighting
                    _ => new SolidColorBrush(Colors.Transparent)
                };
            }
            
            return new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>
        /// Converts back a brush to a MatchType value (not implemented)
        /// </summary>
        /// <param name="value">The source data</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Not implemented</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}