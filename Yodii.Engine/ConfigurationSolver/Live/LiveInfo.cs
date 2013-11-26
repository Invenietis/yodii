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
        List<YodiiCommand> _yodiiCommands;

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
            ILiveServiceInfo service = _services.FirstOrDefault(s => s.ServiceInfo.ServiceFullName == fullName);
            return service;
        }

        public ILivePluginInfo FindPlugin( Guid pluginId )
        {
            ILivePluginInfo plugin = _plugins.FirstOrDefault(p => p.PluginInfo.PluginId == pluginId);
            return plugin;
        }

        public void RevokeCaller( object caller )
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
