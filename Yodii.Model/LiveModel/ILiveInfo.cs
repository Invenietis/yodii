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
    /// This is the observable façade to the whole <see cref="IYodiiEngine"/>.
    /// </summary>
    /// <remarks>
    /// Gives information about which services and plugins were successfully resolved and started and supports 
    /// start and stop capabilities.
    /// </remarks>
    public interface ILiveInfo
    {
        /// <summary>
        /// Gets an observable list of currently available plugins.
        /// </summary>
        IObservableReadOnlyList<ILivePluginInfo> Plugins { get; }
        
        /// <summary>
        /// Gets an observable list of currently available services.
        /// </summary>
        IObservableReadOnlyList<ILiveServiceInfo> Services { get; }
        
        /// <summary>
        /// Currently active <see cref="YodiiCommand"/>.
        /// </summary>
        /// <remarks>
        /// Commands are are used to dynamically reconfigure the system.
        /// They are memorized from the newest one to the latest one and are automatically optimized: only 
        /// commands that are actually used to preserve the integrity of the system state are kept.
        /// </remarks>
        IObservableReadOnlyList<YodiiCommand> YodiiCommands { get; }
        
        /// <summary>
        /// Find a live service by its full name.
        /// </summary>
        /// <param name="fullName">Full name of the live service</param>
        /// <returns>Live service</returns>
        ILiveServiceInfo FindService( string fullName );
        
        /// <summary>
        /// Find a plugin by its full name.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name</param>
        /// <returns>Live plugin</returns>
        ILivePluginInfo FindPlugin( string pluginFullName );

        /// <summary>
        /// Cancels any start or stop made by the given caller.
        /// </summary>
        /// <param name="callerKey">The caller key that identifies the caller. Null is considered to be the same as <see cref="String.Empty"/>.</param>
        /// <returns>Since canceling commands may trigger a runtime error, this method must return a result.</returns>
        IYodiiEngineResult RevokeCaller( string callerKey = null );

    }

}
