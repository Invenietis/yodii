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

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } }

        /// <summary>
        /// Called after Service ResetDynamicState.
        /// </summary>
        public void ResetDynamicState()
        {
            switch ( ConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled: 
                    {
                        _dynamicReason = PluginRunningStatusReason.StoppedByConfig;
                        _dynamicStatus = RunningStatus.Disabled; 
                        break;
                    }
                case SolvedConfigurationStatus.Running: 
                    {
                        Debug.Assert( Service.ConfigSolvedStatus == SolvedConfigurationStatus.Running );
                        _dynamicReason = PluginRunningStatusReason.StartedByConfig;
                        _dynamicStatus = RunningStatus.RunningLocked; 
                        break;
                    }
                default:
                    {
                        Debug.Assert( Service == null || Service.ConfigSolvedStatus != SolvedConfigurationStatus.Disabled );
                        if( Service != null ) Service.OnPluginAvailable();
                        _dynamicReason = PluginRunningStatusReason.None;
                        _dynamicStatus = null;
                        break;
                    }
            }
        }

        public bool StopByCommand()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value < RunningStatus.Running;
            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = PluginRunningStatusReason.StoppedByCommand;
            if( Service != null ) Service.OnPluginStopped();
            return true;
        }

        public bool StartByCommand( StartDependencyImpact impact )
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            if( CanStart() )
            {
                DoStartByCommand( impact );
                Debug.Assert( _dynamicStatus.Value == RunningStatus.Running );
                return true;
            }
            return false;
        }

        internal void StopByService( PluginRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            Debug.Assert( reason == PluginRunningStatusReason.StoppedByStoppedService
                            || reason == PluginRunningStatusReason.StoppedByRunningSibling );
            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = reason;
        }

        internal void StopByStoppedReference()
        {
            Debug.Assert( _dynamicStatus == null );
            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = PluginRunningStatusReason.StoppedByStoppedReference;
            if( Service != null ) Service.OnPluginStopped();
        }

        internal bool CanStart()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            Debug.Assert( PluginInfo.ServiceReferences.Where( s => s.Requirement == DependencyRequirement.Running ).All( s => _allServices[s.Reference.ServiceFullName].CanStart() ) );
            return true;
        }

        internal void StartBy( PluginRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            _dynamicStatus = RunningStatus.Running;
            _dynamicReason = reason;

            if( Service != null ) Service.OnPluginStarted( this );

            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                ServiceData s = _allServices[sRef.Reference.ServiceFullName];
                if( sRef.Requirement == DependencyRequirement.Running )
                {
                    Debug.Assert( s.CanStart() );
                    s.StartBy( ServiceRunningStatusReason.StartedByRunningReference );
                }
            }
        }       

        void DoStartByCommand( StartDependencyImpact impact )
        {
            Debug.Assert( _dynamicStatus == null );
            Debug.Assert( Service == null || Service.CanStart() );
            _dynamicStatus = RunningStatus.Running;
            _dynamicReason = PluginRunningStatusReason.StartedByCommand;

            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                ServiceData s = _allServices[sRef.Reference.ServiceFullName];
                switch( sRef.Requirement )
                {
                    case DependencyRequirement.Running:
                        {
                            Debug.Assert( s.CanStart() );
                            s.StartBy( ServiceRunningStatusReason.StartedByRunningReference );
                            break;
                        }
                    case DependencyRequirement.RunnableTryStart:
                        {
                            if( impact >= StartDependencyImpact.StartRecommended )
                            {
                                if( s.CanStart() ) s.StartBy( ServiceRunningStatusReason.StartedByRunnableTryStartReference );
                            }
                            else if( impact == StartDependencyImpact.FullStop )
                            {
                                if( s.DynamicStatus == null ) s.StopBy( ServiceRunningStatusReason.StoppedByRunnableTryStartReference );
                            }
                            break;
                        }
                    case DependencyRequirement.Runnable:
                        {
                            if( impact == StartDependencyImpact.FullStart )
                            {
                                if( s.CanStart() ) s.StartBy( ServiceRunningStatusReason.StartedByRunnableReference );
                            }
                            else if( impact <= StartDependencyImpact.StopOptionalAndRunnable )
                            {
                                if( s.DynamicStatus == null ) s.StopBy( ServiceRunningStatusReason.StoppedByRunnableReference );
                            }
                            break;
                        }
                    case DependencyRequirement.OptionalTryStart:
                        {
                            if( impact >= StartDependencyImpact.StartRecommended )
                            {
                                if( s.CanStart() ) s.StartBy( ServiceRunningStatusReason.StartedByOptionalTryStartReference );
                            }
                            else if( impact == StartDependencyImpact.FullStop )
                            {
                                if( s.DynamicStatus == null ) s.StopBy( ServiceRunningStatusReason.StoppedByOptionalTryStartReference );
                            }
                            break;
                        }
                    case DependencyRequirement.Optional:
                        {
                            if( impact == StartDependencyImpact.FullStart )
                            {
                                if( s.CanStart() ) s.StartBy( ServiceRunningStatusReason.StartedByOptionalReference );
                            }
                            else if( impact <= StartDependencyImpact.StopOptionalAndRunnable )
                            {
                                if( s.DynamicStatus == null ) s.StopBy( ServiceRunningStatusReason.StoppedByOptionalReference );
                            }
                            break;
                        }
                }
            }
        }
    }
}
