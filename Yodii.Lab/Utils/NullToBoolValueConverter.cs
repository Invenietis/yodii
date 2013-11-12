using System;
using System.Globalization;
using System.Windows.Data;

namespace Yodii.Lab
{
    [ValueConversion( typeof( object ), typeof( bool ) )]
    public class NullToBoolValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            bool result = value == null ? true : false;
            if( parameter != null )
                return !result;
            return result;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return value;
        }
    }
}
