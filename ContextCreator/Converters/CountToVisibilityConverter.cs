using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ContextCreator.Converters
{
    /// <summary>
    /// Converts a count value to a visibility value
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a count value to a visibility value
        /// </summary>
        /// <param name="value">The source count</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Threshold count</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Visibility.Visible if count > threshold, Visibility.Collapsed otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && parameter is string strThreshold && int.TryParse(strThreshold, out int threshold))
            {
                return count == threshold ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts back a visibility value to a count (not implemented)
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