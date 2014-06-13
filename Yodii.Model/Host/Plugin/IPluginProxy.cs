using System;

namespace Yodii.Model
{
    public interface IPluginProxy
    {
        /// <summary>
        /// Gets a key object that uniquely identifies a plugin.
        /// </summary>
        IPluginInfo PluginKey { get; }

        /// <summary>
        /// Gets the real instance of the underlying plugin.
        /// </summary>
        object RealPluginObject { get; }

        /// <summary>
        /// Exception raised when the plugin was last activated. Null if no error occured.
        /// </summary>
        Exception LoadError { get; }

        /// <summary>
        /// True if the concrete plugin has been activated without error.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Current running status of the plugin.
        /// </summary>
        InternalRunningStatus Status { get; }
    }
}
