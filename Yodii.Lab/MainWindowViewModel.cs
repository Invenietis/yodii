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
using Yodii.Lab.ConfigurationEditor;
using System.Windows;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using GraphX.GraphSharp.Algorithms.Layout;
using GraphX;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Tree;

namespace Yodii.Lab
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        readonly YodiiGraph _graph;
        readonly ServiceInfoManager _serviceInfoManager;

        readonly ICommand _removeSelectedVertexCommand;
        readonly ICommand _runStaticSolverCommand;
        readonly ICommand _openFileCommand;
        readonly ICommand _saveAsFileCommand;
        readonly ICommand _reorderGraphLayoutCommand;
        readonly ICommand _createServiceCommand;
        readonly ICommand _createPluginCommand;
        readonly ICommand _openConfigurationEditorCommand;

        ConfigurationManager _configurationManager; // Can be swapped through XML loading.
        YodiiGraphVertex _selectedVertex;
        bool _isLive;
        LayoutAlgorithmTypeEnum _graphLayoutAlgorithmType;
        ILayoutParameters _graphLayoutParameters;

        ConfigurationEditorWindow _activeConfEditorWindow = null;

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            _configurationManager = new ConfigurationManager();
            _serviceInfoManager = new ServiceInfoManager();

            // Live objects and static infos are managed in the ServiceInfoManager.

            _graph = new YodiiGraph( _configurationManager, _serviceInfoManager );

            _removeSelectedVertexCommand = new RelayCommand( RemoveSelectedVertexExecute, HasSelectedVertex );
            _runStaticSolverCommand = new RelayCommand( RunStaticSolverExecute );
            _openFileCommand = new RelayCommand( OpenFileExecute );
            _saveAsFileCommand = new RelayCommand( SaveAsFileExecute );
            _reorderGraphLayoutCommand = new RelayCommand( ReorderGraphLayoutExecute );
            _createPluginCommand = new RelayCommand( CreatePluginExecute );
            _createServiceCommand = new RelayCommand( CreateServiceExecute );
            _openConfigurationEditorCommand = new RelayCommand( OpenConfigurationEditorExecute );

            GraphLayoutAlgorithmType = LayoutAlgorithmTypeEnum.Tree;
            GraphLayoutParameters = GetDefaultLayoutParameters( GraphLayoutAlgorithmType );

            LoadDefaultState();
        }

        private void LoadDefaultState()
        {
            _serviceInfoManager.ClearState();
            XmlReader r = XmlReader.Create( new StringReader( Yodii.Lab.Properties.Resources.DefaultState ) );

            LoadStateFromXmlReader( r );
        }

        #endregion Constructor

        #region Command handlers

        private void OpenConfigurationEditorExecute( object param )
        {
            Debug.Assert( param == null || param is Window);
            if( _activeConfEditorWindow != null )
            {
                _activeConfEditorWindow.Activate();
            }
            else
            {
                _activeConfEditorWindow = new ConfigurationEditorWindow( ConfigurationManager, ServiceInfoManager );
                _activeConfEditorWindow.Owner = (Window)param;
                _activeConfEditorWindow.Closing += ( s, e2 ) => { _activeConfEditorWindow = null; };

                _activeConfEditorWindow.Show();
            }
        }

        private void ReorderGraphLayoutExecute( object param )
        {
            if( param == null )
            {
                // Refresh layout.
                Graph.RaiseGraphUpdateRequested( GraphGenerationRequestType.RelayoutGraph );
            }
            else
            {
                // Re-create graph with new layout and parameters.
                GraphLayoutAlgorithmType = (GraphX.LayoutAlgorithmTypeEnum)param;
                GraphLayoutParameters = GetDefaultLayoutParameters(GraphLayoutAlgorithmType);

                Graph.RaiseGraphUpdateRequested( GraphGenerationRequestType.RegenerateGraph, GraphLayoutAlgorithmType, GraphLayoutParameters );
            }
        }

        private void CreateServiceExecute( object param )
        {
            Debug.Assert( param == null || param is Window );
            IServiceInfo selectedService = null;

            if( SelectedVertex != null )
            {
                if( SelectedVertex.IsService )
                {
                    selectedService = SelectedVertex.LiveServiceInfo.ServiceInfo;
                }
                else if( SelectedVertex.IsPlugin )
                {
                    selectedService = SelectedVertex.LivePluginInfo.PluginInfo.Service;
                }
            }

            AddServiceWindow window = new AddServiceWindow( ServiceInfos, selectedService );

            window.NewServiceCreated += ( s, nse ) =>
            {
                if( ServiceInfos.Any( si => si.ServiceFullName == nse.ServiceName ) )
                {
                    nse.CancelReason = String.Format( "Service with name {0} already exists. Pick another name.", nse.ServiceName );
                }
                else
                {
                    IServiceInfo newService = CreateNewService( nse.ServiceName, nse.Generalization );
                    SelectService( newService );
                }
            };

            window.Owner = (Window)param;

            window.ShowDialog();
        }

        private void CreatePluginExecute( object param )
        {
            Debug.Assert( param != null || param is Window );
            IServiceInfo selectedService = null;

            if( SelectedVertex != null )
            {
                if( SelectedVertex.IsService )
                {
                    selectedService = SelectedVertex.LiveServiceInfo.ServiceInfo;
                }
                else if( SelectedVertex.IsPlugin )
                {
                    selectedService = SelectedVertex.LivePluginInfo.PluginInfo.Service;
                }
            }

            AddPluginWindow window = new AddPluginWindow( ServiceInfos, selectedService );

            window.NewPluginCreated += ( s, npe ) =>
            {
                if( PluginInfos.Any( si => si.PluginId == npe.PluginId ) )
                {
                    npe.CancelReason = String.Format( "Plugin with GUID {0} already exists. Pick another GUID.", npe.PluginId.ToString() );
                }
                else
                {
                    IPluginInfo newPlugin = CreateNewPlugin( npe.PluginId, npe.PluginName, npe.Service );
                    foreach( var kvp in npe.ServiceReferences )
                    {
                        SetPluginDependency( newPlugin, kvp.Key, kvp.Value );
                    }
                    SelectPlugin( newPlugin );
                }
            };

            window.Owner = (Window)param;

            window.ShowDialog();
        }

        private void OpenFileExecute( object param )
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".xml";
            dlg.Filter = "Yodii.Lab XML Files (*.xml)|*.xml";
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;

            Nullable<bool> result = dlg.ShowDialog();

            if( result == true )
            {
                string filePath = dlg.FileName;
                var r = LoadState( filePath );
                if( !r )
                {
                    MessageBox.Show( r.Reason, "Couldn't open file" );
                }
            }
        }

        private void SaveAsFileExecute( object param )
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".xml";
            dlg.Filter = "Yodii.Lab XML Files (*.xml)|*.xml";
            dlg.CheckPathExists = true;
            dlg.OverwritePrompt = true;
            dlg.AddExtension = true;

            Nullable<bool> result = dlg.ShowDialog();

            if( result == true )
            {
                string filePath = dlg.FileName;
                var r = SaveState( filePath );
                if( !r )
                {
                    MessageBox.Show( r.Reason, "Couldn't save file" );
                }
            }
        }

        private void RunStaticSolverExecute( object obj )
        {
            // TODO: Complete static solver.
            ConfigurationSolver solver = new ConfigurationSolver();
            var result = solver.Initialize( _configurationManager.FinalConfiguration, _serviceInfoManager );


            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "ConfigurationSolver result:" );
            if( result.ConfigurationSuccess ) sb.AppendLine( "Success" );
            else sb.AppendLine( "Failure" );
            sb.AppendLine( String.Format( "{0} running plugins\n", result.RunningPlugins.Count) );

            MessageBox.Show( sb.ToString() );
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

                    if( _activeConfEditorWindow != null )
                    {
                        _activeConfEditorWindow.Close();
                    }

                    RaisePropertyChanged( "ConfigurationManager" );
                }
            }
        }

        /// <summary>
        /// The currently selected graph vertex.GraphLayoutAlgorithmType
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
        /// The current graph layout type.
        /// </summary> parameters
        public LayoutAlgorithmTypeEnum GraphLayoutAlgorithmType
        {
            get
            {
                return _graphLayoutAlgorithmType;
            }
            set
            {
                if( value != _graphLayoutAlgorithmType )
                {
                    _graphLayoutAlgorithmType = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// The current graph layout parameters.
        /// </summary>
        public ILayoutParameters GraphLayoutParameters
        {
            get
            {
                return _graphLayoutParameters;
            }
            set
            {
                if( value != _graphLayoutParameters )
                {
                    _graphLayoutParameters = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Available graph layout types.
        /// </summary>
        public IEnumerable<GraphX.LayoutAlgorithmTypeEnum> GraphLayoutAlgorithmTypes
        {
            get
            {
                return (IEnumerable<GraphX.LayoutAlgorithmTypeEnum>)Enum.GetValues( typeof( GraphX.LayoutAlgorithmTypeEnum ) );
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
        public ICommand OpenConfigurationEditorCommand { get { return _openConfigurationEditorCommand; } }
        public ICommand RunStaticSolverCommand { get { return _runStaticSolverCommand; } }
        public ICommand OpenFileCommand { get { return _openFileCommand; } }
        public ICommand SaveAsFileCommand { get { return _saveAsFileCommand; } }
        public ICommand ReorderGraphLayoutCommand { get { return _reorderGraphLayoutCommand; } }
        public ICommand CreatePluginCommand { get { return _createPluginCommand; } }
        public ICommand CreateServiceCommand { get { return _createServiceCommand; } }
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
        public void SetPluginDependency( IPluginInfo plugin, IServiceInfo service, DependencyRequirement runningRequirement )
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
                        LoadStateFromXmlReader(xr);
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

        private static ILayoutParameters GetDefaultLayoutParameters(LayoutAlgorithmTypeEnum layoutType)
        {
            switch( layoutType )
            {
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                    EfficientSugiyamaLayoutParameters sugiyamaParams = new EfficientSugiyamaLayoutParameters();
                    sugiyamaParams.VertexDistance = 70.0;
                    sugiyamaParams.MinimizeEdgeLength = false;
                    sugiyamaParams.PositionMode = 0;
                    sugiyamaParams.EdgeRouting = SugiyamaEdgeRoutings.Orthogonal;
                    sugiyamaParams.OptimizeWidth = false;
                    return sugiyamaParams;
                case LayoutAlgorithmTypeEnum.Tree:
                    SimpleTreeLayoutParameters treeParams = new SimpleTreeLayoutParameters();
                    treeParams.Direction = LayoutDirection.BottomToTop;
                    //treeParams.VertexGap = 30.0;
                    //treeParams.OptimizeWidthAndHeight = false;
                    treeParams.SpanningTreeGeneration = SpanningTreeGeneration.BFS;
                    return treeParams;
                default:
                    return null;
            }
        }

        private void LoadStateFromXmlReader(XmlReader xr)
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

        #endregion Private methods
    }
}
