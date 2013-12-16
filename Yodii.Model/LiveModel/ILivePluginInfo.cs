using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Live status of a plugin, when the engine is started.
    /// </summary>
    public interface ILivePluginInfo : IDynamicSolvedPlugin, INotifyPropertyChanged
    {
        /// <summary>
        /// Whether the plugin is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// The live service this plugin implements.
        /// </summary>
        /// <remarks>Can be null.</remarks>
        ILiveServiceInfo Service { get; }

        /// <summary>
        /// Attempts to start this plugin.
        /// </summary>
        /// <param name="callerKey">Caller identifier of this method.</param>
        /// <param name="impact">Dependency impact.</param>
        /// <returns>True of the plugin was started, false otherwise.</returns>
        IYodiiEngineResult Start( string callerKey, StartDependencyImpact impact = StartDependencyImpact.Unknown );

        /// <summary>
        /// Stops this plugin.
        /// </summary>
        /// <param name="callerKey">Caller identifier of this method.</param>
        IYodiiEngineResult Stop( string callerKey );

        /// <summary>
        /// Runtime plugin exception.
        /// </summary>
        Exception CurrentError { get; }
    }
}
