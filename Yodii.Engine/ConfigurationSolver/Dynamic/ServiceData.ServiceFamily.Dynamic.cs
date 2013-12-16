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

        public partial class ServiceFamily
        {
            ServiceData _dynRunningService;
            PluginData _dynRunningPlugin;

            public ServiceData DynRunningService
            {
                get { return _dynRunningService; }
            }

            public PluginData DynRunningPlugin
            {
                get { return _dynRunningPlugin; }
            }


            internal void DynamicOnAllPluginsStateInitialized()
            {
                if( !Root.Disabled )
                {
                    if( _runningPlugin != null )
                    {
                        DynamicSetRunningPlugin( _runningPlugin );
                        Debug.Assert( _dynRunningService == _runningPlugin.Service );
                    }
                    else
                    {
                        if( _runningService != null )
                        {
                            Debug.Assert( _runningService._dynamicStatus != null && _runningService.DynamicStatus.Value == RunningStatus.RunningLocked );
                            DynamicSetRunningService( _runningService, ServiceRunningStatusReason.None );
                        }
                        Root.OnAllPluginsDynamicStateInitialized();
                    }
                }
                else Root.OnAllPluginsDynamicStateInitialized();
            }

            public void DynamicSetRunningPlugin( PluginData running )
            {
                Debug.Assert( running.Service != null && running.Service.Family == this );
                Debug.Assert( running != null && running.DynamicStatus.HasValue && running.DynamicStatus.Value >= RunningStatus.Running );
                Debug.Assert( _dynRunningPlugin == null );

                _dynRunningPlugin = running;
                DynamicSetRunningService( running.Service, ServiceRunningStatusReason.StartedBySpecialization );
                // Stops all plugins except the one that started.
                PluginData p = running.Service.FirstPlugin;
                while( p != null )
                {
                    if( p != running )
                    {
                        Debug.Assert( p.DynamicStatus == null || p.DynamicStatus.Value <= RunningStatus.Stopped );
                        if( p.DynamicStatus == null ) p.DynamicStopBy( PluginRunningStatusReason.StoppedByRunningSibling );
                    }
                    p = p.NextPluginForService;
                }
                // Stops all specializations.
                ServiceData s = running.Service.FirstSpecialization;
                while( s != null )
                {
                    Debug.Assert( s.DynamicStatus == null || s.DynamicStatus.Value <= RunningStatus.Stopped );
                    if( s.DynamicStatus == null ) s.DynamicStopBy( ServiceRunningStatusReason.StoppedByGeneralization );
                    s = s.NextSpecialization;
                }
            }

            public void DynamicSetRunningService( ServiceData s, ServiceRunningStatusReason reason )
            {
                Debug.Assert( s != null && s.Family == this && !s.Disabled );
                Debug.Assert( _dynRunningPlugin == null || s.IsGeneralizationOf( _dynRunningPlugin.Service ), "If there is running plugin, it can only be one of us." );
                Debug.Assert( s._dynamicStatus == null || s._dynamicStatus.Value >= RunningStatus.Running );               
                if( s._dynamicStatus == null )
                {
                    s._dynamicStatus = RunningStatus.Running;
                    s._dynamicReason = reason;
                }
                if( s == _dynRunningService ) return;
                Debug.Assert( _dynRunningService == null || _dynRunningService.IsStrictGeneralizationOf( s ), "If there is already a current running service, it can only be a more specialized one." );

                var g = s.Generalization;
                while( g != null )
                {
                    if( g._dynamicStatus == null )
                    {
                        g._dynamicStatus = RunningStatus.Running;
                        g._dynamicReason = ServiceRunningStatusReason.StartedBySpecialization;
                    }
                    g = g.Generalization;
                }
                var prevRunningService = _dynRunningService;
                _dynRunningService = s;
                // We must now stop sibling services (and plugins) from this up to the currently running one and 
                // when the current one is null, up to the root.
                g = s.Generalization;
                var specRunning = s;
                while( g != null )
                {
                    var spec = g.FirstSpecialization;
                    while( spec != null )
                    {
                        if( spec != specRunning && !spec.Disabled )
                        {
                            Debug.Assert( spec.DynamicStatus == null || spec.DynamicStatus.Value <= RunningStatus.Stopped );
                            if( spec.DynamicStatus == null ) spec.DynamicStopBy( ServiceRunningStatusReason.StoppedByGeneralization );
                        }
                        spec = spec.NextSpecialization;
                    }
                    PluginData p = g.FirstPlugin;
                    while( p != null )
                    {
                        Debug.Assert( p.DynamicStatus == null || p.DynamicStatus.Value <= RunningStatus.Stopped );
                        if( p.DynamicStatus == null ) p.DynamicStopBy( PluginRunningStatusReason.StoppedByStoppedService );
                        p = p.NextPluginForService;
                    }
                    specRunning = g;
                    g = g.Generalization;
                    if( prevRunningService != null && g == prevRunningService.Generalization ) break;
                }


            }

            internal void DynamicFinalDecision()
            {
                Debug.Assert( !Root.Disabled );
                Debug.Assert( _dynRunningPlugin == null 
                                || (_dynRunningPlugin.DynamicStatus.Value >= RunningStatus.Running 
                                    && Root.FindFirstPluginData( p => p != _dynRunningPlugin && p.DynamicStatus.Value >= RunningStatus.Running ) == null), 
                              "A running plugin exists => All plugins are stopped except the running plugin." );

                if( _dynRunningPlugin == null )
                {
                    ServiceData startPoint = _dynRunningService ?? Root;
                    if( startPoint.FinalConfigSolvedStatus == ConfigurationStatus.Running  || (startPoint.DynamicStatus != null && startPoint.DynamicStatus.Value == RunningStatus.Running) )
                    {
                        Debug.Assert( startPoint.DynamicCanStart( startPoint.ConfigSolvedImpact ) );
                        PluginData firstRunnable = startPoint.FindFirstPluginData( p => p.DynamicCanStart( StartDependencyImpact.Minimal ) );
                        Debug.Assert( firstRunnable != null );
                        firstRunnable.DynamicStartBy( StartDependencyImpact.Minimal, PluginRunningStatusReason.StartedByFinalDecision );
                        Debug.Assert( _dynRunningPlugin == firstRunnable );
                        Debug.Assert( _dynRunningPlugin.DynamicStatus.Value == RunningStatus.Running
                                            && Root.FindFirstPluginData( p => p != _dynRunningPlugin && p.DynamicStatus.Value >= RunningStatus.Running ) == null,
                                            "All plugins are stopped except the running plugin." );
                    }
                    else                    
                    {
                        Debug.Assert( startPoint.DynamicStatus == null || startPoint.DynamicStatus.Value <= RunningStatus.Stopped );
                        if( startPoint.DynamicStatus == null ) startPoint.DynamicStopBy( ServiceRunningStatusReason.StoppedByFinalDecision );
                    }
                }
            }


        }
    }
}
