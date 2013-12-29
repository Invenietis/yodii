using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Engine
{
    enum ConfigurationSolverStep
    {
        RegisterServices,
        RegisterPlugins,
        OnAllPluginsAdded,
        PropagatePluginStatus,
        InitializeFinalStartableStatus,
        BlockingDetection,
        DynamicResolution,
        StaticError,
        WaitingForDynamicResolution
    }

    /// <summary>
    /// Internal ConfigurationSolver interface.
    /// </summary>
    interface IConfigurationSolver
    {
        /// <summary>
        /// Gets the current step.
        /// </summary>
        ConfigurationSolverStep Step { get; }

        /// <summary>
        /// Finds a service by its name that must exist (otherwise an exception is thrown).
        /// </summary>
        /// <param name="serviceFullName">The service name.</param>
        /// <returns>The ServiceData.</returns>
        ServiceData FindExistingService( string serviceFullName );

        /// <summary>
        /// Finds a plugin by its name that must exist (otherwise an exception is thrown).
        /// </summary>
        /// <param name="pluginFullName">The plugin name.</param>
        /// <returns>The PluginData.</returns>
        PluginData FindExistingPlugin( string pluginFullName );

        /// <summary>
        /// Finds a service by its name. Returns null if it does not exist.
        /// </summary>
        /// <param name="serviceFullName">The service name.</param>
        /// <returns>Null if not found.</returns>
        ServiceData FindService( string serviceFullName );

        /// <summary>
        /// Finds a plugin by its name. Returns null if it does not exist.
        /// </summary>
        /// <param name="pluginFullName">The plugin name.</param>
        /// <returns>Null if not found.</returns>
        PluginData FindPlugin( string pluginFullName );
        
        /// <summary>
        /// Gets all the ServiceData ordered by their name.
        /// </summary>
        IEnumerable<ServiceData> AllServices { get; }
        
        /// <summary>
        /// Gets all the PluginData ordered by their name.
        /// </summary>
        IEnumerable<PluginData> AllPlugins { get; }
        
        /// <summary>
        /// Used during static resolution to stack propagation 
        /// instead of relying on recursion.
        /// </summary>
        /// <param name="s">The ServiceData for which status must be propagated.</param>
        void DeferPropagation( ServiceData s );
    }

}
