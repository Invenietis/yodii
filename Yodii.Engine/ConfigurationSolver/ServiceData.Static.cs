#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\ServiceData.Static.cs) is part of CiviKey. 
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
using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace Yodii.Engine
{
    internal partial class ServiceData : IYodiiItemData
    {
        ServiceData[] _inheritedServicesWithThis;
        ServiceDisabledReason _configDisabledReason;
        
        SolvedConfigurationStatus _configSolvedStatus;
        ServiceSolvedConfigStatusReason _configSolvedStatusReason;
        readonly StartDependencyImpact _configSolvedImpact;
        readonly List<BackReference> _backReferences;
        FinalConfigStartableStatus _finalConfigStartableStatus;

        ServiceData( IServiceInfo s, ConfigurationStatus serviceStatus, StartDependencyImpact impact, ServiceData generalization )
        {
            _backReferences = new List<BackReference>();
            ServiceInfo = s;
            ConfigOriginalStatus = serviceStatus;
            Generalization = generalization;
            // Impact
            _configSolvedImpact = ConfigOriginalImpact = impact;
            if( generalization != null )
            {
                _configSolvedImpact = (ConfigOriginalImpact | generalization.ConfigSolvedImpact).ClearUselessTryBits();
            }
            if( _configSolvedImpact == StartDependencyImpact.Unknown )
            {
                _configSolvedImpact = StartDependencyImpact.Minimal;
            }
        }

        internal ServiceData( IServiceInfo s, ServiceData generalization, ConfigurationStatus serviceStatus, StartDependencyImpact impact = StartDependencyImpact.Unknown )
            : this( s, serviceStatus, impact, generalization )
        {
            Family = generalization.Family;
            _inheritedServicesWithThis = new ServiceData[generalization._inheritedServicesWithThis.Length + 1];
            generalization._inheritedServicesWithThis.CopyTo( _inheritedServicesWithThis, 0 );
            _inheritedServicesWithThis[_inheritedServicesWithThis.Length - 1] = this;
            NextSpecialization = Generalization.FirstSpecialization;
            Generalization.FirstSpecialization = this;
            Initialize();
            if( !Disabled ) 
                for( int i = 0; i < _inheritedServicesWithThis.Length - 1; ++i ) 
                    ++_inheritedServicesWithThis[i].AvailableServiceCount;
        }

        internal ServiceData( IConfigurationSolver solver, IServiceInfo s, ConfigurationStatus serviceStatus, StartDependencyImpact impact = StartDependencyImpact.Unknown )
            : this( s, serviceStatus, impact, null )
        {
            Family = new ServiceFamily( solver, this );
            _inheritedServicesWithThis = new ServiceData[] { this };
            Initialize();
        }

        void Initialize()
        {
            switch( ConfigOriginalStatus )
            {
                case ConfigurationStatus.Disabled: _configSolvedStatus = SolvedConfigurationStatus.Disabled; break;
                case ConfigurationStatus.Running: _configSolvedStatus = SolvedConfigurationStatus.Running; break;
                default: _configSolvedStatus = SolvedConfigurationStatus.Runnable; break;
            }
            _configSolvedStatusReason = ServiceSolvedConfigStatusReason.Config;
            if( ConfigOriginalStatus == ConfigurationStatus.Disabled )
            {
                _configDisabledReason = ServiceDisabledReason.Config;
            }
            else if( ServiceInfo.HasError )
            {
                _configDisabledReason = ServiceDisabledReason.ServiceInfoHasError;
            }
            else if( Generalization != null && Generalization.Disabled )
            {
                _configDisabledReason = ServiceDisabledReason.GeneralizationIsDisabledByConfig;
            }

            if( Disabled )
            {
                _configSolvedStatusReason = ServiceSolvedConfigStatusReason.Config;
            }
            else if( ConfigOriginalStatus == ConfigurationStatus.Running )
            {
                Family.SetRunningService( this, ServiceSolvedConfigStatusReason.Config );
            }
            else if( Family.RunningService != null && !Family.RunningService.IsStrictGeneralizationOf( this ) )
            {
                _configDisabledReason = ServiceDisabledReason.AnotherServiceIsRunningByConfig;
            }
        }

        public bool IsPlugin { get { return false; } }

        /// <summary>
        /// The ServiceInfo discovered object.
        /// </summary>
        public readonly IServiceInfo ServiceInfo;

        /// <summary>
        /// Never null.
        /// </summary>
        public readonly ServiceData.ServiceFamily Family;

        /// <summary>
        /// Gets whether this service is disabled. 
        /// </summary>
        public bool Disabled
        {
            get { return _configDisabledReason != ServiceDisabledReason.None; }
        }

        /// <summary>
        /// Gets the inherited services including this one.
        /// </summary>
        public IEnumerable<ServiceData> InheritedServicesWithThis
        {
            get { return _inheritedServicesWithThis; }
        }

        /// <summary>
        /// Head of the linked list of ServiceData that specialize Service.
        /// </summary>
        public ServiceData FirstSpecialization;

        /// <summary>
        /// Linked list to another ServiceData that specialize Service.
        /// </summary>
        public readonly ServiceData NextSpecialization;

        /// <summary>
        /// Head of the linked list of PluginData that implement this exact Service (not specialized ones).
        /// </summary>
        public PluginData FirstPlugin;

        /// <summary>
        /// The direct generalization if any.
        /// </summary>
        public readonly ServiceData Generalization;

        /// <summary>
        /// The ConfigurationStatus of the Service. 
        /// </summary>
        public readonly ConfigurationStatus ConfigOriginalStatus;

        /// <summary>
        /// The original, configured, StartDependencyImpact of the service itself.
        /// </summary>
        public readonly StartDependencyImpact ConfigOriginalImpact;

        /// <summary>
        /// The solved StartDependencyImpact: it is this ConfigOriginalImpact if known or the Generalization's one if it exists.
        /// </summary>
        public StartDependencyImpact ConfigSolvedImpact
        {
            get { return _configSolvedImpact; }
        }

        /// <summary>
        /// Number of direct specialization that are not disabled.
        /// </summary>
        public int AvailableServiceCount;

        /// <summary>
        /// Number of plugins for this exact service.
        /// </summary>
        public int PluginCount;

        /// <summary>
        /// Number of plugins for this exact service that are disabled.
        /// </summary>
        public int DisabledPluginCount;

        /// <summary>
        /// Gets the number of available plugins for this exact service ((<see cref="PluginCount"/> - <see cref="DisabledPluginCount"/>).
        /// </summary>
        public int AvailablePluginCount
        {
            get { return PluginCount - DisabledPluginCount; }
        }

        /// <summary>
        /// Number of total plugins (the ones for this service and for any of our specializations).
        /// </summary>
        public int TotalPluginCount;

        /// <summary>
        /// Number of total plugins that are disabled (the ones for this service and for any of our specializations).
        /// </summary>
        public int TotalDisabledPluginCount;

        /// <summary>
        /// Gets the number of available plugins (<see cref="TotalPluginCount"/> - <see cref="TotalDisabledPluginCount"/>)
        /// for this service and its specializations.
        /// </summary>
        public int TotalAvailablePluginCount
        {
            get { return TotalPluginCount - TotalDisabledPluginCount; }
        }

        /// <summary>
        /// Gets all the services in the family except this one, its ancestors and its specializations.
        /// Never null.
        /// </summary>
        internal IEnumerable<ServiceData> DirectExcludedServices
        {
            get { return Family.AvailableServices.Where( s => !s.IsGeneralizationOf( this ) && !this.IsStrictGeneralizationOf( s ) ); }
        }

        /// <summary>
        /// Tests whether this service is a generalization of another one.
        /// </summary>
        /// <param name="s">Service that is a potential specialization.</param>
        /// <returns>True if this generalizes <paramref name="s"/>.</returns>
        internal bool IsStrictGeneralizationOf( ServiceData s )
        {
            var g = s.Generalization;
            if( g == null || s.Family != Family ) return false;
            do
            {
                if( g == this ) return true;
                g = g.Generalization;
            }
            while( g != null );
            return false;
        }

        internal bool IsGeneralizationOf( ServiceData d )
        {
            return this == d || IsStrictGeneralizationOf( d );
        }

        /// <summary>
        /// Gets the first reason why this service is disabled. 
        /// </summary>
        public string DisabledReason
        {
            get { return _configDisabledReason == ServiceDisabledReason.None ? null : _configDisabledReason.ToString(); }
        }

        /// <summary>
        /// Gets the minimal running requirement. It is initialized by the configuration, but may evolve.
        /// </summary>
        public SolvedConfigurationStatus ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        /// <summary>
        /// Gets the ConfigSolvedStatus that is ConfigurationStatus.Disabled if the service is actually Disabled.
        /// </summary>
        public SolvedConfigurationStatus FinalConfigSolvedStatus
        {
            get { return _configDisabledReason == ServiceDisabledReason.None ? _configSolvedStatus : SolvedConfigurationStatus.Disabled; }
        }

        struct BackReference
        {
            public readonly PluginData PluginData;
            public readonly DependencyRequirement Requirement;

            public BackReference( PluginData p, DependencyRequirement r )
            {
                PluginData = p;
                Requirement = r;
            }
        }

        internal void RegisterPluginReference( PluginData p, DependencyRequirement r )
        {
            _backReferences.Add( new BackReference( p, r ) );
        }

        internal void SetDisabled( ServiceDisabledReason r )
        {
            Debug.Assert( r != ServiceDisabledReason.None );
            Debug.Assert( _configDisabledReason == ServiceDisabledReason.None );
            _configDisabledReason = r;

            for( int i = 0; i < _inheritedServicesWithThis.Length - 1; ++i )
                --_inheritedServicesWithThis[i].AvailableServiceCount;

            if( Family.Solver.Step > ConfigurationSolverStep.OnAllPluginsAdded )
            {
                Debug.Assert( Family.AvailableServices.Contains( this ) );
                Family.AvailableServices.Remove( this );
            }
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                if( !spec.Disabled ) spec.SetDisabled( ServiceDisabledReason.GeneralizationIsDisabled );
                spec = spec.NextSpecialization;
            }
            PluginData plugin = FirstPlugin;
            while( plugin != null )
            {
                if( !plugin.Disabled ) plugin.SetDisabled( PluginDisabledReason.ServiceIsDisabled );
                plugin = plugin.NextPluginForService;
            }
            foreach( var backRef in _backReferences )
            {
                PluginDisabledReason reason = backRef.PluginData.GetDisableReasonForDisabledReference( backRef.Requirement );
                if( reason != PluginDisabledReason.None && !backRef.PluginData.Disabled ) backRef.PluginData.SetDisabled( reason );
            }

            if( Family.RunningService == this )
            {
                ServiceData g = Generalization;
                while( g != null )
                {
                    if( !g.Disabled ) g.SetDisabled( ServiceDisabledReason.RunningServiceDisabled );
                    g = g.Generalization;
                }
            }

            Debug.Assert( Family.RunningService != this || _inheritedServicesWithThis.All( s => s.Disabled ), "If we were the RunningService, no one else is running." );
        }

        internal bool SetRunningStatus( ServiceSolvedConfigStatusReason reason )
        {
            if( _configSolvedStatus == SolvedConfigurationStatus.Running ) return !Disabled;
            if( !Family.SetRunningService( this, reason ) ) return false;
            PropagateSolvedStatus();
            return !Disabled;
        }

        internal PluginData FindFirstPluginData( Func<PluginData, bool> filter )
        {
            PluginData p = FirstPlugin;
            while( p != null )
            {
                if( filter( p ) ) return p;
                p = p.NextPluginForService;
            }
            ServiceData s = FirstSpecialization;
            while( s != null )
            {
                p = s.FindFirstPluginData( filter );
                if( p != null ) return p;
                s = s.NextSpecialization;
            }
            return null;
        }

        internal void AddPlugin( PluginData p )
        {
            p.NextPluginForService = FirstPlugin;
            FirstPlugin = p;
            ++PluginCount;
            if( p.Disabled ) ++DisabledPluginCount;
            ServiceData g = this;
            do
            {
                ++g.TotalPluginCount;
                if( p.Disabled ) ++g.TotalDisabledPluginCount;
                g = g.Generalization;
            }
            while( g != null );
        }

        internal void OnAllPluginsAdded( Action<ServiceData> availableServiceCollector )
        {
            Debug.Assert( !Disabled, "Must NOT be called on already disabled service." );

            // First, disables a service without plugins.
            if( TotalPluginCount == 0 )
            {
                SetDisabled( ServiceDisabledReason.NoPlugin );
            }
            else if( TotalAvailablePluginCount == 0 )
            {
                SetDisabled( ServiceDisabledReason.AllPluginsAreDisabled );
            }

            if( !Disabled )
            {
                ServiceData spec = FirstSpecialization;
                while( spec != null )
                {
                    if( !spec.Disabled ) spec.OnAllPluginsAdded( availableServiceCollector );
                    spec = spec.NextSpecialization;
                }
                if( !Disabled )
                {
                    availableServiceCollector( this );
                    Family.Solver.DeferPropagation( this );
                }
            }
        }

        internal void OnPluginDisabled( PluginData p )
        {
            Debug.Assert( this.IsGeneralizationOf( p.Service ) && p.Disabled );
            if( p.Service == this ) ++DisabledPluginCount;
            ++TotalDisabledPluginCount;
            if( Generalization != null ) Generalization.OnPluginDisabled( p );
            if( !Disabled && Family.Solver.Step >= ConfigurationSolverStep.OnAllPluginsAdded )
            {
                if( TotalAvailablePluginCount == 0 )
                {
                    SetDisabled( ServiceDisabledReason.AllPluginsAreDisabled );
                }
                else Family.Solver.DeferPropagation( this );
            }
        }

        internal FinalConfigStartableStatus FinalStartableStatus
        {
            get { return _finalConfigStartableStatus; }
        }
        
        internal void InitializeFinalStartableStatus()
        {
            if( !Disabled )
            {
                _finalConfigStartableStatus = new FinalConfigStartableStatusService( this );
            }
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            ToString( b, String.Empty );
            return b.ToString();
        }


        [ExcludeFromCodeCoverage]
        public void ToString( StringBuilder b, string prefix )
        {
            b.Append( prefix );
            if( Disabled )
            {
                b.AppendFormat( "{0} - DisableReason: {1}, Solved: {2} => Dynamic: {3}",
                    ServiceInfo.ServiceFullName,
                    DisabledReason,
                    ConfigSolvedStatus,
                    _dynamicStatus );
            }
            else
            {
                b.AppendFormat( "{0} - Solved: {1}, Config: {2} => Dynamic: {3}",
                    ServiceInfo.ServiceFullName,
                    ConfigSolvedStatus,
                    ConfigOriginalStatus,
                    _dynamicStatus );
            }
            if( PluginCount > 0 )
            {
                b.AppendLine();
                b.Append( prefix ).AppendFormat( " - {0}/{1} plugins - {2}/{3} total plugins.", AvailablePluginCount, PluginCount, TotalAvailablePluginCount, TotalPluginCount );
                var prefix2 = prefix + " --- ";
                PluginData p = FirstPlugin;
                while( p != null )
                {
                    b.AppendLine().Append( prefix2 ).Append( p.ToString() );
                    p = p.NextPluginForService;
                }
            }
            var prefixS = prefix + "   > ";
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                b.AppendLine();
                spec.ToString( b, prefixS );
                spec = spec.NextSpecialization;
            }
        }

        #region IYodiiItemData explicit implementation of properties

        ConfigurationStatus IYodiiItemData.ConfigOriginalStatus
        {
            get { return ConfigOriginalStatus; }
        }

        string IYodiiItemData.DisabledReason
        {
            get { return DisabledReason; }
        }

        FinalConfigStartableStatus IYodiiItemData.FinalStartableStatus
        {
            get { return FinalStartableStatus; }
        }

        StartDependencyImpact IYodiiItemData.ConfigOriginalImpact
        {
            get { return ConfigOriginalImpact; }
        }

        #endregion

    }
}
