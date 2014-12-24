using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{
    partial class PluginData : IServiceDependentObject, IYodiiItemData
    {
        readonly IConfigurationSolver _solver;
        IReadOnlyList<ServiceData> _runningIncludedServices;
        IReadOnlyList<ServiceData> _runnableIncludedServices;
        IReadOnlyList<ServiceData>[] _exclServices;
        IReadOnlyList<ServiceData>[] _inclServices;
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
            ConfigOriginalStatus = pluginStatus;
            switch( pluginStatus )
            {
                case ConfigurationStatus.Disabled: _configSolvedStatus = SolvedConfigurationStatus.Disabled; break;
                case ConfigurationStatus.Running: _configSolvedStatus = SolvedConfigurationStatus.Running; break;
                default: _configSolvedStatus = SolvedConfigurationStatus.Runnable; break;
            }
            _configSolvedStatusReason = PluginRunningRequirementReason.Config;

            RawConfigSolvedImpact = ConfigOriginalImpact = impact;
            if( RawConfigSolvedImpact == StartDependencyImpact.Unknown && Service != null )
            {
                RawConfigSolvedImpact = Service.ConfigSolvedImpact;
            }
            _configSolvedImpact = RawConfigSolvedImpact;
            if( _configSolvedImpact == StartDependencyImpact.Unknown || (_configSolvedImpact & StartDependencyImpact.IsTryOnly) != 0 )
            {
                _configSolvedImpact = StartDependencyImpact.Minimal;
            }

            if( ConfigOriginalStatus == ConfigurationStatus.Disabled )
            {
                _configDisabledReason = PluginDisabledReason.Config;
            }
            else if( p.HasError )
            {
                _configDisabledReason = PluginDisabledReason.PluginInfoHasError;
            }
            else if( Service != null )
            {
                if( Service.Disabled )
                {
                    _configDisabledReason = PluginDisabledReason.ServiceIsDisabled;
                }
                else if( Service.Family.RunningPlugin != null )
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
            if( Service != null )
            {
                Service.AddPlugin( this );
            }
            if( !Disabled  )
            {
                // If the plugin is not yet disabled, we register it:
                // whenever the referenced service is disabled (or stopped during dynamic resolution), this 
                // will disable (or stop) the plugin according to its ConfigSolvedImpact.
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
        /// The configured StartDependencyImpact (either ConfigOriginalImpact or the Service's one if ConfigOriginalImpact is unknown).
        /// </summary>
        public readonly StartDependencyImpact RawConfigSolvedImpact;

        /// <summary>
        /// The solved StartDependencyImpact for the static resolution: never IsTryOnly nor unknown.
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
                foreach( var s in GetIncludedServices( _configSolvedImpact, false ) )
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
                _finalConfigStartableStatus = new FinalConfigStartableStatus( this );
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
                    if( _configSolvedImpact >= StartDependencyImpact.StartRecommended )
                    {
                        return PluginDisabledReason.ByOptionalRecommendedReference;
                    }
                    break;
                case DependencyRequirement.Optional:
                    if( _configSolvedImpact == StartDependencyImpact.FullStart )
                    {
                        return PluginDisabledReason.ByOptionalReference;
                    }
                    break;
            }
            return PluginDisabledReason.None;
        }

        public PluginDisabledReason GetDisableReasonForRunningReference( DependencyRequirement req )
        {
            switch( req )
            {
                case DependencyRequirement.Running: return PluginDisabledReason.ByRunningReference;
                case DependencyRequirement.RunnableRecommended: return PluginDisabledReason.ByRunnableRecommendedReference;
                case DependencyRequirement.Runnable: return PluginDisabledReason.ByRunnableReference;
                case DependencyRequirement.OptionalRecommended:
                    if( _configSolvedImpact >= StartDependencyImpact.StartRecommended )
                    {
                        return PluginDisabledReason.ByOptionalRecommendedReference;
                    }
                    break;
                case DependencyRequirement.Optional:
                    if( _configSolvedImpact == StartDependencyImpact.FullStart )
                    {
                        return PluginDisabledReason.ByOptionalReference;
                    }
                    break;
            }
            return PluginDisabledReason.None;
        }

        public void FillTransitiveIncludedServices( HashSet<IYodiiItemData> set )
        {
            if( !set.Add( this ) ) return;

            foreach( var s in GetIncludedServices( ConfigSolvedImpact, forRunnableStatus: false ) )
            {
                s.FillTransitiveIncludedServices( set );
            }
        }

        public bool CheckInvalidLoop()
        {
            Debug.Assert( !Disabled );
            HashSet<IYodiiItemData> running = new HashSet<IYodiiItemData>();
            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                if( sRef.Requirement == DependencyRequirement.Running )
                {
                    ServiceData service = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                    service.FillTransitiveIncludedServices( running );
                }
            }
            if( running.Overlaps( GetExcludedServices( ConfigSolvedImpact ) ) )
            {
                SetDisabled( PluginDisabledReason.InvalidStructureLoop );
                return false;
            }
            ///  Following code checks that each runnable, one by one, can CoRun with this plugin.
            ///  Was deemed confusing to the user: this does not enforces that a Start of a Service
            ///  when another dependency is running, will not stop this plugin.
            ///  Preventing suicide seems to require more things like a set of CoRunnig( i1 ... in ) that states
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
            /// This could be the default, but I'm not sure that it is a good idea. I'd rather let the suicicide be the defaulto therwise we would need a 
            /// kind of AllowSuicideWhenStarting(S) declaration that will not be really easy to understand.
            /// 
            /// This has an impact on the dynamic pahases: to handle this we must "boost" the command that has started P (if it exists, ie. if it is not running by configuration).
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
            return true;
        }


        public IEnumerable<ServiceData> GetIncludedServices( StartDependencyImpact impact, bool forRunnableStatus )
        {
            if( _runningIncludedServices == null )
            {
                var running = Service != null ? new HashSet<ServiceData>( Service.InheritedServicesWithThis ) : new HashSet<ServiceData>();
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    if( sRef.Requirement == DependencyRequirement.Running ) running.Add( _solver.FindExistingService( sRef.Reference.ServiceFullName ) );
                }
                _runningIncludedServices = running.ToReadOnlyList();
                bool newRunnableAdded = false;
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    if( sRef.Requirement == DependencyRequirement.Runnable || sRef.Requirement == DependencyRequirement.RunnableRecommended )
                    {
                        newRunnableAdded |= running.Add( _solver.FindExistingService( sRef.Reference.ServiceFullName ) );
                    }
                }
                _runnableIncludedServices = newRunnableAdded ? running.ToReadOnlyList() : _runningIncludedServices;
            }

            if( impact == StartDependencyImpact.Minimal ) return forRunnableStatus ? _runnableIncludedServices : _runningIncludedServices;

            if( _inclServices == null ) _inclServices = new IReadOnlyList<ServiceData>[8];
            int iImpact = (int)impact;
            if( impact > StartDependencyImpact.Minimal ) --impact;
            if( forRunnableStatus ) iImpact *= 2;
            --iImpact;

            IReadOnlyList<ServiceData> i = _inclServices[iImpact];
            if( i == null )
            {
                var baseSet = forRunnableStatus ? _runnableIncludedServices : _runningIncludedServices;
                var incl = new HashSet<ServiceData>( baseSet );
                bool newAdded = false;
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    ServiceData sr = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                    switch( sRef.Requirement )
                    {
                        case DependencyRequirement.RunnableRecommended:
                            {
                                if( impact >= StartDependencyImpact.StartRecommended )
                                {
                                    newAdded |= incl.Add( sr );
                                }
                                break;
                            }
                        case DependencyRequirement.Runnable:
                            {
                                if( impact == StartDependencyImpact.FullStart )
                                {
                                    newAdded |= incl.Add( sr );
                                }
                                break;
                            }
                        case DependencyRequirement.OptionalRecommended:
                            {
                                if( impact >= StartDependencyImpact.StartRecommended )
                                {
                                    newAdded |= incl.Add( sr );
                                }
                                break;
                            }
                        case DependencyRequirement.Optional:
                            {
                                if( impact == StartDependencyImpact.FullStart )
                                {
                                    newAdded |= incl.Add( sr );
                                }
                                break;
                            }
                    }
                }
                i = _inclServices[iImpact] = newAdded ? incl.ToReadOnlyList() : baseSet;
            }
            return i;
        }

        public IEnumerable<ServiceData> GetExcludedServices( StartDependencyImpact impact )
        {
            if( _exclServices == null ) _exclServices = new IReadOnlyList<ServiceData>[6];
            IReadOnlyList<ServiceData> e = _exclServices[(int)impact-1];
            if( e == null )
            {
                HashSet<ServiceData> excl;
                if( Service != null )
                {
                    excl = new HashSet<ServiceData>( Service.Family.AvailableServices );
                    excl.ExceptWith( Service.InheritedServicesWithThis );
                }
                else excl = new HashSet<ServiceData>();
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    ServiceData sr = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                    switch( sRef.Requirement )
                    {
                        case DependencyRequirement.Running:
                            {
                                excl.UnionWith( sr.DirectExcludedServices );
                                break;
                            }
                        case DependencyRequirement.RunnableRecommended:
                            {
                                if( impact >= StartDependencyImpact.StartRecommended )
                                {
                                    excl.UnionWith( sr.DirectExcludedServices );
                                }
                                else if( impact == StartDependencyImpact.FullStop )
                                {
                                    excl.Add( sr );
                                }
                                break;
                            }
                        case DependencyRequirement.Runnable:
                            {
                                if( impact == StartDependencyImpact.FullStart )
                                {
                                    excl.UnionWith( sr.DirectExcludedServices );
                                }
                                else if( impact <= StartDependencyImpact.StopOptionalAndRunnable 
                                    || impact == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable )
                                {
                                    excl.Add( sr );
                                }
                                break;
                            }
                        case DependencyRequirement.OptionalRecommended:
                            {
                                if( impact >= StartDependencyImpact.StartRecommended )
                                {
                                    excl.UnionWith( sr.DirectExcludedServices );
                                }
                                else if( impact == StartDependencyImpact.FullStop )
                                {
                                    excl.Add( sr );
                                }
                                break;
                            }
                        case DependencyRequirement.Optional:
                            {
                                if( impact == StartDependencyImpact.FullStart )
                                {
                                    excl.UnionWith( sr.DirectExcludedServices );
                                }
                                else if( impact <= StartDependencyImpact.StopOptionalAndRunnable 
                                    || impact == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable )
                                {
                                    excl.Add( sr );
                                }
                                break;
                            }
                    }
                }
                e = _exclServices[(int)impact-1] = excl.ToReadOnlyList();
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

        StartDependencyImpact IYodiiItemData.RawConfigSolvedImpact
        {
            get { return RawConfigSolvedImpact; }
        }

        #endregion
    }
}
