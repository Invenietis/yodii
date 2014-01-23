using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace Yodii.Lab
{
    /// <summary>
    /// Value conversion utility to convert a service or plugin ID to its long description.
    /// </summary>
    public class ServiceOrPluginIdToDescriptionConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts a service or plugin ID to its long description, using data in parameter.
        /// </summary>
        /// <param name="values">
        /// Array:
        /// [0] is the service or plugin ID, as string.
        /// [1] is the LabStateManager it should ask the description from.
        /// </param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if( values == null ) return String.Empty;
            Debug.Assert( values.Length == 2 );
            if( values[0] == null || values[0] == DependencyProperty.UnsetValue || values[1] == null || !(values[1] is LabStateManager) ) return String.Empty;

            return ((LabStateManager)values[1]).GetDescriptionOfServiceOrPluginFullName( (string)values[0] );
        }

        /// <summary>
        /// Reverse conversion not implemented.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
