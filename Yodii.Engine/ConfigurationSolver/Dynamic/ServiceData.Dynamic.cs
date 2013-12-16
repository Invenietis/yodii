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
        StartDependencyImpact _dynFirstImpact;
        RunningStatus? _dynamicStatus;
        ServiceRunningStatusReason _dynamicReason;
        int _nbAllAvailablePlugins;

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } }

        /// <summary>
        /// Called before DynamicResetState on plugins.
        /// </summary>
        public void DynamicResetState()
        {
            _nbAllAvailablePlugins = 0;
            _dynFirstImpact = StartDependencyImpact.Unknown;
            switch( FinalConfigSolvedStatus )
            {
                case ConfigurationStatus.Disabled:
                    {
                        _dynamicReason = ServiceRunningStatusReason.StoppedByConfig;
                        _dynamicStatus = RunningStatus.Disabled;
                        break;
                    }
                case ConfigurationStatus.Running:
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
        }

        /// <summary>
        /// Called by plugin.DynamicResetState when a plugin is in an undetermined state.
        /// </summary>
        internal void OnPluginAvailable()
        {
            ++_nbAllAvailablePlugins;
            if( Generalization != null ) Generalization.OnPluginAvailable();
        }

        public void OnAllPluginsDynamicStateInitialized()
        {
            // Iterate across all services (even disabled ones).
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                spec.OnAllPluginsDynamicStateInitialized();
                spec = spec.NextSpecialization;
            }

            #if DEBUG
            switch( FinalConfigSolvedStatus )
            {
                case ConfigurationStatus.Disabled:
                    {
                        Debug.Assert( _nbAllAvailablePlugins == 0 );
                        Debug.Assert( _dynamicStatus == RunningStatus.Disabled );
                        break;
                    }
                case ConfigurationStatus.Running:
                    {
                        if( _nbAllAvailablePlugins == 0 )
                        {
                            if( Family.DynRunningPlugin != null )
                            {
                                Debug.Assert( Family.DynRunningPlugin.DynamicStatus.Value == RunningStatus.RunningLocked );
                                Debug.Assert( Family.DynRunningPlugin.PluginInfo.AllServices().Contains( ServiceInfo ) );
                            }
                            if( Family.DynRunningService != null )
                            {
                                Debug.Assert( Family.DynRunningService.DynamicStatus.Value == RunningStatus.RunningLocked );
                                Debug.Assert( Family.DynRunningService.IsGeneralizationOf( this ) );
                            }
                        }
                        Debug.Assert( _dynamicStatus == RunningStatus.RunningLocked );
                        break;
                    }
                default:
                    {
                        Debug.Assert( _nbAllAvailablePlugins > 0 );
                        Debug.Assert( _dynamicStatus == null );
                        break;
                    }
            }
            #endif
        }


    
        internal void DynamicStartBy( ServiceRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running );
            Debug.Assert( reason == ServiceRunningStatusReason.StartedByCommand
                            || reason == ServiceRunningStatusReason.StartedByOptionalReference
                            || reason == ServiceRunningStatusReason.StartedByOptionalTryStartReference
                            || reason == ServiceRunningStatusReason.StartedByRunnableReference
                            || reason == ServiceRunningStatusReason.StartedByRunnableTryStartReference
                            || reason == ServiceRunningStatusReason.StartedByRunningReference
                            );
            if( _dynamicStatus == null ) Family.DynamicSetRunningService( this, reason );
        }

        internal bool DynamicCanStart()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            Debug.Assert( _nbAllAvailablePlugins != 0 );
            DynamicPropagation p = DynGetUsefulPropagationInfo();
            Debug.Assert( p != null );
            return p.TestCanStart();
        } 
        
        public bool DynamicStartByCommand( StartDependencyImpact impact )
        {
            if( DynamicCanStart() )
            {
                if( _dynamicStatus == null ) DynamicStartBy( ServiceRunningStatusReason.StartedByCommand );
                return true;
            }
            return false;
        }

        public bool DynamicStopByCommand()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value <= RunningStatus.Stopped;
            DynamicStopBy( ServiceRunningStatusReason.StoppedByCommand );
            return true;
        }

        internal void DynamicStartByDependency( StartDependencyImpact impact, DependencyRequirement req )
        {
            switch( req )
            {
                case DependencyRequirement.Running:
                    {
                        Debug.Assert( DynamicCanStart() );
                        DynamicStartBy( ServiceRunningStatusReason.StartedByRunningReference );
                        break;
                    }
                case DependencyRequirement.RunnableTryStart:
                    {
                        if( impact >= StartDependencyImpact.StartRecommended )
                        {
                            if( DynamicCanStart() ) DynamicStartBy( ServiceRunningStatusReason.StartedByRunnableTryStartReference );
                        }
                        else if( impact == StartDependencyImpact.FullStop )
                        {
                            if( DynamicStatus == null ) DynamicStopBy( ServiceRunningStatusReason.StoppedByRunnableTryStartReference );
                        }
                        break;
                    }
                case DependencyRequirement.Runnable:
                    {
                        if( impact == StartDependencyImpact.FullStart )
                        {
                            if( DynamicCanStart() ) DynamicStartBy( ServiceRunningStatusReason.StartedByRunnableReference );
                        }
                        else if( impact <= StartDependencyImpact.StopOptionalAndRunnable )
                        {
                            if( DynamicStatus == null ) DynamicStopBy( ServiceRunningStatusReason.StoppedByRunnableReference );
                        }
                        break;
                    }
                case DependencyRequirement.OptionalTryStart:
                    {
                        if( impact >= StartDependencyImpact.StartRecommended )
                        {
                            if( DynamicCanStart() ) DynamicStartBy( ServiceRunningStatusReason.StartedByOptionalTryStartReference );
                        }
                        else if( impact == StartDependencyImpact.FullStop )
                        {
                            if( DynamicStatus == null ) DynamicStopBy( ServiceRunningStatusReason.StoppedByOptionalTryStartReference );
                        }
                        break;
                    }
                case DependencyRequirement.Optional:
                    {
                        if( impact == StartDependencyImpact.FullStart )
                        {
                            if( DynamicCanStart() ) DynamicStartBy( ServiceRunningStatusReason.StartedByOptionalReference );
                        }
                        else if( impact <= StartDependencyImpact.StopOptionalAndRunnable )
                        {
                            if( DynamicStatus == null ) DynamicStopBy( ServiceRunningStatusReason.StoppedByOptionalReference );
                        }
                        break;
                    }
            }
        }

        internal void DynamicStopBy( ServiceRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            Debug.Assert( reason == ServiceRunningStatusReason.StoppedByGeneralization
                        || reason == ServiceRunningStatusReason.StoppedByCommand
                        || reason == ServiceRunningStatusReason.StoppedByPluginStopped
                        || reason == ServiceRunningStatusReason.StoppedBySiblingRunningService
                        || reason == ServiceRunningStatusReason.StoppedByOptionalReference
                        || reason == ServiceRunningStatusReason.StoppedByOptionalTryStartReference
                        || reason == ServiceRunningStatusReason.StoppedByRunnableReference
                        || reason == ServiceRunningStatusReason.StoppedByRunnableTryStartReference
                        || reason == ServiceRunningStatusReason.StoppedByFinalDecision );

            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = reason;

            Debug.Assert( (reason == ServiceRunningStatusReason.StoppedByPluginStopped) == (_nbAllAvailablePlugins == 0), "StoppedByPluginStopped <==> _nbAvailablePlugins == 0" );
            if( reason != ServiceRunningStatusReason.StoppedByPluginStopped )
            {
                _nbAllAvailablePlugins = 0; 
                PluginData p = FirstPlugin;
                while( p != null )
                {
                    Debug.Assert( p.DynamicStatus == null || p.DynamicStatus.Value <= RunningStatus.Stopped );
                    if( p.DynamicStatus == null ) p.DynamicStopBy( PluginRunningStatusReason.StoppedByStoppedService );
                    p = p.NextPluginForService;
                }
                // Stops the specialized services.
                ServiceData child = FirstSpecialization;
                while( child != null )
                {
                    Debug.Assert( child.DynamicStatus == null || child.DynamicStatus.Value < RunningStatus.Running );
                    if( child.DynamicStatus == null ) child.DynamicStopBy( ServiceRunningStatusReason.StoppedByGeneralization );
                    child = child.NextSpecialization;
                }
            }
            foreach( var backRef in _backReferences )
            {
                Debug.Assert( backRef.PluginData.DynamicStatus == null || backRef.PluginData.DynamicStatus.Value <= RunningStatus.Stopped );
                if( backRef.PluginData.DynamicStatus == null )
                {
                    PluginRunningStatusReason r = backRef.PluginData.GetStoppedReasonForStoppedReference( backRef.Requirement );
                    if( r != PluginRunningStatusReason.None ) backRef.PluginData.DynamicStopBy( r );
                }
            }
        }

        internal void OnPluginStopped()
        {
            if( _dynamicStatus != null && _dynamicStatus.Value <= RunningStatus.Stopped ) return;
            Debug.Assert( _nbAllAvailablePlugins > 0 );
            
            --_nbAllAvailablePlugins;
            if( _nbAllAvailablePlugins == 0 ) DynamicStopBy( ServiceRunningStatusReason.StoppedByPluginStopped );
            else DynPropagateStart();

            if( Generalization != null ) Generalization.OnPluginStopped();
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
            Family.DynamicSetRunningPlugin( runningPlugin );
        }


    }
}
