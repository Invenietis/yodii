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
    /// Yodii resolution engine.
    /// Generates a viable state for a Yodii host to be in, with a given configuration and
    /// given host information about what plugins and services are available.
    /// </summary>
    public interface IYodiiEngine : INotifyPropertyChanged
    {
        /// <summary>
        /// Host information about what plugins are available on the system.
        /// </summary>
        IDiscoveredInfo DiscoveredInfo { get; }

        /// <summary>
        /// Configuration manager.
        /// </summary>
        IConfigurationManager ConfigurationManager { get; }
        
        /// <summary>
        /// Currently active <see cref="YodiiCommand"/> on the engine.
        /// </summary>
        IObservableReadOnlyList<YodiiCommand> YodiiCommands { get; }

        /// <summary>
        /// Live information about the running services and plugins, when the engine is started.
        /// </summary>
        ILiveInfo LiveInfo { get; }

        /// <summary>
        /// Whether this IYodiiEngine is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the engine, performs all possible resolutions,
        /// and begins monitoring configuration for changes.
        /// </summary>
        /// <returns>Engine start result.</returns>
        IYodiiEngineResult Start();

        /// <summary>
        /// Stops the engine, and halts all configuration monitoring.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Change the current set of <see cref="IPluginInfo"/> and <see cref="IServiceInfo"/>.
        /// </summary>
        /// <param name="dicoveredInfo">Discovered information to work with.</param>
        /// <returns>Engine start result.</returns>
        IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo dicoveredInfo );

    }
}
