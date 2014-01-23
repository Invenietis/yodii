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
        readonly IReadOnlyList<IDynamicSolvedYodiiItem> _items;

        internal DynamicSolvedConfiguration( IReadOnlyList<IDynamicSolvedPlugin> plugins, IReadOnlyList<IDynamicSolvedService> services )
        {
            Debug.Assert(plugins != null && services != null);
            _plugins = plugins;
            _services = services;
            _items = _services.Cast<IDynamicSolvedYodiiItem>().Concat( _plugins ).ToReadOnlyList();
        }

        public IReadOnlyList<IDynamicSolvedPlugin> Plugins
        {
            get { return _plugins; }
        }

        public IReadOnlyList<IDynamicSolvedService> Services
        {
            get { return _services; }
        }

        public IReadOnlyList<IDynamicSolvedYodiiItem> YodiiItems
        {
            get { return _items; }
        }

        public IDynamicSolvedYodiiItem FindItem( string fullName )
        {
            return (IDynamicSolvedYodiiItem)FindService( fullName ) ?? FindPlugin( fullName );
        }

        public IDynamicSolvedService FindService( string serviceFullName )
        {
            return _services.FirstOrDefault( s => s.ServiceInfo.ServiceFullName == serviceFullName);
        }

        public IDynamicSolvedPlugin FindPlugin( string pluginFullName )
        {
            return _plugins.FirstOrDefault( p => p.PluginInfo.PluginFullName == pluginFullName );
        }
    }
}
