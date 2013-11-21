using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;
using System.ComponentModel;

namespace Yodii.Engine
{
    class LiveInfo : ILiveInfo
    {
        ICKObservableReadOnlyList<ILivePluginInfo> _plugins;
        ICKObservableReadOnlyList<ILiveServiceInfo> _services;

        internal LiveInfo()
        {

        }

        public ICKObservableReadOnlyList<ILivePluginInfo> Plugins
        {
            get { return _plugins; }
        }

        public ICKObservableReadOnlyList<ILiveServiceInfo> Services
        {
            get { return _services; }
        }

        public ILiveServiceInfo FindService( string fullName )
        {
            throw new NotImplementedException();
        }

        public ILivePluginInfo FindPlugin( Guid pluginId )
        {
            throw new NotImplementedException();
        }

        public void RevokeCaller( object caller )
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
