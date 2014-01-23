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
        readonly IReadOnlyList<IStaticSolvedYodiiItem> _items;

        public StaticSolvedConfiguration( IReadOnlyList<IStaticSolvedPlugin> plugins, IReadOnlyList<IStaticSolvedService> services )
        {
            Debug.Assert( plugins != null && services != null );
            _plugins = plugins;
            _services = services;
            _items = _services.Cast<IStaticSolvedYodiiItem>().Concat( _plugins ).ToReadOnlyList();
        }

        public IReadOnlyList<IStaticSolvedPlugin> Plugins
        {
            get { return _plugins; }
        }

        public IReadOnlyList<IStaticSolvedService> Services
        {
            get { return _services; }
        }

        public IReadOnlyList<IStaticSolvedYodiiItem> YodiiItems
        {
            get { return _items; }
        }

        public IStaticSolvedYodiiItem FindItem( string fullName )
        {
            return (IStaticSolvedYodiiItem)FindService( fullName ) ?? FindPlugin( fullName );
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
