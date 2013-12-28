﻿using System;
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
        IConfigurationManager Configuration { get; }
        
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
        IYodiiEngineResult Start( IEnumerable<YodiiCommand> persistedCommands = null );

        /// <summary>
        /// Stops the engine: stops monitoring configuration.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Change the current set of <see cref="IPluginInfo"/> and <see cref="IServiceInfo"/>.
        /// </summary>
        /// <param name="dicoveredInfo">Discovered information to work with.</param>
        /// <returns>Engine start result.</returns>
        IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo dicoveredInfo );

        /// <summary>
        /// Triggers the static resolution of the graph (with the current <see cref="DiscoveredInfo"/> and <see cref="Configuration"/>).
        /// This has no impact on the engine and can be called when <see cref="IsRunning"/> is false.
        /// </summary>
        /// <returns>
        /// The result with a potential non null <see cref="IYodiiEngineResult.StaticFailureResult"/> but always an 
        /// available <see cref="IYodiiEngineStaticOnlyResult.StaticSolvedConfiguration"/>.
        /// </returns>
        IYodiiEngineStaticOnlyResult StaticResolutionOnly();

    }
}
