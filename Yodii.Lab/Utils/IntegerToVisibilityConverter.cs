using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Yodii.Lab
{
    [ValueConversion( typeof( ICollection ), typeof( bool ) )]
    class IntegerToVisibilityConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            int c = (int)value;

            Visibility result;
            if( parameter != null )
            {
                // With parameter, return collapsed if zero
                result = c == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                // Without parameter, return visible if zero
                result = c == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return result;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return value;
        }
    }
}
