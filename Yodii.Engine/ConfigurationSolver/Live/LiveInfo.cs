using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;
using System.ComponentModel;
using System.Diagnostics;

namespace Yodii.Engine
{
    class LiveInfo : ILiveInfo
    {
        readonly YodiiEngine _engine;
        readonly CKObservableSortedArrayKeyList<LivePluginInfo,string> _plugins;
        readonly CKObservableSortedArrayKeyList<LiveServiceInfo,string> _services;

        internal LiveInfo(YodiiEngine engine)
        {
            Debug.Assert( engine != null );
            _engine = engine;
            _plugins = new CKObservableSortedArrayKeyList<LivePluginInfo, string>( l => l.PluginInfo.PluginFullName );
            _services = new CKObservableSortedArrayKeyList<LiveServiceInfo, string>( l => l.ServiceInfo.ServiceFullName );
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
            if( fullName == null ) throw new ArgumentNullException( "fullName" );
            return _services.GetByKey( fullName );
        }

        public ILivePluginInfo FindPlugin( string pluginFullName )
        {
            return _plugins.GetByKey( pluginFullName );
        }

        public bool Contains( string serviceFullName )
        {
            return _services.Contains( serviceFullName );
        }

        internal void UpdateFrom( IConfigurationSolver solver )
        {
            _services.RemoveWhereAndReturnsRemoved( s => solver.FindService( s.ServiceInfo.ServiceFullName ) == null ).Count();
            _plugins.RemoveWhereAndReturnsRemoved( p => solver.FindPlugin( p.PluginInfo.PluginFullName ) == null ).Count();

            DelayedPropertyNotification notifier = new DelayedPropertyNotification();

            List<LiveServiceInfo> servicesToAdd = new List<LiveServiceInfo>();
            foreach( var s in solver.AllServices )
            {
                LiveServiceInfo ls = _services.GetByKey( s.ServiceInfo.ServiceFullName );
                if( ls == null ) servicesToAdd.Add( new LiveServiceInfo( s, _engine ) );
                else ls.UpdateFrom( s, notifier );
            }

            List<LivePluginInfo> pluginsToAdd = new List<LivePluginInfo>();
            foreach( var s in solver.AllPlugins )
            {
                LivePluginInfo lp = _plugins.GetByKey( s.PluginInfo.PluginFullName );
                if( lp == null ) pluginsToAdd.Add( new LivePluginInfo( s, _engine ) );
                else lp.UpdateFrom( s, notifier );
            }

            Func<string,LiveServiceInfo> serviceFinder = name => _services.GetByKey( name ) ?? servicesToAdd.First( ls => ls.ServiceInfo.ServiceFullName == name );
            Func<string,LivePluginInfo> pluginFinder = id => _plugins.GetByKey( id ) ?? pluginsToAdd.First( lp => lp.PluginInfo.PluginFullName == id );

            using( notifier.SilentMode() )
            {
                foreach( var ls in servicesToAdd ) ls.Bind( solver.FindExistingService( ls.ServiceInfo.ServiceFullName ), serviceFinder, pluginFinder, notifier );
            }
            foreach( var ls in _services ) ls.Bind( solver.FindExistingService( ls.ServiceInfo.ServiceFullName ), serviceFinder, pluginFinder, notifier );

            using( notifier.SilentMode() )
            {
                foreach( var lp in pluginsToAdd ) lp.Bind( solver.FindExistingPlugin( lp.PluginInfo.PluginFullName ), serviceFinder, notifier );
            }
            foreach( var lp in _plugins ) lp.Bind( solver.FindExistingPlugin( lp.PluginInfo.PluginFullName ), serviceFinder, notifier );

            foreach( var ls in servicesToAdd ) _services.Add( ls );
            foreach( var lp in pluginsToAdd ) _plugins.Add( lp );

            notifier.RaiseEvents();
        }

        internal void UpdateRuntimeErrors( IEnumerable<Tuple<IPluginInfo, Exception>> errors )
        {
            foreach( var e in errors )
            {
                LivePluginInfo pluginInfo = _plugins.GetByKey( e.Item1.PluginFullName );
                Debug.Assert( pluginInfo != null, "The plugin cannot be not found in UpdateRuntimeErrors function" );
                pluginInfo.CurrentError = e.Item2;
            }
        }

        /// <summary>
        /// Called by YodiiEngine.Stop().
        /// </summary>
        internal void Clear()
        {
            _plugins.Clear();
            _services.Clear();
        }

        public IYodiiEngineResult RevokeCaller( string callerKey )
        {
            return _engine.RevokeYodiiCommandCaller( callerKey );
        }
    }
}
