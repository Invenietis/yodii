#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\IConfigurationLayer.cs) is part of CiviKey. 
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
using System.ComponentModel;
namespace Yodii.Model
{
    /// <summary>
    /// Configuration layer, containing configuration items.
    /// <see cref="IConfigurationManager"/>
    /// </summary>
    public interface IConfigurationLayer : INotifyPropertyChanged
    {
        /// <summary>
        /// Parent <see cref="IConfigurationManager"/>.
        /// </summary>
        IConfigurationManager ConfigurationManager { get; }

        /// <summary>
        /// Collection of IConfigurationItems in this layer.
        /// </summary>
        IConfigurationItemCollection Items { get; }

        /// <summary>
        /// Display name of the layer.
        /// </summary>
        string LayerName { get; set; }
    }
}
