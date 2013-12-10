using CK.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface ILiveInfo
    {
        ICKObservableReadOnlyList<ILivePluginInfo> Plugins { get; }
        
        ICKObservableReadOnlyList<ILiveServiceInfo> Services { get; }
        
        ILiveServiceInfo FindService( string fullName );
        
        ILivePluginInfo FindPlugin( Guid pluginId );

        /// <summary>
        /// Cancels any start or stop made by this caller.
        /// </summary>
        /// <param name="caller">The caller key. Must not be null.</param>
        /// <returns>Since canceling commands may trigger a runtime error, this method must return a result.</returns>
        IYodiiEngineResult RevokeCaller( string callerKey );
    }

}
