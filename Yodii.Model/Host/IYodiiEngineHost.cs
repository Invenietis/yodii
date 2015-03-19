#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Host\IYodiiEngineHost.cs) is part of CiviKey. 
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
        /// Gets or sets the associated <see cref="IYodiiEngineExternal"/>. 
        /// It must be set before starting the engine and only once.
        /// </summary>
        IYodiiEngineExternal Engine { get; set; }

        /// <summary>
        /// Gets or sets whether exceptions that occurred during calls to <see cref="IYodiiPlugin.PreStart"/> 
        /// or <see cref="IYodiiPlugin.PreStop"/> are intercepted and considered as if the PreStart/Stop rejected the transition.
        /// Defaults to false. Can be changed at any moment.
        /// </summary>
        bool CatchPreStartOrPreStopExceptions { get; set; }
        
        /// <summary>
        /// Applies the given plugin configuration to the system.
        /// </summary>
        /// <remarks>
        /// Called by the IYodiiEngine when the engine is successfully started, or when configuration changes.
        /// </remarks>
        /// <param name="solvedConfiguration">List of plugins with their state to apply.</param>
        /// <param name="postActionsCollector">
        /// Collector for actions triggered by the start or stop of the plugins.
        /// Null when the engine is stopping (<paramref name="solvedConfiguration"/> contains all the plugins with a disable state). 
        /// </param>
        /// <returns>List of exceptions encountered while each plugin changed state.</returns>
        IYodiiEngineHostApplyResult Apply( IReadOnlyList<KeyValuePair<IPluginInfo,RunningStatus>> solvedConfiguration, Action<Action<IYodiiEngineExternal>> postActionsCollector ); 
    }
}
