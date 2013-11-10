using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class LiveServiceInfo : ILiveServiceInfo
    {
        readonly IServiceInfo _serviceInfo;
        readonly RunningRequirement _configRequirement;
        readonly RunningStatus _status;
        readonly bool _isRunning;
        readonly ILiveServiceInfo _generalization;
        readonly ILivePluginInfo _runningPlugin;
        readonly string _serviceFullName;

        internal LiveServiceInfo(IServiceInfo service)
        {
            _serviceInfo = service;
            _serviceFullName = _serviceInfo.ServiceFullName;
        }

        public IServiceInfo ServiceInfo
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
            get { return _isRunning; }
        }

        public ILiveServiceInfo Generalization
        {
            get { return _generalization; }
        }

        public ILivePluginInfo RunningPlugin
        {
            get { return _runningPlugin; }
        }

        public bool Start()
        {
            if ( _status == RunningStatus.Disabled || _status == RunningStatus.RunningLocked ) throw new InvalidOperationException();            
            YodiiCommandService command = new YodiiCommandService( _serviceFullName, true );
            return true;
        }

        public void Stop()
        {
            if ( _status == RunningStatus.Disabled || _status == RunningStatus.RunningLocked ) throw new InvalidOperationException();            
            YodiiCommandService command = new YodiiCommandService( _serviceFullName, false );
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
