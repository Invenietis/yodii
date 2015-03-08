#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Discoverer\IServiceInfo.cs) is part of CiviKey. 
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
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    /// <summary>
    /// Service information.
    /// </summary>
    public interface IServiceInfo : IDiscoveredItem
    {
        /// <summary>
        /// Gets the unique full name of the service (namespace and interface name).
        /// </summary>
        string ServiceFullName { get; }

        /// <summary>
        /// Gets the <see cref="IServiceInfo"/> that generalizes this one if it exists.
        /// </summary>
        IServiceInfo Generalization { get; }

        /// <summary>
        /// Gets the assembly info that contains (defines) this interface.
        /// If the service interface itself has not been found, this is null.
        /// </summary>
        IAssemblyInfo AssemblyInfo { get; }

    }
}
