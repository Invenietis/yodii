using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    public interface IPluginProxy
    {
        /// <summary>
        /// Gets the plugin information object.
        /// </summary>
        IPluginInfo PluginInfo { get; }

        /// <summary>
        /// Current running status of the plugin.
        /// </summary>
        PluginStatus Status { get; }

        /// <summary>
        /// Gets the real instance of the underlying plugin.
        /// </summary>
        IYodiiPlugin RealPluginObject { get; }

        /// <summary>
        /// Exception raised when the plugin was last activated. Null if no error occured.
        /// </summary>
        Exception LoadError { get; }

    }

}
