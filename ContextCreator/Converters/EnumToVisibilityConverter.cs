using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ContextCreator.Converters
{
    /// <summary>
    /// Converts an enum value to a Visibility value
    /// </summary>
    public class EnumToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts an enum value to a Visibility value
        /// </summary>
        /// <param name="value">The source enum</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The enum value to compare with</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Visibility.Visible if the values match, Visibility.Collapsed otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            return value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts back a Visibility value to an enum (not implemented)
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