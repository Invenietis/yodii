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
        internal class DynamicPropagation : BasePropagation
        {
            public DynamicPropagation( StaticPropagation staticP )
                : base( staticP )
            {
            }

            public override void Refresh()
            {
                Refresh( Service._dynamicTotalAvailablePluginsCount, Service._dynamicAvailablePluginsCount, Service._dynamicAvailableServicesCount );
            }

            protected override bool IsValidPlugin( PluginData p )
            {
                return p.DynamicStatus == null;
            }

            protected override bool IsValidSpecialization( ServiceData s )
            {
                return s.DynamicStatus == null || s.DynamicStatus.Value >= RunningStatus.Running;
            }

            protected override BasePropagation GetPropagationInfo( ServiceData s )
            {
                return s.DynGetPropagationInfo();
            }

            internal bool TestCanStart( StartDependencyImpact impact )
            {
                Debug.Assert( Service.DynamicStatus == null && (Service.FinalConfigSolvedStatus == ConfigurationStatus.Runnable || Service.FinalConfigSolvedStatus == ConfigurationStatus.Optional ) );
                Debug.Assert( impact != StartDependencyImpact.Unknown );

                if( TheOnlyPlugin != null )
                {
                    if( !TheOnlyPlugin.DynamicCanStart( impact ) ) return false;
                }
                else
                {
                    foreach( var s in GetExcludedServices( impact ) )
                    {
                        if( s.DynamicStatus != null && s.DynamicStatus.Value >= RunningStatus.Running ) return false;
                    }
                    foreach( var s in GetIncludedServices( impact, false ) )
                    {
                        if( s.DynamicStatus != null && s.DynamicStatus.Value <= RunningStatus.Stopped ) return false;
                    }
                }
                return true;
            }

            internal void PropagateStart()
            {
                Debug.Assert( Service.DynamicStatus != null && Service.DynamicStatus.Value >= RunningStatus.Running );

                StartDependencyImpact impact = Service.ConfigSolvedImpact;

                if( TheOnlyPlugin != null )
                {
                    TheOnlyPlugin.DynamicStartBy( impact, PluginRunningStatusReason.StartedByRunningService );
                }
                else if( TheOnlyService != null )
                {
                    TheOnlyService.DynamicStartBy( ServiceRunningStatusReason.StartedByPropagation );
                }
                else
                {
                    foreach( var s in GetExcludedServices( impact ) )
                    {
                        Debug.Assert( s.DynamicStatus == null || s.DynamicStatus.Value <= RunningStatus.Stopped );
                        if( s.DynamicStatus == null ) s.DynamicStopBy( ServiceRunningStatusReason.StoppedByPropagation );
                    }
                    foreach( var s in GetIncludedServices( impact, false ) )
                    {
                        Debug.Assert( s.DynamicStatus == null || s.DynamicStatus.Value >= RunningStatus.Running );
                        if( s.DynamicStatus == null ) s.DynamicStartBy( ServiceRunningStatusReason.StartedByPropagation );
                    }
                }
            }
        }

        DynamicPropagation _dynPropagation;

        public void DynPropagateStart()
        {
            if( DynamicStatus != null
                && DynamicStatus.Value >= RunningStatus.Running
                && Family.DynRunningPlugin == null
                && (Family.DynRunningService == null || !this.IsStrictGeneralizationOf( Family.DynRunningService )) )
            {
                var p = DynGetPropagationInfo();
                if( p != null ) p.PropagateStart();
            }
        }

        DynamicPropagation DynGetPropagationInfo()
        {
            if( DynamicStatus != null && DynamicStatus.Value <= RunningStatus.Stopped ) return null;
            Debug.Assert( _propagation != null );
            if( _dynPropagation == null ) _dynPropagation = new DynamicPropagation( _propagation );
            _dynPropagation.Refresh();
            return _dynPropagation;
        }
    }
}
