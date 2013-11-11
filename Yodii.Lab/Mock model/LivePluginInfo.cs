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
        private RunningRequirement _configRequirement;
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

        internal LivePluginInfo( PluginInfo pluginInfo, RunningRequirement configRequirement = RunningRequirement.Optional, LiveServiceInfo liveServiceInfo = null )
        {
            Debug.Assert( pluginInfo != null );

            _pluginInfo = pluginInfo;
            _service = liveServiceInfo;

            _configRequirement = configRequirement;
            _status = RunningStatus.Stopped; 
        }

        #region ILivePluginInfo Members
        IPluginInfo ILivePluginInfo.PluginInfo
        {
            get { return _pluginInfo; }
        }

        RunningRequirement ILivePluginInfo.ConfigRequirement
        {
            get { return _configRequirement; }
        }

        RunningStatus ILivePluginInfo.Status
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

        bool ILivePluginInfo.Start()
        {
            throw new NotImplementedException();
        }

        void ILivePluginInfo.Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
