using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class LivePluginInfo : ILivePluginInfo
    {
        readonly ILiveServiceInfo _service;
        Exception _currentError;
        readonly IPluginInfo _pluginInfo;
        RunningStatus _runningStatus;
        PluginDisabledReason _disabledReason;
        readonly ConfigurationStatus _configOriginalStatus;
        SolvedConfigurationStatus _configSolvedStatus;

        LivePluginInfo( IDynamicSolvedPlugin dynamicPlugin, ILiveServiceInfo liveService )
        {
            _pluginInfo = dynamicPlugin.PluginInfo;
            _service = liveService;
            _runningStatus = dynamicPlugin.RunningStatus;
            _disabledReason = dynamicPlugin.DisabledReason;
            _configOriginalStatus = dynamicPlugin.ConfigOriginalStatus;
            _configSolvedStatus = dynamicPlugin.ConfigSolvedStatus;
        }
        public bool IsRunning
        {
            get { return _runningStatus == RunningStatus.Running || _runningStatus == RunningStatus.RunningLocked; }
        }

        public ILiveServiceInfo Service
        {
            get { return _service; }
        }

        public bool Start( object caller, StartDependencyImpact impact = StartDependencyImpact.None )
        {
            YodiiCommand cmd = new YodiiCommand(caller, true, impact, _pluginInfo.PluginId);
            return true;
        }

        public void Stop( object caller, StartDependencyImpact impact = StartDependencyImpact.None )
        {
            YodiiCommand cmd = new YodiiCommand( caller, false, impact, _pluginInfo.PluginId );
        }

        public Exception CurrentError
        {
            get { return _currentError; }
        }

        public IPluginInfo PluginInfo
        {
            get { return _pluginInfo; }
        }

        public PluginDisabledReason DisabledReason
        {
            get { return _disabledReason; }
        }

        public ConfigurationStatus ConfigOriginalStatus
        {
            get { return _configOriginalStatus; }
        }

        public SolvedConfigurationStatus ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        public RunningStatus RunningStatus
        {
            get { return _runningStatus; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
