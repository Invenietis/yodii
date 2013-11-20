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
using System.Collections.Specialized;
using Yodii.Lab.Utils;
using System.Xml;
using System.IO;
using Yodii.Engine;

namespace Yodii.Lab
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        readonly YodiiGraph _graph;
        readonly ServiceInfoManager _serviceInfoManager;

        readonly ICommand _removeSelectedVertexCommand;
        readonly ICommand _runStaticSolverCommand;

        ConfigurationManager _configurationManager; // Can be swapped through XML loading.
        YodiiGraphVertex _selectedVertex;
        bool _isLive;

        #endregion

        #region Constructor & initializers

        public MainWindowViewModel()
        {
            _configurationManager = new ConfigurationManager();
            _serviceInfoManager = new ServiceInfoManager();

            // Live objects and static infos are managed in the ServiceInfoManager.

            _graph = new YodiiGraph( _configurationManager, _serviceInfoManager );

            _removeSelectedVertexCommand = new RelayCommand( RemoveSelectedVertexExecute, HasSelectedVertex );
            _runStaticSolverCommand = new RelayCommand( RunStaticSolverExecute );
        }

        #endregion Constructor & initializers

        #region Command handlers

        private void RunStaticSolverExecute( object obj )
        {
            // TODO: Complete static solver.
            ConfigurationSolver solver = new ConfigurationSolver();
            var result = solver.Initialize( _configurationManager.FinalConfiguration, _serviceInfoManager );
        }

        private bool HasSelectedVertex( object obj )
        {
            return SelectedVertex != null;
        }

        private void RemoveSelectedVertexExecute( object obj )
        {
            if( SelectedVertex == null ) return;

            if( SelectedVertex.IsPlugin )
            {
                this.RemovePlugin( SelectedVertex.LivePluginInfo.PluginInfo );
            }
            else if( SelectedVertex.IsService )
            {
                this.RemoveService( SelectedVertex.LiveServiceInfo.ServiceInfo );
            }

            SelectedVertex = null;
        }

        #endregion Command handlers

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
            get { return _serviceInfoManager.ServiceInfos; }
        }

        /// <summary>
        /// Plugins created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<IPluginInfo> PluginInfos
        {
            get { return _serviceInfoManager.PluginInfos; }
        }

        /// <summary>
        /// Services created in this Lab (live).
        /// </summary>
        public ICKObservableReadOnlyCollection<ILiveServiceInfo> LiveServiceInfos
        {
            get { return _serviceInfoManager.LiveServiceInfos; }
        }

        /// <summary>
        /// Plugins created in this Lab (live).
        /// </summary>
        public ICKObservableReadOnlyCollection<ILivePluginInfo> LivePluginInfos
        {
            get { return _serviceInfoManager.LivePluginInfos; }
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
            private set
            {
                if( value != _configurationManager )
                {
                    _configurationManager = value;
                    _graph.ConfigurationManager = value;
                    RaisePropertyChanged( "ConfigurationManager" );
                }
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

        /// <summary>
        /// The currently selected graph vertex.
        /// </summary>
        public YodiiGraphVertex SelectedVertex
        {
            get
            {
                return _selectedVertex;
            }
            set
            {
                Debug.Assert( value == null || Graph.Vertices.Contains( value ), "Graph contains vertex to select" );

                if( value != _selectedVertex )
                {
                    if( _selectedVertex != null ) _selectedVertex.IsSelected = false;
                    _selectedVertex = value;
                    RaisePropertyChanged( "HasSelection" );
                    RaisePropertyChanged( "SelectedVertex" );
                    if( value != null ) value.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Returns true if a vertex is selected.
        /// </summary>
        public bool HasSelection
        {
            get { return SelectedVertex != null; }
        }

        public ServiceInfoManager ServiceInfoManager
        {
            get { return _serviceInfoManager; }
        }

        public ICommand RemoveSelectedVertexCommand { get { return _removeSelectedVertexCommand; } }

        public ICommand RunStaticSolverCommand { get { return _runStaticSolverCommand; } }
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
            return CreateNewService( serviceName, null );
        }

        /// <summary>
        /// Creates a new named service, which specializes another service.
        /// </summary>
        /// <param name="serviceName">Name of the new service</param>
        /// <param name="generalization">Specialized service</param>
        /// <returns>New service</returns>
        public IServiceInfo CreateNewService( string serviceName, IServiceInfo generalization = null )
        {
            if( serviceName == null ) throw new ArgumentNullException( "serviceName" );

            ServiceInfo newService = _serviceInfoManager.CreateNewService( serviceName, (ServiceInfo)generalization );


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
            return CreateNewPlugin( pluginGuid, pluginName, null );
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

            if( service != null && !ServiceInfos.Contains<IServiceInfo>( service ) ) throw new InvalidOperationException( "Service does not exist in this Lab" );

            PluginInfo newPlugin = _serviceInfoManager.CreateNewPlugin( pluginGuid, pluginName, (ServiceInfo)service );

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

            if( !ServiceInfos.Contains( (ServiceInfo)service ) ) throw new InvalidOperationException( "Service does not exist in this Lab" );
            if( !PluginInfos.Contains( (PluginInfo)plugin ) ) throw new InvalidOperationException( "Plugin does not exist in this Lab" );

            _serviceInfoManager.SetPluginDependency( (PluginInfo)plugin, (ServiceInfo)service, runningRequirement );
        }

        public void RemovePlugin( IPluginInfo pluginInfo )
        {
            _serviceInfoManager.RemovePlugin( (PluginInfo)pluginInfo );
        }

        public void RemoveService( IServiceInfo serviceInfo )
        {
            _serviceInfoManager.RemoveService( (ServiceInfo)serviceInfo );
        }

        public ServiceInfo GetServiceInfoByName( string name )
        {
            return _serviceInfoManager.ServiceInfos.Where( x => x.ServiceFullName == name ).First();
        }

        public IEnumerable<PluginInfo> GetPluginInfosByName( string name )
        {
            return _serviceInfoManager.PluginInfos.Where( x => x.PluginFullName == name );
        }

        public PluginInfo GetPluginInfoById( Guid guid )
        {
            return _serviceInfoManager.PluginInfos.Where( x => x.PluginId == guid ).First();
        }

        public DetailedOperationResult LoadState( string filePath )
        {
            _serviceInfoManager.ClearState();

            XmlReaderSettings rs = new XmlReaderSettings();

            try
            {
                using( FileStream fs = File.Open( filePath, FileMode.Open ) )
                {
                    using( XmlReader xr = XmlReader.Create( fs, rs ) )
                    {
                        while( xr.Read() )
                        {
                            if( xr.IsStartElement() && xr.Name == "ServicePluginInfos" )
                            {
                                _serviceInfoManager.LoadFromXmlReader( xr.ReadSubtree() );
                            }
                            else if( xr.IsStartElement() && xr.Name == "ConfigurationManager" )
                            {
                                var manager = ConfigurationManagerXmlSerializer.DeserializeConfigurationManager( xr.ReadSubtree() );
                                ConfigurationManager = manager;
                            }
                        }
                    }
                }
            }
            catch( Exception e ) // TODO: Detailed exception handling and undo
            {
                return new DetailedOperationResult( false, e.Message );
            }

            return new DetailedOperationResult( true );
        }

        public DetailedOperationResult SaveState( string tempFilePath )
        {
            XmlWriterSettings ws = new XmlWriterSettings();
            ws.NewLineHandling = NewLineHandling.None;
            ws.Indent = true;

            try
            {
                using( FileStream fs = File.Open( tempFilePath, FileMode.Create ) )
                {
                    using( XmlWriter xw = XmlWriter.Create( fs, ws ) )
                    {
                        xw.WriteStartDocument( true );
                        xw.WriteStartElement( "YodiiLabState" );

                        xw.WriteStartElement( "ServicePluginInfos" );

                        MockInfoXmlSerializer.SerializeLabStateToXmlWriter( this, xw );

                        xw.WriteEndElement();

                        xw.WriteStartElement( "ConfigurationManager" );

                        ConfigurationManagerXmlSerializer.SerializeConfigurationManager( _configurationManager, xw );

                        xw.WriteEndElement();

                        xw.WriteEndElement();
                        xw.WriteEndDocument();
                    }
                }
            }
            catch( Exception e ) // TODO: Detailed exception handling
            {
                return new DetailedOperationResult( false, e.Message );
            }

            return new DetailedOperationResult( true );
        }

        public void SelectService( IServiceInfo serviceInfo )
        {
            YodiiGraphVertex vertexToSelect = Graph.Vertices.Where( x => x.IsService && x.LiveServiceInfo.ServiceInfo == serviceInfo ).First();
            SelectedVertex = vertexToSelect;
        }

        public void SelectPlugin( IPluginInfo pluginInfo )
        {
            YodiiGraphVertex vertexToSelect = Graph.Vertices.Where( x => x.IsPlugin && x.LivePluginInfo.PluginInfo == pluginInfo ).First();
            SelectedVertex = vertexToSelect;
        }

        public DetailedOperationResult RenameService( ServiceInfo serviceInfo, string newName )
        {
            return _serviceInfoManager.RenameService( serviceInfo, newName );
        }
        #endregion Public methods

        #region Private methods
        #endregion Private methods



    }
}
