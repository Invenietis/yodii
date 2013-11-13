using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Manager of IServiceInfo and IPluginInfo for the lab.
    /// Handles item bindings.
    /// </summary>
    public class ServiceInfoManager
    {
        readonly CKObservableSortedArrayKeyList<ServiceInfo, string> _serviceInfos;
        readonly CKObservableSortedArrayKeyList<PluginInfo, Guid> _pluginInfos;

        readonly CKObservableSortedArrayKeyList<LiveServiceInfo, ServiceInfo> _liveServiceInfos;
        readonly CKObservableSortedArrayKeyList<LivePluginInfo, PluginInfo> _livePluginInfos;

        internal ServiceInfoManager()
        {
            _serviceInfos = new CKObservableSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, false );
            _pluginInfos = new CKObservableSortedArrayKeyList<PluginInfo, Guid>( p => p.PluginId, false );

            _liveServiceInfos = new CKObservableSortedArrayKeyList<LiveServiceInfo, ServiceInfo>( s => s.ServiceInfo, ( x, y ) => String.CompareOrdinal( x.ServiceFullName, y.ServiceFullName ), false );
            _livePluginInfos = new CKObservableSortedArrayKeyList<LivePluginInfo, PluginInfo>( p => p.PluginInfo, ( x, y ) => String.CompareOrdinal( x.PluginId.ToString(), y.PluginId.ToString() ), false );
        }

        #region Properties
        /// <summary>
        /// Services created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<ServiceInfo> ServiceInfos
        {
            get { return _serviceInfos; }
        }

        /// <summary>
        /// Plugins created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<PluginInfo> PluginInfos
        {
            get { return _pluginInfos; }
        }
        /// <summary>
        /// Services created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<LiveServiceInfo> LiveServiceInfos
        {
            get { return _liveServiceInfos; }
        }

        /// <summary>
        /// Plugins created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<LivePluginInfo> LivePluginInfos
        {
            get { return _livePluginInfos; }
        }
        #endregion Properties

        #region Internal methods
        internal void ClearState()
        {
            _livePluginInfos.Clear();
            _liveServiceInfos.Clear();
            _pluginInfos.Clear();
            _serviceInfos.Clear();
        }

        /// <summary>
        /// Creates a new named service, which specializes another service.
        /// </summary>
        /// <param name="serviceName">Name of the new service</param>
        /// <param name="generalization">Specialized service</param>
        /// <returns>New service</returns>
        internal ServiceInfo CreateNewService( string serviceName, ServiceInfo generalization = null )
        {
            Debug.Assert( serviceName != null );
            Debug.Assert( _serviceInfos.Any( x => x.ServiceFullName == serviceName ) == false, "Service does not exist and can be added" );

            if( generalization != null ) Debug.Assert( ServiceInfos.Contains( generalization ) );

            ServiceInfo newService = new ServiceInfo( serviceName, AssemblyInfoHelper.ExecutingAssemblyInfo, generalization );

            _serviceInfos.Add( newService ); // Throws on duplicate

            CreateLiveService( newService );

            return newService;
        }

        /// <summary>
        /// Creates a new named plugin, which implements an existing service.
        /// </summary>
        /// <param name="pluginGuid">Guid of the new plugin</param>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <param name="service">Implemented service</param>
        /// <returns>New plugin</returns>
        internal PluginInfo CreateNewPlugin( Guid pluginGuid, string pluginName = null, ServiceInfo service = null )
        {
            Debug.Assert( pluginGuid != null );
            if( service != null ) Debug.Assert( ServiceInfos.Contains( service ) );

            PluginInfo plugin = new PluginInfo( pluginGuid, pluginName, AssemblyInfoHelper.ExecutingAssemblyInfo, service );

            _pluginInfos.Add( plugin );

            CreateLivePlugin( plugin );

            return plugin;
        }

        private void CreateLiveService( ServiceInfo s )
        {
            LiveServiceInfo newServiceInfo;
            if( s.Generalization != null )
            {
                LiveServiceInfo generalizationLiveInfo = _liveServiceInfos.GetByKey( (ServiceInfo)s.Generalization );
                newServiceInfo = new LiveServiceInfo( s, RunningRequirement.Optional, generalizationLiveInfo ); // TODO: Running requirement
            }
            else
            {
                newServiceInfo = new LiveServiceInfo( s );
            }
            _liveServiceInfos.Add( newServiceInfo );
        }

        private void CreateLivePlugin( PluginInfo p )
        {
            LivePluginInfo lp;

            if( p.Service != null )
            {
                p.Service.InternalImplementations.Add( p );
                LiveServiceInfo liveService = _liveServiceInfos.GetByKey( p.Service );
                lp = new LivePluginInfo( p, RunningRequirement.Optional, liveService ); // TODO: Running requirement
            }
            else
            {
                lp = new LivePluginInfo( p );
            }

            _livePluginInfos.Add( lp );
        }


        /// <summary>
        /// Set an existing plugin's dependency to an existing service.
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="service">Service the plugin depends on</param>
        /// <param name="runningRequirement">How the plugin depends on the service</param>
        internal void SetPluginDependency( PluginInfo plugin, ServiceInfo service, RunningRequirement runningRequirement )
        {
            Debug.Assert( plugin != null );
            Debug.Assert( service != null );
            Debug.Assert( ServiceInfos.Contains( service ) );
            Debug.Assert( PluginInfos.Contains( plugin ) );

            MockServiceReferenceInfo reference = new MockServiceReferenceInfo( plugin, service, RunningRequirement.Running );
            plugin.InternalServiceReferences.Add( reference );

        }
        #endregion Internal methods

        internal void RemoveService( ServiceInfo serviceInfo )
        {
            // If we delete a service : Unbind linked plugins and services.

            // Unbind generalized services
            foreach( ServiceInfo s in ServiceInfos.Where( si => si.Generalization == serviceInfo ).ToList() )
            {
                s.Generalization = null;
            }

            // Unbind implementations
            foreach( PluginInfo p in PluginInfos.Where( pi => pi.Service == serviceInfo ).ToList() )
            {
                p.Service = null;
            }

            // Delete all existing service references

            foreach( PluginInfo p in PluginInfos )
            {
                foreach( MockServiceReferenceInfo reference in p.InternalServiceReferences.Where( r => r.Reference == serviceInfo ).ToList() )
                {
                    p.InternalServiceReferences.Remove( reference );
                }
            }

            _liveServiceInfos.Remove( serviceInfo );
            _serviceInfos.Remove( serviceInfo );
        }

        internal void RemovePlugin( PluginInfo pluginInfo )
        {
            if( pluginInfo.Service != null )
            {
                pluginInfo.InternalService.InternalImplementations.Remove( pluginInfo );
            }

            LivePluginInfo livePlugin = _livePluginInfos.GetByKey( pluginInfo );
            _livePluginInfos.Remove( livePlugin );
            _pluginInfos.Remove( pluginInfo );
        }

        internal Utils.DetailedOperationResult RenameService( ServiceInfo serviceInfo, string newName )
        {
            bool exists;
            ServiceInfo existingInfo = _serviceInfos.GetByKey( newName, out exists );

            if( exists ) return new Utils.DetailedOperationResult( false, "A service with this name already exists." );

            serviceInfo.ServiceFullName = newName;

            return new Utils.DetailedOperationResult( true );
        }

        internal void LoadFromXmlReader( XmlReader r )
        {
            // May throw
            var state = MockInfoXmlSerializer.DeserializeLabStateFromXmlReader( r );

            ClearState();

            foreach( var serviceInfo in state.ServiceInfos )
            {
                LoadServiceInfo( serviceInfo );
            }

            foreach( var pluginInfo in state.PluginInfos )
            {
                LoadPluginInfo( pluginInfo );
            }
        }

        private void LoadServiceInfo( ServiceInfo serviceInfo )
        {
            if( _serviceInfos.Contains( serviceInfo ) ) return; // Already loaded
            _serviceInfos.Add( serviceInfo );

            if( serviceInfo.Generalization != null )
            {
                LoadServiceInfo( (ServiceInfo)serviceInfo.Generalization );
            }

            CreateLiveService( serviceInfo );
        }

        private void LoadPluginInfo( PluginInfo pluginInfo )
        {
            if( _pluginInfos.Contains( pluginInfo ) ) return; // Already loaded
            _pluginInfos.Add( pluginInfo );

            if( pluginInfo.Service != null )
            {
                LoadServiceInfo( pluginInfo.Service );
            }

            foreach( var serviceRef in pluginInfo.ServiceReferences )
            {
                Debug.Assert( serviceRef.Owner == pluginInfo );
                LoadServiceInfo( (ServiceInfo)serviceRef.Reference );
            }

            CreateLivePlugin( pluginInfo );
        }
    }
}
