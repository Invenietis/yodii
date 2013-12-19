using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Live plugin or service info.
    /// </summary>
    public interface ILiveYodiiItem : IDynamicYodiiItem, INotifyPropertyChanged
    {
        /// <summary>
        /// Whether this live Yodii item, plugin or status, is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Attempts to start the service or plugin.
        /// </summary>
        /// <param name="callerKey">Caller identifier.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <returns>Result detailing whether the service or plugin was successfully started or not.</returns>
        IYodiiEngineResult Start( string callerKey, StartDependencyImpact impact );

        /// <summary>
        /// Result detailing whether the service or plugin was successfully stopped or not.
        /// </summary>
        /// <param name="callerKey">Caller identifier.</param>
        IYodiiEngineResult Stop( string callerKey );

    }
}
