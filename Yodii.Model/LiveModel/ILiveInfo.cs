using CK.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{

    public interface ILiveInfo : INotifyPropertyChanged
    {
        ICKObservableReadOnlyList<ILivePluginInfo> Plugins { get; }
        
        ICKObservableReadOnlyList<ILiveServiceInfo> Services { get; }
        
        ILiveServiceInfo FindService( string fullName );
        
        ILivePluginInfo FindPlugin( Guid pluginId );

        void RevokeCaller( object caller );
    }

}
