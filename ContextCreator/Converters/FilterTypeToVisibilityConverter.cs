using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ContextCreator.Models;

namespace ContextCreator.Converters
{
    /// <summary>
    /// Converts a FilterType enum value to a Visibility value
    /// </summary>
    public class FilterTypeToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a FilterType value to a Visibility value
        /// </summary>
        /// <param name="value">The source FilterType</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The FilterType to compare with</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Visibility.Visible if values match, Visibility.Collapsed otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FilterType filterType && parameter is FilterType targetFilterType)
            {
                return filterType == targetFilterType ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts back a Visibility value to a FilterType (not implemented)
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