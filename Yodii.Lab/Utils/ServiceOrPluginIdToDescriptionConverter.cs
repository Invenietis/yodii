#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Utils\ServiceOrPluginIdToDescriptionConverter.cs) is part of CiviKey. 
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
