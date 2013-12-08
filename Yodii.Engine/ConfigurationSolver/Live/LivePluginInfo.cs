﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class LivePluginInfo : ILivePluginInfo
    {
        readonly IPluginInfo _pluginInfo;

        YodiiEngine _engine;

        RunningStatus _runningStatus;
        PluginDisabledReason _disabledReason;
        ConfigurationStatus _configOriginalStatus;
        SolvedConfigurationStatus _configSolvedStatus;

        LiveServiceInfo _service;
        Exception _currentError;

        internal LivePluginInfo( PluginData p, YodiiEngine engine )
        {
            Debug.Assert( p != null && engine != null );

            _engine = engine;

            _pluginInfo = p.PluginInfo;

            Debug.Assert( p.DynamicStatus != null );
            _runningStatus = p.DynamicStatus.Value;
            _configOriginalStatus = p.ConfigOriginalStatus;
            _configSolvedStatus = p.ConfigSolvedStatus;
        }

        public bool IsRunning
        {
            get { return _runningStatus >= RunningStatus.Running; }
        }

        ILiveServiceInfo ILivePluginInfo.Service
        {
            get { return _service; }
        }

        internal LiveServiceInfo Service
        {
            set 
            {
                Debug.Assert( _service != value );
                _service = value;
            }
        }

        public IPluginInfo PluginInfo
        {
            get { return _pluginInfo; }
        }

        public Exception CurrentError
        {
            get { return _currentError; }
            internal set
            {
                if( _currentError != value )
                {
                    _currentError = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PluginDisabledReason DisabledReason
        {
            get { return _disabledReason; }
            internal set
            {
                if( _disabledReason != value )
                {
                    _disabledReason = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ConfigurationStatus ConfigOriginalStatus
        {
            get { return _configOriginalStatus; }
            internal set
            {
                if( _configOriginalStatus != value )
                {
                    _configOriginalStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public SolvedConfigurationStatus ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
            internal set
            {
                if( _configSolvedStatus != value )
                {
                    _configSolvedStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RunningStatus RunningStatus
        {
            get { return _runningStatus; }
            internal set
            {
                if( _runningStatus != value )
                {
                    _runningStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public IYodiiEngineResult Start( object caller, StartDependencyImpact impact = StartDependencyImpact.None )
        {
            if( caller == null ) throw new ArgumentNullException( "caller" );
            if( RunningStatus == RunningStatus.Disabled ) throw new InvalidOperationException( "the service is disabled" );

            YodiiCommand command = new YodiiCommand( caller, true, impact, _pluginInfo.PluginId );
            _engine.YodiiCommands.Insert( 0, command );

            if( RunningStatus >= RunningStatus.Running ) return new SuccessYodiiEngineResult();

            IYodiiEngineResult result = _engine.DynamicResolutionByLiveInfo();
            if( result.Success )
            {
                _engine.CleanYodiiCommands( command );
                return result;
            }
            _engine.YodiiCommands.RemoveAt( 0 );
            return result;
        }

        public IYodiiEngineResult Stop( object caller )
        {
            if( caller == null ) throw new ArgumentNullException( "caller" );
            if( RunningStatus == RunningStatus.RunningLocked ) throw new InvalidOperationException( "the service is running locked" );

            YodiiCommand command = new YodiiCommand( caller, false, StartDependencyImpact.None, _pluginInfo.PluginId );
            _engine.YodiiCommands.Insert( 0, command );

            if( RunningStatus <= RunningStatus.Stopped ) return new SuccessYodiiEngineResult();

            IYodiiEngineResult result = _engine.DynamicResolutionByLiveInfo();
            if( result.Success )
            {
                _engine.CleanYodiiCommands( command );
                return result;
            }
            _engine.YodiiCommands.RemoveAt( 0 );
            return result;
        }

        internal void UpdateInfo( PluginData p )
        {
            Debug.Assert( p.DynamicStatus != null );
            _runningStatus = p.DynamicStatus.Value;
            _configOriginalStatus = p.ConfigOriginalStatus;
            _configSolvedStatus = p.ConfigSolvedStatus;

            if( _service != null && p.DynamicStatus >= RunningStatus.Running ) _service.RunningPlugin = this;
        }
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( [CallerMemberName]string propertyName = "" )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }
}
