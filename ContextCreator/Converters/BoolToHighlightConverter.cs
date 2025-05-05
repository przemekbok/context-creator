using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ContextCreator.Converters
{
    /// <summary>
    /// Converts a boolean value to a highlight color
    /// </summary>
    public class BoolToHighlightConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a highlight brush
        /// </summary>
        /// <param name="value">The source data</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>A brush for highlighting</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isMatch && isMatch ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>
        /// Converts back a brush to a boolean value (not implemented)
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