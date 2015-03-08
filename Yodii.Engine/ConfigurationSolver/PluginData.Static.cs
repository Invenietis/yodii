#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\PluginData.Static.cs) is part of CiviKey. 
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

namespace Yodii.Engine
{
    partial class PluginData : IYodiiItemData
    {
        readonly IConfigurationSolver _solver;
        IReadOnlyList<ServiceData>[] _includedServices;
        IReadOnlyList<ServiceData>[] _excludedServices;
        HashSet<ServiceData>[] _includedServicesClosure;
        PluginDisabledReason _configDisabledReason;
        SolvedConfigurationStatus _configSolvedStatus;
        PluginRunningRequirementReason _configSolvedStatusReason;
        readonly StartDependencyImpact _configSolvedImpact;
        FinalConfigStartableStatus _finalConfigStartableStatus;

        internal PluginData( IConfigurationSolver solver, IPluginInfo p, ServiceData service, ConfigurationStatus pluginStatus, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            _solver = solver;
            PluginInfo = p;
            Service = service;

            // Status
            ConfigOriginalStatus = pluginStatus;
            switch( pluginStatus )
            {
                case ConfigurationStatus.Disabled: _configSolvedStatus = SolvedConfigurationStatus.Disabled; break;
                case ConfigurationStatus.Running: _configSolvedStatus = SolvedConfigurationStatus.Running; break;
                default: _configSolvedStatus = SolvedConfigurationStatus.Runnable; break;
            }
            _configSolvedStatusReason = PluginRunningRequirementReason.Config;

            // Impact
            _configSolvedImpact = ConfigOriginalImpact = impact;
            if( service != null )
            {
                _configSolvedImpact = (ConfigOriginalImpact | service.ConfigSolvedImpact).ClearUselessTryBits();
            }
            if( _configSolvedImpact == StartDependencyImpact.Unknown )
            {
                _configSolvedImpact = StartDependencyImpact.Minimal;
            }

            // 
            if( ConfigOriginalStatus == ConfigurationStatus.Disabled )
            {
                _configDisabledReason = PluginDisabledReason.Config;
            }
            else if( p.HasError )
            {
                _configDisabledReason = PluginDisabledReason.PluginInfoHasError;
            }
            else if( service != null )
            {
                if( service.Disabled )
                {
                    _configDisabledReason = PluginDisabledReason.ServiceIsDisabled;
                }
                else if( service.Family.RunningPlugin != null )
                {
                    _configDisabledReason = PluginDisabledReason.AnotherRunningPluginExistsInFamilyByConfig;
                }
                else
                {
                    ServiceData siblingService = Service.FirstSpecialization;
                    while( siblingService != null )
                    {
                        if( siblingService.ConfigSolvedStatus == SolvedConfigurationStatus.Running )
                        {
                             _configDisabledReason = PluginDisabledReason.ServiceSpecializationRunning;
                             break;
                        }
                        siblingService = siblingService.NextSpecialization;
                    }
                }
            }
            // Immediately check for Runnable references to Disabled Services: this disables us.
            if( !Disabled )
            {
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    if( sRef.Requirement >= DependencyRequirement.Runnable )
                    {
                        // If the required service is already disabled, we immediately disable this plugin.
                        if( sRef.Reference.HasError && !Disabled )
                        {
                            _configDisabledReason = PluginDisabledReason.RunnableReferenceServiceIsOnError;
                            break;
                        }
                        ServiceData sr = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                        if( sr.Disabled && !Disabled )
                        {
                            _configDisabledReason = PluginDisabledReason.RunnableReferenceIsDisabled;
                            break;
                        }
                    }
                }
            }
            if( service != null )
            {
                service.AddPlugin( this );
            }
            if( !Disabled  )
            {
                // If the plugin is not yet disabled, we register it:
                // whenever the referenced service is disabled, this will disable the plugin.
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    _solver.FindExistingService( sRef.Reference.ServiceFullName ).RegisterPluginReference( this, sRef.Requirement );
                }
                if( Service != null && ConfigOriginalStatus == ConfigurationStatus.Running )
                {
                    Service.Family.SetRunningPlugin( this );
                }
            }
        }

        public bool IsPlugin { get { return true; } }

        /// <summary>
        /// The discovered plugin information.
        /// </summary>
        public readonly IPluginInfo PluginInfo;

        /// <summary>
        /// The ServiceData if this plugin implements a Service, Null otherwise.
        /// </summary>
        public readonly ServiceData Service;

        /// <summary>
        /// Link to the next element in the list of sibling PluginData that implement the same Service.
        /// </summary>
        public PluginData NextPluginForService;

        /// <summary>
        /// The original, configured, ConfigurationStatus of the plugin itself.
        /// </summary>
        public readonly ConfigurationStatus ConfigOriginalStatus;

        /// <summary>
        /// The original, configured, StartDependencyImpact of the plugin itself.
        /// </summary>
        public readonly StartDependencyImpact ConfigOriginalImpact;

        /// <summary>
        /// The solved StartDependencyImpact: combination of ConfigOriginalImpact and the Service's one.
        /// </summary>
        public StartDependencyImpact ConfigSolvedImpact
        {
            get { return _configSolvedImpact; }
        }

        /// <summary>
        /// Gets whether this plugin must exist or run. It is initialized by the configuration, but may evolve
        /// if this plugin implements a service.
        /// This is the minimal requirement after having propagated the constraints through the graph.
        /// </summary>
        public SolvedConfigurationStatus ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        /// <summary>
        /// Gets the ConfigSolvedStatus that is ConfigurationStatus.Disabled if the plugin is actually Disabled.
        /// </summary>
        public SolvedConfigurationStatus FinalConfigSolvedStatus
        {
            get { return _configDisabledReason == PluginDisabledReason.None ? _configSolvedStatus : SolvedConfigurationStatus.Disabled; }
        }

        /// <summary>
        /// Gets the strongest reason that explains this plugin ConfigSolvedStatus. 
        /// </summary>
        public PluginRunningRequirementReason ConfigSolvedStatusReason
        {
            get { return _configSolvedStatusReason; }
        }
        
        /// <summary>
        /// Gets whether this plugin is disabled.
        /// This is a solved configuration information.
        /// </summary>
        public bool Disabled
        {
            get { return _configDisabledReason != PluginDisabledReason.None; }
        }

        /// <summary>
        /// Gets the first reason why this plugin is disabled. 
        /// </summary>
        internal string DisabledReason
        {
            get { return _configDisabledReason == PluginDisabledReason.None ? null : _configDisabledReason.ToString(); }
        }

        internal void SetDisabled( PluginDisabledReason r )
        {
            Debug.Assert( r != PluginDisabledReason.None );
            Debug.Assert( _configDisabledReason == PluginDisabledReason.None );
            _includedServicesClosure = null;
            _configDisabledReason = r;
            if( Service != null ) Service.OnPluginDisabled( this );
        }

        internal bool SetRunningStatus( PluginRunningRequirementReason reason )
        {
            if( _configSolvedStatus == SolvedConfigurationStatus.Running ) return !Disabled;
            _configSolvedStatus = SolvedConfigurationStatus.Running;
            _configSolvedStatusReason = reason;
            if( Service != null && !Service.Family.SetRunningPlugin( this ) ) return false;
            return PropagateRunningStatus();
        }

        internal bool PropagateRunningStatus()
        {
            Debug.Assert( FinalConfigSolvedStatus == SolvedConfigurationStatus.Running );
            if( !Disabled )
            {
                foreach( var s in GetIncludedServices( _configSolvedImpact | StartDependencyImpact.IsStartRunnableRecommended | StartDependencyImpact.IsStartRunnableOnly ) )
                {
                    if( !s.SetRunningStatus( ServiceSolvedConfigStatusReason.FromPropagation ) )
                    {
                        if( !Disabled )
                        {
                            // Instead of generic "PropagationFailed", take the time to compute a detailed reason
                            // for the first reason why this plugin is disabled.
                            var requirement = PluginInfo.ServiceReferences.First( sRef => sRef.Reference == s.ServiceInfo ).Requirement;
                            var reason = GetDisableReasonForDisabledReference( requirement );
                            Debug.Assert( reason != PluginDisabledReason.None );
                            SetDisabled( reason );
                        }
                    }
                }
                //Debug.Assert( Disabled || GetExcludedServices( _configSolvedImpact | StartDependencyImpact.IsStartRunnableRecommended | StartDependencyImpact.IsStartRunnableOnly ).All( s => s.Disabled ) );
                if( !Disabled )
                {
                    foreach( var s in GetExcludedServices( _configSolvedImpact ) )
                    {
                        if( !s.Disabled ) s.SetDisabled( ServiceDisabledReason.StopppedByPropagation );
                    }
                }
            }
            return !Disabled;
        }

        internal FinalConfigStartableStatus FinalStartableStatus
        {
            get { return _finalConfigStartableStatus; }
        }

        internal void InitializeFinalStartableStatus()
        {
            if( !Disabled )
            {
                _finalConfigStartableStatus = new FinalConfigStartableStatusPlugin( this );
            }
        }

        public PluginDisabledReason GetDisableReasonForDisabledReference( DependencyRequirement req )
        {
            switch( req )
            {
                case DependencyRequirement.Running: return PluginDisabledReason.ByRunningReference;
                case DependencyRequirement.RunnableRecommended: return PluginDisabledReason.ByRunnableRecommendedReference;
                case DependencyRequirement.Runnable: return PluginDisabledReason.ByRunnableReference;
                case DependencyRequirement.OptionalRecommended:
                    if( (_configSolvedImpact & StartDependencyImpact.IsStartOptionalRecommended) != 0 )
                    {
                        return PluginDisabledReason.ByOptionalRecommendedReference;
                    }
                    break;
                case DependencyRequirement.Optional:
                    if( (_configSolvedImpact & StartDependencyImpact.IsStartOptionalOnly) != 0 )
                    {
                        return PluginDisabledReason.ByOptionalReference;
                    }
                    break;
            }
            return PluginDisabledReason.None;
        }

        public HashSet<ServiceData> GetIncludedServicesClosure( StartDependencyImpact impact, bool refreshCache = false )
        {
            Debug.Assert( !Disabled );
            if( _includedServicesClosure == null ) _includedServicesClosure = new HashSet<ServiceData>[16];
            impact = impact.ClearAllTryBits();
            int i = (int)impact >> 1;

            HashSet<ServiceData> result = _includedServicesClosure[i];
            if( result == null || refreshCache )
            {
                result = new HashSet<ServiceData>();
                foreach( var service in GetIncludedServices( impact ) )
                {
                    service.FillTransitiveIncludedServices( result );
                }
                _includedServicesClosure[i] = result;
            }
            return result;
        }

        #region CoRunning discussion - Must be kept
        //public bool BuildTransitiveIncludedServiceSetAndCheckInvalidLoop()
        //{
        //    Debug.Assert( !Disabled );
        //    // If this plugin is running by configuration, we consider runnable references: we want runnable references to be able to start
        //    // and their start must not stop this plugin whatever this configured impact is.
        //    // If this plugin is only runnable, we take runnable references (and optional ones) into account depending on this configured impact.
        //    var impact = ConfigSolvedImpact;
        //    if( ConfigSolvedStatus == SolvedConfigurationStatus.Running ) impact |= StartDependencyImpact.IsStartRunnableOnly | StartDependencyImpact.IsStartRunnableRecommended;

        //    HashSet<ServiceData> running = new HashSet<ServiceData>();
        //    foreach( var service in GetIncludedServices( impact ) )
        //    {
        //        service.FillTransitiveIncludedServices( running );
        //    }
        //    if( running.Overlaps( GetExcludedServices( impact ) ) )
        //    {
        //        return false;
        //    }


            ///  Following code checks that each runnable, one by one, can CoRun with this plugin.
            ///  This was deemed confusing to the user: this does not guaranty that the Start of a Service
            ///  when another dependency is running, will not stop this plugin.
            ///  Preventing suicide seems to require more things like a set of CoRunning( i1 ... in ) that states
            ///  that nothing prevents any of these sets of items to be running together.
            ///    
            ///     HashSet<IYodiiItemData> runnable = new HashSet<IYodiiItemData>();
            ///     foreach( var sRef in PluginInfo.ServiceReferences )
            ///     {
            ///         if( sRef.Requirement == DependencyRequirement.Runnable )
            ///         {
            ///             runnable.Clear();
            ///             runnable.AddRange(running);
            ///             ServiceData service2 = _solver.FindExistingService( sRef.Reference.ServiceFullName );
            ///             service2.FillTransitiveIncludedServices( runnable );
            ///             if( runnable.Overlaps( GetExcludedServices( ConfigSolvedImpact ) ) )
            ///             {
            ///                 SetDisabled( PluginDisabledReason.InvalidStructureLoop );
            ///                 return false;
            ///             }
            ///         }
            ///     }
            /// 
            /// Following this idea leads to a structure where each Item I can be associated to a 
            /// description of its requirement regarding other items:
            ///  - Items that when I is running must run (MustRunWith): this contains at least Running dependencies to Services and this generalizes the code above.
            ///  - A set of sets ("CanRunWith" items): each of them contains a set of items that must be able to run while I is running.
            ///    These sets must be combined to the MustRunWith set and each of them must be satisfied.
            /// 
            /// Thess "CanRunWith" sets can be defined with an attribute (AllowMultiple = true) on the Plugin:
            /// [CanRunWith( typeof(IService1), typeof(IService2) )]
            /// [CanRunWith( typeof(IService1), typeof(IService3), typeof(IService4) )]
            /// [CanRunWith( typeof(IService5) )]
            /// 
            /// For runnables and optionals, if we want to prevent the suicide of P, we can now automatically consider that a CanRunWith( S ) exists for each dependency.
            /// This could be the default, but I'm not sure that it is a good idea. I'd rather let the suicicide be the default otherwise we would need a 
            /// kind of AllowSuicideWhenStarting(S) declaration that will not be really easy to understand.
            /// 
            /// This has an impact on the dynamic phase: to handle this we must "boost" the command that has started P (if it exists, ie. if it is not running by configuration).
            /// When P starts S, we start S, and right after we must start P otherwise there is no guaranty that another command leads to the fact that P can no more be running.
            /// For multiple "CanRunWith", should we also boost the commands that started the other services that are actually running?
            /// If yes, which set should we consider? Considering the previous examples:
            ///   - We must start the dependency IService3.
            ///   - We have already running dependencies: IService1 and IService2.
            /// Nothing prevents the start of IService3 to stop IService2. That is understandable. But IService1 must be kept alive since it belongs to the second set: we start it 
            /// right after. But what if IService3 also belongs to another set: [CanRunWith( typeof(IService2), typeof(IService3) )] ?
            /// We then need to also start IService2. Can the start of IService1 have led IService2 to be stopped? No since the CanRunWith has been statically statisfied.
            /// 
            /// All this seems to be logically consistent. To be implemented this definitely requires the "launcher" to be known. 
            /// The current IService<T> (the proxy) exposes the Start()/Stop() methods. This is annoying: we don't know/control who is calling us.
            /// One need an internal IInternalYodiiEngine interface that will be injected into each plugin (if they require it) that captures the plugin identity
            /// and exposes Start/Stop capabilities and, ideally, access to IInternalLiveInfo observable models without Start/Stop capabilities.
            /// 
            /// 2015-10-01: Start/Stop have been moved away from ILive info objects to IYodiiEngine. Live info can now be exposed directly to plugins, they do not
            /// have to be per-plugin.
            /// 
            ///
        //    return true;
        //}
        #endregion

        public IEnumerable<ServiceData> GetIncludedServices( StartDependencyImpact impact )
        {
            if( _includedServices == null ) _includedServices = new IReadOnlyList<ServiceData>[16];
            impact = impact.ClearAllTryBits();
            int i = (int)impact >> 1;
            var result = _includedServices[i];
            if( result == null )
            {
                if( _includedServices[0] == null )
                {
                    var running = Service != null ? new HashSet<ServiceData>( Service.InheritedServicesWithThis ) : new HashSet<ServiceData>();
                    foreach( var sRef in PluginInfo.ServiceReferences )
                    {
                        if( sRef.Requirement == DependencyRequirement.Running ) running.Add( _solver.FindExistingService( sRef.Reference.ServiceFullName ) );
                    }
                    result = _includedServices[0] = running.ToReadOnlyList();
                }
                if( i > 0 )
                {
                    var running = new HashSet<ServiceData>( _includedServices[0] );
                    foreach( var sRef in PluginInfo.ServiceReferences )
                    {
                        ServiceData sr = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                        switch( sRef.Requirement )
                        {
                            case DependencyRequirement.RunnableRecommended:
                                    if( (impact & StartDependencyImpact.IsStartRunnableRecommended) != 0 ) running.Add( sr );
                                    break;
                            case DependencyRequirement.OptionalRecommended:
                                    if( (impact&StartDependencyImpact.IsStartOptionalRecommended) != 0 ) running.Add( sr );
                                    break;
                            case DependencyRequirement.Runnable:
                                    if( (impact & StartDependencyImpact.IsStartRunnableOnly) != 0 ) running.Add( sr );
                                    break;
                            case DependencyRequirement.Optional:
                                    if( (impact & StartDependencyImpact.IsStartOptionalOnly) != 0 ) running.Add( sr );
                                    break;
                        }
                    }
                    result = _includedServices[i] = running.ToReadOnlyList();
                }
            }
            return result;
        }

        public IEnumerable<ServiceData> GetExcludedServices( StartDependencyImpact impact )
        {
            if( _excludedServices == null ) _excludedServices = new IReadOnlyList<ServiceData>[16];
            impact = impact.ClearAllTryBits();
            int i = (int)impact >> 1;
            IReadOnlyList<ServiceData> e = _excludedServices[i];
            if( e == null )
            {
                HashSet<ServiceData> excl;
                if( Service != null )
                {
                    excl = new HashSet<ServiceData>( Service.Family.AvailableServices );
                    excl.ExceptWith( Service.InheritedServicesWithThis );
                }
                else excl = new HashSet<ServiceData>();
                excl.UnionWith( GetIncludedServices( impact ).SelectMany( s => s.DirectExcludedServices ) );

                e = _excludedServices[i] = excl.ToReadOnlyList();
            }
            return e;
        }

        public override string ToString()
        {
            if( Disabled )
            {
                return String.Format( "{0} - DisableReason: {1}, Solved: {2} => Dynamic: {3}",
                    PluginInfo.PluginFullName,
                    DisabledReason,
                    ConfigSolvedStatus,
                    _dynamicStatus );
            }
            return String.Format( "{0} - Solved: {1}, Config: {2} => Dynamic: {3}",
                PluginInfo.PluginFullName,
                ConfigSolvedStatus,
                ConfigOriginalStatus,
                _dynamicStatus );
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
