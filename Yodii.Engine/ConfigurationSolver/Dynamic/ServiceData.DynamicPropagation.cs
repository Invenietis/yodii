#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\Dynamic\ServiceData.DynamicPropagation.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
                return p.DynamicStatus == null || p.DynamicStatus.Value >= RunningStatus.Running;
            }

            protected override bool IsValidSpecialization( ServiceData s )
            {
                return s.DynamicStatus == null || s.DynamicStatus.Value >= RunningStatus.Running;
            }

            protected override BasePropagation GetPropagationInfo( ServiceData s )
            {
                return s.DynGetPropagationInfo();
            }

            //internal bool TestCanStart( StartDependencyImpact impact )
            //{
            //    Debug.Assert( Service.DynamicStatus == null || Service.DynamicStatus.Value == RunningStatus.RunningLocked );
            //    Debug.Assert( impact != StartDependencyImpact.Unknown );

            //    if( TheOnlyPlugin != null )
            //    {
            //        if( !TheOnlyPlugin.DynamicCanStart( impact ) ) return false;
            //    }
            //    else if( TheOnlyService != null )
            //    {
            //        if( !TheOnlyService.DynamicCanStart( impact ) ) return false;
            //    }
            //    else
            //    {
            //        foreach( var s in GetExcludedServices( impact ) )
            //        {
            //            if( s.DynamicStatus != null && s.DynamicStatus.Value >= RunningStatus.Running ) return false;
            //        }
            //        foreach( var s in GetIncludedServices( impact ) )
            //        {
            //            if( s.DynamicStatus != null && s.DynamicStatus.Value <= RunningStatus.Stopped ) return false;
            //        }
            //    }
            //    return true;
            //}

            internal void PropagateStart()
            {
                Debug.Assert( Service.DynamicStatus != null && Service.DynamicStatus.Value >= RunningStatus.Running );

                if( TheOnlyPlugin != null )
                {
                    TheOnlyPlugin.DynamicStartBy( PluginRunningStatusReason.StartedByRunningService );
                }
                else if( TheOnlyService != null )
                {
                    TheOnlyService.DynamicStartBy( ServiceRunningStatusReason.StartedByPropagation );
                }
                else
                {
                    foreach( var s in GetExcludedServices( Service._dynamicImpact ) )
                    {
                        Debug.Assert( s.DynamicStatus == null || s.DynamicStatus.Value <= RunningStatus.Stopped );
                        if( s.DynamicStatus == null ) s.DynamicStopBy( ServiceRunningStatusReason.StoppedByPropagation );
                    }
                    foreach( var s in GetIncludedServices( Service._dynamicImpact ) )
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

        public void DynFillTransitiveIncludedServices( HashSet<ServiceData> set )
        {
            if( !set.Add( this ) ) return;
            var propagation = DynGetPropagationInfo();
            if( propagation == null ) return;
            foreach( var s in propagation.GetIncludedServices( ConfigSolvedImpact ) )
            {
                s.DynFillTransitiveIncludedServices( set );
            }
        }
    }
}
