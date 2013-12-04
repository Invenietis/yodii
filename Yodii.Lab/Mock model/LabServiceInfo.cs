using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    [DebuggerDisplay( "Lab {ServiceInfo.ServiceFullName}" )]
    public class LabServiceInfo : ViewModelBase
    {
        #region Fields

        ServiceInfo _serviceInfo;
        //LabServiceInfo _generalization;

        ILiveServiceInfo _liveServiceInfo;

        #endregion Fields

        #region Constructor
        internal LabServiceInfo( ServiceInfo serviceInfo)
        {
            Debug.Assert( serviceInfo != null );

            _serviceInfo = serviceInfo;
            //_generalization = generalization;
        }
        #endregion

        #region Properties
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
                if( value != null )
                {
                    Debug.Assert( value.ServiceInfo == ServiceInfo );

                    _liveServiceInfo = value;
                }

                RaisePropertyChanged();
                RaisePropertyChanged("IsLive");
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
