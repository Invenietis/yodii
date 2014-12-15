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
        /// <returns>List of exceptions encountered while each plugin changed state.</returns>
        IYodiiEngineHostApplyResult Apply( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> toStart ); 
    }
}
