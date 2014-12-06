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
    /// Legacy code (used to be YodiiEngineHostMock), unused atm.
    /// </remarks>
    class BuggyYodiiEngineHostMock : IYodiiEngineHost
    {
        internal BuggyYodiiEngineHostMock()
        {

        }

        public IYodiiEngineHostApplyResult Apply( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> toStart )
        {
            Debug.Assert( toDisable.Any() || toStop.Any() || toStart.Any() );

            List<IPluginHostApplyCancellationInfo> pluginErrors = new List<IPluginHostApplyCancellationInfo>();
            List<Action<IYodiiEngine>> actions = new List<Action<IYodiiEngine>>();

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

            return new Result( pluginErrors.AsReadOnlyList(), actions.AsReadOnlyList() );
        }

        class Result : IYodiiEngineHostApplyResult
        {
            public Result( IReadOnlyList<IPluginHostApplyCancellationInfo> errors, IReadOnlyList<Action<IYodiiEngine>> actions )
            {
                CancellationInfo = errors;
                PostStartActions = actions;
            }

            public IReadOnlyList<IPluginHostApplyCancellationInfo> CancellationInfo { get; private set; }

            public IReadOnlyList<Action<IYodiiEngine>> PostStartActions { get; private set; }
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
