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
    /// Yodii engine base interface for <see cref="IYodiiEngine"/> and <see cref="IInnerYodiiEngine"/>.
    /// </summary>
    public interface IYodiiEngineBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current information about available plugins and services.
        /// </summary>
        IDiscoveredInfo DiscoveredInfo { get; }

        /// <summary>
        /// Change the current set of <see cref="IPluginInfo"/> and <see cref="IServiceInfo"/>.
        /// If <see cref="IYodiiEngine.IsRunning"/> is true, this can be rejected: the result will indicate the reason of the failure.
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
        /// Attempts to start a service or a plugin. 
        /// </summary>
        /// <param name="pluginOrService">The plugin or service live object to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// The <see cref="ILiveYodiiItem.Capability">pluginOrService.Capability</see> property <see cref="ILiveRunCapability.CanStart"/> 
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </para>
        /// or
        /// <para>
        /// This engine is not running.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter <paramref name="pluginOrService"/> is null.
        /// </exception>
        /// <returns>Result detailing whether the service or plugin was successfully started or not.</returns>
        IYodiiEngineResult Start( ILiveYodiiItem pluginOrService, StartDependencyImpact impact = StartDependencyImpact.Unknown );

        /// <summary>
        /// Attempts to stop this service or plugin.
        /// </summary>
        /// <param name="pluginOrService">The plugin or service live object to stop.</param>
        /// <returns>Result detailing whether the service or plugin was successfully stopped or not.</returns>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// The <see cref="ILiveYodiiItem.Capability">pluginOrService.Capability</see> property <see cref="ILiveRunCapability.CanStop"/> must be true.
        /// </para>
        /// or
        /// <para>
        /// This engine is not running.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter <paramref name="pluginOrService"/> is null.
        /// </exception>
        IYodiiEngineResult Stop( ILiveYodiiItem pluginOrService );


    }
}
