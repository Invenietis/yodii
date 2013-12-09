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
        readonly CKObservableSortedArrayKeyList<LivePluginInfo,Guid> _plugins;
        readonly CKObservableSortedArrayKeyList<LiveServiceInfo,string> _services;

        internal LiveInfo(YodiiEngine engine)
        {
            Debug.Assert( engine != null );
            _engine = engine;
            _plugins = new CKObservableSortedArrayKeyList<LivePluginInfo, Guid>( l => l.PluginInfo.PluginId );
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

        public ILivePluginInfo FindPlugin( Guid pluginId )
        {
            if( pluginId == null ) throw new ArgumentNullException( "pluginId" );
            return _plugins.GetByKey( pluginId );
        }

        internal void UpdateInfo( ServiceData serviceData )
        {
            Debug.Assert( serviceData != null );
            LiveServiceInfo serviceInfo = _services.GetByKey( serviceData.ServiceInfo.ServiceFullName );
            if( serviceInfo != null )
            {
                serviceInfo.UpdateInfo( serviceData );
            }
            else
            {
                Debug.Fail( "serviceData cannot be not found in UpdateInfo function" );
            }
        }

        internal void UpdateInfo( PluginData pluginData )
        {
            Debug.Assert( pluginData != null );
            LivePluginInfo pluginInfo = _plugins.GetByKey( pluginData.PluginInfo.PluginId );
            if( pluginInfo != null )
            {
                pluginInfo.UpdateInfo( pluginData );
            }
            else
            {
                Debug.Fail( "pluginData cannot be not found in UpdateInfo function" );
            }
        }

        internal void AddInfo( ServiceData serviceData )
        {
            Debug.Assert( serviceData != null );
            Debug.Assert( !_services.Contains( serviceData.ServiceInfo.ServiceFullName ) );
            _services.Add( new LiveServiceInfo( serviceData, _engine ) );
        }

        internal void AddInfo( PluginData pluginData )
        {
            Debug.Assert( pluginData != null );
            Debug.Assert( !_plugins.Contains( pluginData.PluginInfo.PluginId ) );
            _plugins.Add( new LivePluginInfo( pluginData, _engine ) );
        }

        internal void UpdateRuntimeErrors( IEnumerable<Tuple<IPluginInfo, Exception>> errors )
        {
            foreach( var e in errors )
            {
                LivePluginInfo pluginInfo = _plugins.GetByKey( e.Item1.PluginId );
                if( pluginInfo != null )
                {
                    pluginInfo.CurrentError = e.Item2;
                }
                else
                {
                    Debug.Fail( "The plugin cannot be not found in UpdateRuntimeErrors function" );
                }
            }
        }

        internal void CreateGraphOfDependencies()
        {
            foreach( var p in _plugins )
            {
                if( p.PluginInfo.Service != null ) p.Service = _services.GetByKey( p.PluginInfo.Service.ServiceFullName );
            }
            foreach( var s in _services )
            {
                if( s.ServiceInfo.Generalization != null ) s.Generalization = _services.GetByKey( s.ServiceInfo.Generalization.ServiceFullName );
            }
        }

        public void RevokeCaller( object caller )
        {
            throw new NotImplementedException();
        }
    }
}
