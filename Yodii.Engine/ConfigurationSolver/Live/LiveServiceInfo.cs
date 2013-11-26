using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class LiveServiceInfo : ILiveServiceInfo
    {
        RunningStatus _runningStatus;
        readonly ILiveServiceInfo _generalization;
        readonly ILivePluginInfo _runningPlugin;
        ILivePluginInfo _lastRunningPlugin;
        readonly IServiceInfo _serviceInfo;
        IReadOnlyList<IDynamicSolvedPlugin> _dynamicPlugins;
        IReadOnlyList<IDynamicSolvedService> _dynamicServices;

        internal LiveServiceInfo(IDynamicSolvedService dynamicService, ILiveServiceInfo generalization)
        {
            _runningStatus = dynamicService.RunningStatus;
            _generalization = generalization;
            _serviceInfo = dynamicService.ServiceInfo;
        }
        public bool IsRunning
        {
            get { return _runningStatus == RunningStatus.Running || _runningStatus == RunningStatus.RunningLocked; }
        }

        public ILiveServiceInfo Generalization
        {
            get { return _generalization; }
        }

        public ILivePluginInfo RunningPlugin
        {
            get { return _runningPlugin; }
        }

        public ILivePluginInfo LastRunningPlugin
        {
            get { return _lastRunningPlugin; }
            set { if ( value != _lastRunningPlugin ) _lastRunningPlugin = value; }
        }

        public bool Start( object caller )
        {
            YodiiCommand cmd = new YodiiCommand( caller, true, StartDependencyImpact.None, _serviceInfo.ServiceFullName );
            return true;
        }

        public void Stop( object caller )
        {
            YodiiCommand cmd = new YodiiCommand( caller, false, StartDependencyImpact.None, _serviceInfo.ServiceFullName );
        }

        public IReadOnlyList<IDynamicSolvedPlugin> Plugins
        {
            get { return _dynamicPlugins; }
        }

        public IReadOnlyList<IDynamicSolvedService> Services
        {
            get { return _dynamicServices; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
