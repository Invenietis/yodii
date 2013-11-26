using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    [DebuggerDisplay( "Live {PluginInfo.PluginFullName} = {PluginInfo.PluginId}" )]
    public class LivePluginInfo : ViewModelBase, ILivePluginInfo
    {
        private PluginInfo _pluginInfo;
        private DependencyRequirement _configRequirement;
        private RunningStatus _status;
        private LiveServiceInfo _service;

        #region Properties
        public PluginInfo PluginInfo
        {
            get { return _pluginInfo; }
        }

        public LiveServiceInfo Service
        {
            get { return _service; }
        }
        #endregion Properties

        internal LivePluginInfo( PluginInfo pluginInfo, DependencyRequirement configRequirement = DependencyRequirement.Optional, LiveServiceInfo liveServiceInfo = null )
        {
            Debug.Assert( pluginInfo != null );

            _pluginInfo = pluginInfo;
            _service = liveServiceInfo;

            _configRequirement = configRequirement;
            _status = RunningStatus.Stopped; 
        }

        #region ILivePluginInfo Members

        public RunningStatus Status
        {
            get { return _status; }
        }

        bool ILivePluginInfo.IsRunning
        {
            get { return _status == RunningStatus.Running || _status == RunningStatus.RunningLocked; }
        }

        ILiveServiceInfo ILivePluginInfo.Service
        {
            get { return _service; }
        }

        public bool Start( Object o )
        {
            throw new NotImplementedException();
        }

        public void Stop( Object o )
        {
            throw new NotImplementedException();
        }

        bool ILivePluginInfo.Start( object caller, StartDependencyImpact impact )
        {
            throw new NotImplementedException();
        }

        void ILivePluginInfo.Stop( object caller )
        {
            throw new NotImplementedException();
        }

        Exception ILivePluginInfo.CurrentError
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDynamicSolvedPlugin Members

        IPluginInfo IDynamicSolvedPlugin.PluginInfo
        {
            get { throw new NotImplementedException(); }
        }

        PluginDisabledReason IDynamicSolvedPlugin.DisabledReason
        {
            get { throw new NotImplementedException(); }
        }

        ConfigurationStatus IDynamicSolvedPlugin.ConfigOriginalStatus
        {
            get { throw new NotImplementedException(); }
        }

        SolvedConfigurationStatus IDynamicSolvedPlugin.ConfigSolvedStatus
        {
            get { throw new NotImplementedException(); }
        }

        RunningStatus IDynamicSolvedPlugin.RunningStatus
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
