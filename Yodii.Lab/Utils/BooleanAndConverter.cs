using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Yodii.Lab
{
    /// <summary>
    /// Returns the 'and' logic of all boolean values.
    /// </summary>
    public class BooleanAndConverter : IMultiValueConverter
    {
        /// <summary>
        /// Returns the 'and' logic of all boolean values.
        /// </summary>
        /// <param name="values">Values to use.</param>
        /// <param name="targetType">Boolean.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Boolean "and" result.</returns>
        public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            foreach( object value in values )
            {
                if( (value is bool) && (bool)value == false )
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetTypes">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Not used.</returns>
        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}
