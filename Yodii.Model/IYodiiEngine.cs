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
    /// Yodii engine is the primary object of Yodii.
    /// It is in charge of maintaining coherency among available plugins and services, their configuration and the evolution at runtime.
    /// It exposes all relevant information to the external world thanks to its <see cref="LiveInfo"/>.
    /// </summary>
    public interface IYodiiEngine : IYodiiEngineBase
    {
        /// <summary>
        /// Whether this engine is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the engine (that must be stopped), performs all possible resolutions,
        /// and begins monitoring configuration for changes.
        /// </summary>
        /// <param name="persistedCommands">Optional list of commands that will be initialized.</param>
        /// <returns>Engine start result.</returns>
        /// <exception cref="InvalidOperationException">This engine must not be running (<see cref="IsRunning"/> must be false).</exception>
        IYodiiEngineResult StartEngine( IEnumerable<YodiiCommand> persistedCommands = null );

        /// <summary>
        /// Stops the engine: stops all plugins and stops monitoring configuration.
        /// </summary>
        void StopEngine();

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
