using CK.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Live information of a running engine.
    /// </summary>
    /// <remarks>
    /// Gives information about which services and plugins were successfully resolved and started.
    /// </remarks>
    public interface ILiveInfo
    {
        /// <summary>
        /// List of plugins which were successfully resolved and started.
        /// </summary>
        ICKObservableReadOnlyList<ILivePluginInfo> Plugins { get; }
        
        /// <summary>
        /// List of services which were successfully resolved and started.
        /// </summary>
        ICKObservableReadOnlyList<ILiveServiceInfo> Services { get; }
        
        /// <summary>
        /// Find a live service by its full name.
        /// </summary>
        /// <param name="fullName">Full name of the live service</param>
        /// <returns>Live service</returns>
        ILiveServiceInfo FindService( string fullName );
        
        /// <summary>
        /// Find a plugin by its GUID.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name</param>
        /// <returns>Live plugin</returns>
        ILivePluginInfo FindPlugin( string pluginFullName );

        /// <summary>
        /// Cancels any start or stop made by this caller.
        /// </summary>
        /// <param name="callerKey">The caller key that identifies the caller. Null is considered to be the same as <see cref="String.Empty"/>.</param>
        /// <returns>Since canceling commands may trigger a runtime error, this method must return a result.</returns>
        IYodiiEngineResult RevokeCaller( string callerKey = null );

    }

}
