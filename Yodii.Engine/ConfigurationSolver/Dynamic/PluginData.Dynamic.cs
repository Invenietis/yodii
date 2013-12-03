using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    partial class PluginData
    {
        RunningStatus? _dynamicStatus;

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } set { _dynamicStatus = value; } }

        public void ResetDynamicState()
        {
            switch ( ConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled: _dynamicStatus = RunningStatus.Disabled; break;
                case SolvedConfigurationStatus.Running: _dynamicStatus = RunningStatus.RunningLocked; break;
                default: _dynamicStatus = null; break;
            }
        }

        public bool Stop()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value < RunningStatus.Running;
            Debug.Assert( CanStop() );
            Debug.Assert( _dynamicStatus.Value == RunningStatus.Running );
            DoStop();
            Debug.Assert( _dynamicStatus.Value == RunningStatus.Stopped );
            return true;
        }

        bool CanStop()
        {
            if( _dynamicStatus.HasValue ) return _dynamicStatus.Value < RunningStatus.Running;
            return true;
        }

        void DoStop()
        {
            Service.Stop();
            _dynamicStatus = RunningStatus.Stopped;
        }

        public bool Start( StartDependencyImpact impact )
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            if( CanStart() )
            {
                Debug.Assert( _dynamicStatus.Value == RunningStatus.Stopped || _dynamicStatus.Value == null );
                DoStart( impact );
                Debug.Assert( _dynamicStatus.Value == RunningStatus.Running );
                return true;
            }
            DoStop();
            return false;
        }

        bool CanStart()
        {
            if( _dynamicStatus.HasValue ) return _dynamicStatus.Value >= RunningStatus.Running;
            foreach( var s in PluginInfo.ServiceReferences )
            {
                if( s.Requirement == DependencyRequirement.Running && !_allServices[s.Reference].CanStart() )
                {
                    return false;
                }
            }
            return true;
        }

        void DoStart( StartDependencyImpact impact )
        {
            Debug.Assert( _dynamicStatus == null );
            foreach( var s in PluginInfo.ServiceReferences )
            {
                PropagateRunningDependancyRequirement( s );

                switch( impact)
                {
                    case StartDependencyImpact.StartRecommended :
                        if( s.Requirement == DependencyRequirement.RunnableTryStart
                         || s.Requirement == DependencyRequirement.OptionalTryStart )
                        {
                            _allServices[s.Reference].Start();
                        }
                        break;
                    case StartDependencyImpact.StopOptionalAndRunnable :
                        if( s.Requirement == DependencyRequirement.Optional
                            || s.Requirement == DependencyRequirement.Runnable )
                        {
                            _allServices[s.Reference].Stop();
                        }
                        break;
                    case StartDependencyImpact.FullStop :
                        _allServices[s.Reference].Stop();
                        break;
                    case StartDependencyImpact.FullStart :
                        _allServices[s.Reference].Start();
                        break;
                    default :
                        break;
                }
            }

            Service.Start(); //Debug assert ? if return false
            _dynamicStatus = RunningStatus.Running;
        }

        void PropagateRunningDependancyRequirement( IServiceReferenceInfo s )
        {
            if( s.Requirement == DependencyRequirement.Running
                && (_allServices[s.Reference].DynamicStatus != RunningStatus.Running || _allServices[s.Reference].DynamicStatus != RunningStatus.RunningLocked)
                && !_allServices[s.Reference].Start() )
            {
                Debug.Fail( "There is a problem with CanStart()" );
            }
        }        
    }
}
