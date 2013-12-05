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
    public interface ILiveInfo : INotifyPropertyChanged
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
        /// <param name="pluginId">Plugin GUID</param>
        /// <returns>Live plugin</returns>
        ILivePluginInfo FindPlugin( Guid pluginId );

        /// <summary>
        /// Removes all Yodii commands created by the given caller.
        /// </summary>
        /// <param name="caller">Calling object.</param>
        /// <remarks>
        /// <seealso cref="ILivePluginInfo.Start(object, StartDependencyImpact)"/>
        /// <seealso cref="ILiveServiceInfo.Start(object)"/>
        /// </remarks>
        void RevokeCaller( object caller );
    }

}
