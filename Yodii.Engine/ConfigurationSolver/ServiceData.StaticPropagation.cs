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
                Refresh( Service.TotalAvailablePluginCount, Service.AvailablePluginCount, Service.AvailableServiceCount );
            }

            protected override bool IsValidPlugin( PluginData p )
            {
                return !p.Disabled;
            }

            protected override bool IsValidSpecialization( ServiceData s )
            {
                return !s.Disabled;
            }

            protected override BasePropagation GetPropagationInfo( ServiceData s )
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
                else if( TheOnlyService != null )
                {
                    if( !TheOnlyService.SetSolvedStatus( Service.ConfigSolvedStatus, ServiceSolvedConfigStatusReason.FromServiceToSingleSpecialization ) )
                    {
                        if( !Service.Disabled ) Service.SetDisabled( ServiceDisabledReason.PropagationToSingleSpecializationFailed );
                        else Service._configDisabledReason = ServiceDisabledReason.PropagationToSingleSpecializationFailed;
                        return false;
                    }
                }
                else
                {
                    StartDependencyImpact impact = Service.ConfigSolvedImpact;
                    Debug.Assert( impact != StartDependencyImpact.Unknown && (impact & StartDependencyImpact.IsTryOnly) == 0 );

                    foreach( var s in GetIncludedServices( impact, Service.ConfigSolvedStatus == ConfigurationStatus.Runnable ) )
                    {
                        if( !s.SetSolvedStatus( Service.ConfigSolvedStatus, ServiceSolvedConfigStatusReason.FromPropagation ) )
                        {
                            if( !Service.Disabled ) Service.SetDisabled( ServiceDisabledReason.PropagationToCommonPluginReferencesFailed );
                            else Service._configDisabledReason = ServiceDisabledReason.PropagationToCommonPluginReferencesFailed;
                            return false;
                        }
                    }
                    if( !Service.Disabled )
                    {
                        foreach( var s in GetExcludedServices( impact ) )
                        {
                            if( !s.Disabled ) s.SetDisabled( ServiceDisabledReason.StopppedByPropagation );
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
                return Disabled
                        || Family.Solver.Step < ConfigurationSolverStep.OnAllPluginsAdded 
                        || Family.RunningPlugin != null; 
            }
        }

        public bool PropagateSolvedStatus()
        {
            if( ConfigSolvedStatus >= ConfigurationStatus.Runnable )
            {
                var p = GetUsefulPropagationInfo();
                if( p != null ) return p.PropagateSolvedStatus();
            }
            return true;
        }

        StaticPropagation GetUsefulPropagationInfo()
        {
            if( Family.Solver.Step == ConfigurationSolverStep.InitializeFinalStartableStatus )
            {
                if( Disabled ) return null;
            }
            else if( PropagationIsUseless ) return null;
            if( _propagation == null ) _propagation = new StaticPropagation( this );
            _propagation.Refresh();
            return _propagation;
        }
        
    }
}
