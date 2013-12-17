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
    class LivePluginInfo : ILivePluginInfo, INotifyRaisePropertyChanged
    {
        readonly YodiiEngine _engine;

        IPluginInfo _pluginInfo;
        RunningStatus _runningStatus;
        string _disabledReason;
        ConfigurationStatus _configOriginalStatus;
        ConfigurationStatus _configSolvedStatus;
        StartDependencyImpact _configOriginalImpact;
        StartDependencyImpact _configSolvedImpact;
        ILiveServiceInfo _service;
        Exception _currentError;

        internal LivePluginInfo( PluginData p, YodiiEngine engine )
        {
            Debug.Assert( p != null && engine != null );

            _engine = engine;
            _pluginInfo = p.PluginInfo;
            Debug.Assert( p.DynamicStatus != null );
            _disabledReason = p.DisabledReason.ToString();
            _runningStatus = p.DynamicStatus.Value;
            _configOriginalStatus = p.ConfigOriginalStatus;
            _configSolvedStatus = p.ConfigSolvedStatus;
            _configOriginalImpact = p.ConfigOriginalImpact;
            _configSolvedImpact = p.ConfigSolvedImpact;
        }

        internal void UpdateFrom( PluginData p, DelayedPropertyNotification notifier )
        {
            Debug.Assert( p.DynamicStatus != null );
            notifier.Update( this, ref _pluginInfo, p.PluginInfo, () => PluginInfo );
            notifier.Update( this, ref _disabledReason, p.DisabledReason.ToString(), () => DisabledReason );
            notifier.Update( this, ref _runningStatus, p.DynamicStatus.Value, () => RunningStatus );
            notifier.Update( this, ref _configOriginalStatus, p.ConfigOriginalStatus, () => ConfigOriginalStatus );
            notifier.Update( this, ref _configSolvedStatus, p.ConfigSolvedStatus, () => ConfigSolvedStatus );
            notifier.Update( this, ref _configOriginalImpact, p.ConfigOriginalImpact, () => ConfigOriginalImpact );
            notifier.Update( this, ref _configSolvedImpact, p.ConfigSolvedImpact, () => ConfigSolvedImpact );
        }

        internal void Bind( PluginData p, Func<string, LiveServiceInfo> serviceFinder, DelayedPropertyNotification notifier )
        {
            var newService = p.Service != null ? serviceFinder( p.Service.ServiceInfo.ServiceFullName ) : null;
            notifier.Update( this, ref _service, newService, () => Service ); 
        }

        public bool IsRunning
        {
            get { return _runningStatus >= RunningStatus.Running; }
        }

        public ILiveServiceInfo Service
        {
            get { return _service; }
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
                    RaisePropertyChanged();
                }
            }
        }

        public string DisabledReason { get { return _disabledReason; } }

        public ConfigurationStatus ConfigOriginalStatus { get { return _configOriginalStatus; } }

        public ConfigurationStatus ConfigSolvedStatus { get { return _configSolvedStatus; } }

        public StartDependencyImpact ConfigOriginalImpact { get { return _configOriginalImpact; } }

        public StartDependencyImpact ConfigSolvedImpact { get { return _configSolvedImpact; } }

        public RunningStatus RunningStatus { get { return _runningStatus; } }

        public IYodiiEngineResult Start( string callerKey, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            if( callerKey == null ) throw new ArgumentNullException( "callerKey" );
            if( RunningStatus == RunningStatus.Disabled ) throw new InvalidOperationException( "the service is disabled" );

            YodiiCommand command = new YodiiCommand( callerKey, _pluginInfo.PluginFullName, true, impact, isPlugin: true );
            return _engine.AddYodiiCommand( command );
        }

        public IYodiiEngineResult Stop( string callerKey )
        {
            if( callerKey == null ) throw new ArgumentNullException( "callerKey" );
            if( RunningStatus == RunningStatus.RunningLocked ) throw new InvalidOperationException( "the service is running locked" );

            YodiiCommand command = new YodiiCommand( callerKey, _pluginInfo.PluginFullName, false, StartDependencyImpact.Unknown, isPlugin:true );
            return _engine.AddYodiiCommand( command );
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }
}
