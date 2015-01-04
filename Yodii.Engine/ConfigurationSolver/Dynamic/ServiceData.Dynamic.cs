using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    partial class ServiceData
    {
        RunningStatus? _dynamicStatus;
        ServiceRunningStatusReason _dynamicReason;
        int _dynamicTotalAvailablePluginsCount;
        int _dynamicAvailablePluginsCount;
        int _dynamicAvailableServicesCount;
        StartDependencyImpact _dynamicImpact;

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } }

        /// <summary>
        /// Called before DynamicResetState on plugins.
        /// </summary>
        public void DynamicResetState()
        {
            _dynPropagation = null;
            _dynamicImpact = ConfigSolvedImpact;
            _dynamicTotalAvailablePluginsCount = TotalAvailablePluginCount;
            _dynamicAvailablePluginsCount = AvailablePluginCount;
            _dynamicAvailableServicesCount = AvailableServiceCount;
            switch( FinalConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled:
                    {
                        _dynamicReason = ServiceRunningStatusReason.StoppedByConfig;
                        _dynamicStatus = RunningStatus.Disabled;
                        break;
                    }
                case SolvedConfigurationStatus.Running:
                    {
                        _dynamicReason = ServiceRunningStatusReason.StartedByConfig;
                        _dynamicStatus = RunningStatus.RunningLocked;
                        break;
                    }
                default:
                    {
                        _dynamicReason = ServiceRunningStatusReason.None;
                        _dynamicStatus = null;
                        break;
                    }
            }
            ServiceData s = FirstSpecialization;
            while( s != null )
            {
                s.DynamicResetState();
                s = s.NextSpecialization;
            }
        }

        [Conditional("DEBUG")]
        public void DebugCheckOnAllPluginsDynamicStateInitialized()
        {
            // Iterate across all services (even disabled ones).
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                spec.DebugCheckOnAllPluginsDynamicStateInitialized();
                spec = spec.NextSpecialization;
            }
            switch( FinalConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled:
                    {
                        Debug.Assert( _dynamicTotalAvailablePluginsCount == 0 );
                        Debug.Assert( _dynamicStatus == RunningStatus.Disabled );
                        break;
                    }
                case SolvedConfigurationStatus.Running:
                    {
                        Debug.Assert( _dynamicTotalAvailablePluginsCount > 0 );
                        Debug.Assert( _dynamicStatus == RunningStatus.RunningLocked );
                        break;
                    }
                default:
                    {
                        Debug.Assert( _dynamicTotalAvailablePluginsCount > 0 );
                        Debug.Assert( _dynamicStatus == null );
                        break;
                    }
            }
        }
    
        internal void DynamicStartBy( ServiceRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running );
            Debug.Assert( reason == ServiceRunningStatusReason.StartedByCommand
                            || reason == ServiceRunningStatusReason.StartedByPropagation
                            || reason == ServiceRunningStatusReason.StartedByOptionalReference
                            || reason == ServiceRunningStatusReason.StartedByOptionalRecommendedReference
                            || reason == ServiceRunningStatusReason.StartedByRunnableReference
                            || reason == ServiceRunningStatusReason.StartedByRunnableRecommendedReference
                            || reason == ServiceRunningStatusReason.StartedByRunningReference
                            );
            if( _dynamicStatus == null )
            {
                Family.DynamicSetRunningService( this, reason );
                DynPropagateStart();
            }
        }

        internal bool DynamicCanStart( StartDependencyImpact impact )
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            Debug.Assert( _dynamicTotalAvailablePluginsCount != 0 );
            if( impact == StartDependencyImpact.Unknown ) impact = ConfigSolvedImpact;
            return DynTestCanStart( impact );
        } 
        
        public bool DynamicStartByCommand( StartDependencyImpact impact, bool isFirst = false )
        {
            Debug.Assert( (impact&ConfigSolvedImpact) == ConfigSolvedImpact );
            if( isFirst )
            {
                Debug.Assert( (_dynamicStatus == null || _dynamicStatus == RunningStatus.RunningLocked) && DynTestCanStart( impact ) );
                SetDynamicImpact( impact );
            }
            else
            {
                if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
                if( !DynTestCanStart( ConfigSolvedImpact ) ) return false;
                SetDynamicImpact( impact.ToTryBits() );
            }
            DynamicStartBy( ServiceRunningStatusReason.StartedByCommand );
            return true;
        }

        void SetDynamicImpact( StartDependencyImpact impact )
        {
            _dynamicImpact |= impact;
            ServiceData s = FirstSpecialization;
            while( s != null )
            {
                if( !s.Disabled ) s.SetDynamicImpact( impact );
                s = s.NextSpecialization;
            }
            PluginData d = FirstPlugin;
            while( d != null )
            {
                if( !d.Disabled ) d.DynamicImpact |= impact;
                d = d.NextPluginForService;
            }
        }

        bool DynTestCanStart( StartDependencyImpact impact )
        {
            DynamicPropagation p = DynGetPropagationInfo();
            Debug.Assert( p != null );
            return p.TestCanStart( impact );
        }

        public bool DynamicStopByCommand()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value <= RunningStatus.Stopped;
            DynamicStopBy( ServiceRunningStatusReason.StoppedByCommand );
            return true;
        }

        internal bool DynamicStartByDependency( StartDependencyImpact impact, DependencyRequirement req )
        {
            switch( req )
            {
                case DependencyRequirement.Running:
                    {
                        Debug.Assert( DynamicCanStart( ConfigSolvedImpact ) );
                        DynamicStartBy( ServiceRunningStatusReason.StartedByRunningReference );
                        break;
                    }
                case DependencyRequirement.RunnableRecommended:
                    {
                        if( (impact & StartDependencyImpact.IsStartRunnableRecommended) != 0 )
                        {
                            Debug.Assert( DynamicCanStart( ConfigSolvedImpact ) );
                            DynamicStartBy( ServiceRunningStatusReason.StartedByRunnableRecommendedReference );
                        }
                        else if( (impact & StartDependencyImpact.IsTryStartRunnableRecommended) != 0 ) return true;
                        break;
                    }
                case DependencyRequirement.Runnable:
                    {
                        if( (impact & StartDependencyImpact.IsStartRunnableOnly) != 0 )
                        {
                            Debug.Assert( DynamicCanStart( ConfigSolvedImpact ) );
                            DynamicStartBy( ServiceRunningStatusReason.StartedByRunnableReference );
                        }
                        else if( (impact & StartDependencyImpact.IsTryStartRunnableOnly) != 0 ) return true;
                        break;
                    }
                case DependencyRequirement.OptionalRecommended:
                    {
                        if( (impact & StartDependencyImpact.IsStartOptionalRecommended) != 0 )
                        {
                            Debug.Assert( DynamicCanStart( ConfigSolvedImpact ) );
                            DynamicStartBy( ServiceRunningStatusReason.StartedByOptionalRecommendedReference );
                        }
                        else if( (impact & StartDependencyImpact.IsTryStartOptionalRecommended) != 0 ) return true;
                        break;
                    }
                case DependencyRequirement.Optional:
                    {
                        if( (impact & StartDependencyImpact.IsStartOptionalOnly) != 0 )
                        {
                            Debug.Assert( DynamicCanStart( ConfigSolvedImpact ) );
                            DynamicStartBy( ServiceRunningStatusReason.StartedByOptionalReference );
                        }
                        else if( (impact & StartDependencyImpact.IsTryStartOptionalOnly) != 0 ) return true;
                        break;
                    }
            }
            return false;
        }

        internal void DynamicStopBy( ServiceRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            Debug.Assert( reason == ServiceRunningStatusReason.StoppedByGeneralization
                        || reason == ServiceRunningStatusReason.StoppedByCommand
                        || reason == ServiceRunningStatusReason.StoppedByPluginStopped
                        || reason == ServiceRunningStatusReason.StoppedBySiblingRunningService
                        || reason == ServiceRunningStatusReason.StoppedByOptionalReference
                        || reason == ServiceRunningStatusReason.StoppedByOptionalRecommendedReference
                        || reason == ServiceRunningStatusReason.StoppedByRunnableReference
                        || reason == ServiceRunningStatusReason.StoppedByRunnableRecommendedReference
                        || reason == ServiceRunningStatusReason.StoppedByFinalDecision );

            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = reason;
            for( int i = 0; i < _inheritedServicesWithThis.Length - 1; ++i )
                --_inheritedServicesWithThis[i]._dynamicAvailableServicesCount;

            if( _dynamicTotalAvailablePluginsCount > 0 )
            {
                // Stops the specialized services.
                ServiceData child = FirstSpecialization;
                while( child != null )
                {
                    Debug.Assert( child.DynamicStatus == null || child.DynamicStatus.Value <= RunningStatus.Stopped );
                    if( child.DynamicStatus == null ) child.DynamicStopBy( ServiceRunningStatusReason.StoppedByGeneralization );
                    child = child.NextSpecialization;
                }
                // Stops all the plugins.
                PluginData p = FirstPlugin;
                while( p != null )
                {
                    Debug.Assert( p.DynamicStatus == null || p.DynamicStatus.Value <= RunningStatus.Stopped );
                    if( p.DynamicStatus == null ) p.DynamicStopBy( PluginRunningStatusReason.StoppedByStoppedService );
                    p = p.NextPluginForService;
                }
                Debug.Assert( _dynamicTotalAvailablePluginsCount == 0 );
            }
            foreach( var backRef in _backReferences )
            {
                Debug.Assert( backRef.PluginData.DynamicStatus == null 
                                || backRef.PluginData.DynamicStatus.Value <= RunningStatus.Stopped
                                || (backRef.Requirement == DependencyRequirement.Optional && (backRef.PluginData.ConfigSolvedImpact & StartDependencyImpact.IsStartOptionalOnly) == 0)
                                || (backRef.Requirement == DependencyRequirement.OptionalRecommended && (backRef.PluginData.ConfigSolvedImpact & StartDependencyImpact.IsStartOptionalRecommended) == 0)
                                || (backRef.Requirement == DependencyRequirement.Runnable && (backRef.PluginData.ConfigSolvedImpact & StartDependencyImpact.IsStartRunnableOnly) == 0)
                                || (backRef.Requirement == DependencyRequirement.RunnableRecommended && (backRef.PluginData.ConfigSolvedImpact & StartDependencyImpact.IsStartRunnableRecommended) == 0)
                                || backRef.Requirement != DependencyRequirement.Running );
                if( backRef.PluginData.DynamicStatus == null )
                {
                    PluginRunningStatusReason r = backRef.PluginData.GetStoppedReasonForStoppedReference( backRef.Requirement );
                    if( r != PluginRunningStatusReason.None ) backRef.PluginData.DynamicStopBy( r );
                }
            }
        }

        internal void OnPluginStopped( bool direct )
        {
            if( Generalization != null ) Generalization.OnPluginStopped( false );
            Debug.Assert( _dynamicTotalAvailablePluginsCount > 0 );
            --_dynamicTotalAvailablePluginsCount;
            if( direct ) --_dynamicAvailablePluginsCount;
        }

        internal void OnPostPluginStopped()
        {
            if( _dynamicTotalAvailablePluginsCount == 0 )
            {
                Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value <= RunningStatus.Stopped );
                if( _dynamicStatus == null ) DynamicStopBy( ServiceRunningStatusReason.StoppedByPluginStopped );
            }
            else if( _dynamicStatus != null && _dynamicStatus.Value >= RunningStatus.Running )
            {
                Family.Solver.DeferPropagation( this );
            }
            if( Generalization != null ) Generalization.OnPostPluginStopped();
        }

        internal void OnDirectPluginStarted( PluginData runningPlugin )
        {
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running );
            Debug.Assert( runningPlugin != null );
            if( _dynamicStatus == null )
            {
                _dynamicStatus = RunningStatus.Running;
                _dynamicReason = ServiceRunningStatusReason.StartedByPlugin;
            }
            Debug.Assert( runningPlugin.DynamicStatus.HasValue && runningPlugin.DynamicStatus.Value >= RunningStatus.Running );
            Family.DynamicSetRunningPlugin( runningPlugin );
        }


    }
}
