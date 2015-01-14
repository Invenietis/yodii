#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Mocks\BuggyYodiiEngineHostMock.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{

    /// <summary>
    /// IYodiiEngineHost implementation that returns plugin errors when their name contains "buggy", but otherwise does nothing.
    /// </summary>
    /// <remarks>
    /// Legacy code (formerly YodiiEngineHostMock).
    /// </remarks>
    class BuggyYodiiEngineHostMock : IYodiiEngineHost
    {
        internal BuggyYodiiEngineHostMock()
        {
        }

        public IYodiiEngine Engine { get; set; }

        public Action<IYodiiEngineExternal> PostActionToAdd;

        public IYodiiEngineHostApplyResult Apply( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> toStart, Action<Action<IYodiiEngineExternal>> actionCollector )
        {
            Debug.Assert( toDisable.Any() || toStop.Any() || toStart.Any() );

            List<IPluginHostApplyCancellationInfo> pluginErrors = new List<IPluginHostApplyCancellationInfo>();
            List<Action<IYodiiEngineExternal>> actions = new List<Action<IYodiiEngineExternal>>();

            IEnumerable<IPluginInfo> toCheck = toDisable.Concat( toStop ).Concat( toStart );

            foreach( IPluginInfo plugin in toCheck )
            {
                if( plugin.PluginFullName.Contains( "buggy" ) )
                {
                    var cancelInfo = new PluginHostApplyCancellationInfo() {
                        Error = new Exception("HostError"),
                        ErrorMessage = "HostError",
                        IsLoadError = true,
                        IsStartCanceled = true,
                        IsStopCanceled = true,
                        Plugin = plugin
                    };
                    pluginErrors.Add( cancelInfo );
                }
            }
            if( actionCollector != null && PostActionToAdd != null ) actionCollector( PostActionToAdd );
            return new Result( pluginErrors.AsReadOnlyList(), actions.AsReadOnlyList() );
        }

        class Result : IYodiiEngineHostApplyResult
        {
            public Result( IReadOnlyList<IPluginHostApplyCancellationInfo> errors, IReadOnlyList<Action<IYodiiEngineExternal>> actions )
            {
                CancellationInfo = errors;
                PostStartActions = actions;
            }

            public IReadOnlyList<IPluginHostApplyCancellationInfo> CancellationInfo { get; private set; }

            public IReadOnlyList<Action<IYodiiEngineExternal>> PostStartActions { get; private set; }
        }

        class PluginHostApplyCancellationInfo : IPluginHostApplyCancellationInfo
        {
            public IPluginInfo Plugin { get; internal set; }

            public bool IsLoadError { get; internal set; }

            public bool IsStartCanceled { get; internal set; }

            public bool IsStopCanceled { get; internal set; }

            public string ErrorMessage { get; internal set; }

            public Exception Error { get; internal set; }
        }

    }
}
