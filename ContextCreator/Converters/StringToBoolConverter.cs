using System;
using System.Globalization;
using System.Windows.Data;

namespace ContextCreator.Converters
{
    /// <summary>
    /// Converts a string value to a boolean value
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string value to a boolean value
        /// </summary>
        /// <param name="value">The source string</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>True if the string is not null or empty, false otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        /// <summary>
        /// Converts a boolean value back to a string (not implemented)
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