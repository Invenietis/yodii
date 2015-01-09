#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Discoverer\IAssemblyInfo.cs) is part of CiviKey. 
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
using System.Reflection;
using CK.Core;

namespace Yodii.Model
{
    /// <summary>
    /// Assembly information for Yodii plugins and services.
    /// </summary>
    public interface IAssemblyInfo 
    {
        /// <summary>
        /// Gets the assembly location.
        /// </summary>
        Uri AssemblyLocation { get; }
        
        /// <summary>
        /// The name of the assembly.
        /// </summary>
        AssemblyName AssemblyName { get; }

        /// <summary>
        /// Gets the plugins located in this assembly.
        /// </summary>
        IReadOnlyList<IPluginInfo> Plugins { get; }
        
        /// <summary>
        /// Gets the services located in this assembly.
        /// </summary>
        IReadOnlyList<IServiceInfo> Services { get; }
    }
}
