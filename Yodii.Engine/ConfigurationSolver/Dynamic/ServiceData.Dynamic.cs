#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\Dynamic\ServiceData.Dynamic.cs) is part of CiviKey. 
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
    partial class ServiceData
    {
        RunningStatus? _dynamicStatus;
        ServiceRunningStatusReason _dynamicReason;
        int _dynamicTotalAvailablePluginsCount;
        int _dynamicAvailablePluginsCount;
        int _dynamicAvailableServicesCount;
        StartDependencyImpact _dynamicImpact;

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } }

        /// <summary>
        /// Called before DynamicResetState on plugins.
        /// </summary>
        public void DynamicResetState()
        {
            _dynPropagation = null;
            _dynamicImpact = ConfigSolvedImpact;
            _dynamicTotalAvailablePluginsCount = TotalAvailablePluginCount;
            _dynamicAvailablePluginsCount = AvailablePluginCount;
            _dynamicAvailableServicesCount = AvailableServiceCount;
            switch( FinalConfigSolvedStatus )
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
            ServiceData s = FirstSpecialization;
            while( s != null )
            {
                s.DynamicResetState();
                s = s.NextSpecialization;
            }
        }

        [Conditional("DEBUG")]
        public void DebugCheckOnAllPluginsDynamicStateInitialized()
        {
            // Iterate across all services (even disabled ones).
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                spec.DebugCheckOnAllPluginsDynamicStateInitialized();
                spec = spec.NextSpecialization;
            }
            switch( FinalConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled:
                    {
                        Debug.Assert( _dynamicTotalAvailablePluginsCount == 0 );
                        Debug.Assert( _dynamicStatus == RunningStatus.Disabled );
                        break;
                    }
                case SolvedConfigurationStatus.Running:
                    {
                        Debug.Assert( _dynamicTotalAvailablePluginsCount > 0 );
                        Debug.Assert( _dynamicStatus == RunningStatus.RunningLocked );
                        break;
                    }
                default:
                    {
                        Debug.Assert( _dynamicTotalAvailablePluginsCount > 0 );
                        Debug.Assert( _dynamicStatus == null );
                        break;
                    }
            }
        }
    
        internal void DynamicStartBy( ServiceRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value >= RunningStatus.Running );
            Debug.Assert( CanStartOrIsStarted );
            Debug.Assert( reason == ServiceRunningStatusReason.StartedByCommand
                            || reason == ServiceRunningStatusReason.StartedByPropagation
                            || reason == ServiceRunningStatusReason.StartedByOptionalReference
                            || reason == ServiceRunningStatusReason.StartedByOptionalRecommendedReference
                            || reason == ServiceRunningStatusReason.StartedByRunnableReference
                            || reason == ServiceRunningStatusReason.StartedByRunnableRecommendedReference
                            || reason == ServiceRunningStatusReason.StartedByRunningReference
                            );
            if( _dynamicStatus == null )
            {
                Family.DynamicSetRunningService( this, reason );
                DynPropagateStart();
            }
        }

        public bool DynamicStartByCommand( StartDependencyImpact impact, bool isFirst = false )
        {
            Debug.Assert( (impact&ConfigSolvedImpact) == ConfigSolvedImpact );
            if( isFirst )
            {
                Debug.Assert( (_dynamicStatus == null || _dynamicStatus == RunningStatus.RunningLocked) && _finalConfigStartableStatus.CanStartWith( impact ) );
                SetDynamicImpact( impact );
            }
            else
            {
                if( _dynamicStatus != null ) return _dynamicStatus.Value >= RunningStatus.Running;
                Debug.Assert( CanStartOrIsStarted );
                SetDynamicImpact( impact.ToTryBits() );
            }
            DynamicStartBy( ServiceRunningStatusReason.StartedByCommand );
            return true;
        }

        void SetDynamicImpact( StartDependencyImpact impact )
        {
            _dynamicImpact |= impact;
            ServiceData s = FirstSpecialization;
            while( s != null )
            {
                if( !s.Disabled ) s.SetDynamicImpact( impact );
                s = s.NextSpecialization;
            }
            PluginData p = FirstPlugin;
            while( p != null )
            {
                if( p.DynamicStatus == null )
                {
                    p.DynamicImpact |= impact;
                    if( !p.CanStartOrIsStarted )
                    {
                        p.DynamicStopBy( PluginRunningStatusReason.StoppedByServiceCommandImpact );
                    }
                }
                p = p.NextPluginForService;
            }
        }

        internal bool CanStartOrIsStarted
        {
            get
            {
                if( _dynamicStatus.HasValue )
                {
                    return _dynamicStatus.Value >= RunningStatus.Running;
                }
                return FindFirstPluginData( p => p.CanStartOrIsStarted ) != null;
            }
        }

        public bool DynamicStopByCommand()
        {
            if( _dynamicStatus != null ) return _dynamicStatus.Value <= RunningStatus.Stopped;
            DynamicStopBy( ServiceRunningStatusReason.StoppedByCommand );
            return true;
        }

        internal bool DynamicStartByDependency( StartDependencyImpact impact, DependencyRequirement req )
        {
            switch( req )
            {
                case DependencyRequirement.Running:
                    {
                        Debug.Assert( CanStartOrIsStarted );
                        if( _dynamicStatus == null ) DynamicStartBy( ServiceRunningStatusReason.StartedByRunningReference );
                        break;
                    }
                case DependencyRequirement.RunnableRecommended:
                    {
                        if( (impact & StartDependencyImpact.IsStartRunnableRecommended) != 0 )
                        {
                            Debug.Assert( CanStartOrIsStarted );
                            if( _dynamicStatus == null ) DynamicStartBy( ServiceRunningStatusReason.StartedByRunnableRecommendedReference );
                        }
                        else if( (impact & StartDependencyImpact.IsTryStartRunnableRecommended) != 0 ) return true;
                        break;
                    }
                case DependencyRequirement.Runnable:
                    {
                        if( (impact & StartDependencyImpact.IsStartRunnableOnly) != 0 )
                        {
                            Debug.Assert( CanStartOrIsStarted );
                            if( _dynamicStatus == null ) DynamicStartBy( ServiceRunningStatusReason.StartedByRunnableReference );
                        }
                        else if( (impact & StartDependencyImpact.IsTryStartRunnableOnly) != 0 ) return true;
                        break;
                    }
                case DependencyRequirement.OptionalRecommended:
                    {
                        if( (impact & StartDependencyImpact.IsStartOptionalRecommended) != 0 )
                        {
                            Debug.Assert( CanStartOrIsStarted );
                            if( _dynamicStatus == null ) DynamicStartBy( ServiceRunningStatusReason.StartedByOptionalRecommendedReference );
                        }
                        else if( (impact & StartDependencyImpact.IsTryStartOptionalRecommended) != 0 ) return true;
                        break;
                    }
                case DependencyRequirement.Optional:
                    {
                        if( (impact & StartDependencyImpact.IsStartOptionalOnly) != 0 )
                        {
                            Debug.Assert( CanStartOrIsStarted );
                            if( _dynamicStatus == null ) DynamicStartBy( ServiceRunningStatusReason.StartedByOptionalReference );
                        }
                        else if( (impact & StartDependencyImpact.IsTryStartOptionalOnly) != 0 ) return true;
                        break;
                    }
            }
            return false;
        }

        internal void DynamicStopBy( ServiceRunningStatusReason reason )
        {
            Debug.Assert( _dynamicStatus == null );
            Debug.Assert( reason == ServiceRunningStatusReason.StoppedByGeneralization
                        || reason == ServiceRunningStatusReason.StoppedByCommand
                        || reason == ServiceRunningStatusReason.StoppedByPluginStopped
                        || reason == ServiceRunningStatusReason.StoppedBySiblingRunningService
                        || reason == ServiceRunningStatusReason.StoppedByOptionalReference
                        || reason == ServiceRunningStatusReason.StoppedByOptionalRecommendedReference
                        || reason == ServiceRunningStatusReason.StoppedByRunnableReference
                        || reason == ServiceRunningStatusReason.StoppedByRunnableRecommendedReference
                        || reason == ServiceRunningStatusReason.StoppedByFinalDecision );

            _dynamicStatus = RunningStatus.Stopped;
            _dynamicReason = reason;
            for( int i = 0; i < _inheritedServicesWithThis.Length - 1; ++i )
                --_inheritedServicesWithThis[i]._dynamicAvailableServicesCount;

            if( _dynamicTotalAvailablePluginsCount > 0 )
            {
                // Stops the specialized services.
                ServiceData child = FirstSpecialization;
                while( child != null )
                {
                    Debug.Assert( child.DynamicStatus == null || child.DynamicStatus.Value <= RunningStatus.Stopped );
                    if( child.DynamicStatus == null ) child.DynamicStopBy( ServiceRunningStatusReason.StoppedByGeneralization );
                    child = child.NextSpecialization;
                }
                // Stops all the plugins.
                PluginData p = FirstPlugin;
                while( p != null )
                {
                    Debug.Assert( p.DynamicStatus == null || p.DynamicStatus.Value <= RunningStatus.Stopped );
                    if( p.DynamicStatus == null ) p.DynamicStopBy( PluginRunningStatusReason.StoppedByStoppedService );
                    p = p.NextPluginForService;
                }
                Debug.Assert( _dynamicTotalAvailablePluginsCount == 0 );
            }
            foreach( var backRef in _backReferences )
            {
                Debug.Assert( backRef.PluginData.DynamicStatus == null 
                                || backRef.PluginData.DynamicStatus.Value <= RunningStatus.Stopped
                                || (backRef.Requirement == DependencyRequirement.Optional && (backRef.PluginData.ConfigSolvedImpact & StartDependencyImpact.IsStartOptionalOnly) == 0)
                                || (backRef.Requirement == DependencyRequirement.OptionalRecommended && (backRef.PluginData.ConfigSolvedImpact & StartDependencyImpact.IsStartOptionalRecommended) == 0)
                                || (backRef.Requirement == DependencyRequirement.Runnable && (backRef.PluginData.ConfigSolvedImpact & StartDependencyImpact.IsStartRunnableOnly) == 0)
                                || (backRef.Requirement == DependencyRequirement.RunnableRecommended && (backRef.PluginData.ConfigSolvedImpact & StartDependencyImpact.IsStartRunnableRecommended) == 0)
                                || backRef.Requirement != DependencyRequirement.Running );
                if( backRef.PluginData.DynamicStatus == null )
                {
                    PluginRunningStatusReason r = backRef.PluginData.GetStoppedReasonForStoppedReference( backRef.Requirement );
                    if( r != PluginRunningStatusReason.None ) backRef.PluginData.DynamicStopBy( r );
                }
            }
        }

        internal void OnPluginStopped( bool direct )
        {
            if( Generalization != null ) Generalization.OnPluginStopped( false );
            Debug.Assert( _dynamicTotalAvailablePluginsCount > 0 );
            --_dynamicTotalAvailablePluginsCount;
            if( direct ) --_dynamicAvailablePluginsCount;
        }

        internal void OnPostPluginStopped()
        {
            if( _dynamicTotalAvailablePluginsCount == 0 )
            {
                Debug.Assert( _dynamicStatus == null || _dynamicStatus.Value <= RunningStatus.Stopped );
                if( _dynamicStatus == null ) DynamicStopBy( ServiceRunningStatusReason.StoppedByPluginStopped );
            }
            else if( _dynamicStatus != null && _dynamicStatus.Value >= RunningStatus.Running )
            {
                DynPropagateStart();
                //Family.Solver.DeferPropagation( this );
            }
            if( Generalization != null ) Generalization.OnPostPluginStopped();
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
            Debug.Assert( runningPlugin.DynamicStatus.HasValue && runningPlugin.DynamicStatus.Value >= RunningStatus.Running );
            Family.DynamicSetRunningPlugin( runningPlugin );
        }


    }
}
