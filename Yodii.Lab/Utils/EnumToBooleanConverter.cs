using System;
using System.Windows;
using System.Windows.Data;

namespace Yodii.Lab
{
    /// <summary>
    /// Utility class to convert an enum value to a bool depending on whether it equals parameter.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts an enum value to true if it equals parameter.
        /// </summary>
        /// <param name="value">Value of an enum, as string.</param>
        /// <param name="targetType">Target type/Boolean.</param>
        /// <param name="parameter">Value it should equal to. From the same enum as value.</param>
        /// <param name="culture">Current system culture.</param>
        /// <returns>True if value equals parameter.</returns>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( Enum.IsDefined( value.GetType(), value ) == false )
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse( value.GetType(), value.ToString() );

            return parameterValue.Equals( value );
        }

        /// <summary>
        /// Gives the enum value of parameter.
        /// </summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Target type/Enum.</param>
        /// <param name="parameter">Value to use.</param>
        /// <param name="culture">System culture.</param>
        /// <returns></returns>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            string parameterString = parameter as string;
            if( parameterString == null )
                return DependencyProperty.UnsetValue;

            return Enum.Parse( targetType, parameterString );
        }
    }
}
