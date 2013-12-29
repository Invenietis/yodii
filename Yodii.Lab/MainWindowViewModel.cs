using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using CK.Core;
using GraphX;
using GraphX.GraphSharp.Algorithms.Layout;
using Yodii.Lab.ConfigurationEditor;
using Yodii.Lab.Mocks;
using Yodii.Lab.Utils;
using Yodii.Model;

using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Globalization;
namespace Yodii.Lab
{
    /// <summary>
    /// View model for the main window.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Fired when a new notification is requested, to display data the user should see.
        /// </summary>
        internal event EventHandler<NotificationEventArgs> NewNotification;

        internal event EventHandler CloseBackstageRequest;

        #region Fields

        /// <summary>
        /// Image URI for Plugin Running notifications.
        /// </summary>
        public static readonly string RUNNING_NOTIFICATION_IMAGE_URI = @"/Yodii.Lab;component/Assets/RunningStatusRunning.png";

        /// <summary>
        /// Image URI for Plugin Stopped notifications.
        /// </summary>
        public static readonly string STOPPED_NOTIFICATION_IMAGE_URI = @"/Yodii.Lab;component/Assets/RunningStatusStopped.png";

        readonly YodiiGraph _graph;
        readonly LabStateManager _labStateManager;

        readonly CKObservableSortedArrayList<RecentFile> _recentFiles;

        readonly ICommand _removeSelectedVertexCommand;
        readonly ICommand _toggleEngineCommand;
        readonly ICommand _openFileCommand;
        readonly ICommand _saveAsFileCommand;
        readonly ICommand _saveCommand;
        readonly ICommand _reorderGraphLayoutCommand;
        readonly ICommand _createServiceCommand;
        readonly ICommand _createPluginCommand;
        readonly ICommand _openConfigurationEditorCommand;
        readonly ICommand _newFileCommand;
        readonly ICommand _revokeAllCommandsCommand;

        readonly ActivityMonitor _activityMonitor;
        readonly DispatcherTimer _autosaveTimer;

        readonly IYodiiEngine _engine; // Loaded from LabStateManager.
        YodiiGraphVertex _selectedVertex;

        ConfigurationEditorWindow _activeConfEditorWindow = null;
        bool _hideNotifications = false;

        bool _changedSinceLastSave;
        string _lastSavePath;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of this ViewModel.
        /// </summary>
        /// <param name="loadDefaultState">True if the default XML state should be loaded, false to start on an empty state.</param>
        public MainWindowViewModel( bool loadDefaultState = false )
        {
            _recentFiles = new CKObservableSortedArrayList<RecentFile>( ( a, b ) => DateTime.Compare( b.AccessTime, a.AccessTime ) );

            // Lab objects, live objects and static infos are managed in the LabStateManager.
            _labStateManager = new LabStateManager();
            _engine = _labStateManager.Engine;

            _labStateManager.Engine.PropertyChanged += Engine_PropertyChanged;
            _labStateManager.ServiceInfos.CollectionChanged += ServiceInfos_CollectionChanged;
            _labStateManager.PluginInfos.CollectionChanged += PluginInfos_CollectionChanged;
            _labStateManager.RunningPlugins.CollectionChanged += RunningPlugins_CollectionChanged;

            _engine.Configuration.Layers.CollectionChanged += Layers_CollectionChanged;

            _activityMonitor = new ActivityMonitor();

            _activityMonitor.OpenTrace().Send( "Hello world" );

            _graph = new YodiiGraph( _engine.Configuration, _labStateManager );

            _removeSelectedVertexCommand = new RelayCommand( RemoveSelectedVertexExecute, CanEditSelectedVertex );
            _toggleEngineCommand = new RelayCommand( ToggleEngineExecute );
            _openFileCommand = new RelayCommand( OpenFileExecute );
            _saveAsFileCommand = new RelayCommand( SaveAsFileExecute );
            _saveCommand = new RelayCommand( SaveExecute ); // Save is always available.e
            _reorderGraphLayoutCommand = new RelayCommand( ReorderGraphLayoutExecute );
            _createPluginCommand = new RelayCommand( CreatePluginExecute, CanEditItems );
            _createServiceCommand = new RelayCommand( CreateServiceExecute, CanEditItems );
            _openConfigurationEditorCommand = new RelayCommand( OpenConfigurationEditorExecute );
            _newFileCommand = new RelayCommand( NewFileExecute );
            _revokeAllCommandsCommand = new RelayCommand( RevokeAllCommandsExecute, CanRevokeAllCommands );

            LoadRecentFiles();

            // Appication is only available on WPF context.
            if( Application.Current != null )
            {
                _autosaveTimer = new DispatcherTimer( DispatcherPriority.Background, Application.Current.Dispatcher );
            }
            else
            {
                _autosaveTimer = new DispatcherTimer( DispatcherPriority.Background, Dispatcher.CurrentDispatcher );
            }

            _autosaveTimer.Interval = new TimeSpan( 0, 0, 5 );
            _autosaveTimer.Tick += AutosaveTick;

            // Autosave timer is started from outside, using StartAutosaveTimer().

            if( loadDefaultState ) LoadDefaultState();
        }

        #endregion Constructor

        #region Event handlers

        void Layers_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach( var i in e.NewItems )
                    {
                        IConfigurationLayer layer = (IConfigurationLayer)i;
                        layer.PropertyChanged += staticInfo_PropertyChanged;
                        layer.Items.CollectionChanged += Items_CollectionChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach( var i in e.OldItems )
                    {
                        IConfigurationLayer layer = (IConfigurationLayer)i;
                        layer.PropertyChanged -= staticInfo_PropertyChanged;
                        layer.Items.CollectionChanged -= Items_CollectionChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
            }

            ChangedSinceLastSave = true;
        }

        void Items_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach( var i in e.NewItems )
                    {
                        IConfigurationItem item = (IConfigurationItem)i;
                        item.PropertyChanged += staticInfo_PropertyChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach( var i in e.OldItems )
                    {
                        IConfigurationItem item = (IConfigurationItem)i;
                        item.PropertyChanged -= staticInfo_PropertyChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
            }

            ChangedSinceLastSave = true;
        }

        void RunningPlugins_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach( var i in e.NewItems )
                    {
                        IPluginInfo p = (IPluginInfo)i;
                        RaiseNewNotification( @"Plugin running",
                            String.Format( @"Plugin '{0}' is now running.", p.PluginFullName ),
                            RUNNING_NOTIFICATION_IMAGE_URI
                            );
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach( var i in e.OldItems )
                    {
                        IPluginInfo p = (IPluginInfo)i;
                        RaiseNewNotification( @"Plugin stopped",
                            String.Format( @"Plugin '{0}' has been stopped.", p.PluginFullName ),
                            STOPPED_NOTIFICATION_IMAGE_URI
                            );
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RaiseNewNotification( "Plugins stopped", "Stopped all plugins" );
                    break;
            }
        }

        void Engine_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            switch( e.PropertyName )
            {
                case "IsRunning":
                    RaisePropertyChanged( "ToggleEngineText" );
                    if( _engine.IsRunning )
                    {
                        RaiseNewNotification( "Entering simulation mode", "Yodii engine is now running." );
                    }
                    else
                    {
                        RaiseNewNotification( "Entering build mode", "Yodii engine has been stopped." );
                    }
                    break;
            }
        }

        void PluginInfos_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach( var i in e.NewItems )
                    {
                        // Reset selection if deleted vertex was selected.
                        if( SelectedVertex != null && SelectedVertex.IsService && SelectedVertex.LabServiceInfo.ServiceInfo == i )
                        {
                            SelectedVertex = null;
                        }

                        PluginInfo newPlugin = (PluginInfo)i;

                        newPlugin.PropertyChanged += staticInfo_PropertyChanged;
                        newPlugin.InternalServiceReferences.CollectionChanged += InternalServiceReferences_CollectionChanged;

                        RaiseNewNotification( "Plugin added",
                            String.Format( "Added new plugin: '{0}'", newPlugin.PluginFullName )
                            );
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach( var i in e.OldItems )
                    {
                        // Reset selection if deleted vertex was selected.
                        if( SelectedVertex != null && SelectedVertex.IsPlugin && SelectedVertex.LabPluginInfo.PluginInfo == i )
                        {
                            SelectedVertex = null;
                        }

                        PluginInfo oldPlugin = (PluginInfo)i;

                        oldPlugin.PropertyChanged -= staticInfo_PropertyChanged;
                        oldPlugin.InternalServiceReferences.CollectionChanged += InternalServiceReferences_CollectionChanged;

                        RaiseNewNotification( "Plugin removed",
                            String.Format( "Removed plugin: '{0}'", oldPlugin.PluginFullName )
                            );
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RaiseNewNotification( "Plugins reset", "Removed all plugins." );
                    break;
            }
            ChangedSinceLastSave = true;
        }

        void InternalServiceReferences_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach( var i in e.NewItems )
                    {
                        MockServiceReferenceInfo serviceRef = (MockServiceReferenceInfo)i;
                        serviceRef.PropertyChanged += staticInfo_PropertyChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach( var i in e.OldItems )
                    {
                        MockServiceReferenceInfo serviceRef = (MockServiceReferenceInfo)i;
                        serviceRef.PropertyChanged -= staticInfo_PropertyChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
            }

            ChangedSinceLastSave = true;
        }

        void ServiceInfos_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach( var i in e.NewItems )
                    {
                        ServiceInfo newService = (ServiceInfo)i;
                        newService.PropertyChanged += staticInfo_PropertyChanged;

                        RaiseNewNotification( "Service added",
                            String.Format( "Added new service: '{0}'", newService.ServiceFullName )
                            );

                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach( var i in e.OldItems )
                    {
                        ServiceInfo oldService = (ServiceInfo)i;
                        oldService.PropertyChanged -= staticInfo_PropertyChanged;
                        RaiseNewNotification( "Service removed",
                            String.Format( "Removed service: '{0}'", oldService.ServiceFullName )
                            );

                        if( SelectedVertex != null && SelectedVertex.IsService && SelectedVertex.LabServiceInfo.ServiceInfo == oldService )
                        {
                            SelectedVertex = null;
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RaiseNewNotification( "Services reset", "Removed all services." );
                    SelectedVertex = null;
                    break;
            }
            ChangedSinceLastSave = true;
        }

        void staticInfo_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            ChangedSinceLastSave = true;
        }
        #endregion

        #region Command handlers

        private void OpenConfigurationEditorExecute( object param )
        {
            Debug.Assert( param == null || param is Window );
            if( _activeConfEditorWindow != null )
            {
                _activeConfEditorWindow.Activate();
            }
            else
            {
                _activeConfEditorWindow = new ConfigurationEditorWindow( ConfigurationManager, LabState );
                _activeConfEditorWindow.Owner = (Window)param;
                _activeConfEditorWindow.Closing += ( s, e2 ) => { _activeConfEditorWindow = null; };

                _activeConfEditorWindow.Show();
            }
        }

        private void ReorderGraphLayoutExecute( object param )
        {

            RaiseNewNotification( new Notification() { Title = "Calling relayout..." } );
            // Refresh layout.
            Graph.RaiseGraphUpdateRequested();
        }

        private void CreateServiceExecute( object param )
        {
            Debug.Assert( param == null || param is Window );
            IServiceInfo selectedService = null;

            if( SelectedVertex != null )
            {
                if( SelectedVertex.IsService )
                {
                    selectedService = SelectedVertex.LabServiceInfo.ServiceInfo;
                }
                else if( SelectedVertex.IsPlugin )
                {
                    selectedService = SelectedVertex.LabPluginInfo.PluginInfo.Service;
                }
            }

            AddServiceWindow window = new AddServiceWindow( ServiceInfos, selectedService );

            window.NewServiceCreated += ( s, nse ) =>
            {
                if( ServiceInfos.Any( si => si.ServiceFullName == nse.ServiceName ) )
                {
                    RaiseNewNotification( new Notification() { Title = String.Format( "Service {0} already exists", nse.ServiceName ) } );
                    nse.CancelReason = String.Format( "Service with name {0} already exists. Pick another name.", nse.ServiceName );
                }
                else if( _labStateManager.IsPlugin( nse.ServiceName ) )
                {
                    string reason = String.Format( "A plugin with the name '{0}' name already exists.", nse.ServiceName );
                    RaiseNewNotification( "Can't add service", reason );
                    nse.CancelReason = reason;
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
                    selectedService = SelectedVertex.LabServiceInfo.ServiceInfo;
                }
                else if( SelectedVertex.IsPlugin )
                {
                    selectedService = SelectedVertex.LabPluginInfo.PluginInfo.Service;
                }
            }

            AddPluginWindow window = new AddPluginWindow( ServiceInfos, selectedService );

            window.NewPluginCreated += ( s, npe ) =>
            {
                if( String.IsNullOrWhiteSpace( npe.PluginName ) )
                {
                    RaiseNewNotification( "Can't add plugin", "Plugin must have a name." );
                    npe.CancelReason = "Please enter a name for this plugin.";
                }
                else if( LabState.PluginInfos.Any( x => x.PluginFullName == npe.PluginName ) )
                {
                    string reason = String.Format( "A plugin with the name '{0}' name already exists.", npe.PluginName );
                    RaiseNewNotification( "Can't add plugin", reason );
                    npe.CancelReason = reason;
                }
                else if( _labStateManager.IsService( npe.PluginName ) )
                {
                    string reason = String.Format( "A service with the name '{0}' name already exists.", npe.PluginName );
                    RaiseNewNotification( "Can't add plugin", reason );
                    npe.CancelReason = reason;
                }
                else
                {
                    IPluginInfo newPlugin = CreateNewPlugin( npe.PluginName, npe.Service );
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
            if( param != null && param is string )
            {
                string fileName = (string)param;

                if( !File.Exists( fileName ) )
                {
                    RemoveFileFromRecentFiles( fileName );
                    MessageBox.Show(
                        String.Format( "File does not exist:\n{0}", fileName ),
                        "File not found",
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK );
                }
                else
                {
                    var r = LoadState( fileName );
                    if( !r )
                    {
                        MessageBox.Show( r.Reason, "Couldn't open file" );
                    }
                }

            }
            else
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
        }

        private void SaveAsFileExecute( object param )
        {
            SaveAs();
        }

        private void ToggleEngineExecute( object obj )
        {
            if( _labStateManager.Engine.IsRunning )
            {
                LabState.Engine.Stop();
            }
            else
            {
                RaiseNewNotification( "Starting simulation", "Starting Yodii engine." );
                var startResult = _engine.Start();

                if( startResult == null )
                {
                    RaiseNewNotification( "Error", "YodiiEngine.Start() returned null!" );
                    return;
                }

                if( !startResult.Success )
                {
                    RaiseNewNotification( "Engine startup failed", "Couldn't start engine." );
                    MessageBox.Show( startResult.Describe(), "Engine start error details", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK );
                }
            }
        }

        private bool CanEditSelectedVertex( object obj )
        {
            return SelectedVertex != null && CanEditItems( null );
        }

        private bool CanEditItems( object obj )
        {
            return true;
        }

        private void RemoveSelectedVertexExecute( object obj )
        {
            if( SelectedVertex == null ) return;

            if( SelectedVertex.IsPlugin )
            {
                var name = SelectedVertex.LabPluginInfo.PluginInfo.Description;
                this.RemovePlugin( SelectedVertex.LabPluginInfo.PluginInfo );
            }
            else if( SelectedVertex.IsService )
            {
                var name = SelectedVertex.LabServiceInfo.ServiceInfo.ServiceFullName;
                this.RemoveService( SelectedVertex.LabServiceInfo.ServiceInfo );
            }

            SelectedVertex = null;
        }

        private void NewFileExecute( object obj )
        {
            if( SaveBeforeClosingFile() )
            {
                OpenedFilePath = null;
                LabState.ClearState();
                ChangedSinceLastSave = false;
            }

            RaiseCloseBackstageRequest();
        }

        private bool CanRevokeAllCommands( object obj )
        {
            return LabState.Engine.IsRunning && LabState.Engine.YodiiCommands.Count > 0;
        }

        private void RevokeAllCommandsExecute( object obj )
        {
            if( !CanRevokeAllCommands( obj ) ) return;

            IEnumerable<string> callers = LabState.Engine.YodiiCommands.Select( x => x.CallerKey ).Distinct().ToList();

            foreach( string callerKey in callers )
            {
                var result = LabState.Engine.LiveInfo.RevokeCaller( callerKey );

                if( !result.Success )
                {
                    MessageBox.Show(
                        String.Format( "Couldn't revoke caller key '{0}' as it would raise this error:\n\n{1}", callerKey, result.Describe() ),
                        "Command revoke failed", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK );
                }
            }
        }

        private void SaveExecute( object obj )
        {
            Save();
        }

        #endregion Command handlers

        #region Properties

        /// <summary>
        /// Window title.
        /// </summary>
        public string WindowTitle
        {
            get
            {
                return String.Format(
                    "Yodii.Lab - {0}{1}",
                    OpenedFilePath != null ? Path.GetFileName( OpenedFilePath ) : "(New file)",
                    ChangedSinceLastSave == true ? " *" : String.Empty
                    );
            }
        }

        /// <summary>
        /// Services created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<IServiceInfo> ServiceInfos
        {
            get { return _labStateManager.ServiceInfos; }
        }

        /// <summary>
        /// Plugins created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<IPluginInfo> PluginInfos
        {
            get { return _labStateManager.PluginInfos; }
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
        /// State changed since it was last saved/loaded.
        /// </summary>
        public bool ChangedSinceLastSave
        {
            get
            {
                return _changedSinceLastSave;
            }
            private set
            {
                if( value != _changedSinceLastSave )
                {
                    _changedSinceLastSave = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged( "WindowTitle" );
                }
            }
        }

        /// <summary>
        /// Last path loaded/saved.
        /// </summary>
        public string OpenedFilePath
        {
            get
            {
                return _lastSavePath;
            }
            private set
            {
                if( value != _lastSavePath )
                {
                    _lastSavePath = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged( "WindowTitle" );
                }
            }
        }

        /// <summary>
        /// The Yodii ConfigurationManager linked to this Lab.
        /// </summary>
        public IConfigurationManager ConfigurationManager
        {
            get
            {
                return _engine.Configuration;
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
        /// Returns true if a vertex is selected.
        /// </summary>
        public bool HasSelection
        {
            get { return SelectedVertex != null; }
        }

        /// <summary>
        /// State manager for this lab.
        /// </summary>
        public LabStateManager LabState
        {
            get { return _labStateManager; }
        }

        /// <summary>
        /// Text on the toggle engine button.
        /// </summary>
        public string ToggleEngineText
        {
            get
            {
                if( _labStateManager.Engine.IsRunning )
                {
                    return "Stop engine";
                }
                else
                {
                    return "Start engine";
                }
            }
        }

        /// <summary>
        /// Recent files.
        /// </summary>
        public CKObservableSortedArrayList<RecentFile> RecentFiles
        { get { return _recentFiles; } }

        /// <summary>
        /// Command to remove a vertex that was selected beforehand.
        /// </summary>
        public ICommand RemoveSelectedVertexCommand { get { return _removeSelectedVertexCommand; } }
        /// <summary>
        /// Command to open the ConfigurationManager editor.
        /// </summary>
        public ICommand OpenConfigurationEditorCommand { get { return _openConfigurationEditorCommand; } }
        /// <summary>
        /// Command to stop the engine.
        /// </summary>
        public ICommand ToggleEngineCommand { get { return _toggleEngineCommand; } }
        /// <summary>
        /// Command to open a state file and load it.
        /// </summary>
        public ICommand OpenFileCommand { get { return _openFileCommand; } }
        /// <summary>
        /// Command to select a state file to save to, then save.
        /// </summary>
        public ICommand SaveAsFileCommand { get { return _saveAsFileCommand; } }
        /// <summary>
        /// Command to save the opened file, or define a new file to save to.
        /// </summary>
        public ICommand SaveCommand { get { return _saveCommand; } }
        /// <summary>
        /// Command to reorder the graph, or to change the graph's layout.
        /// </summary>
        public ICommand ReorderGraphLayoutCommand { get { return _reorderGraphLayoutCommand; } }
        /// <summary>
        /// Command to open a window allowing creation of a new plugin.
        /// </summary>
        public ICommand CreatePluginCommand { get { return _createPluginCommand; } }
        /// <summary>
        /// Command to open a window allowing creation of a new service.
        /// </summary>
        public ICommand CreateServiceCommand { get { return _createServiceCommand; } }
        /// <summary>
        /// Command to create a new file.
        /// </summary>
        public ICommand NewFileCommand { get { return _newFileCommand; } }
        /// <summary>
        /// Command to revoke all callers.
        /// </summary>
        public ICommand RevokeAllCommandsCommand { get { return _revokeAllCommandsCommand; } }
        #endregion Properties

        #region Public methods

        public bool SaveBeforeClosingFile()
        {
            if( ChangedSinceLastSave )
            {
                var result = MessageBox.Show( "Save changes?\nUnsaved data will be lost if you press No.", "Save file", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel );
                if( result == MessageBoxResult.Cancel ) return false;

                if( result == MessageBoxResult.Yes )
                {
                    return Save();
                }
            }

            return true;
        }

        /// <summary>
        /// Whether the Lab has an auto-saved state.
        /// </summary>
        /// <returns>True if there is an auto-saved state.</returns>
        public bool HasAutosave()
        {
            return !String.IsNullOrWhiteSpace( Yodii.Lab.Properties.Settings.Default.LastAutosaveFileContents );
        }

        /// <summary>
        /// Load autosaved state, if present.
        /// </summary>
        public void LoadAutosave()
        {
            if( !HasAutosave() ) return;
            OpenedFilePath = null;
            LabState.ClearState();

            if( !String.IsNullOrWhiteSpace( Yodii.Lab.Properties.Settings.Default.LastAutosaveFilePath ) )
            {
                OpenedFilePath = Yodii.Lab.Properties.Settings.Default.LastAutosaveFilePath;
            }

            using( StringReader sr = new StringReader( Yodii.Lab.Properties.Settings.Default.LastAutosaveFileContents ) )
            {
                using( XmlReader r = XmlReader.Create( sr ) )
                {
                    try
                    {
                        _hideNotifications = true;
                        Graph.LockGraphUpdates = true;
                        LabXmlSerialization.DeserializeAndResetStateFromXml( LabState, r );
                        Graph.LockGraphUpdates = false;

                        Graph.RaiseGraphUpdateRequested();

                        _hideNotifications = false;
                        RaiseNewNotification( "Autosave loaded", OpenedFilePath );
                    }
                    catch( Exception ex )
                    {
                        MessageBox.Show(
                            String.Format( "Load from autosave failed:\n{0}\n\n{1}", ex.Message, ex.StackTrace ),
                            "Loading failed",
                            MessageBoxButton.OK, MessageBoxImage.Error,
                            MessageBoxResult.OK
                            );

                    }
                    finally
                    {
                        _hideNotifications = false;
                        Graph.LockGraphUpdates = false;
                    }
                }
            }
        }

        /// <summary>
        /// Remove any existing autosave.
        /// </summary>
        public void ClearAutosave()
        {
            Yodii.Lab.Properties.Settings.Default.LastAutosaveFilePath = String.Empty;
            Yodii.Lab.Properties.Settings.Default.LastAutosaveFileContents = String.Empty;

            Yodii.Lab.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Starts the autosave timer.
        /// </summary>
        public void StartAutosaveTimer()
        {
            _autosaveTimer.Start();
        }

        /// <summary>
        /// Stops the autosave timer.
        /// </summary>
        public void StopAutosaveTimer()
        {
            _autosaveTimer.Stop();
        }

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

            ServiceInfo newService = _labStateManager.CreateNewService( serviceName, (ServiceInfo)generalization );


            return newService;
        }

        /// <summary>
        /// Creates a new named plugin, which does not implement a service.
        /// </summary>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <returns>New plugin</returns>
        public IPluginInfo CreateNewPlugin( string pluginName )
        {
            return CreateNewPlugin( pluginName, null );
        }

        /// <summary>
        /// Creates a new named plugin, which implements an existing service.
        /// </summary>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <param name="service">Implemented service</param>
        /// <returns>New plugin</returns>
        public IPluginInfo CreateNewPlugin( string pluginName, IServiceInfo service )
        {
            if( String.IsNullOrWhiteSpace( pluginName ) ) throw new ArgumentNullException( "pluginName" );

            if( service != null && !ServiceInfos.Contains<IServiceInfo>( service ) ) throw new InvalidOperationException( "Service does not exist in this Lab" );

            PluginInfo newPlugin = _labStateManager.CreateNewPlugin( pluginName, (ServiceInfo)service );

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

            _labStateManager.SetPluginDependency( (PluginInfo)plugin, (ServiceInfo)service, runningRequirement );
        }

        /// <summary>
        /// Removes an existing dependency from a plugin to a service.
        /// </summary>
        /// <param name="plugin">Plugin owner.</param>
        /// <param name="service">Service reference.</param>
        public void RemovePluginDependency( IPluginInfo plugin, IServiceInfo service )
        {
            if( plugin == null ) throw new ArgumentNullException( "plugin" );
            if( service == null ) throw new ArgumentNullException( "service" );

            if( !ServiceInfos.Contains( (ServiceInfo)service ) ) throw new InvalidOperationException( "Service does not exist in this Lab" );
            if( !PluginInfos.Contains( (PluginInfo)plugin ) ) throw new InvalidOperationException( "Plugin does not exist in this Lab" );

            _labStateManager.RemovePluginDependency( (PluginInfo)plugin, (ServiceInfo)service );
        }

        /// <summary>
        /// Removes and deletes a plugin.
        /// </summary>
        /// <param name="pluginInfo">Plugin to remove</param>
        public void RemovePlugin( IPluginInfo pluginInfo )
        {
            _labStateManager.RemovePlugin( (PluginInfo)pluginInfo );
        }

        /// <summary>
        /// Removes and deletes a service.
        /// </summary>
        /// <param name="serviceInfo">Service to remove</param>
        public void RemoveService( IServiceInfo serviceInfo )
        {
            _labStateManager.RemoveService( (ServiceInfo)serviceInfo );
        }

        /// <summary>
        /// Gets a service from the state manager with its name.
        /// </summary>
        /// <param name="name">Service name</param>
        /// <returns>Service</returns>
        public ServiceInfo GetServiceInfoByName( string name )
        {
            return _labStateManager.ServiceInfos.Where( x => x.ServiceFullName == name ).First();
        }

        /// <summary>
        /// Get plugins with names matching this one.
        /// </summary>
        /// <param name="name">Plugin name.</param>
        /// <returns>Plugins matching name.</returns>
        public IEnumerable<PluginInfo> GetPluginInfosByName( string name )
        {
            return _labStateManager.PluginInfos.Where( x => x.PluginFullName == name );
        }

        /// <summary>
        /// Get plugin associated to this plugin full name.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name.</param>
        /// <returns>Plugin.</returns>
        public PluginInfo GetPluginInfoById( string pluginFullName )
        {
            return _labStateManager.PluginInfos.Where( x => x.PluginFullName == pluginFullName ).First();
        }

        /// <summary>
        /// Load the lab state from a XML file.
        /// </summary>
        /// <param name="filePath">XML file to load</param>
        /// <returns>Operation result</returns>
        public DetailedOperationResult LoadState( string filePath )
        {
            _hideNotifications = true;
            Graph.LockGraphUpdates = true;

            XmlReaderSettings rs = new XmlReaderSettings();

            try
            {
                using( FileStream fs = File.Open( filePath, FileMode.Open ) )
                {
                    using( XmlReader xr = XmlReader.Create( fs, rs ) )
                    {
                        LabXmlSerialization.DeserializeAndResetStateFromXml( LabState, xr );
                    }
                }
                _hideNotifications = false;
                Graph.LockGraphUpdates = false;

                Graph.RaiseGraphUpdateRequested();

                RaiseNewNotification( new Notification() { Title = "Loaded file", Message = filePath } );

                ChangedSinceLastSave = false;
                OpenedFilePath = filePath;

                SaveOpenedFileAsRecentFile();
                RaiseCloseBackstageRequest();
                return new DetailedOperationResult( true );
            }
            catch( Exception ex )
            {
                // TODO: Detailed exceptions

                string reason = ex.ToString();

                RaiseNewNotification( new Notification() { Title = "Failed to load file", Message = reason } );
            }
            finally
            {
                _hideNotifications = false;
                Graph.LockGraphUpdates = false;
            }
            RaiseCloseBackstageRequest();
            return new DetailedOperationResult( false );
        }

        /// <summary>
        /// Saves the lab state to a file.
        /// </summary>
        /// <param name="filePath">XML file to use</param>
        /// <returns>Operation result</returns>
        public DetailedOperationResult SaveState( string filePath )
        {
            XmlWriterSettings ws = new XmlWriterSettings();
            ws.NewLineHandling = NewLineHandling.None;
            ws.Indent = true;

            try
            {
                using( FileStream fs = File.Open( filePath, FileMode.Create ) )
                {
                    using( XmlWriter xw = XmlWriter.Create( fs, ws ) )
                    {
                        xw.WriteStartDocument( true );
                        LabXmlSerialization.SerializeToXml( LabState, xw );
                        xw.WriteEndDocument();
                    }
                }
                ChangedSinceLastSave = false;
                OpenedFilePath = filePath;
                ClearAutosave();
            }
            catch( Exception e ) // TODO: Detailed exception handling
            {
                RaiseCloseBackstageRequest();
                return new DetailedOperationResult( false, e.Message );
            }

            RaiseCloseBackstageRequest();

            RaiseNewNotification( new Notification() { Title = "Saved state", Message = filePath } );
            return new DetailedOperationResult( true );
        }

        /// <summary>
        /// Sets service as selected.
        /// </summary>
        /// <param name="serviceInfo">Service to select</param>
        public void SelectService( IServiceInfo serviceInfo )
        {
            YodiiGraphVertex vertexToSelect = Graph.Vertices.Where( x => x.IsService && x.LabServiceInfo.ServiceInfo == serviceInfo ).First();
            SelectedVertex = vertexToSelect;
        }

        /// <summary>
        /// Sets plugin as selected.
        /// </summary>
        /// <param name="pluginInfo">Plugin to select</param>
        public void SelectPlugin( IPluginInfo pluginInfo )
        {
            YodiiGraphVertex vertexToSelect = Graph.Vertices.Where( x => x.IsPlugin && x.LabPluginInfo.PluginInfo == pluginInfo ).First();
            SelectedVertex = vertexToSelect;
        }

        /// <summary>
        /// Attempts to rename a service.
        /// </summary>
        /// <param name="serviceInfo">Service to rename</param>
        /// <param name="newName">New service name</param>
        /// <returns>Operation result</returns>
        internal DetailedOperationResult RenameService( ServiceInfo serviceInfo, string newName )
        {
            return _labStateManager.RenameService( serviceInfo, newName );
        }
        #endregion Public methods

        #region Private methods

        private void AutosaveTick( object sender, EventArgs e )
        {
            Autosave();
        }

        private void Autosave()
        {
            if( !ChangedSinceLastSave ) return;

            string xmlString;

            using( StringWriter sw = new StringWriter() )
            {
                using( XmlWriter xw = XmlWriter.Create( sw ) )
                {
                    try
                    {
                        xw.WriteStartDocument( true );
                        LabXmlSerialization.SerializeToXml( LabState, xw );
                        xw.WriteEndDocument();
                    }
                    catch( Exception ex )
                    {
                        MessageBox.Show(
                            String.Format( "Autosave failed:\n{0}\n\n{1}", ex.Message, ex.StackTrace ),
                            "Autosave failed",
                            MessageBoxButton.OK, MessageBoxImage.Error,
                            MessageBoxResult.OK
                            );

                    }
                }

                xmlString = sw.ToString();
            }

            Yodii.Lab.Properties.Settings.Default.LastAutosaveFileContents = xmlString;
            if( String.IsNullOrWhiteSpace( OpenedFilePath ) )
            {
                Yodii.Lab.Properties.Settings.Default.LastAutosaveFilePath = String.Empty;
            }
            else
            {
                Yodii.Lab.Properties.Settings.Default.LastAutosaveFilePath = OpenedFilePath;
            }

            Yodii.Lab.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// In case the file changed, attempts to save the last file.
        /// </summary>
        /// <returns>True if save was successful. False if save failed, or was cancelled.</returns>
        private bool Save()
        {
            if( !String.IsNullOrWhiteSpace( OpenedFilePath ) )
            {
                var r = SaveState( OpenedFilePath );
                if( !r )
                {
                    MessageBox.Show( r.Reason, "Couldn't save file" );
                }
                return r.IsSuccessful;
            }
            else
            {
                return SaveAs();
            }
        }

        private bool SaveAs()
        {
            string saveFilePath = SelectSaveFile();

            if( saveFilePath != null )
            {
                var r = SaveState( saveFilePath );
                if( !r )
                {
                    MessageBox.Show( r.Reason, "Couldn't save file" );
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private string SelectSaveFile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".xml";
            dlg.Filter = "Yodii.Lab XML Files (*.xml)|*.xml";
            dlg.CheckPathExists = true;
            dlg.OverwritePrompt = true;
            dlg.AddExtension = true;
            dlg.FileName = OpenedFilePath;

            Nullable<bool> result = dlg.ShowDialog();

            if( result == true )
            {
                return dlg.FileName;
            }
            else
            {
                return null;
            }
        }

        private void RaiseNewNotification( Notification n )
        {
            if( _hideNotifications ) return;

            if( NewNotification != null )
            {
                NewNotification( this, new NotificationEventArgs( n ) );
            }
        }

        private void RaiseNewNotification( string title = "Notification", string message = "", string imageUri = null )
        {
            Notification n = new Notification()
            {
                Title = title,
                Message = message,
                ImageUrl = imageUri
            };

            RaiseNewNotification( n );
        }

        private void RaiseCloseBackstageRequest()
        {
            if( CloseBackstageRequest != null )
            {
                CloseBackstageRequest( this, new EventArgs() );
            }
        }

        private void LoadDefaultState()
        {
            _labStateManager.ClearState();
            XmlReader r = XmlReader.Create( new StringReader( Yodii.Lab.Properties.Resources.DefaultState ) );

            LabXmlSerialization.DeserializeAndResetStateFromXml( LabState, r );
            ChangedSinceLastSave = false;
        }

        private void LoadRecentFiles()
        {
            if( Application.Current == null ) return; // Settings are not available outside app context
            _recentFiles.Clear();

            StringCollection files = Properties.Settings.Default.RecentFiles;
            foreach( string f in files )
            {
                var m = Regex.Match( f, @"^(.*),(\d+)$" );

                Debug.Assert( m.Success, "Recent file failed to match regex" );

                FileInfo file = new FileInfo( m.Groups[1].Value );

                DateTime accessTime = DateTime.ParseExact( m.Groups[2].Value, "yyyyMMddHHmmss", CultureInfo.CurrentCulture );

                var recentFile = new RecentFile( file, accessTime );
                _recentFiles.Add( recentFile );
            }
        }

        private void SaveRecentFiles()
        {
            StringCollection coll = new StringCollection();

            foreach( var f in _recentFiles )
            {
                string dateString = f.AccessTime.ToString( "yyyyMMddHHmmss" );

                string serializedString = String.Format( "{0},{1}", f.File.FullName, dateString );
                coll.Add( serializedString );
            }

            Properties.Settings.Default.RecentFiles = coll;

            Properties.Settings.Default.Save();

        }

        private void SaveOpenedFileAsRecentFile()
        {
            if( OpenedFilePath == null ) return;

            FileInfo f = new FileInfo( OpenedFilePath );
            DateTime d = DateTime.Now;
            RemoveFileFromRecentFiles( f.FullName );

            _recentFiles.Add( new RecentFile( f, d ) );

            TrimRecentFiles();
            SaveRecentFiles();
        }

        private void TrimRecentFiles()
        {
            while( _recentFiles.Count > 5 )
            {
                var lastFile = _recentFiles.Last();

                _recentFiles.Remove( lastFile );
            }
        }

        private void RemoveFileFromRecentFiles( string fileFullName )
        {
            var existingFiles = _recentFiles.Where( x => x.File.FullName == fileFullName ).ToList();
            foreach( var file in existingFiles )
            {
                _recentFiles.Remove( file );
            }
        }

        #endregion Private methods
    }

    /// <summary>
    /// Recent file. Used in ribbon backstage.
    /// </summary>
    [Serializable]
    public class RecentFile
    {
        readonly DateTime _accessTime;
        readonly FileInfo _file;

        /// <summary>
        /// Instanciates a new recent file.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="accessTime"></param>
        public RecentFile( FileInfo info, DateTime accessTime )
        {
            _file = info;
            _accessTime = accessTime;
        }

        /// <summary>
        /// File name.
        /// </summary>
        public string FileName
        {
            get
            {
                return _file.Name;
            }
        }

        /// <summary>
        /// Directory.
        /// </summary>
        public string Directory
        {
            get
            {
                return _file.DirectoryName;
            }
        }

        /// <summary>
        /// Access time
        /// </summary>
        public DateTime AccessTime
        {
            get
            {
                return _accessTime;
            }
        }

        /// <summary>
        /// File info
        /// </summary>
        public FileInfo File
        {
            get
            {
                return _file;
            }
        }
    }
}
