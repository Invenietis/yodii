using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Model;
using System.Windows.Input;

namespace Yodii.Lab
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly YodiiGraph _graph;
        readonly ServiceInfoManager _serviceManager;
        readonly ConfigurationManager _configurationManager;
        YodiiGraphVertex _selectedVertex;

        bool _isLive;

        #region Constructor & initializers
        public MainWindowViewModel()
        {
            _configurationManager = new ConfigurationManager();
            _serviceManager = new ServiceInfoManager();
            _graph = new YodiiGraph( _serviceManager.ServiceInfos, _serviceManager.PluginInfos );

            initCommands();
        }
        private void initCommands()
        {
            RemoveSelectedVertex = new RelayCommand(RemoveSelectedVertexExecute, HasSelectedVertex);
        }

        #region Command handlers
        private bool HasSelectedVertex(object obj)
        {
 	        return SelectedVertex != null;
        }

        private void RemoveSelectedVertexExecute(object obj)
        {
            if( SelectedVertex == null ) return;

 	        if( SelectedVertex.IsPlugin )
            {
                this.RemovePlugin(SelectedVertex.PluginInfo);
            } else if( SelectedVertex.IsService )
            {
                this.RemoveService(SelectedVertex.ServiceInfo);
            }
        }
        #endregion #region Command handlers

        #endregion Constructor & initializers

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

        /// <summary>
        /// The Yodii ConfigurationManager linked to this Lab.
        /// </summary>
        public ConfigurationManager ConfigurationManager
        {
            get
            {
                return _configurationManager;
            }
        }

        /// <summary>
        /// Available Layout algorithms.
        /// </summary>
        /// <remarks>
        /// Pulled from Graph#/Algorithms/Layout/StandardLayoutAlgorithmFactory.cs
        /// </remarks>
        public List<String> GraphLayoutAlgorithms
        {
            get
            {
                return new List<String>() { "Circular", "Tree", "FR", "BoundedFR", "KK", "ISOM", "LinLog", "EfficientSugiyama", /*"Sugiyama",*/ "CompoundFDP" };
            }
        }

        public YodiiGraphVertex SelectedVertex
        {
            get
            {
                return _selectedVertex;
            }
            set
            {
                if( value != _selectedVertex)
                {
                    _selectedVertex = value;
                    RaisePropertyChanged("SelectedVertex");
                }
            }
        }

        #region Command properties
        public ICommand RemoveSelectedVertex { get; private set; }
        #endregion
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

            ServiceInfo newService = _serviceManager.CreateNewService( serviceName );

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

            ServiceInfo newService = _serviceManager.CreateNewService( serviceName, (ServiceInfo)generalization );

            return newService;
        }

        /// <summary>
        /// Creates a new named plugin, which does not implement a service.
        /// </summary>
        /// <param name="pluginGuid">Guid of the new plugin</param>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <returns>New plugin</returns>
        public IPluginInfo CreateNewPlugin(Guid pluginGuid, string pluginName)
        {
            if( pluginGuid == null ) throw new ArgumentNullException( "pluginGuid" );
            if( pluginName == null ) throw new ArgumentNullException( "pluginName" );

            PluginInfo newPlugin = _serviceManager.CreateNewPlugin( pluginGuid, pluginName );

            return newPlugin;
        }

        /// <summary>
        /// Creates a new named plugin, which implements an existing service.
        /// </summary>
        /// <param name="pluginGuid">Guid of the new plugin</param>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <param name="service">Implemented service</param>
        /// <returns>New plugin</returns>
        public IPluginInfo CreateNewPlugin(Guid pluginGuid, string pluginName, IServiceInfo service)
        {
            if( pluginGuid == null ) throw new ArgumentNullException( "pluginGuid" );
            if( pluginName == null ) throw new ArgumentNullException( "pluginName" );
            if( service == null ) throw new ArgumentNullException( "service" );

            if( !ServiceInfos.Contains<IServiceInfo>( service ) ) throw new InvalidOperationException( "Service does not exist in this Lab" );

            PluginInfo newPlugin = _serviceManager.CreateNewPlugin( pluginGuid, pluginName, (ServiceInfo)service );

            return newPlugin;
        }

        /// <summary>
        /// Set an existing plugin's dependency to an existing service.
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="service">Service the plugin depends on</param>
        /// <param name="runningRequirement">How the plugin depends on the service</param>
        public void SetPluginDependency(IPluginInfo plugin, IServiceInfo service, RunningRequirement runningRequirement)
        {
            if( plugin == null ) throw new ArgumentNullException( "plugin" );
            if( service == null ) throw new ArgumentNullException( "service" );

            if( !ServiceInfos.Contains( (ServiceInfo)service ) ) throw new InvalidOperationException( "Service does not exist in this Lab" );
            if( !PluginInfos.Contains( (PluginInfo)plugin ) ) throw new InvalidOperationException( "Plugin does not exist in this Lab" );

            _serviceManager.SetPluginDependency( (PluginInfo)plugin, (ServiceInfo)service, runningRequirement );
        }
        #endregion Public methods

        #region Private methods
        #endregion Private methods

        public void RemovePlugin( IPluginInfo pluginInfo )
        {
            _serviceManager.RemovePlugin( (PluginInfo)pluginInfo );
        }

        public void RemoveService( IServiceInfo serviceInfo)
        {
            _serviceManager.RemoveService( (ServiceInfo)serviceInfo );
        }

        internal void SelectVertex(YodiiGraphVertex vertex)
        {
            SelectedVertex = vertex;
        }
    }
}
