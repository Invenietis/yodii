using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CK.Core;

using Yodii.Model;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// View model for the main window.
    /// </summary>
    public class ObjectExplorerWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Fired when a new notification is requested, to display data the user should see.
        /// </summary>
        internal event EventHandler<NotificationEventArgs> NewNotification;

        internal event EventHandler<VertexPositionEventArgs> VertexPositionRequest;

        internal event EventHandler AutoPositionRequest;

        #region Fields

        /// <summary>
        /// Image URI for Plugin Running notifications.
        /// </summary>
        public static readonly string RUNNING_NOTIFICATION_IMAGE_URI = @"/Yodii.ObjectExplorer.Wpf;component/Assets/Icons/RunningStatusRunning.png";

        /// <summary>
        /// Image URI for Plugin Stopped notifications.
        /// </summary>
        public static readonly string STOPPED_NOTIFICATION_IMAGE_URI = @"/Yodii.ObjectExplorer.Wpf;component/Assets/Icons/RunningStatusStopped.png";

        readonly YodiiGraph _graph;

        readonly ICommand _reorderGraphLayoutCommand;
        readonly ICommand _autoPositionCommand;
        //readonly ICommand _revokeLastCommand;

        readonly ActivityMonitor _activityMonitor;

        readonly IYodiiEngine _engine; // Loaded from LabStateManager.
        YodiiGraphVertex _selectedVertex;

        bool _hideNotifications = false;

        bool _changedSinceLastSave;
        string _lastSavePath;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of this ViewModel.
        /// </summary>
        /// <param name="loadDefaultState">True if the default XML state should be loaded, false to start on an empty state.</param>
        public ObjectExplorerWindowViewModel( IYodiiEngine engine )
        {
            _engine = engine;

            _activityMonitor = new ActivityMonitor();

            _activityMonitor.OpenTrace().Send( "Hello world" );

            _graph = new YodiiGraph( _engine );

            _reorderGraphLayoutCommand = new RelayCommand( ReorderGraphLayoutExecute );
            _autoPositionCommand = new RelayCommand( AutoPositionExecute );
        }

        #endregion Constructor


        #region Command handlers

        private void ReorderGraphLayoutExecute( object param )
        {

            RaiseNewNotification( new Notification() { Title = "Calling relayout..." } );
            // Refresh layout.
            Graph.RaiseGraphUpdateRequested();
        }

        private void AutoPositionExecute( object obj )
        {
            if( AutoPositionRequest != null )
            {
                AutoPositionRequest( this, new EventArgs() );
            }
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

        public IYodiiEngine Engine
        {
            get { return _engine; }
        }

        public IEnumerable<IAssemblyInfo> YodiiAssemblies
        {
            get { return _engine.DiscoveredInfo.AssemblyInfos.Where( a => a.Plugins.Count > 0 || a.Services.Count > 0 ); }
        }

        public IObservableReadOnlyList<ILiveServiceInfo> ServiceInfos
        {
            get { return _engine.LiveInfo.Services; }
        }

        /// <summary>
        /// Plugins created in this Lab.
        /// </summary>
        public IObservableReadOnlyList<ILivePluginInfo> PluginInfos
        {
            get { return _engine.LiveInfo.Plugins; }
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
        /// Command to reorder the graph, or to change the graph's layout.
        /// </summary>
        public ICommand ReorderGraphLayoutCommand { get { return _reorderGraphLayoutCommand; } }
        /// <summary>
        /// Command to auto-position all elements.
        /// </summary>
        public ICommand AutoPositionCommand { get { return _autoPositionCommand; } }
        #endregion Properties

        #region Public methods

        /// <summary>
        /// Gets a service from the state manager with its name.
        /// </summary>
        /// <param name="name">Service name</param>
        /// <returns>Service</returns>
        public ILiveServiceInfo GetServiceInfoByName( string name )
        {
            return ServiceInfos.Where( x => x.ServiceInfo.ServiceFullName == name ).SingleOrDefault();
        }

        /// <summary>
        /// Get plugins with names matching this one.
        /// </summary>
        /// <param name="name">Plugin name.</param>
        /// <returns>Plugins matching name.</returns>
        public IEnumerable<ILivePluginInfo> GetPluginInfosByName( string name )
        {
            return PluginInfos.Where( x => x.PluginInfo.PluginFullName == name );
        }

        /// <summary>
        /// Get plugin associated to this plugin full name.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name.</param>
        /// <returns>Plugin.</returns>
        public ILivePluginInfo GetPluginInfoById( string pluginFullName )
        {
            return PluginInfos.Where( x => x.PluginInfo.PluginFullName == pluginFullName ).SingleOrDefault();
        }

        /// <summary>
        /// Sets service as selected.
        /// </summary>
        /// <param name="serviceInfo">Service to select</param>
        public void SelectService( ILiveServiceInfo serviceInfo )
        {
            YodiiGraphVertex vertexToSelect = Graph.Vertices.Where( x => x.IsService && x.LiveServiceInfo == serviceInfo ).SingleOrDefault();
            SelectedVertex = vertexToSelect;
        }

        /// <summary>
        /// Sets plugin as selected.
        /// </summary>
        /// <param name="pluginInfo">Plugin to select</param>
        public void SelectPlugin( ILivePluginInfo pluginInfo )
        {
            YodiiGraphVertex vertexToSelect = Graph.Vertices.Where( x => x.IsPlugin && x.LivePluginInfo == pluginInfo ).SingleOrDefault();
            SelectedVertex = vertexToSelect;
        }
        #endregion Public methods

        #region Private methods

        private void RaiseNewNotification( Notification n )
        {
            if( _hideNotifications ) return;
            var h = NewNotification;
            if( h != null ) h( this, new NotificationEventArgs( n ) );
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

        private IDictionary<YodiiGraphVertex, Point> RaisePositionRequest()
        {
            var v = VertexPositionRequest;
            VertexPositionEventArgs args = new VertexPositionEventArgs();
            if( v != null )
            {
                v( this, args );
            }

            return args.VertexPositions;
        }

        #endregion Private methods
    }

    class VertexPositionEventArgs : EventArgs
    {
        public IDictionary<YodiiGraphVertex, Point> VertexPositions
        {
            get;
            set;
        }
    }

}
