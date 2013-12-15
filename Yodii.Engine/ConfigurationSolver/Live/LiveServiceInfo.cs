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

            if( wasRunning != _runningStatus >= RunningStatus.Running )
            {
                notifier.Notify( this, () => IsRunning );
            }
        }

        internal void Bind( ServiceData s, Func<string, LiveServiceInfo> serviceFinder, Func<Guid, LivePluginInfo> pluginFinder, DelayedPropertyNotification notifier )
        {
            var newGeneralization = s.Generalization != null ? serviceFinder( s.Generalization.ServiceInfo.ServiceFullName ) : null;
            notifier.Update( this, ref _generalization, newGeneralization, () => Generalization );

            var familyRunning = s.Family.DynRunningPlugin;
            Debug.Assert( IsRunning == (familyRunning != null && s.IsGeneralizationOf( familyRunning.Service )) );

            ILivePluginInfo newRunningPlugin = null;
            if( IsRunning )
            {
                newRunningPlugin = pluginFinder( familyRunning.PluginInfo.PluginId );
            }
            if( _runningPlugin != null )
            {
                notifier.Update( this, ref _lastRunningPlugin, _runningPlugin, () => LastRunningPlugin );
            }
            notifier.Update( this, ref _runningPlugin, newRunningPlugin, () => RunningPlugin );
        }

        public YodiiEngine Engine
        {
            get { return _engine; }
        }

        #region ILiveServiceInfo Members

        public bool IsRunning
        {
            get { return _runningStatus >= RunningStatus.Running; }
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
        }

        #endregion

        #region IDynamicSolvedService Members

        public string DisabledReason
        {
            get { return _disabledReason; }
        }

        public IServiceInfo ServiceInfo
        {
            get { return _serviceInfo; }
        }

        public ConfigurationStatus ConfigOriginalStatus
        {
            get { return _configOriginalStatus; }
        }

        public ConfigurationStatus ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        public RunningStatus RunningStatus
        {
            get { return _runningStatus; }
        }

        #endregion

        public IYodiiEngineResult Start( string callerKey )
        {
            if( callerKey == null ) throw new ArgumentNullException( "callerKey" );
            if( RunningStatus == RunningStatus.Disabled ) throw new InvalidOperationException( "the service is disabled" );

            YodiiCommand command = new YodiiCommand( callerKey, _serviceInfo.ServiceFullName, true );
            return _engine.AddYodiiCommand( command );
        }

        public IYodiiEngineResult Stop( string callerKey )
        {
            if( callerKey == null ) throw new ArgumentNullException( "callerKey" );
            if( RunningStatus == RunningStatus.RunningLocked ) throw new InvalidOperationException( "the service is running locked" );

            YodiiCommand command = new YodiiCommand( callerKey, _serviceInfo.ServiceFullName, false );
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
