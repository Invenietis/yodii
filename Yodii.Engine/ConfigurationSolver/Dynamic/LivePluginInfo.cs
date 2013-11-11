using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class LivePluginInfo : ILivePluginInfo
    {
        readonly IPluginInfo _pluginInfo;
        readonly RunningRequirement _configRequirement;
        readonly RunningStatus _status;
        readonly bool _isRunning;
        readonly Guid _id;
        readonly ILiveServiceInfo _service;

        internal LivePluginInfo( IPluginInfo plugin )
        {
            _id = plugin.PluginId;
        }

        public IPluginInfo PluginInfo
        {
            get { return _pluginInfo; }
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

        public ILiveServiceInfo Service
        {
            get { return _service; }
        }

        public bool Start()
        {
            if ( _status == RunningStatus.Disabled || _status == RunningStatus.RunningLocked ) throw new InvalidOperationException();            
            YodiiCommandPlugin command = new YodiiCommandPlugin( _id, true );
            return true;
        }

        public void Stop()
        {
            if ( _status == RunningStatus.Disabled || _status == RunningStatus.RunningLocked ) throw new InvalidOperationException();            
            YodiiCommandPlugin command = new YodiiCommandPlugin( _id, false );
        }
        public event PropertyChangedEventHandler PropertyChanged;    
    }
}
