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

        public bool Start( StartDependencyImpact impact )
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            if( CanStart() )
            {
                DoStart( impact );
                Debug.Assert( _dynamicStatus.Value == RunningStatus.Running );
                return true;
            }
            DoStop();
            return false;
        }

        private void DoStop()
        {
            throw new NotImplementedException();
        }

        public bool Stop()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value < RunningStatus.Running;
            Debug.Assert( CanStop() );
            DoStop();
            Debug.Assert( _dynamicStatus.Value == RunningStatus.Stopped );
            return true;
        }

        bool CanStop()
        {
            if( _dynamicStatus.HasValue ) return _dynamicStatus.Value < RunningStatus.Running;
            return true;
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


        bool DoStart( StartDependencyImpact impact )
        {
            Debug.Assert( _dynamicStatus == null );
            if( impact == StartDependencyImpact.None )
            {
                foreach( var s in PluginInfo.ServiceReferences )
                {
                    if( !PropagateRunningDependancyRequirement( s ) ) return false;
                }
            }
            else if( impact == StartDependencyImpact.StartRecommended )
            {
                foreach( var s in PluginInfo.ServiceReferences )
                {
                    if( !PropagateRunningDependancyRequirement( s ) ) return false;
                    if( s.Requirement == DependencyRequirement.RunnableTryStart
                        || s.Requirement == DependencyRequirement.OptionalTryStart )
                    {
                        _allServices[s.Reference].SetDynamicStatus( RunningStatus.Running );
                    }
                }
            }
            else if( impact == StartDependencyImpact.StopOptionalAndRunnable )
            {
                foreach( var s in PluginInfo.ServiceReferences )
                {
                    if( !PropagateRunningDependancyRequirement( s ) ) return false;
                    if( s.Requirement == DependencyRequirement.Optional
                        || s.Requirement == DependencyRequirement.Runnable )
                    {
                        _allServices[s.Reference].SetDynamicStatus( RunningStatus.Stopped );
                    }
                }
            }
            else if( impact == StartDependencyImpact.FullStop )
            {
                foreach( var s in PluginInfo.ServiceReferences )
                {
                    if( !PropagateRunningDependancyRequirement( s ) ) return false;
                    _allServices[s.Reference].SetDynamicStatus( RunningStatus.Stopped );
                }
            }
            else if( impact == StartDependencyImpact.FullStart )
            {
                foreach( var s in PluginInfo.ServiceReferences )
                {
                    if( !PropagateRunningDependancyRequirement( s ) ) return false;
                    _allServices[s.Reference].SetDynamicStatus( RunningStatus.Running );
                }
            }
            if( Service.SetDynamicStatus( RunningStatus.Running ) )
            {
                _dynamicStatus = RunningStatus.Running;
                return true;
            }
            return false;
        }

        bool PropagateRunningDependancyRequirement( IServiceReferenceInfo s )
        {
            if( s.Requirement == DependencyRequirement.Running
                            && (_allServices[s.Reference].DynamicStatus != RunningStatus.Running || _allServices[s.Reference].DynamicStatus != RunningStatus.RunningLocked)
                            && !_allServices[s.Reference].SetDynamicStatus( RunningStatus.Running ) )
            {
                _dynamicStatus = RunningStatus.Stopped;
                return false;
            }
            return true;
        }        
    }
}
