#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\IYodiiEngineHost.cs) is part of CiviKey. 
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
    /// Yodii engine host, handling the runtime for started plugins.
    /// </summary>
    public interface IYodiiEngineHost
    {
        /// <summary>
        /// Applies the given plugin configuration to the system.
        /// </summary>
        /// <remarks>
        /// Called by the IYodiiEngine when the engine is successfully started, or when configuration changes.
        /// </remarks>
        /// <param name="toDisable">List of plugins to stop and disable, effectively preventing them from running.</param>
        /// <param name="toStop">List of plugins to stop.</param>
        /// <param name="toStart">List of plugins to start.</param>
        /// <param name="postActionsCollector">
        /// Collector for actions triggered by the start or stop of the plugins.
        /// Null when the engine is stopping (<paramref name="toDisable"/> contains all the plugins in this case). 
        /// </param>
        /// <returns>List of exceptions encountered while each plugin changed state.</returns>
        IYodiiEngineHostApplyResult Apply( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> toStart, Action<Action<IYodiiEngine>> postActionsCollector ); 
    }
}
