using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class LiveServiceInfo : ILiveServiceInfo, INotifyRaisePropertyChanged
    {
        readonly YodiiEngine _engine;
        IServiceInfo _serviceInfo;
        string _disabledReason;
        ConfigurationStatus _configOriginalStatus;
        ConfigurationStatus _configSolvedStatus;
        StartDependencyImpact _configOriginalImpact;
        StartDependencyImpact _configSolvedImpact;
        RunningStatus _runningStatus;

        ILiveServiceInfo _generalization;
        ILivePluginInfo _runningPlugin;
        ILivePluginInfo _lastRunningPlugin;

        internal LiveServiceInfo( ServiceData serviceData, YodiiEngine engine )
        {
            Debug.Assert( serviceData != null );
            Debug.Assert( engine != null );

            _engine = engine;
            _serviceInfo = serviceData.ServiceInfo;
            _disabledReason = serviceData.DisabledReason.ToString();
            _configOriginalStatus = serviceData.ConfigOriginalStatus;
            _configSolvedStatus = serviceData.ConfigSolvedStatus;
            _configOriginalImpact = serviceData.ConfigOriginalImpact;
            _configSolvedImpact = serviceData.ConfigSolvedImpact;

            Debug.Assert( serviceData.DynamicStatus != null );
            _runningStatus = serviceData.DynamicStatus.Value;
        }

        internal void UpdateFrom( ServiceData s, DelayedPropertyNotification notifier )
        {
            bool wasRunning = _runningStatus >= RunningStatus.Running;
            notifier.Update( this, ref _serviceInfo, s.ServiceInfo, () => ServiceInfo );
            notifier.Update( this, ref _disabledReason, s.DisabledReason.ToString(), () => DisabledReason );
            notifier.Update( this, ref _configOriginalStatus, s.ConfigOriginalStatus, () => ConfigOriginalStatus );
            notifier.Update( this, ref _configSolvedStatus, s.ConfigSolvedStatus, () => ConfigSolvedStatus );
            notifier.Update( this, ref _runningStatus, s.DynamicStatus.Value, () => RunningStatus );
            notifier.Update( this, ref _configOriginalImpact, s.ConfigOriginalImpact, () => ConfigOriginalImpact );
            notifier.Update( this, ref _configSolvedImpact, s.ConfigSolvedImpact, () => ConfigSolvedImpact );

            if( wasRunning != _runningStatus >= RunningStatus.Running )
            {
                notifier.Notify( this, () => IsRunning );
            }
        }

        internal void Bind( ServiceData s, Func<string, LiveServiceInfo> serviceFinder, Func<string, LivePluginInfo> pluginFinder, DelayedPropertyNotification notifier )
        {
            var newGeneralization = s.Generalization != null ? serviceFinder( s.Generalization.ServiceInfo.ServiceFullName ) : null;
            notifier.Update( this, ref _generalization, newGeneralization, () => Generalization );

            var familyRunning = s.Family.DynRunningPlugin;
            Debug.Assert( IsRunning == (familyRunning != null && s.IsGeneralizationOf( familyRunning.Service )) );

            ILivePluginInfo newRunningPlugin = null;
            if( IsRunning )
            {
                newRunningPlugin = pluginFinder( familyRunning.PluginInfo.PluginFullName );
            }
            if( _runningPlugin != null )
            {
                notifier.Update( this, ref _lastRunningPlugin, _runningPlugin, () => LastRunningPlugin );
            }
            notifier.Update( this, ref _runningPlugin, newRunningPlugin, () => RunningPlugin );
        }

        public YodiiEngine Engine { get { return _engine; } }

        public bool IsRunning { get { return _runningStatus >= RunningStatus.Running; } }

        public ILiveServiceInfo Generalization { get { return _generalization; } }

        public ILivePluginInfo RunningPlugin { get { return _runningPlugin; } }

        public ILivePluginInfo LastRunningPlugin { get { return _lastRunningPlugin; } }

        public string DisabledReason { get { return _disabledReason; } }

        public IServiceInfo ServiceInfo { get { return _serviceInfo; } }

        public ConfigurationStatus ConfigOriginalStatus { get { return _configOriginalStatus; } }

        public ConfigurationStatus ConfigSolvedStatus { get { return _configSolvedStatus; } }

        public RunningStatus RunningStatus { get { return _runningStatus; } }

        public StartDependencyImpact ConfigOriginalImpact { get { return _configOriginalImpact; } }

        public StartDependencyImpact ConfigSolvedImpact { get { return _configSolvedImpact; } }

        public IYodiiEngineResult Start( string callerKey, StartDependencyImpact impact )
        {
            if( RunningStatus == RunningStatus.Disabled ) throw new InvalidOperationException( "the service is disabled" );

            YodiiCommand command = new YodiiCommand( callerKey ?? String.Empty, _serviceInfo.ServiceFullName, true, impact );
            return _engine.AddYodiiCommand( command );
        }

        public IYodiiEngineResult Stop( string callerKey )
        {
            if( RunningStatus == RunningStatus.RunningLocked ) throw new InvalidOperationException( "the service is running locked" );

            YodiiCommand command = new YodiiCommand( callerKey ?? String.Empty, _serviceInfo.ServiceFullName, false );
            return _engine.AddYodiiCommand( command );
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged( string propertyName )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion

    }
}
