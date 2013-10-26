using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Model;
using Yodii.Model.CoreModel;

namespace Yodii.Lab
{
    public class MainWindowViewModel : ViewModelBase
    {
        YodiiGraph _graph;
        ServiceInfoManager _serviceManager;

        bool _isLive;

        #region Constructor
        public MainWindowViewModel()
        {
            _serviceManager = new ServiceInfoManager();
            _graph = new YodiiGraph( _serviceManager.ServiceInfos, _serviceManager.PluginInfos );
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Returns true if the Lab is live and running (plugins can be started, stopped, and monitored).
        /// Returns false if the Lab is not running (plugins can be changed).
        /// </summary>
        public bool IsLive
        {
            get
            {
                return _isLive;
            }
        }

        /// <summary>
        /// Services created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<IServiceInfo> ServiceInfos
        {
            get { return _serviceManager.ServiceInfos; }
        }

        /// <summary>
        /// Plugins created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<IPluginInfo> PluginInfos
        {
            get { return _serviceManager.PluginInfos; }
        }

        /// <summary>
        /// Active graph.
        /// </summary>
        public YodiiGraph Graph
        {
            get
            {
                return _graph;
            }
        }
        #endregion Properties

        #region Public methods
        /// <summary>
        /// Creates a new named service, which does not specialize another service.
        /// </summary>
        /// <param name="serviceName">Name of the new service</param>
        /// <returns>New service</returns>
        /// <seealso cref="CreateNewService( string, IServiceInfo )">Create a new service, which specializes another.</seealso>
        public IServiceInfo CreateNewService( string serviceName )
        {
            if( serviceName == null ) throw new ArgumentNullException( "serviceName" );

            IServiceInfo newService = _serviceManager.CreateNewService( serviceName );

            return newService;
        }

        /// <summary>
        /// Creates a new named service, which specializes another service.
        /// </summary>
        /// <param name="serviceName">Name of the new service</param>
        /// <param name="generalization">Specialized service</param>
        /// <returns>New service</returns>
        public IServiceInfo CreateNewService( string serviceName, IServiceInfo generalization )
        {
            if( serviceName == null ) throw new ArgumentNullException( "serviceName" );
            if( generalization == null ) throw new ArgumentNullException( "generalization" );

            IServiceInfo newService = _serviceManager.CreateNewService( serviceName, generalization );

            return newService;
        }

        /// <summary>
        /// Creates a new named plugin, which does not implement a service.
        /// </summary>
        /// <param name="pluginGuid">Guid of the new plugin</param>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <returns>New plugin</returns>
        public IPluginInfo CreateNewPlugin( Guid pluginGuid, string pluginName )
        {
            if( pluginGuid == null ) throw new ArgumentNullException( "pluginGuid" );
            if( pluginName == null ) throw new ArgumentNullException( "pluginName" );

            IPluginInfo newPlugin = _serviceManager.CreateNewPlugin( pluginGuid, pluginName );

            return newPlugin;
        }

        /// <summary>
        /// Creates a new named plugin, which implements an existing service.
        /// </summary>
        /// <param name="pluginGuid">Guid of the new plugin</param>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <param name="service">Implemented service</param>
        /// <returns>New plugin</returns>
        public IPluginInfo CreateNewPlugin( Guid pluginGuid, string pluginName, IServiceInfo service )
        {
            if( pluginGuid == null ) throw new ArgumentNullException( "pluginGuid" );
            if( pluginName == null ) throw new ArgumentNullException( "pluginName" );
            if( service == null ) throw new ArgumentNullException( "service" );

            if( !ServiceInfos.Contains<IServiceInfo>( service ) ) throw new InvalidOperationException( "Service does not exist in this Lab" );

            IPluginInfo newPlugin = _serviceManager.CreateNewPlugin( pluginGuid, pluginName, service );

            return newPlugin;
        }

        /// <summary>
        /// Set an existing plugin's dependency to an existing service.
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="service">Service the plugin depends on</param>
        /// <param name="runningRequirement">How the plugin depends on the service</param>
        public void SetPluginDependency( IPluginInfo plugin, IServiceInfo service, RunningRequirement runningRequirement )
        {
            if( plugin == null ) throw new ArgumentNullException( "plugin" );
            if( service == null ) throw new ArgumentNullException( "service" );

            if( !ServiceInfos.Contains<IServiceInfo>( service ) ) throw new InvalidOperationException( "Service does not exist in this Lab" );
            if( !PluginInfos.Contains<IPluginInfo>( plugin ) ) throw new InvalidOperationException( "Plugin does not exist in this Lab" );

            _serviceManager.SetPluginDependency( (PluginInfo)plugin, (ServiceInfo)service, runningRequirement );
        }
        #endregion Public methods

        #region Private methods
        #endregion Private methods

        public void RemovePlugin( IPluginInfo pluginInfo )
        {
            _serviceManager.RemovePlugin( (PluginInfo)pluginInfo );
        }

        public void RemoveService( IServiceInfo serviceInfo )
        {
            _serviceManager.RemoveService( (ServiceInfo)serviceInfo );
        }
    }
}
