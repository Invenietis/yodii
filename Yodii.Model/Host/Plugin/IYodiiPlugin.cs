using System;

namespace Yodii.Model
{
    /// <summary>
    /// This interface defines the minimal properties and behavior of a plugin.
    /// </summary>
    public interface IYodiiPlugin
    {
        /// <summary>
        /// This method initializes the plugin: own resources must be acquired and running conditions should be tested.
        /// No interaction with other plugins must occur (interactions must be in <see cref="Start"/>).
        /// </summary>
        /// <param name="info">Enables the implementation to give detailed information in case of error.</param>
        /// <returns>True on success. When returning false, <see cref="PluginSetupInfo"/> should be used to return detailed explanations.</returns>
        bool Setup( PluginSetupInfo info );

        /// <summary>
        /// This method must start the plugin: it is called only if <see cref="Setup"/> returned true.
        /// Implementations can interact with other components (such as subscribing to their events).
        /// </summary>
        void Start();

        /// <summary>
        /// This method uninitializes the plugin (it is called after <see cref="Stop"/>).
        /// Implementations MUST NOT interact with any other external components: only internal resources should be freed.
        /// </summary>
        void Teardown();

        /// <summary>
        /// This method is called by the host when the plugin must not be running anymore.
        /// Implementations can interact with other components (such as unsubscribing to their events). 
        /// <see cref="Teardown"/> will be called to finalize the stop.
        /// </summary>
        void Stop();
    }
}
