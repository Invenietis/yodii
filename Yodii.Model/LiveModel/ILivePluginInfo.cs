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
        /// <param name="callerKey">The caller key that identifies the caller. Null is considered to be the same as <see cref="String.Empty"/>.</param>
        /// <param name="impact">Dependency impact.</param>
        /// <returns>Engine result.</returns>
        IYodiiEngineResult Start( string callerKey = null, StartDependencyImpact impact = StartDependencyImpact.Unknown );

        /// <summary>
        /// Stops this plugin.
        /// </summary>
        /// <param name="callerKey">The caller key that identifies the caller. Null is considered to be the same as <see cref="String.Empty"/>.</param>
        /// <returns>Engine result.</returns>
        IYodiiEngineResult Stop( string callerKey = null );

        /// <summary>
        /// Runtime plugin exception.
        /// </summary>
        Exception CurrentError { get; }
    }
}
