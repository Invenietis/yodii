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

        public StaticSolvedConfiguration( IReadOnlyList<IStaticSolvedPlugin> plugins, IReadOnlyList<IStaticSolvedService> services )
        {
            Debug.Assert( plugins != null && services != null );
            _plugins = plugins;
            _services = services;
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
            return _services.FirstOrDefault( s => s.ServiceInfo.ServiceFullName == fullName );
        }

        public IStaticSolvedPlugin FindPlugin( string pluginFullName )
        {
            return _plugins.FirstOrDefault( p => p.PluginInfo.PluginFullName == pluginFullName );
       }
    }
}
