using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{
    class StaticSolvedConfiguration : IStaticSolvedConfiguration
    {
        readonly IReadOnlyList<IStaticSolvedPlugin> _plugins;
        readonly IReadOnlyList<IStaticSolvedService> _services;

        public StaticSolvedConfiguration(List<IStaticSolvedPlugin> plugins, List<IStaticSolvedService> services)
        {
            Debug.Assert( plugins != null && services != null );
            _plugins = plugins.AsReadOnlyList();
            _services = services.AsReadOnlyList();
        }

        public IReadOnlyList<IStaticSolvedPlugin> Plugins
        {
            get { return _plugins; }
        }

        public IReadOnlyList<IStaticSolvedService> Services
        {
            get { return _services; }
        }

        public IStaticSolvedService FindService( string fullName )
        {
            IStaticSolvedService service = _services.FirstOrDefault( s => s.ServiceInfo.ServiceFullName == fullName );
            if ( service != null ) return service;
            return null;
        }

        public IStaticSolvedPlugin FindPlugin( Guid pluginId )
        {
            IStaticSolvedPlugin plugin = _plugins.FirstOrDefault( p => p.PluginInfo.PluginId == pluginId );
            if ( plugin != null ) return plugin;
            return null;
        }
    }
}
