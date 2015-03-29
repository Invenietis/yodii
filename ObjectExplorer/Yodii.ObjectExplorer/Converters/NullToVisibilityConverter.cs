using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NullGuard;

namespace Yodii.ObjectExplorer.Converters
{
    /// <summary>
    /// Converts a Null value to Visibility.Collapsed, and a non-null value to Visibility.Visible.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NullToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Converts a Null value to Visibility.Collapsed, and a non-null value to Visibility.Visible.
        /// A non-null parameter reverses the conversion.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use. If not null, the conversion will be reversed.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// Visibility.Collapsed if value was null, Visibility.Visible otherwise. A non-null parameter reverses this behavior.
        /// </returns>
        public object Convert( [AllowNull] object value, Type targetType, [AllowNull] object parameter, System.Globalization.CultureInfo culture )
        {
            bool visible = value != null;

            if( parameter != null ) visible = !visible;

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public object ConvertBack( [AllowNull] object value, Type targetType, [AllowNull]  object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
