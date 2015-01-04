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
        PluginRunningStatusReason _dynamicReason;
        internal StartDependencyImpact DynamicImpact;

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } }

        /// <summary>
        /// Called after Service DynamicResetState.
        /// </summary>
        public void DynamicResetState()
        {
            DynamicImpact = ConfigSolvedImpact;
            switch( FinalConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled: 
                    {
                        _dynamicReason = PluginRunningStatusReason.StoppedByConfig;
                        _dynamicStatus = RunningStatus.Disabled;
                        break;
                    }
                case SolvedConfigurationStatus.Running: 
                    {
                        Debug.Assert( Service == null || (Service.Family.RunningPlugin == this) );
                        _dynamicReason = PluginRunningStatusReason.StartedByConfig;
                        _dynamicStatus = RunningStatus.RunningLocked;
                        break;
                    }
                default:
                    {
                        Debug.Assert( Service == null || !Service.Disabled );
                        _dynamicReason = PluginRunningStatusReason.None;
                        _dynamicStatus = null;
                        break;
                    }
            }
        }

        internal bool DynamicCanStart( StartDependencyImpact impact )
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            if( impact == StartDependencyImpact.Unknown ) impact = ConfigSolvedImpact;
            return DynTestCanStart( impact );
        }

        public bool DynamicStartByCommand( StartDependencyImpact impact, bool isFirst = false )
        {
            Debug.Assert( (impact & ConfigSolvedImpact) == ConfigSolvedImpact );
            
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            if( isFirst )
            {
                Debug.Assert( (_dynamicStatus == null || _dynamicStatus.Value == RunningStatus.RunningLocked) && DynTestCanStart( impact ) );
                DynamicImpact = impact;
            }
            else
            {
                // Use DynamicImpact to take service propagation into account.
                if( !DynTestCanStart( DynamicImpact ) ) return false;
                DynamicImpact |= impact.ToTryBits();
            }
            _dynamicStatus = RunningStatus.Running;
            _dynamicReason = PluginRunningStatusReason.StartedByCommand;
            DynamicStartBy( PluginRunningStatusReason.StartedByCommand );
            return true;
        }

        bool DynTestCanStart( StartDependencyImpact impact )
        {
            var allIncluded = DynGetIncludedServicesClosure( impact );
            foreach( var s in GetExcludedServices( impact ) )
            {
                if( s.DynamicStatus != null && s.DynamicStatus.Value >= RunningStatus.Running ) return false;
                if( allIncluded.Contains( s ) ) return false;
            }
            foreach( var s in allIncluded )
            {
                if( s.DynamicStatus != null && s.DynamicStatus.Value <= RunningStatus.Stopped ) return false;
            }
            return true;
        }

        public HashSet<ServiceData> DynGetIncludedServicesClosure( StartDependencyImpact impact )
        {
            Debug.Assert( !Disabled );
            impact = impact.ClearAllTryBits();

            var result = new HashSet<ServiceData>();
            foreach( var service in GetIncludedServices( impact ) )
            {
                service.DynFillTransitiveIncludedServices( result );
            }
            return result;
        }

        public PluginRunningStatusReason GetStoppedReasonForStoppedReference( DependencyRequirement requirement )
        {
            StartDependencyImpact impact = _configSolvedImpact;
            switch( requirement )
            {
                case DependencyRequirement.Running: return PluginRunningStatusReason.StoppedByRunningReference;
                case DependencyRequirement.RunnableRecommended:
                    if( (impact&StartDependencyImpact.IsStartRunnableRecommended) != 0 )
                    {
                        return PluginRunningStatusReason.StoppedByRunnableRecommendedReference;
                    }
                    break;
                case DependencyRequirement.Runnable:
                    if( (impact & StartDependencyImpact.IsStartRunnableOnly) != 0 )
                    {
                        return PluginRunningStatusReason.StoppedByRunnableReference;
                    }
                    break;
                case DependencyRequirement.OptionalRecommended:
                    if( (impact & StartDependencyImpact.IsStartOptionalRecommended) != 0 )
                    {
                        return PluginRunningStatusReason.StoppedByOptionalRecommendedReference;
                    }
                    break;
                case DependencyRequirement.Optional:
                    if( (impact & StartDependencyImpact.IsStartOptionalOnly) != 0 )
                    {
                        return PluginRunningStatusReason.StoppedByOptionalReference;
                    }
                    break;
            }
            return PluginRunningStatusReason.None;
        }

        public bool DynamicStopByCommand()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value <= RunningStatus.Stopped;
            DynamicStopBy( PluginRunningStatusReason.StoppedByCommand );
            return true;
        }

        internal void DynamicStopBy( PluginRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            Debug.Assert( reason != PluginRunningStatusReason.None );
            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = reason;
            if( Service != null )
            {
                Service.OnPluginStopped( true );
                Service.OnPostPluginStopped();
            }
        }

        internal void DynamicStartBy( PluginRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running );
            if( _dynamicStatus == null )
            {
                _dynamicStatus = RunningStatus.Running;
                _dynamicReason = reason;
            }
            if( Service != null ) Service.OnDirectPluginStarted( this );

            List<ServiceData> tryStart = null;
            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                ServiceData s = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                if( s.DynamicStartByDependency( DynamicImpact, sRef.Requirement ) && s.DynamicStatus == null )
                {
                    if( tryStart == null ) tryStart = new List<ServiceData>();
                    tryStart.Add( s );
                }
            }
            if( tryStart != null )
            {
                foreach( var s in tryStart )
                {
                    if( s.DynamicStatus == null ) s.DynamicStartBy( ServiceRunningStatusReason.StartedByPropagation );
                }
            }
        }       

    }
}
