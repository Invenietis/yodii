﻿using System;
using System.Windows;
using System.Windows.Data;

namespace Yodii.Lab
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( Enum.IsDefined( value.GetType(), value ) == false )
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse( value.GetType(), value.ToString() );

            return parameterValue.Equals( value );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            string parameterString = parameter as string;
            if( parameterString == null )
                return DependencyProperty.UnsetValue;

            return Enum.Parse( targetType, parameterString );
        }
    }
}