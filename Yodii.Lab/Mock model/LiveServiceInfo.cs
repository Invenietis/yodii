using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    [DebuggerDisplay( "Live {ServiceInfo.ServiceFullName}" )]
    public class LiveServiceInfo : ViewModelBase, ILiveServiceInfo
    {
        #region Fields

        ServiceInfo _serviceInfo;
        DependencyRequirement _configRequirement;
        RunningStatus _status;
        LiveServiceInfo _generalization;
        LivePluginInfo _runningPlugin;
        bool _isRunning;
        LivePluginInfo _lastRunningPlugin;
        ServiceDisabledReason _disabledReason;
        ConfigurationStatus _configOriginalStatus;
        SolvedConfigurationStatus _configSolvedStatus;

        #endregion Fields

        #region Constructor
        internal LiveServiceInfo( ServiceInfo serviceInfo, DependencyRequirement configRequirement = DependencyRequirement.Optional, LiveServiceInfo generalization = null)
        {
            Debug.Assert( serviceInfo != null );

            _serviceInfo = serviceInfo;
            _configRequirement = configRequirement;
            _status = RunningStatus.Stopped; // TODO: Status changes
            _generalization = generalization;
        }
        #endregion

        #region Properties
        public ServiceInfo ServiceInfo
        {
            get { return _serviceInfo; }
        }

        public DependencyRequirement ConfigRequirement
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

        public LiveServiceInfo Generalization
        {
            get { return _generalization; }
            set
            {
                if( value != _generalization )
                {
                    _generalization = value;
                    RaisePropertyChanged( "Generalization" );
                }
            }
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
        public bool Start( string callerKey )
        {
            throw new NotImplementedException();
        }

        public void Stop( string callerKey )
        {
            throw new NotImplementedException();
        }
        #endregion Public methods


        #region ILiveServiceInfo Members

        ILiveServiceInfo ILiveServiceInfo.Generalization
        {
            get { return _generalization; }
        }

        ILivePluginInfo ILiveServiceInfo.RunningPlugin
        {
            get { return _runningPlugin; }
        }

        bool ILiveServiceInfo.IsRunning
        {
            get { return _isRunning; }
        }

        ILivePluginInfo ILiveServiceInfo.LastRunningPlugin
        {
            get { return _lastRunningPlugin; }
        }

        bool ILiveServiceInfo.Start( object caller )
        {
            throw new NotImplementedException();
        }

        void ILiveServiceInfo.Stop( object caller )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDynamicSolvedService Members

        IServiceInfo IDynamicSolvedService.ServiceInfo
        {
            get { return _serviceInfo; }
        }

        ServiceDisabledReason IDynamicSolvedService.DisabledReason
        {
            get { return _disabledReason; }
        }

        ConfigurationStatus IDynamicSolvedService.ConfigOriginalStatus
        {
            get { return _configOriginalStatus; }
        }

        SolvedConfigurationStatus IDynamicSolvedService.ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        RunningStatus IDynamicSolvedService.RunningStatus
        {
            get { return _status; }
        }

        #endregion
    }
}
