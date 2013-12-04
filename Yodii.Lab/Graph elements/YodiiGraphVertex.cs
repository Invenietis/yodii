using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GraphX;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    public class YodiiGraphVertex : VertexBase, INotifyPropertyChanged
    {
        #region Fields
        readonly bool _isPlugin;
        readonly LabServiceInfo _liveService;
        readonly LabPluginInfo _livePlugin;
        readonly YodiiGraph _parentGraph;

        bool _isSelected = false;
        ConfigurationStatus _configStatus;
        bool _hasConfiguration;
        #endregion

        #region Constructors
        public YodiiGraphVertex()
        {

        }
        /// <summary>
        /// Creates a new plugin vertex.
        /// </summary>
        /// <param name="parentGraph"></param>
        /// <param name="plugin"></param>
        internal YodiiGraphVertex( YodiiGraph parentGraph, LabPluginInfo plugin )
        {
            Debug.Assert( parentGraph != null );
            Debug.Assert( plugin != null );

            _isPlugin = true;
            _livePlugin = plugin;
            _parentGraph = parentGraph;

            _livePlugin.PluginInfo.PropertyChanged += Info_PropertyChanged;
        }

        /// <summary>
        /// Creates a new service vertex.
        /// </summary>
        /// <param name="parentGraph"></param>
        /// <param name="service"></param>
        internal YodiiGraphVertex( YodiiGraph parentGraph, LabServiceInfo service )
        {
            Debug.Assert( parentGraph != null );
            Debug.Assert( service != null );

            _isPlugin = false;
            _liveService = service;
            _parentGraph = parentGraph;

            _liveService.ServiceInfo.PropertyChanged += Info_PropertyChanged;
        }
        #endregion Constructors

        #region Properties
        public bool IsPlugin { get { return _isPlugin; } }
        public bool IsService { get { return !_isPlugin; } }

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

        // Global view properties
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
        public LabServiceInfo LabServiceInfo { get { return _liveService; } }
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
            } else if (IsPlugin)
            {
                _parentGraph.RemovePlugin( LabPluginInfo.PluginInfo );
            }
        }

        void Info_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            RaisePropertyChanged( "Title" );
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
