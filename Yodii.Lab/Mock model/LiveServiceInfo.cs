using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    public class LiveServiceInfo : ViewModelBase, ILiveServiceInfo
    {
        #region Fields
        private ServiceInfo _serviceInfo;
        private RunningRequirement _configRequirement;
        private RunningStatus _status;
        private LiveServiceInfo _generalization;
        private LivePluginInfo _runningPlugin;
        #endregion Fields

        #region Constructor
        internal LiveServiceInfo( ServiceInfo serviceInfo, RunningRequirement configRequirement = RunningRequirement.Optional, LiveServiceInfo generalization = null)
        {
            Debug.Assert( serviceInfo != null );

            _serviceInfo = serviceInfo;
            _configRequirement = configRequirement;
            _status = RunningStatus.Stopped; // TODO: Status changes
            _generalization = generalization;
        }
        #endregion

        #region Properties
        IServiceInfo ILiveServiceInfo.ServiceInfo
        {
            get { return _serviceInfo; }
        }

        public ServiceInfo ServiceInfo
        {
            get { return _serviceInfo; }
        }

        public RunningRequirement ConfigRequirement
        {
            get { return _configRequirement; }
        }

        public RunningStatus Status
        {
            get { return _status; }
        }

        public bool IsRunning
        {
            get { return _status == RunningStatus.Running || _status == RunningStatus.RunningLocked; }
        }

        ILiveServiceInfo ILiveServiceInfo.Generalization
        {
            get { return _generalization; }
        }

        public LiveServiceInfo Generalization
        {
            get { return _generalization; }
        }

        ILivePluginInfo ILiveServiceInfo.RunningPlugin
        {
            get { return _runningPlugin; }
        }

        public LivePluginInfo RunningPlugin
        {
            get { return _runningPlugin; }
            set
            {
                if( value != _runningPlugin )
                {
                    _runningPlugin = value;
                    RaisePropertyChanged( "RunningPlugin" );
                }
            }
        }
        #endregion Properties

        #region Public methods
        bool ILiveServiceInfo.Start()
        {
            throw new NotImplementedException();
        }

        void ILiveServiceInfo.Stop()
        {
            throw new NotImplementedException();
        }
        #endregion Public methods
    }
}
