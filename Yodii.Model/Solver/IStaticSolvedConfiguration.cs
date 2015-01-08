#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Solver\IStaticSolvedConfiguration.cs) is part of CiviKey. 
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
    /// Solved configuration after static resolution.
    /// </summary>
    public interface IStaticSolvedConfiguration
    {
        /// <summary>
        /// Gets an immutable list of static solved plugins.
        /// </summary>
        IReadOnlyList<IStaticSolvedPlugin> Plugins { get; }

        /// <summary>
        /// Gets an immutable list of static solved services.
        /// </summary>
        IReadOnlyList<IStaticSolvedService> Services { get; }

        /// <summary>
        /// Gets an immutable list of static items information (the union of <see cref="Plugins"/> and <see cref="Services"/>).
        /// </summary>
        IReadOnlyList<IStaticSolvedYodiiItem> YodiiItems { get; }

        /// <summary>
        /// Finds a plugin or a service by its full name.
        /// </summary>
        /// <param name="fullName">Service's or Plugin's full name.</param>
        /// <returns>Static solved Yodii item information.</returns>
        IStaticSolvedYodiiItem FindItem( string fullName );

        /// <summary>
        /// Finds a service by its full name.
        /// </summary>
        /// <param name="serviceFullName">Service's full name.</param>
        /// <returns>Static solved service.</returns>
        IStaticSolvedService FindService( string serviceFullName );

        /// <summary>
        /// Finds a plugin by its full name.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name.</param>
        /// <returns>Static solved plugin.</returns>
        IStaticSolvedPlugin FindPlugin( string pluginFullName );
    }
}
