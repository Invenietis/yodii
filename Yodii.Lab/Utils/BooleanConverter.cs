#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Utils\BooleanConverter.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Yodii.Lab
{
    /// <summary>
    /// General boolean converter.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    public class BooleanConverter<T> : IValueConverter
    {
        /// <summary>
        /// Creates a new converter.
        /// </summary>
        /// <param name="trueValue">When true</param>
        /// <param name="falseValue">When false</param>
        public BooleanConverter( T trueValue, T falseValue )
        {
            True = trueValue;
            False = falseValue;
        }

        /// <summary>
        /// When true
        /// </summary>
        public T True { get; set; }

        /// <summary>
        /// When false
        /// </summary>
        public T False { get; set; }

        /// <summary>
        /// Performs the conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return value is bool && ((bool)value) ? True : False;
        }

        /// <summary>
        /// Performs inverse conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return value is T && EqualityComparer<T>.Default.Equals( (T)value, True );
        }
    }

    /// <summary>
    /// Boolean to visibility converter.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        /// <summary>
        /// General Boolean to visibility converter.
        /// </summary>
        public BooleanToVisibilityConverter() :
            base( Visibility.Visible, Visibility.Collapsed ) { }
    }
}
