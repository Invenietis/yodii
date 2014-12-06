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
