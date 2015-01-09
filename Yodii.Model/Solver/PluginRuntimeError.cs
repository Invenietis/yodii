#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Solver\PluginRuntimeError.cs) is part of CiviKey. 
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
    /// Runtime error encountered by a plugin when running in the host.
    /// </summary>
    public struct PluginRuntimeError
    {
        /// <summary>
        /// Culprit plugin.
        /// </summary>
        public readonly IDynamicSolvedPlugin Plugin;

        /// <summary>
        /// Detailed information about the error.
        /// </summary>
        public readonly IPluginHostApplyCancellationInfo CancellationInfo;

        /// <summary>
        /// Creates a new instance of PluginRuntimeError.
        /// </summary>
        /// <param name="plugin">Culprit plugin.</param>
        /// <param name="error">Error information.</param>
        public PluginRuntimeError( IDynamicSolvedPlugin plugin, IPluginHostApplyCancellationInfo error )
        {
            if( plugin == null ) throw new ArgumentNullException( "plugin" );
            if( error == null ) throw new ArgumentNullException( "error" );
            Plugin = plugin;
            CancellationInfo = error;
        }
    }
}
