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
        ILiveServiceInfo _generalization;
        ILivePluginInfo _runningPlugin;
        ILivePluginInfo _lastRunningPlugin;
        IServiceInfo _serviceInfo;
        IReadOnlyList<IDynamicSolvedPlugin> _dynamicPlugins;
        IReadOnlyList<IDynamicSolvedService> _dynamicServices;

        LiveServiceInfo(IDynamicSolvedService dynamicService, ILiveServiceInfo generalization)
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
            get { throw new NotImplementedException(); }
        }

        public ILivePluginInfo RunningPlugin
        {
            get { throw new NotImplementedException(); }
        }

        public ILivePluginInfo LastRunningPlugin
        {
            get { throw new NotImplementedException(); }
        }

        public bool Start( object caller )
        {
            throw new NotImplementedException();
        }

        public void Stop( object caller )
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IDynamicSolvedPlugin> Plugins
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IDynamicSolvedService> Services
        {
            get { throw new NotImplementedException(); }
        }

        public IDynamicSolvedService FindService( string fullName )
        {
            throw new NotImplementedException();
        }

        public IDynamicSolvedPlugin FindPlugin( Guid pluginId )
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
