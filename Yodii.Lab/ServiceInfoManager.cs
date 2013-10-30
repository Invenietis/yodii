using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Model.CoreModel;

namespace Yodii.Lab
{
    /// <summary>
    /// Manager of IServiceInfo and IPluginInfo for the lab.
    /// Handles item bindings.
    /// </summary>
    class ServiceInfoManager
    {
        readonly CKObservableSortedArrayKeyList<ServiceInfo, string> _serviceInfos;
        readonly CKObservableSortedArrayKeyList<PluginInfo, Guid> _pluginInfos;

        internal ServiceInfoManager()
        {
            _serviceInfos = new CKObservableSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, false );
            _pluginInfos = new CKObservableSortedArrayKeyList<PluginInfo, Guid>( p => p.PluginId, false );
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
        #endregion Properties

        #region Internal methods
        /// <summary>
        /// Creates a new named service, which does not specialize another service.
        /// </summary>
        /// <param name="serviceName">Name of the new service</param>
        /// <returns>New service</returns>
        /// <seealso cref="CreateNewService( string, IServiceInfo )">Create a new service, which specializes another.</seealso>
        internal ServiceInfo CreateNewService( string serviceName )
        {
            Debug.Assert( serviceName != null );

            ServiceInfo newService = new ServiceInfo( serviceName, AssemblyInfoHelper.ExecutingAssemblyInfo );
            _serviceInfos.Add( newService ); // Throws on duplicate

            return newService;
        }

        /// <summary>
        /// Creates a new named service, which specializes another service.
        /// </summary>
        /// <param name="serviceName">Name of the new service</param>
        /// <param name="generalization">Specialized service</param>
        /// <returns>New service</returns>
        internal ServiceInfo CreateNewService( string serviceName, ServiceInfo generalization )
        {
            Debug.Assert( serviceName != null );
            Debug.Assert( generalization != null );
            Debug.Assert( ServiceInfos.Contains( generalization ) );

            ServiceInfo newService = new ServiceInfo( serviceName, AssemblyInfoHelper.ExecutingAssemblyInfo, generalization );
            _serviceInfos.Add( newService ); // Throws on duplicate

            return newService;
        }

        /// <summary>
        /// Creates a new named plugin, which does not implement a service.
        /// </summary>
        /// <param name="pluginGuid">Guid of the new plugin</param>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <returns>New plugin</returns>
        internal PluginInfo CreateNewPlugin( Guid pluginGuid, string pluginName )
        {
            Debug.Assert( pluginGuid != null );
            Debug.Assert( pluginName != null );

            PluginInfo plugin = new PluginInfo( pluginGuid, pluginName, AssemblyInfoHelper.ExecutingAssemblyInfo );
            _pluginInfos.Add( plugin );

            return plugin;
        }

        /// <summary>
        /// Creates a new named plugin, which implements an existing service.
        /// </summary>
        /// <param name="pluginGuid">Guid of the new plugin</param>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <param name="service">Implemented service</param>
        /// <returns>New plugin</returns>
        internal PluginInfo CreateNewPlugin( Guid pluginGuid, string pluginName, ServiceInfo service )
        {
            Debug.Assert( pluginGuid != null );
            Debug.Assert( pluginName != null );
            Debug.Assert( service != null );
            Debug.Assert( ServiceInfos.Contains( service ) );

            PluginInfo plugin = new PluginInfo( pluginGuid, pluginName, AssemblyInfoHelper.ExecutingAssemblyInfo, service );
            _pluginInfos.Add( plugin );
            service.InternalImplementations.Add(plugin);

            return plugin;
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

            MockServiceReferenceInfo reference = new MockServiceReferenceInfo(plugin, service, RunningRequirement.Running);
            plugin.InternalServiceReferences.Add(reference);
        }
        #endregion Internal methods

        internal void RemoveService( ServiceInfo serviceInfo )
        {
            // If we delete a service : Delete its entire tree.

            // Delete generalized services
            foreach( ServiceInfo s in ServiceInfos.Where( si => si.Generalization == serviceInfo ).ToList() )
            {
                RemoveService( s );
            }

            // Delete implementations
            foreach( PluginInfo p in PluginInfos.Where( pi => pi.Service == serviceInfo ).ToList() )
            {
                RemovePlugin( p );
            }

            // Delete all other  existing service references

            foreach( PluginInfo p in PluginInfos )
            {
                foreach (MockServiceReferenceInfo reference in p.InternalServiceReferences.Where(r => r.Reference == serviceInfo).ToList())
                {
                    p.InternalServiceReferences.Remove(reference);
                }
            }

            _serviceInfos.Remove( serviceInfo );
        }

        internal void RemovePlugin( PluginInfo pluginInfo )
        {
            if( pluginInfo.Service != null )
            {
                pluginInfo.InternalService.InternalImplementations.Remove(pluginInfo);
            }

            _pluginInfos.Remove( pluginInfo );
        }
    }
}
