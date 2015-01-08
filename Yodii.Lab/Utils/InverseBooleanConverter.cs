#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Utils\InverseBooleanConverter.cs) is part of CiviKey. 
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
