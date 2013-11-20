﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace Yodii.Lab
{
    public class ServiceOrPluginIdToDescriptionConverter : IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( values == null ) return String.Empty;
            Debug.Assert( values.Length == 2 );
            if( values[0] == null ) return String.Empty;

            Debug.Assert( values[1] != null );
            Debug.Assert( values[1] is ServiceInfoManager );

            return ((ServiceInfoManager)values[1]).GetDescriptionOfServiceOrPluginId( (string)values[0] );
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
