using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// Conversion utility between null and a Visibility value..
    /// </summary>
    [ValueConversion( typeof( object ), typeof( Visibility ) )]
    public class NullToVisibilityValueConverter : IValueConverter
    {
        /// <summary>
        /// Returns Collapsed if value is null, or Visible if parameter is set.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if( parameter != null )
                return value == null ? Visibility.Visible : Visibility.Collapsed;
            else
                return value == null ? Visibility.Collapsed : Visibility.Visible;

        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
