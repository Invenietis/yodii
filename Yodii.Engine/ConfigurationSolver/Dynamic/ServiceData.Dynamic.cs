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
        int _nbAllAvailablePlugins;

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } set { _dynamicStatus = value; } }

        /// <summary>
        /// Called before ResetDynamicState on plugins.
        /// </summary>
        public void ResetDynamicState()
        {
            switch( ConfigSolvedStatus )
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
            _nbAllAvailablePlugins = 0;
        }

        /// <summary>
        /// Called by plugin.ResetDynamicState when a plugin is in an undetermined state.
        /// </summary>
        internal void OnPluginAvailable()
        {
            ++_nbAllAvailablePlugins;
            if( Generalization != null ) Generalization.OnPluginAvailable();
        }

        [Conditional( "DEBUG")]
        public void OnAllPluginsDynamicStateInitialized()
        {
            Debug.Assert( (_nbAllAvailablePlugins > 0) == (_dynamicStatus == null || _dynamicStatus == RunningStatus.RunningLocked) );
            switch( ConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled:
                    {
                        Debug.Assert( _nbAllAvailablePlugins == 0 );
                        Debug.Assert( _dynamicStatus == RunningStatus.Disabled );
                        break;
                    }
                case SolvedConfigurationStatus.Running:
                    {
                        Debug.Assert( _nbAllAvailablePlugins > 0
                                        || (GeneralizationRoot.TheOnlyPlugin != null
                                            && GeneralizationRoot.TheOnlyPlugin.DynamicStatus.Value == RunningStatus.RunningLocked
                                            && GeneralizationRoot.TheOnlyPlugin.PluginInfo.AllServices().Contains( ServiceInfo )) );
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
        }

        /// <summary>
        /// It goes likes this until we have gone through the whole graph (i.e. until the GeneralizationRoot):
        /// -> From the Service to StartByCommand, go to the upper node and set its RunningStatus to Running
        /// -> Set RunningStatus to Stopped to all child nodes except the one we come from (in this case, the one we want started in the first place)
        /// </summary>
        /// <returns></returns>
        public bool StartByCommand()
        {
            if( CanStart() )
            {
                StartBy( ServiceRunningStatusReason.StartedByCommand );
                return true;
            }
            return false;
        }

        internal void OnSpecializationStarted( ServiceData runningSpec )
        {
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running );
            if( _dynamicStatus != null ) return;

            _dynamicStatus = RunningStatus.Running;
            _dynamicReason = ServiceRunningStatusReason.StartedBySpecialization;

            if( Generalization != null ) Generalization.OnSpecializationStarted( this );

            ServiceData s = FirstSpecialization;
            while( s != null )
            {
                if( s != runningSpec )
                {
                    Debug.Assert( s._dynamicStatus == null || s._dynamicStatus.Value <= RunningStatus.Running );
                    if( s.DynamicStatus == null ) s.StopBy( ServiceRunningStatusReason.StoppedBySiblingRunningService );
                }
                s = s.NextSpecialization;
            }
        }

        internal void StartBy( ServiceRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            _dynamicStatus = RunningStatus.Running;
            Debug.Assert( reason == ServiceRunningStatusReason.StartedByCommand
                            || reason == ServiceRunningStatusReason.StartedByOptionalReference
                            || reason == ServiceRunningStatusReason.StartedByOptionalTryStartReference
                            || reason == ServiceRunningStatusReason.StartedByRunnableReference
                            || reason == ServiceRunningStatusReason.StartedByRunnableTryStartReference
                            || reason == ServiceRunningStatusReason.StartedByRunningReference
                            );
            _dynamicReason = reason;

            if( _nbAllAvailablePlugins == 1 )
            {
                PluginData lastAvailable = FindFirstPluginData( p => p.DynamicStatus == null );
                // This sets Running on plugin generalizations that may be one of our specialization.
                lastAvailable.StartBy( PluginRunningStatusReason.StartedByRunningService );
            }

            // This sets Running on all our generalizations if no plugin has been started above.
            if( Generalization != null ) Generalization.OnSpecializationStarted( this );
        }

        public bool StopByCommand()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value <= RunningStatus.Stopped;
            StopBy( ServiceRunningStatusReason.StoppedByCommand );
            return true;
        }

        internal void StopBy( ServiceRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            Debug.Assert( reason == ServiceRunningStatusReason.StoppedByGeneralization 
                        || reason == ServiceRunningStatusReason.StoppedByCommand
                        || reason == ServiceRunningStatusReason.StoppedByPluginStopped
                        || reason == ServiceRunningStatusReason.StoppedBySiblingRunningService
                        || reason == ServiceRunningStatusReason.StoppedByOptionalReference
                        || reason == ServiceRunningStatusReason.StoppedByOptionalTryStartReference
                        || reason == ServiceRunningStatusReason.StoppedByRunnableReference
                        || reason == ServiceRunningStatusReason.StoppedByRunnableTryStartReference );

            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = reason;

            // Stops the direct plugins if required.
            if( reason != ServiceRunningStatusReason.StoppedByPluginStopped )
            {
                PluginData p = FirstPlugin;
                while( p != null )
                {
                    if( p.DynamicStatus == null ) p.StopByService( PluginRunningStatusReason.StoppedByStoppedService );
                    p = p.NextPluginForService;
                }
                // Stops the specialized services.
                ServiceData child = FirstSpecialization;
                while( child != null )
                {
                    Debug.Assert( child._dynamicStatus == null || child._dynamicStatus.Value < RunningStatus.Running );
                    if( child.DynamicStatus == null ) child.StopBy( ServiceRunningStatusReason.StoppedByGeneralization );
                    child = child.NextSpecialization;
                }
            }
            // Stops all plugins that require this service as Running.
            BackReference br = _firstBackRunnableReference;
            while( br != null )
            {
                if( br.Requirement == DependencyRequirement.Running )
                {
                    Debug.Assert( br.PluginData.DynamicStatus == null || br.PluginData.DynamicStatus < RunningStatus.Running ); 
                    br.PluginData.StopByStoppedReference();
                }
                br = br.Next;
            }
        }

        internal void OnPluginStopped()
        {
            Debug.Assert( _nbAllAvailablePlugins > 0 );
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running, "If we know that we are stopped, we do not call this function." );
            
            // The implication below is the same as:
            //    (nbAllAvailablePlugins == 1) ==> (dynamicStatus == null)
            //
            Debug.Assert( _dynamicStatus == null || _nbAllAvailablePlugins >= 2, "(dynamicStatus != null) ==> (nbAllAvailablePlugins >= 2)" );

            Debug.Assert( Generalization == null || Generalization._nbAllAvailablePlugins >= _nbAllAvailablePlugins );

            --_nbAllAvailablePlugins;
            if( _nbAllAvailablePlugins >= 2 )
            {
                if( Generalization != null ) Generalization.OnPluginStopped();
                return;
            }
            
            if( _nbAllAvailablePlugins == 0 )
            {
                Debug.Assert( _dynamicStatus == null );
                StopBy( ServiceRunningStatusReason.StoppedByPluginStopped );

                if( Generalization != null ) Generalization.OnPluginStopped();
            }
            else if( _nbAllAvailablePlugins == 1 )
            {
                if( _dynamicStatus != null )
                {
                    Debug.Assert( _dynamicStatus.Value >= RunningStatus.Running );
                    PluginData lastAvailable = FindFirstPluginData( p => p.DynamicStatus == null );
                    lastAvailable.StartBy( PluginRunningStatusReason.StartedByRunningService );
                }
                else
                {
                    // We can not conclude anything yet.
                    if( Generalization != null ) Generalization.OnPluginStopped();
                }
            }
        }

        internal void OnPluginStarted( PluginData runningPlugin )
        {
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running );
            if( _dynamicStatus == null )
            {
                _dynamicStatus = RunningStatus.Running;
                _dynamicReason = ServiceRunningStatusReason.StartedByPlugin;
            }
            // Stops all plugins except the one that started.
            PluginData p = FirstPlugin;
            while( p != null )
            {
                if( p != runningPlugin )
                {
                    if( p.DynamicStatus == null ) p.StopByService( PluginRunningStatusReason.StoppedByRunningSibling );
                    p = p.NextPluginForService;
                }
            }

            ServiceData s = FirstSpecialization;
            while( s != null )
            {
                Debug.Assert( s._dynamicStatus == null || s._dynamicStatus.Value <= RunningStatus.Running );
                if( s.DynamicStatus == null ) s.StopBy( ServiceRunningStatusReason.StoppedByGeneralization );
                s = s.NextSpecialization;
            }

            if( Generalization != null ) Generalization.OnSpecializationStarted( this );
        }

        internal bool CanStart()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            Debug.Assert( _nbAllAvailablePlugins > 0 );
            Debug.Assert( FindFirstPluginData( p => p.CanStart() ) != null );
            return true;
        }
    }
}
