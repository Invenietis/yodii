#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\IConfigurationItem.cs) is part of CiviKey. 
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
    /// Configuration item data unifies <see cref="IConfigurationItem"/> that is bound to a <see cref="ICinfigurationManager"/> 
    /// and the <see cref="YodiiConfigurationItem"/> independent POCO.
    /// </summary>
    public interface IConfigurationItemData
    {
        /// <summary>
        /// Service or plugin identifier this configuration applies to.
        /// </summary>
        string ServiceOrPluginFullName { get; }

        /// <summary>
        /// Gets the required configuration status for this item.
        /// </summary>
        ConfigurationStatus Status { get; }

        /// <summary>
        /// Gets the required configuration impact for this item.
        /// </summary>
        StartDependencyImpact Impact { get; }

        /// <summary>
        /// Gets the optional description for this item.
        /// </summary>
        string Description { get; }
    }
}
