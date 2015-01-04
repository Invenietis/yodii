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

            public AllFlags( SolvedConfigurationStatus finalConfigStatus, FinalConfigStartableStatus s )
            {
                Debug.Assert( (s == null) == (finalConfigStatus == SolvedConfigurationStatus.Disabled), "!Disabled <==> StartableStatus != null" );
                CanStop = finalConfigStatus != SolvedConfigurationStatus.Running;
                if( s != null )
                {
                    CanStart = true;
                    CanStartWithFullStart = s.CanStartWith( StartDependencyImpact.FullStart );
                    CanStartWithStartRecommended = s.CanStartWith( StartDependencyImpact.StartRecommended );
                }
                else
                {
                    CanStart = CanStartWithFullStart = CanStartWithStartRecommended = false;
                }
            }
        }
        FinalConfigStartableStatus _startableStatus;
        AllFlags _flags;

        internal LiveRunCapability( SolvedConfigurationStatus finalConfigStatus, FinalConfigStartableStatus s )
        {
            _startableStatus = s;
            _flags = new AllFlags( finalConfigStatus, s );
        }

        internal void UpdateFrom( SolvedConfigurationStatus finalConfigStatus, FinalConfigStartableStatus s, DelayedPropertyNotification notifier )
        {
            _startableStatus = s;
            AllFlags newOne = new AllFlags( finalConfigStatus, s );
            notifier.Update( this, ref _flags.CanStop, newOne.CanStop, () => CanStop );
            notifier.Update( this, ref _flags.CanStart, newOne.CanStart, () => CanStart );
            notifier.Update( this, ref _flags.CanStartWithFullStart, newOne.CanStartWithFullStart, () => CanStartWithFullStart );
            notifier.Update( this, ref _flags.CanStartWithStartRecommended, newOne.CanStartWithStartRecommended, () => CanStartWithStartRecommended );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanStop { get { return _flags.CanStop; } }

        public bool CanStart { get { return _flags.CanStart; } }

        public bool CanStartWithFullStart { get { return _flags.CanStartWithFullStart; } }

        public bool CanStartWithStartRecommended { get { return _flags.CanStartWithStartRecommended; } }

        public bool CanStartWith( StartDependencyImpact impact )
        {
            return _startableStatus.CanStartWith( impact );
        }

        public void RaisePropertyChanged( string propertyName )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
