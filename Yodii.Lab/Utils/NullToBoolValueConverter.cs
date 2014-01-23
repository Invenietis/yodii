using System;
using System.Globalization;
using System.Windows.Data;

namespace Yodii.Lab
{
    /// <summary>
    /// Conversion utility between null and bool.
    /// </summary>
    [ValueConversion( typeof( object ), typeof( bool ) )]
    public class NullToBoolValueConverter : IValueConverter
    {
        /// <summary>
        /// Returns true if value is null, unless parameter is set.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            bool result = value == null ? true : false;
            if( parameter != null )
                return !result;
            return result;
        }

        /// <summary>
        /// Returns original value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return value;
        }
    }
}
