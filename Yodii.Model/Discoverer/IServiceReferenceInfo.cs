#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Discoverer\IServiceReferenceInfo.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// Service reference information.
    /// </summary>
    public interface IServiceReferenceInfo
    {
        /// <summary>
        /// Gets the <see cref="IPluginInfo"/> that defines this reference.
        /// </summary>
        IPluginInfo Owner { get; }

        /// <summary>
        /// Gets a reference to the actual service.
        /// </summary>
        IServiceInfo Reference { get; }

        /// <summary>
        /// Gets the requirement for the referenced service.
        /// </summary>
        DependencyRequirement Requirement { get; }
        
        /// <summary>
        /// Gets the name of the constructor parameter that references the service.
        /// </summary>
        string ConstructorParameterName { get; }

        /// <summary>
        /// Gets the index of the parameter in the constructor.
        /// This is used by the dependency injection engine (IServiceHost and IPluginHost).
        /// </summary>
        int ConstructorParameterIndex { get; }

        /// <summary>
        /// Gets whether the <see cref="Reference"/> is directly expressed without <see cref="IRunningService{T}"/> wrapper.
        /// </summary>
        bool IsNakedRunningService { get; }
    }
}
