using System;
using System.Globalization;
using System.Windows.Data;

namespace ContextCreator.Converters
{
    /// <summary>
    /// Converts an enum value to a boolean value for use with radio buttons
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts an enum value to a boolean value
        /// </summary>
        /// <param name="value">The source enum value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The enum value to compare with</param>
        /// <param name="culture">Culture info</param>
        /// <returns>True if the values match, false otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.Equals(parameter);
        }

        /// <summary>
        /// Converts a boolean value back to an enum value
        /// </summary>
        /// <param name="value">The source boolean value</param>
        /// <param name="targetType">The target enum type</param>
        /// <param name="parameter">The enum value to use if the boolean is true</param>
        /// <param name="culture">Culture info</param>
        /// <returns>The enum value if the boolean is true, otherwise DependencyProperty.UnsetValue</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null)
                return parameter;

            return System.Windows.DependencyProperty.UnsetValue;
        }
    }
}