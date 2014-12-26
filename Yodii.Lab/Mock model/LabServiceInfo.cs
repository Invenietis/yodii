using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    /// <summary>
    /// Lab service. Wrapper class around a mock ServiceInfo, binding a LiveServiceInfo when the engine is started.
    /// </summary>
    [DebuggerDisplay( "Lab {ServiceInfo.ServiceFullName}" )]
    public class LabServiceInfo : ViewModelBase
    {
        #region Fields

        readonly IYodiiEngine _engine;
        readonly ServiceInfo _serviceInfo;
        ILiveServiceInfo _liveServiceInfo;

        #endregion Fields

        internal LabServiceInfo( IYodiiEngine engine, ServiceInfo serviceInfo )
        {
            Debug.Assert( serviceInfo != null );

            _engine = engine;
            _serviceInfo = serviceInfo;

            StartServiceCommand = new RelayCommand( ExecuteStartService, CanExecuteStartService );
            StopServiceCommand = new RelayCommand( ExecuteStopService, CanExecuteStopService );
        }

        private bool CanExecuteStopService( object obj )
        {
            return LiveServiceInfo != null && LiveServiceInfo.RunningStatus == RunningStatus.Running && LiveServiceInfo.Capability.CanStop;
        }

        private void ExecuteStopService( object obj )
        {
            if( !CanExecuteStopService( null ) ) return;
            var result = _engine.Stop( LiveServiceInfo );
            if( !result.Success )
            {
                MessageBox.Show( result.Describe() );
            }
        }

        private bool CanExecuteStartService( object obj )
        {
            StartDependencyImpact impact = StartDependencyImpact.Unknown;
            if( obj != null && obj is StartDependencyImpact ) impact = (StartDependencyImpact)obj;
            return LiveServiceInfo != null && LiveServiceInfo.Capability.CanStartWith( impact );
        }

        private void ExecuteStartService( object obj )
        {
            if( !CanExecuteStartService( obj ) ) return;
            StartDependencyImpact impact = StartDependencyImpact.Unknown;
            if( obj != null && obj is StartDependencyImpact ) impact = (StartDependencyImpact)obj;
            var result = _engine.Start( LiveServiceInfo, impact );
            if( !result.Success )
            {
                MessageBox.Show( result.Describe() );
            }
        }

        #region Properties

        /// <summary>
        /// Command to start the service.
        /// </summary>
        public ICommand StartServiceCommand { get; private set; }

        /// <summary>
        /// Command to stop the service.
        /// </summary>
        public ICommand StopServiceCommand { get; private set; }

        /// <summary>
        /// Attached service info. Read-only.
        /// </summary>
        public ServiceInfo ServiceInfo
        {
            get { return _serviceInfo; }
        }

        /// <summary>
        /// Active LiveServiceInfo attached to this lab.
        /// Null if the lab is in building mode, when the engine hasn't started.
        /// </summary>
        public ILiveServiceInfo LiveServiceInfo
        {
            get { return _liveServiceInfo; }
            internal set
            {
                Debug.Assert( value == null || value.ServiceInfo == ServiceInfo );
                _liveServiceInfo = value;
                RaisePropertyChanged();
                RaisePropertyChanged( "IsLive" );
            }
        }

        /// <summary>
        /// True if the lab is in simulation mode, and this LabServiceInfo has a LiveServiceInfo.
        /// False if the lab is in building mode.
        /// </summary>
        public bool IsLive
        {
            get
            {
                return LiveServiceInfo != null;
            }
        }

        #endregion Properties
    }
}
