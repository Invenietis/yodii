using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GraphX;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// Vertex from a Yodii graph. Represents either a lab service or a lab plugin.
    /// </summary>
    [DebuggerDisplay( "{Description}" )]
    public class YodiiGraphVertex : VertexBase, INotifyPropertyChanged
    {
        #region Fields
        readonly bool _isPlugin;
        readonly ILiveServiceInfo _liveService;
        readonly ILivePluginInfo _livePlugin;
        readonly YodiiGraph _parentGraph;
        readonly ICommand _startItemCommand;
        readonly ICommand _stopItemCommand;
        readonly ILiveYodiiItem _liveItem;

        bool _isSelected = false;
        ConfigurationStatus _configStatus;
        bool _hasConfiguration;
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

            StartDependencyImpact impact = StartDependencyImpact.Unknown;
            if( obj != null && obj is StartDependencyImpact ) impact = (StartDependencyImpact)obj;

            return LiveObject.Capability.CanStartWith( impact );
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
                StartDependencyImpact impact = StartDependencyImpact.Unknown;
                if( obj != null && obj is StartDependencyImpact ) impact = (StartDependencyImpact)obj;

                LiveObject.Start( impact );
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
        internal YodiiGraphVertex( YodiiGraph parentGraph, ILivePluginInfo plugin )
            : this()
        {
            Debug.Assert( parentGraph != null );
            Debug.Assert( plugin != null );

            _isPlugin = true;
            _livePlugin = plugin;
            _liveItem = plugin;
            _parentGraph = parentGraph;
        }

        /// <summary>
        /// Creates a new service vertex.
        /// </summary>
        /// <param name="parentGraph"></param>
        /// <param name="service"></param>
        internal YodiiGraphVertex( YodiiGraph parentGraph, ILiveServiceInfo service )
            : this()
        {
            Debug.Assert( parentGraph != null );
            Debug.Assert( service != null );

            _isPlugin = false;
            _liveService = service;
            _liveItem = service;
            _parentGraph = parentGraph;
        }

        #endregion Constructors

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
                if( IsService ) return String.Format( "Service vertex: {0}", LiveServiceInfo.ServiceInfo.ServiceFullName );
                else return String.Format( "Plugin vertex: {0}", LivePluginInfo.PluginInfo.PluginFullName );
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
        }

        public ILiveServiceInfo LiveServiceInfo
        {
            get
            {
                if( IsService ) return _liveService;
                return null;
            }
        }

        public ILivePluginInfo LivePluginInfo
        {
            get
            {
                if( IsPlugin ) return _livePlugin;
                return null;
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
                    return LiveServiceInfo.ServiceInfo.ServiceFullName;
                else
                    return LivePluginInfo.PluginInfo.PluginFullName;
            }
        }

        /// <summary>
        /// Whether this vertex's object has a live configuration.
        /// </summary>
        public RunningStatus LiveStatus
        {
            get
            {
                return LiveObject.RunningStatus;
            }
        }

        /// <summary>
        /// Whether this vertex's object is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return LiveObject.IsRunning;
            }
        }

        #endregion Properties

        internal void RaiseStatusChange()
        {
            RaisePropertyChanged( "ConfigurationStatus" );
            RaisePropertyChanged( "HasConfiguration" );
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
