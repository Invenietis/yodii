#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Discoverer\IPluginInfo.cs) is part of CiviKey. 
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
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Plugin information.
    /// </summary>
    public interface IPluginInfo : IDiscoveredItem
    {
        /// <summary>
        /// Gets the unique full name of the plugin (namespace and class name).
        /// </summary>
        string PluginFullName { get; }

        /// <summary>
        /// Gets the assembly info that contains this plugin.
        /// </summary>
        IAssemblyInfo AssemblyInfo { get; }

        /// <summary>
        /// Gets the services that this plugin references.
        /// </summary>
        IReadOnlyList<IServiceReferenceInfo> ServiceReferences { get; }

        /// <summary>
        /// Gets the service that this plugin implements. Null if the plugin does not implement any service.
        /// </summary>
        IServiceInfo Service { get; }

        /// <summary>
        /// Gets a desription of the constructor that has been selected and should be used to instanciate a plugin.
        /// </summary>
        IPluginCtorInfo ConstructorInfo { get; }
    }
}
