using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Yodii.Lab
{
    /// <summary>
    /// Inverse boolean converter.
    /// </summary>
    [ValueConversion( typeof( bool ), typeof( bool ) )]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts a boolean value to its inverse.
        /// </summary>
        /// <param name="value">Boolean value to convert.</param>
        /// <param name="targetType">Type to convert to; must be boolean.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns></returns>
        public object Convert( object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture )
        {
            return !(bool)value;
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns></returns>
        public object ConvertBack( object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        #endregion
    }


}
