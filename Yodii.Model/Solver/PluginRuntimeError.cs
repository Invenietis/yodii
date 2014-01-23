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
        /// Throwing Plugin
        /// </summary>
        public readonly IDynamicSolvedPlugin Plugin;

        /// <summary>
        /// Exception detail
        /// </summary>
        public readonly Exception Error;

        /// <summary>
        /// Creates a new instance of PluginRuntimeError.
        /// </summary>
        /// <param name="plugin">Throwing Plugin.</param>
        /// <param name="error">Exception encountered.</param>
        public PluginRuntimeError( IDynamicSolvedPlugin plugin, Exception error )
        {
            Plugin = plugin;
            Error = error;
        }
    }
}
