using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine.Tests.Mocks
{
    class DiscoveredInfo : IDiscoveredInfo
    {
        readonly List<PluginInfo> _plugins;
        readonly List<ServiceInfo> _services;

        public DiscoveredInfo()
        {
            _plugins = new List<PluginInfo>();
            _services = new List<ServiceInfo>();
            DefaultAssembly = new AssemblyInfo( "file://DefaultAssembly" );
        }

        public AssemblyInfo DefaultAssembly;

        public List<ServiceInfo> ServiceInfos
        {
            get { return _services; }
        }

        public ServiceInfo FindService( string serviceFullName )
        {
            return _services.FirstOrDefault( s => s.ServiceFullName == serviceFullName );
        }

        public PluginInfo FindPlugin( string pluginFullName )
        {
            return _plugins.FirstOrDefault( p => p.PluginFullName == pluginFullName );
        }

        public PluginInfo FindPlugin( Guid pluginId )
        {
            return _plugins.FirstOrDefault( p => p.PluginId == pluginId );
        }

        public List<PluginInfo> PluginInfos
        {
            get { return _plugins; }
        }

        IReadOnlyList<IPluginInfo> IDiscoveredInfo.PluginInfos
        {
            get { return _plugins.AsReadOnlyList(); }
        }

        IReadOnlyList<IServiceInfo> IDiscoveredInfo.ServiceInfos
        {
            get { return _services.AsReadOnlyList(); }
        }
    }
}
