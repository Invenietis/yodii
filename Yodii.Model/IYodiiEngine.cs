using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.ComponentModel;

namespace Yodii.Model
{
    /// <summary>
    /// Yodii engine is the primary object of Yodii.
    /// It is in charge of maintaining coherency among available plugins and services, their configuration and the evolution at runtime.
    /// It exposes all relevant information to the external world thanks to its <see cref="LiveInfo"/>.
    /// </summary>
    public interface IYodiiEngine : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current information about available plugins and services.
        /// </summary>
        IDiscoveredInfo DiscoveredInfo { get; }

        /// <summary>
        /// Change the current set of <see cref="IPluginInfo"/> and <see cref="IServiceInfo"/>.
        /// If <see cref="IsRunning"/> is true, this can be rejected: the result will indicate the reason of the failure.
        /// </summary>
        /// <param name="dicoveredInfo">New discovered information to work with.</param>
        /// <returns>Engine operation result.</returns>
        IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo dicoveredInfo );

        /// <summary>
        /// Gets the configuration: gives access to static configuration that will 
        /// necessarily be always satisfied.
        /// </summary>
        IConfigurationManager Configuration { get; }
        
        /// <summary>
        /// Live information about the running services and plugins, when the engine is started.
        /// </summary>
        ILiveInfo LiveInfo { get; }

        /// <summary>
        /// Whether this IYodiiEngine is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the engine (that must be stopped), performs all possible resolutions,
        /// and begins monitoring configuration for changes.
        /// </summary>
        /// <param name="persistedCommands">Optional list of commands that will be initialized.</param>
        /// <returns>Engine start result.</returns>
        /// <exception cref="InvalidOperationException">This engine must not be running (<see cref="IsRunning"/> must be false).</exception>
        IYodiiEngineResult Start( IEnumerable<YodiiCommand> persistedCommands = null );

        /// <summary>
        /// Stops the engine: stops monitoring configuration.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Triggers the static resolution of the graph (with the current <see cref="DiscoveredInfo"/> and <see cref="Configuration"/>).
        /// This has no impact on the engine and can be called when <see cref="IsRunning"/> is false.
        /// </summary>
        /// <returns>
        /// <para>
        /// The result with a potential non null <see cref="IYodiiEngineResult.StaticFailureResult"/> but always an 
        /// available <see cref="IYodiiEngineStaticOnlyResult.StaticSolvedConfiguration"/>.
        /// </para>
        /// <para>
        /// This method is useful only for advanced scenarios (for instance before starting the engine).
        /// </para>
        /// </returns>
        IYodiiEngineStaticOnlyResult StaticResolutionOnly();

    }
}
