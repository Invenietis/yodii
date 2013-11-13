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
        readonly RunningStatus? _status;
        readonly Guid _id;
        readonly ILiveServiceInfo _service;

        internal LivePluginInfo( IPluginSolved plugin, ILiveServiceInfo service )
        {
            _pluginInfo = plugin.PluginInfo;
            _configRequirement = plugin.ConfigSolvedStatus;
            _status = plugin.RunningStatus;
            _id = plugin.PluginInfo.PluginId;
            _service = service;
        }

        public IPluginInfo PluginInfo
        {
            get { return _pluginInfo; }
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

        public ILiveServiceInfo Service
        {
            get { return _service; }
        }

        public bool Start( Object caller )
        {
            if ( _status == RunningStatus.Disabled || _status == RunningStatus.RunningLocked ) throw new InvalidOperationException();
            Debug.Assert( _status == RunningStatus.Stopped );
            YodiiCommandPlugin command = new YodiiCommandPlugin( _id, true );
            command._caller = caller;
            return true;
        }

        public void Stop( Object caller )
        {
            if ( _status == RunningStatus.Disabled || _status == RunningStatus.RunningLocked ) throw new InvalidOperationException();
            Debug.Assert( _status == RunningStatus.Running );
            YodiiCommandPlugin command = new YodiiCommandPlugin( _id, false );
            command._caller = caller;
        }
        public event PropertyChangedEventHandler PropertyChanged;


        RunningStatus ILivePluginInfo.Status
        {
            get { throw new NotImplementedException(); }
        }
    }
}
