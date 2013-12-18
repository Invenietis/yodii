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
    /// Live status of a service, when the engine is started.
    /// </summary>
    public interface ILiveServiceInfo : ILivePluginOrServiceInfo, IDynamicSolvedService, INotifyPropertyChanged
    {
        /// <summary>
        /// Whether this service is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Live generalization, if this service has one.
        /// </summary>
        ILiveServiceInfo Generalization { get; }

        /// <summary>
        /// Running plugin that implements this service when started.
        /// </summary>
        ILivePluginInfo RunningPlugin { get; }

        /// <summary>
        /// Last running plugin.
        /// </summary>
        ILivePluginInfo LastRunningPlugin { get; }

        /// <summary>
        /// Attempts to start the service.
        /// </summary>
        /// <param name="callerKey">Caller identifier.</param>
        /// <returns>True if the service was successfully started, false otherwise.</returns>
        IYodiiEngineResult Start( string callerKey, StartDependencyImpact impact );

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <param name="callerKey">Caller identifier.</param>
         IYodiiEngineResult Stop( string callerKey );

    }
}
