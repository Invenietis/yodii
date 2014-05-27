using System;

namespace Yodii.Model
{
    /// <summary>
    /// Event argument when a plugin <see cref="RunningStatus">status</see> changed.
    /// </summary>
    public class PluginStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the plugin proxy.
        /// </summary>
        public IPluginProxy PluginProxy { get; private set; }

        /// <summary>
        /// Gets the previous status.
        /// </summary>
        public InternalRunningStatus Previous { get; private set; }

        /// <summary>
        /// Initializes a new instance of a <see cref="PluginStatusChangedEventArgs"/>.
        /// </summary>
        /// <param name="previous">The previous running status.</param>
        /// <param name="current">The plugin proxy.</param>
        public PluginStatusChangedEventArgs( InternalRunningStatus previous, IPluginProxy pluginProxy )
        {
            Previous = previous;
            PluginProxy = pluginProxy;
        }
    }
}
