using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{
    class DynamicSolvedConfiguration : IDynamicSolvedConfiguration
    {
        readonly IReadOnlyList<IDynamicSolvedPlugin> _plugins;
        readonly IReadOnlyList<IDynamicSolvedService> _services;

        internal DynamicSolvedConfiguration(List<IDynamicSolvedPlugin> plugins, List<IDynamicSolvedService> services)
        {
            Debug.Assert(plugins != null && services != null);
            _plugins = plugins.AsReadOnlyList();
            _services = services.AsReadOnlyList();
        }

        public IReadOnlyList<IDynamicSolvedPlugin> Plugins
        {
            get { return _plugins; }
        }

        public IReadOnlyList<IDynamicSolvedService> Services
        {
            get { return _services; }
        }

        public IDynamicSolvedService FindService( string fullName )
        {
            IDynamicSolvedService service = _services.FirstOrDefault( s => s.ServiceInfo.ServiceFullName == fullName);
            if ( service != null ) return service;
            return null;
        }

        public IDynamicSolvedPlugin FindPlugin( Guid pluginId )
        {
            IDynamicSolvedPlugin plugin = _plugins.FirstOrDefault( p => p.PluginInfo.PluginId == pluginId );
            if ( plugin != null ) return plugin;
            return null;
        }
    }
}
