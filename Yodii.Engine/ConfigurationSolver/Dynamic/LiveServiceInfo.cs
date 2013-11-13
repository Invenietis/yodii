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
        readonly RunningStatus? _status;
        readonly ILiveServiceInfo _generalization;
        readonly ILivePluginInfo _runningPlugin;
        readonly string _serviceFullName;

        internal LiveServiceInfo(IServiceSolved service)
        {
            _serviceInfo = service.ServiceInfo;
            _configRequirement = service.ConfigSolvedStatus;
            _status = service.RunningStatus;
            //_generalization = ;
            //_runningPlugin = ;
            _serviceFullName = service.ServiceInfo.ServiceFullName;
        }

        public IServiceInfo ServiceInfo
        {
            get { return _serviceInfo; }
        }

        public RunningRequirement ConfigRequirement
        {
            get { return _configRequirement; }
        }

        public RunningStatus? Status
        {
            get { return _status; }
        }

        public bool IsRunning
        {
            get { return _status == RunningStatus.RunningLocked || _status == RunningStatus.Running; }
        }

        public ILiveServiceInfo Generalization
        {
            get { return _generalization; }
        }

        public ILivePluginInfo RunningPlugin
        {
            get { return _runningPlugin; }
        }

        public bool Start( Object caller )
        {
            if ( _status == RunningStatus.Disabled || _status == RunningStatus.RunningLocked ) throw new InvalidOperationException();            
            YodiiCommandService command = new YodiiCommandService( _serviceFullName, true );
            command._caller = caller;
            return true;
        }

        public void Stop( Object caller )
        {
            if ( _status == RunningStatus.Disabled || _status == RunningStatus.RunningLocked ) throw new InvalidOperationException();            
            YodiiCommandService command = new YodiiCommandService( _serviceFullName, false );
            command._caller = caller;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        RunningStatus ILiveServiceInfo.Status
        {
            get { throw new NotImplementedException(); }
        }
    }
}
