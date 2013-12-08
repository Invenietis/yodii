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
    class LiveServiceInfo : ILiveServiceInfo
    {
        YodiiEngine _engine;

        //set with cosntrutor
        readonly IServiceInfo _serviceInfo;
        ServiceDisabledReason _disabledReason;
        ConfigurationStatus _configOriginalStatus;
        SolvedConfigurationStatus _configSolvedStatus;
        RunningStatus _runningStatus;

        //set with function
        LiveServiceInfo _generalization;
        LivePluginInfo _runningPlugin;
        LivePluginInfo _lastRunningPlugin;

        internal LiveServiceInfo( ServiceData serviceData, YodiiEngine engine )
        {
            Debug.Assert( serviceData != null );
            Debug.Assert( engine != null );

            _engine = engine;

            _serviceInfo = serviceData.ServiceInfo;
            _disabledReason = serviceData.DisabledReason;
            _configOriginalStatus = serviceData.ConfigOriginalStatus;
            _configSolvedStatus = serviceData.ConfigSolvedStatus;

            Debug.Assert( serviceData.DynamicStatus != null );
            _runningStatus = serviceData.DynamicStatus.Value;
        }

        public YodiiEngine Engine
        {
            get { return _engine; }
            internal set 
            {
                Debug.Assert( value != null &&  _engine != null );
                _engine = value;
                NotifyPropertyChanged();
            }
        }

        #region ILiveServiceInfo Members

        public bool IsRunning
        {
            get
            {
                return _runningStatus >= RunningStatus.Running; 
            }
        }

        ILiveServiceInfo ILiveServiceInfo.Generalization
        {
            get { return _generalization; }
        }

        internal LiveServiceInfo Generalization
        {
            set
            {
                if( _generalization != value )
                {
                    Debug.Assert( value != null );
                    _generalization = value;
                    NotifyPropertyChanged();
                }
            }
        }

        ILivePluginInfo ILiveServiceInfo.RunningPlugin
        {
            get { return _runningPlugin; }
        }

        internal LivePluginInfo RunningPlugin
        {
            set
            {
                if( _runningPlugin != value )
                {
                    LastRunningPlugin = _runningPlugin;
                    _runningPlugin = value;
                    NotifyPropertyChanged();
                }
            }
        }

        ILivePluginInfo ILiveServiceInfo.LastRunningPlugin
        {
            get { return _lastRunningPlugin; }
        }

        internal LivePluginInfo LastRunningPlugin
        {
            set
            {
                if( _lastRunningPlugin != value )
                {
                    _lastRunningPlugin = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region IDynamicSolvedService Members

        public ServiceDisabledReason DisabledReason
        {
            get { return _disabledReason; }
            set
            {
                if( _disabledReason != value )
                {
                    _disabledReason = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public IServiceInfo ServiceInfo
        {
            get { return _serviceInfo; }
        }

        public ConfigurationStatus ConfigOriginalStatus
        {
            get { return _configOriginalStatus; }
            set
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
            set
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
            set
            {
                if( _runningStatus != value )
                {
                    _runningStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        public IYodiiEngineResult Start( object caller )
        {
            if( caller == null ) throw new ArgumentNullException( "caller" );
            if( RunningStatus == RunningStatus.Disabled ) throw new InvalidOperationException( "the service is disabled" );
            
            YodiiCommand command = new YodiiCommand( caller, true, _serviceInfo.ServiceFullName );
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

            YodiiCommand command = new YodiiCommand( caller, false, _serviceInfo.ServiceFullName );
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

        internal void UpdateInfo( ServiceData s )
        {
            DisabledReason = s.DisabledReason;
            ConfigOriginalStatus = s.ConfigOriginalStatus;
            ConfigSolvedStatus = s.ConfigSolvedStatus;
            RunningStatus = s.DynamicStatus.Value;

            if( s.DynamicStatus <= RunningStatus.Stopped && _runningPlugin != null )
            {
                _lastRunningPlugin = _runningPlugin;
                _runningPlugin = null;
            }
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
