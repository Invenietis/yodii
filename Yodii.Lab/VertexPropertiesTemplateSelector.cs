#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\VertexPropertiesTemplateSelector.cs) is part of CiviKey. 
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

using System.Windows;
using System.Windows.Controls;

namespace Yodii.Lab
{
    /// <summary>
    /// Properties template selector.
    /// Determines whether the plugin or service properties should be displayed.
    /// </summary>
    public class VertexPropertiesTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Template to use for services.
        /// </summary>
        public DataTemplate ServicePropertiesTemplate { get; set; }

        /// <summary>
        /// Template to use for plugins.
        /// </summary>
        public DataTemplate PluginPropertiesTemplate { get; set; }

        /// <summary>
        /// Selects template.
        /// </summary>
        /// <param name="item">Service or plugin.</param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate( object item, DependencyObject container )
        {
            if( item == null ) return null;

            YodiiGraphVertex vertex = (YodiiGraphVertex)item;

            if( vertex.IsPlugin ) {
                return PluginPropertiesTemplate;
            }
            else
            {
                return ServicePropertiesTemplate;
            }

        }
    }
}
