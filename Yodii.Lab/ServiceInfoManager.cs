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
        CKObservableSortedArrayKeyList<ServiceInfo, string> _serviceInfos;
        CKObservableSortedArrayKeyList<PluginInfo, Guid> _pluginInfos;

        internal ServiceInfoManager()
        {
            _serviceInfos = new CKObservableSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, false );
            _pluginInfos = new CKObservableSortedArrayKeyList<PluginInfo, Guid>( p => p.PluginId, false );
        }

        #region Properties
        /// <summary>
        /// Services created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<IServiceInfo> ServiceInfos
        {
            get { return _serviceInfos; }
        }

        /// <summary>
        /// Plugins created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<IPluginInfo> PluginInfos
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
        internal IServiceInfo CreateNewService( string serviceName )
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
        internal IServiceInfo CreateNewService( string serviceName, IServiceInfo generalization )
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
        internal IPluginInfo CreateNewPlugin( Guid pluginGuid, string pluginName )
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
        internal IPluginInfo CreateNewPlugin( Guid pluginGuid, string pluginName, IServiceInfo service )
        {
            Debug.Assert( pluginGuid != null );
            Debug.Assert( pluginName != null );
            Debug.Assert( service != null );
            Debug.Assert( ServiceInfos.Contains( service ) );

            Debug.Assert( service is ServiceInfo );
            ServiceInfo castService = service as ServiceInfo;

            PluginInfo plugin = new PluginInfo( pluginGuid, pluginName, AssemblyInfoHelper.ExecutingAssemblyInfo, service );
            _pluginInfos.Add( plugin );
            castService.BindPlugin( plugin );

            return plugin;
        }

        /// <summary>
        /// Set an existing plugin's dependency to an existing service.
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="service">Service the plugin depends on</param>
        /// <param name="runningRequirement">How the plugin depends on the service</param>
        internal void SetPluginDependency( IPluginInfo plugin, IServiceInfo service, RunningRequirement runningRequirement )
        {
            Debug.Assert( plugin != null );
            Debug.Assert( service != null );
            Debug.Assert( ServiceInfos.Contains( service ) );
            Debug.Assert( PluginInfos.Contains( plugin ) );

            Debug.Assert( plugin is PluginInfo );
            PluginInfo castPlugin = plugin as PluginInfo;

            IServiceReferenceInfo reference = new MockServiceReferenceInfo( plugin, service, RunningRequirement.Running );
            castPlugin.BindServiceRequirement( reference );
        }
        #endregion Internal methods
    }
}
