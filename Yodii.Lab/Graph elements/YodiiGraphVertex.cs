using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GraphX;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Vertex from a Yodii graph. Represents either a lab service or a lab plugin.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    public class YodiiGraphVertex : VertexBase, INotifyPropertyChanged
    {
        #region Fields
        readonly bool _isPlugin;
        readonly LabServiceInfo _liveService;
        readonly LabPluginInfo _livePlugin;
        readonly YodiiGraph _parentGraph;
        readonly ICommand _startItemCommand;
        readonly ICommand _stopItemCommand;

        bool _isSelected = false;
        ConfigurationStatus _configStatus;
        bool _hasConfiguration;
        ILiveYodiiItem _liveItem;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new, empty vertex. Used by GraphX serialization, not implemented yet.
        /// </summary>
        private YodiiGraphVertex()
        {
            _startItemCommand = new RelayCommand( StartLiveItemExecute, CanStartLiveItem );
            _stopItemCommand = new RelayCommand( StopLiveItemExecute, CanStopLiveItem );
        }

        private bool CanStartLiveItem( object obj )
        {
            if( LiveObject == null ) return false;
            if( LiveObject.RunningStatus == RunningStatus.Disabled || LiveObject.RunningStatus == RunningStatus.RunningLocked || LiveObject.RunningStatus == RunningStatus.Running ) return false;

            return true;
        }

        private bool CanStopLiveItem( object obj )
        {
            if( LiveObject == null ) return false;
            if( LiveObject.RunningStatus == RunningStatus.Disabled || LiveObject.RunningStatus == RunningStatus.RunningLocked || LiveObject.RunningStatus == RunningStatus.Stopped ) return false;

            return true;
        }

        private void StartLiveItemExecute( object obj )
        {
            if( !CanStartLiveItem( obj ) ) return;

            if( LiveObject.RunningStatus == RunningStatus.Stopped )
            {
                LiveObject.Start();
            }
        }

        private void StopLiveItemExecute( object obj )
        {
            if( !CanStopLiveItem( obj ) ) return;

            if( LiveObject.RunningStatus == RunningStatus.Running )
            {
                LiveObject.Stop();
            }
        }
        /// <summary>
        /// Creates a new plugin vertex.
        /// </summary>
        /// <param name="parentGraph"></param>
        /// <param name="plugin"></param>
        internal YodiiGraphVertex( YodiiGraph parentGraph, LabPluginInfo plugin )
            : this()
        {
            Debug.Assert( parentGraph != null );
            Debug.Assert( plugin != null );

            _isPlugin = true;
            _livePlugin = plugin;
            _parentGraph = parentGraph;

            _livePlugin.PluginInfo.PropertyChanged += StaticInfo_PropertyChanged;
            _livePlugin.PropertyChanged += _labPlugin_PropertyChanged;
        }

        /// <summary>
        /// Creates a new service vertex.
        /// </summary>
        /// <param name="parentGraph"></param>
        /// <param name="service"></param>
        internal YodiiGraphVertex( YodiiGraph parentGraph, LabServiceInfo service )
            : this()
        {
            Debug.Assert( parentGraph != null );
            Debug.Assert( service != null );

            _isPlugin = false;
            _liveService = service;
            _parentGraph = parentGraph;

            _liveService.ServiceInfo.PropertyChanged += StaticInfo_PropertyChanged;
            _liveService.PropertyChanged += _labService_PropertyChanged;
        }

        #endregion Constructors

        void _labPlugin_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            switch( e.PropertyName )
            {
                case "LivePluginInfo":
                    LiveObject = _livePlugin.LivePluginInfo;
                    RaisePropertyChanged( "IsLive" );
                    RaisePropertyChanged( "IsRunning" );
                    RaisePropertyChanged( "IsEditable" );
                    break;

            }
        }

        void _labService_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            switch( e.PropertyName )
            {
                case "LiveServiceInfo":
                    LiveObject = _liveService.LiveServiceInfo;
                    RaisePropertyChanged( "IsLive" );
                    RaisePropertyChanged( "IsRunning" );
                    RaisePropertyChanged( "IsEditable" );
                    break;

            }
        }

        void StaticInfo_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            RaisePropertyChanged( "Title" );
        }

        #region Properties

        /// <summary>
        /// True if the element represented by this vertex is a plugin.
        /// </summary>
        /// <remarks>
        /// LabPluginInfo contains something in this case.
        /// </remarks>
        public bool IsPlugin { get { return _isPlugin; } }

        /// <summary>
        /// True if the element represented by this vertex is a service.
        /// </summary>
        /// <remarks>
        /// LabServiceInfo contains something in this case.
        /// </remarks>
        public bool IsService { get { return !_isPlugin; } }

        /// <summary>
        /// Whether this vertex is currently selected by the user.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            internal set
            {
                if( value != _isSelected )
                {
                    _isSelected = value;
                    RaisePropertyChanged( "IsSelected" );
                }
            }
        }

        /// <summary>
        /// Whether the Configuration contains a configuration for this element.
        /// </summary>
        public bool HasConfiguration
        {
            get { return _hasConfiguration; }
            internal set
            {
                if( value != _hasConfiguration )
                {
                    _hasConfiguration = value;
                    RaisePropertyChanged( "HasConfiguration" );
                }
            }
        }

        /// <summary>
        /// Vertex description.
        /// </summary>
        public string Description
        {
            get
            {
                if( IsService ) return String.Format("Service vertex: {0}", LabServiceInfo.ServiceInfo.ServiceFullName);
                else return String.Format( "Plugin vertex: {0}", LabPluginInfo.PluginInfo.PluginFullName );
            }
        }

        /// <summary>
        /// Command to toggle stop this live item.
        /// </summary>
        public ICommand StopItemCommand { get { return _stopItemCommand; } }

        /// <summary>
        /// Command to start this live item.
        /// </summary>
        public ICommand StartItemCommand { get { return _startItemCommand; } }

        /// <summary>
        /// The configuration status for this element, from the ConfigurationManager.
        /// </summary>
        public ConfigurationStatus ConfigurationStatus
        {
            get { return _configStatus; }
            internal set
            {
                if( value != _configStatus )
                {
                    _configStatus = value;
                    RaisePropertyChanged( "ConfigurationStatus" );
                }
            }
        }

        /// <summary>
        /// Live object for this element, either ILivePluginInfo or ILiveServiceInfo.
        /// </summary>
        public ILiveYodiiItem LiveObject
        {
            get { return _liveItem; }
            internal set
            {
                if( value != _liveItem )
                {
                    _liveItem = value;
                    RaisePropertyChanged( "LiveObject" );
                }
            }
        }

        /// <summary>
        /// Title of this vertex.
        /// </summary>
        public string Title
        {
            get
            {
                if( IsService )
                    return LabServiceInfo.ServiceInfo.ServiceFullName;
                else
                    return LabPluginInfo.PluginInfo.Description;
            }
        }

        /// <summary>
        /// Whether this vertex's object has a live configuration.
        /// </summary>
        public bool IsLive
        {
            get
            {
                if( IsService )
                {
                    return LabServiceInfo.IsLive;
                }
                else
                {
                    return LabPluginInfo.IsLive;
                }
            }
        }

        /// <summary>
        /// Whether this vertex's object can be edited or deleted.
        /// </summary>
        public bool IsEditable
        {
            get
            {
                if( IsService )
                {
                    return !LabServiceInfo.IsLive;
                }
                else
                {
                    return !LabPluginInfo.IsLive;
                }
            }
        }

        /// <summary>
        /// Whether this vertex's object has a live configuration.
        /// </summary>
        public RunningStatus LiveStatus
        {
            get
            {
                if( IsService && LabServiceInfo.IsLive )
                {
                    return LabServiceInfo.LiveServiceInfo.RunningStatus;
                }
                else if( IsPlugin && LabPluginInfo.IsLive )
                {
                    return LabPluginInfo.LivePluginInfo.RunningStatus;
                }
                else
                {
                    return RunningStatus.Disabled;
                }
            }
        }

        /// <summary>
        /// Whether this vertex's object is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                if( IsService && LabServiceInfo.IsLive )
                {
                    return LabServiceInfo.LiveServiceInfo.RunningStatus == RunningStatus.Running || LabServiceInfo.LiveServiceInfo.RunningStatus == RunningStatus.RunningLocked;
                }
                else if( IsPlugin && LabPluginInfo.IsLive )
                {
                    return LabServiceInfo.LiveServiceInfo.RunningStatus == RunningStatus.Running || LabServiceInfo.LiveServiceInfo.RunningStatus == RunningStatus.RunningLocked;
                }

                return false;
            }
        }

        /// <summary>
        /// LabServiceInfo attached to this vertex, if it's representing a service.
        /// </summary>
        public LabServiceInfo LabServiceInfo { get { return _liveService; } }

        /// <summary>
        /// LabPluginInfo attached to this vertex, if it's representing a plugin.
        /// </summary>
        public LabPluginInfo LabPluginInfo { get { return _livePlugin; } }

        #endregion Properties

        internal void RaiseStatusChange()
        {
            RaisePropertyChanged( "ConfigurationStatus" );
            RaisePropertyChanged( "HasConfiguration" );
        }

        internal void RemoveSelf()
        {
            if( IsService )
            {
                _parentGraph.RemoveService( LabServiceInfo.ServiceInfo );
            }
            else if( IsPlugin )
            {
                _parentGraph.RemovePlugin( LabPluginInfo.PluginInfo );
            }
        }

        #region INotifyPropertyChanged utilities

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Throw new PropertyChanged.
        /// </summary>
        /// <param name="caller">Fill with Member name, when called from a property.</param>
        protected void RaisePropertyChanged( [CallerMemberName] string caller = null )
        {
            Debug.Assert( caller != null );
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        #endregion INotifyPropertyChanged utilities
    }
}
