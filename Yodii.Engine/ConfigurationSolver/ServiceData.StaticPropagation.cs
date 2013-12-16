using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
using Yodii.Model;
using Yodii.Engine;
using CK.Core;

namespace Yodii.Engine
{
    partial class ServiceData
    {
        internal class StaticPropagation : BasePropagation, IServiceDependentObject
        {
            public StaticPropagation( ServiceData s )
                : base( s )
            {
            }

            public override void Refresh()
            {
                Refresh( Service.TotalAvailablePluginCount );
            }

            protected override bool IsValidPlugin( PluginData p )
            {
                return !p.Disabled;
            }

            protected override BasePropagation GetUsefulPropagationInfo( ServiceData s )
            {
                return s.GetUsefulPropagationInfo();
            }

            public bool PropagateSolvedStatus()
            {
                Debug.Assert( Service.FinalConfigSolvedStatus >= ConfigurationStatus.Runnable );
                if( TheOnlyPlugin != null )
                {
                    if( !TheOnlyPlugin.SetSolvedStatus( Service.ConfigSolvedStatus, PluginRunningRequirementReason.FromServiceToSinglePlugin ) )
                    {
                        if( !Service.Disabled ) Service.SetDisabled( ServiceDisabledReason.PropagationToSinglePluginFailed );
                        else Service._configDisabledReason = ServiceDisabledReason.PropagationToSinglePluginFailed;
                        return false;
                    }
                }
                else
                {
                    StartDependencyImpact impact = Service.ConfigSolvedImpact;
                    if( impact == StartDependencyImpact.Unknown ) impact = StartDependencyImpact.Minimal; 

                    foreach( var s in GetExcludedServices( impact ) )
                    {
                        if( !s.Disabled ) s.SetDisabled( ServiceDisabledReason.StopppedByPropagation );
                    }
                    if( !Service.Disabled )
                    {
                        foreach( var s in GetIncludedServices( impact, Service.ConfigSolvedStatus == ConfigurationStatus.Runnable ) )
                        {
                            if( !s.SetSolvedStatus( Service.ConfigSolvedStatus, ServiceSolvedConfigStatusReason.FromPropagation ) )
                            {
                                if( !Service.Disabled ) Service.SetDisabled( ServiceDisabledReason.PropagationToCommonPluginReferencesFailed );
                                else Service._configDisabledReason = ServiceDisabledReason.PropagationToCommonPluginReferencesFailed;
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

            public ConfigurationStatus FinalConfigSolvedStatus
            {
                get { return Service.FinalConfigSolvedStatus; }
            }

            public StartDependencyImpact ConfigSolvedImpact
            {
                get { return Service.ConfigSolvedImpact; }
            }

        }

        StaticPropagation _propagation;

        public bool PropagationIsUseless
        {
            get 
            { 
                return FinalConfigSolvedStatus <= ConfigurationStatus.Runnable 
                        || !Family.AllPluginsHaveBeenAdded 
                        || Family.RunningPlugin != null 
                        || (Family.RunningService != null && this.IsStrictGeneralizationOf( Family.RunningService )); 
            }
        }

        public bool PropagateSolvedStatus()
        {
            var p = GetUsefulPropagationInfo();
            if( p != null ) return p.PropagateSolvedStatus();
            return true;
        }

        StaticPropagation GetUsefulPropagationInfo()
        {
            if( Family.Solver.Step == ConfigurationSolverStep.InitializeFinalStartableStatus )
            {
                if( Disabled || FinalConfigSolvedStatus == ConfigurationStatus.Running ) return null;
            }
            else if( PropagationIsUseless ) return null;
            if( _propagation == null ) _propagation = new StaticPropagation( this );
            _propagation.Refresh();
            return _propagation;
        }
        
    }
}
