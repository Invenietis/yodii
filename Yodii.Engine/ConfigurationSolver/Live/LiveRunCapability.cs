using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class LiveRunCapability : ILiveRunCapability, INotifyRaisePropertyChanged
    {
        struct AllFlags
        {
            public bool CanStop;
            public bool CanStart;
            public bool CanStartWithFullStart;
            public bool CanStartWithStartRecommended;
            public bool CanStartWithStopOptionalAndRunnable;
            public bool CanStartWithFullStop;

            public AllFlags( SolvedConfigurationStatus finalConfigStatus, FinalConfigStartableStatus s )
            {
                Debug.Assert( (s == null) == (finalConfigStatus == SolvedConfigurationStatus.Disabled), "!Disabled <==> StartableStatus != null" );
                CanStop = finalConfigStatus != SolvedConfigurationStatus.Running;
                if( s != null )
                {
                    CanStart = true;
                    CanStartWithFullStart = s.CallableWithFullStart;
                    CanStartWithStartRecommended = s.CallableWithStartRecommended;
                    CanStartWithStopOptionalAndRunnable = s.CallableWithStopOptionalAndRunnable;
                    CanStartWithFullStop = s.CanStartWithFullStop;
                }
                else
                {
                    CanStart = CanStartWithFullStart = CanStartWithStartRecommended = CanStartWithStopOptionalAndRunnable = CanStartWithFullStop = false;
                }
            }
        }
        AllFlags _flags;

        internal LiveRunCapability( SolvedConfigurationStatus finalConfigStatus, FinalConfigStartableStatus s )
        {
            _flags = new AllFlags( finalConfigStatus, s );
        }

        internal void UpdateFrom( SolvedConfigurationStatus finalConfigStatus, FinalConfigStartableStatus s, DelayedPropertyNotification notifier )
        {
            AllFlags newOne = new AllFlags( finalConfigStatus, s );
            notifier.Update( this, ref _flags.CanStop, newOne.CanStop, () => CanStop );
            notifier.Update( this, ref _flags.CanStart, newOne.CanStart, () => CanStart );
            notifier.Update( this, ref _flags.CanStartWithFullStart, newOne.CanStartWithFullStart, () => CanStartWithFullStart );
            notifier.Update( this, ref _flags.CanStartWithStartRecommended, newOne.CanStartWithStartRecommended, () => CanStartWithStartRecommended );
            notifier.Update( this, ref _flags.CanStartWithStopOptionalAndRunnable, newOne.CanStartWithStopOptionalAndRunnable, () => CanStartWithStopOptionalAndRunnable );
            notifier.Update( this, ref _flags.CanStartWithFullStop, newOne.CanStartWithFullStop, () => CanStartWithFullStop );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanStop { get { return _flags.CanStop; } }

        public bool CanStart { get { return _flags.CanStart; } }

        public bool CanStartWithFullStart { get { return _flags.CanStartWithFullStart; } }

        public bool CanStartWithStartRecommended { get { return _flags.CanStartWithStartRecommended; } }

        public bool CanStartWithStopOptionalAndRunnable { get { return _flags.CanStartWithStopOptionalAndRunnable; } }

        public bool CanStartWithFullStop { get { return _flags.CanStartWithFullStop; } }

        public bool CanStartWith( StartDependencyImpact impact )
        {
            if( impact != StartDependencyImpact.Unknown && (impact & StartDependencyImpact.IsTryOnly) == 0 )
            {
                switch( impact )
                {
                    case StartDependencyImpact.FullStart: return _flags.CanStartWithFullStart;
                    case StartDependencyImpact.StartRecommended: return _flags.CanStartWithStartRecommended;
                    case StartDependencyImpact.StopOptionalAndRunnable: return _flags.CanStartWithStopOptionalAndRunnable;
                    case StartDependencyImpact.FullStop: return _flags.CanStartWithFullStop;
                }
            }
            return _flags.CanStart;
        }

        public void RaisePropertyChanged( string propertyName )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
