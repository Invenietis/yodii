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

        internal DynamicSolvedConfiguration( IReadOnlyList<IDynamicSolvedPlugin> plugins, IReadOnlyList<IDynamicSolvedService> services )
        {
            Debug.Assert(plugins != null && services != null);
            _plugins = plugins;
            _services = services;
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
            return _services.FirstOrDefault( s => s.ServiceInfo.ServiceFullName == fullName);
        }

        public IDynamicSolvedPlugin FindPlugin( string pluginFullName )
        {
            return _plugins.FirstOrDefault( p => p.PluginInfo.PluginFullName == pluginFullName );
        }
    }
}
