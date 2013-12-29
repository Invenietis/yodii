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
    abstract class LiveYodiiItemInfo : ILiveYodiiItem, INotifyRaisePropertyChanged
    {
        readonly YodiiEngine _engine;
        readonly LiveRunCapability _capability;
        readonly string _fullName;

        RunningStatus _runningStatus;
        string _disabledReason;
        ConfigurationStatus _configOriginalStatus;
        ConfigurationStatus _configSolvedStatus;
        StartDependencyImpact _configOriginalImpact;
        StartDependencyImpact _configSolvedImpact;

        protected LiveYodiiItemInfo( YodiiEngine engine, IYodiiItemData d, string fullName )
        {
            Debug.Assert( d != null && engine != null && !String.IsNullOrEmpty( fullName ) );

            _engine = engine;
            _capability = new LiveRunCapability( d.FinalConfigSolvedStatus, d.FinalStartableStatus );
            _fullName = fullName;
            Debug.Assert( d.DynamicStatus != null );
            _disabledReason = d.DisabledReason;
            _runningStatus = d.DynamicStatus.Value;
            _configOriginalStatus = d.ConfigOriginalStatus;
            _configSolvedStatus = d.ConfigSolvedStatus;
            _configOriginalImpact = d.ConfigOriginalImpact;
            _configSolvedImpact = d.RawConfigSolvedImpact;
        }

        protected void UpdateItem( IYodiiItemData d, DelayedPropertyNotification notifier )
        {
            Debug.Assert( d.DynamicStatus != null );
            bool wasRunning = _runningStatus >= RunningStatus.Running;
            _capability.UpdateFrom( d.FinalConfigSolvedStatus, d.FinalStartableStatus, notifier );
            notifier.Update( this, ref _disabledReason, d.DisabledReason, () => DisabledReason );
            notifier.Update( this, ref _runningStatus, d.DynamicStatus.Value, () => RunningStatus );
            notifier.Update( this, ref _configOriginalStatus, d.ConfigOriginalStatus, () => ConfigOriginalStatus );
            notifier.Update( this, ref _configSolvedStatus, d.ConfigSolvedStatus, () => ConfigSolvedStatus );
            notifier.Update( this, ref _configOriginalImpact, d.ConfigOriginalImpact, () => ConfigOriginalImpact );
            notifier.Update( this, ref _configSolvedImpact, d.RawConfigSolvedImpact, () => ConfigSolvedImpact );
            if( wasRunning != _runningStatus >= RunningStatus.Running )
            {
                notifier.Notify( this, () => IsRunning );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ILiveRunCapability Capability
        {
            get { return _capability; }
        }

        public string FullName
        {
            get { return _fullName; }
        }

        public bool IsRunning
        {
            get { return _runningStatus >= RunningStatus.Running; }
        }

        public string DisabledReason { get { return _disabledReason; } }

        public ConfigurationStatus ConfigOriginalStatus { get { return _configOriginalStatus; } }

        public ConfigurationStatus ConfigSolvedStatus { get { return _configSolvedStatus; } }

        public StartDependencyImpact ConfigOriginalImpact { get { return _configOriginalImpact; } }

        public StartDependencyImpact ConfigSolvedImpact { get { return _configSolvedImpact; } }

        public RunningStatus RunningStatus { get { return _runningStatus; } }

        protected abstract bool IsPlugin { get; }

        public IYodiiEngineResult Start( string callerKey, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            if( !_capability.CanStartWith( impact ) )
            {
                throw new InvalidOperationException( "You must call Capability.CanStart with the wanted impact and ensure that it returns true before calling Start." );
            }
            YodiiCommand command = new YodiiCommand( true, _fullName, IsPlugin, impact, callerKey );
            return _engine.AddYodiiCommand( command );
        }

        public IYodiiEngineResult Stop( string callerKey )
        {
            if( !_capability.CanStop )
            {
                throw new InvalidOperationException( "You must call Capability.CanStop and ensure that it returns true before calling Stop." );
            }
            YodiiCommand command = new YodiiCommand( false, _fullName, IsPlugin, StartDependencyImpact.Unknown, callerKey );
            return _engine.AddYodiiCommand( command );
        }

        public void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

    }
}
