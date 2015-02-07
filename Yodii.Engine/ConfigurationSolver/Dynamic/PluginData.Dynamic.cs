#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\Dynamic\PluginData.Dynamic.cs) is part of CiviKey. 
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
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    partial class PluginData : IDynamicItem
    {
        RunningStatus? _dynamicStatus;
        PluginRunningStatusReason _dynamicReason;
        internal StartDependencyImpact DynamicImpact;

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } }

        /// <summary>
        /// Called after Service DynamicResetState.
        /// </summary>
        public void DynamicResetState()
        {
            DynamicImpact = ConfigSolvedImpact;
            switch( FinalConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled: 
                    {
                        _dynamicReason = PluginRunningStatusReason.StoppedByConfig;
                        _dynamicStatus = RunningStatus.Disabled;
                        break;
                    }
                case SolvedConfigurationStatus.Running: 
                    {
                        Debug.Assert( Service == null || (Service.Family.RunningPlugin == this) );
                        _dynamicReason = PluginRunningStatusReason.StartedByConfig;
                        _dynamicStatus = RunningStatus.RunningLocked;
                        break;
                    }
                default:
                    {
                        Debug.Assert( Service == null || !Service.Disabled );
                        _dynamicReason = PluginRunningStatusReason.None;
                        _dynamicStatus = null;
                        break;
                    }
            }
        }

        public bool DynamicStartByCommand( StartDependencyImpact impact, bool isFirst = false )
        {
            Debug.Assert( (impact & ConfigSolvedImpact) == ConfigSolvedImpact );
            
            if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
            Debug.Assert( CanStartOrIsStarted );
            if( isFirst )
            {
                DynamicImpact = impact;
            }
            else
            {
                // Use DynamicImpact to take service propagation into account.
                DynamicImpact |= impact.ToTryBits();
            }
            DynamicStartBy( PluginRunningStatusReason.StartedByCommand );
            return true;
        }

        internal bool CanStartOrIsStarted
        {
            get
            {
                if( _dynamicStatus.HasValue )
                {
                    return _dynamicStatus.Value >= RunningStatus.Running;
                }
                Debug.Assert( _finalConfigStartableStatus != null );
                if( !_finalConfigStartableStatus.CanStartWith( DynamicImpact ) ) return false;
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    ServiceData s = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                    switch( sRef.Requirement )
                    {
                        case DependencyRequirement.Running:
                            {
                                if( !s.CanStartOrIsStarted( false ) ) return false;
                                break;
                            }
                        case DependencyRequirement.RunnableRecommended:
                            {
                                if( (DynamicImpact & StartDependencyImpact.IsStartRunnableRecommended) != 0 && !s.CanStartOrIsStarted( false ) ) return false;
                                break;
                            }
                        case DependencyRequirement.Runnable:
                            {
                                if( (DynamicImpact & StartDependencyImpact.IsStartRunnableOnly) != 0 && !s.CanStartOrIsStarted( false ) ) return false;
                                break;
                            }
                        case DependencyRequirement.OptionalRecommended:
                            {
                                if( (DynamicImpact & StartDependencyImpact.IsStartOptionalRecommended) != 0 && !s.CanStartOrIsStarted( false ) ) return false;
                                break;
                            }
                        case DependencyRequirement.Optional:
                            {
                                if( (DynamicImpact & StartDependencyImpact.IsStartOptionalOnly) != 0 && !s.CanStartOrIsStarted( false ) ) return false;
                                break;
                            }
                    }
                }
                return true;
            }
        }

        public PluginRunningStatusReason GetStoppedReasonForStoppedReference( DependencyRequirement requirement )
        {
            StartDependencyImpact impact = _configSolvedImpact;
            switch( requirement )
            {
                case DependencyRequirement.Running: return PluginRunningStatusReason.StoppedByRunningReference;
                case DependencyRequirement.RunnableRecommended:
                    if( (impact&StartDependencyImpact.IsStartRunnableRecommended) != 0 )
                    {
                        return PluginRunningStatusReason.StoppedByRunnableRecommendedReference;
                    }
                    break;
                case DependencyRequirement.Runnable:
                    if( (impact & StartDependencyImpact.IsStartRunnableOnly) != 0 )
                    {
                        return PluginRunningStatusReason.StoppedByRunnableReference;
                    }
                    break;
                case DependencyRequirement.OptionalRecommended:
                    if( (impact & StartDependencyImpact.IsStartOptionalRecommended) != 0 )
                    {
                        return PluginRunningStatusReason.StoppedByOptionalRecommendedReference;
                    }
                    break;
                case DependencyRequirement.Optional:
                    if( (impact & StartDependencyImpact.IsStartOptionalOnly) != 0 )
                    {
                        return PluginRunningStatusReason.StoppedByOptionalReference;
                    }
                    break;
            }
            return PluginRunningStatusReason.None;
        }

        public bool DynamicStopByCommand()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value <= RunningStatus.Stopped;
            DynamicStopBy( PluginRunningStatusReason.StoppedByCommand );
            return true;
        }

        internal void DynamicStopBy( PluginRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            Debug.Assert( reason != PluginRunningStatusReason.None );
            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = reason;
            if( Service != null )
            {
                Service.OnPluginStopped( true );
                Service.OnPostPluginStopped();
            }
        }

        internal void DynamicStartBy( PluginRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running );
            Debug.Assert( CanStartOrIsStarted );
            if( _dynamicStatus == null )
            {
                _dynamicStatus = RunningStatus.Running;
                _dynamicReason = reason;
            }
            if( Service != null ) Service.OnDirectPluginStarted( this );

            List<ServiceData> tryStart = null;
            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                ServiceData s = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                if( s.DynamicStartByDependency( DynamicImpact, sRef.Requirement ) && s.DynamicStatus == null )
                {
                    if( tryStart == null ) tryStart = new List<ServiceData>();
                    tryStart.Add( s );
                }
            }
            if( tryStart != null )
            {
                foreach( var s in tryStart )
                {
                    if( s.DynamicStatus == null ) s.DynamicStartBy( ServiceRunningStatusReason.StartedByPropagation );
                }
            }
        }       

    }
}
