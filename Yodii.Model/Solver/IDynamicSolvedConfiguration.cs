#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Solver\IDynamicSolvedConfiguration.cs) is part of CiviKey. 
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
    /// Solved configuration after dynamic resolution.
    /// </summary>
    public interface IDynamicSolvedConfiguration
    {
        /// <summary>
        /// Solved dynamic plugins.
        /// </summary>
        IReadOnlyList<IDynamicSolvedPlugin> Plugins { get; }

        /// <summary>
        /// Solved dynamic services.
        /// </summary>
        IReadOnlyList<IDynamicSolvedService> Services { get; }

        /// <summary>
        /// Gets an immutable list of dynamic items information (the union of <see cref="Plugins"/> and <see cref="Services"/>).
        /// </summary>
        IReadOnlyList<IDynamicSolvedYodiiItem> YodiiItems { get; }

        /// <summary>
        /// Finds a plugin or a service by its full name.
        /// </summary>
        /// <param name="fullName">Service's or Plugin's full name.</param>
        /// <returns>Solved dynamic item information.</returns>
        IDynamicSolvedYodiiItem FindItem( string fullName );

        /// <summary>
        /// Finds a service by its full name.
        /// </summary>
        /// <param name="serviceFullName">Service's full name.</param>
        /// <returns>Solved dynamic service.</returns>
        IDynamicSolvedService FindService( string serviceFullName );

        /// <summary>
        /// Finds a plugin by its full name.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name.</param>
        /// <returns>Solved dynamic plugin.</returns>
        IDynamicSolvedPlugin FindPlugin( string pluginFullName );
    }
}
