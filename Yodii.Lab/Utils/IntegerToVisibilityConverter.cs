#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Utils\IntegerToVisibilityConverter.cs) is part of CiviKey. 
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
