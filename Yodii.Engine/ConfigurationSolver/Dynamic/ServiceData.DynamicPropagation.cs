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
                Refresh( Service._nbAllAvailablePlugins );
            }

            protected override bool IsValidPlugin( PluginData p )
            {
                return p.DynamicStatus == null;
            }

            protected override BasePropagation GetUsefulPropagationInfo( ServiceData s )
            {
                return s.DynGetUsefulPropagationInfo();
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

                StartDependencyImpact impact = Service._dynFirstImpact;
                if( impact == StartDependencyImpact.Unknown ) impact = Service.ConfigSolvedImpact;
                if( impact == StartDependencyImpact.Unknown ) impact = StartDependencyImpact.Minimal;

                if( TheOnlyPlugin != null )
                {
                    TheOnlyPlugin.DynamicStartBy( impact, PluginRunningStatusReason.StartedByRunningService );
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

        public bool DynPropagationIsUseless
        {
            get
            {
                return (DynamicStatus != null && DynamicStatus.Value <= RunningStatus.Stopped)
                        || Family.DynRunningPlugin != null
                        || (Family.DynRunningService != null && this.IsStrictGeneralizationOf( Family.DynRunningService ));
            }
        }

        public void DynPropagateStart()
        {
            var p = DynGetUsefulPropagationInfo();
            if( p != null ) p.PropagateStart();
        }

        DynamicPropagation DynGetUsefulPropagationInfo()
        {
            if( DynPropagationIsUseless ) return null;
            Debug.Assert( _propagation != null );
            if( _dynPropagation == null ) _dynPropagation = new DynamicPropagation( _propagation );
            else _dynPropagation.Refresh();
            return _dynPropagation;
        }
    }
}
