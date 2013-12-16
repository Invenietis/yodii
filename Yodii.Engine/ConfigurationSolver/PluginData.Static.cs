using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{
    partial class PluginData : IServiceDependentObject
    {
        readonly IConfigurationSolver _solver;
        IReadOnlyList<ServiceData> _runningIncludedServices;
        IReadOnlyList<ServiceData> _runnableIncludedServices;
        IReadOnlyList<ServiceData>[] _exclServices;
        IReadOnlyList<ServiceData>[] _inclServices;
        PluginDisabledReason _configDisabledReason;
        ConfigurationStatus _configSolvedStatus;
        PluginRunningRequirementReason _configSolvedStatusReason;
        readonly StartDependencyImpact _configSolvedImpact;
        FinalConfigStartableStatus _finalConfigStartableStatus;

        internal PluginData( IConfigurationSolver solver, IPluginInfo p, ServiceData service, ConfigurationStatus pluginStatus, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            _solver = solver;
            PluginInfo = p;
            Service = service;
            ConfigOriginalStatus = pluginStatus;
            _configSolvedStatus = pluginStatus;
            _configSolvedStatusReason = PluginRunningRequirementReason.Config;

            _configSolvedImpact = ConfigOriginalImpact = impact;
            if( _configSolvedImpact == StartDependencyImpact.Unknown && Service != null )
            {
                _configSolvedImpact = Service.ConfigSolvedImpact;
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
            }
            // Register Runnable references to Services from this plugin.
            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                if( sRef.Requirement >= DependencyRequirement.Runnable )
                {
                    // If the required service is already disabled, we immediately disable this plugin.
                    if( sRef.Reference.HasError )
                    {
                        SetDisabled( PluginDisabledReason.RunnableReferenceServiceIsOnError );
                        break;
                    }
                    ServiceData sr = _solver.FindExistingService( sRef.Reference.ServiceFullName );
                    if( sr.Disabled )
                    {
                        SetDisabled( PluginDisabledReason.RunnableReferenceIsDisabled );
                        break;
                    }
                }
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
                if( Service != null )
                {
                    if( ConfigOriginalStatus == ConfigurationStatus.Running )
                    {
                        Service.Family.SetRunningPlugin( this );
                    }
                    Service.AddPlugin( this );
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
        /// The solved StartDependencyImpact: it is this ConfigOriginalImpact if known or the Service's one if it exists.
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
        public ConfigurationStatus ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        /// <summary>
        /// Gets the ConfigSolvedStatus that is ConfigurationStatus.Disabled if the plugin is actually Disabled.
        /// </summary>
        public ConfigurationStatus FinalConfigSolvedStatus
        {
            get { return _configDisabledReason != PluginDisabledReason.None ? ConfigurationStatus.Disabled : _configSolvedStatus; }
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
        public PluginDisabledReason DisabledReason
        {
            get { return _configDisabledReason; }
        }

        internal void SetDisabled( PluginDisabledReason r )
        {
            Debug.Assert( r != PluginDisabledReason.None );
            Debug.Assert( _configDisabledReason == PluginDisabledReason.None );
            _configDisabledReason = r;
            if( Service != null ) Service.OnPluginDisabled( this );
        }

        internal bool SetSolvedStatus( ConfigurationStatus status, PluginRunningRequirementReason reason )
        {
            Debug.Assert( status >= ConfigurationStatus.Runnable );
            if( _configSolvedStatus >= status ) return !Disabled;
            _configSolvedStatus = status;
            _configSolvedStatusReason = reason;

            if( status == ConfigurationStatus.Running && Service != null && !Service.Family.SetRunningPlugin( this ) ) return false;

            return PropagateSolvedStatus();
        }

        internal bool PropagateSolvedStatus()
        {
            Debug.Assert( FinalConfigSolvedStatus >= ConfigurationStatus.Runnable );
            StartDependencyImpact impact = _configSolvedImpact;
            if( _configSolvedImpact == StartDependencyImpact.Unknown ) impact = StartDependencyImpact.Minimal;

            if( !Disabled )
            {
                foreach( var s in GetIncludedServices( impact, ConfigSolvedStatus == ConfigurationStatus.Runnable ) )
                {
                    if( !s.SetSolvedStatus( _configSolvedStatus, ServiceSolvedConfigStatusReason.FromPropagation ) )
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
                    foreach( var s in GetExcludedServices( impact ) )
                    {
                        if( !s.Disabled ) s.SetDisabled( ServiceDisabledReason.StopppedByPropagation ); 
                    }
                }
            }
            return !Disabled;
        }

        internal void InitializeFinalStartableStatus()
        {
            ConfigurationStatus final = FinalConfigSolvedStatus;
            if( final == ConfigurationStatus.Optional || final == ConfigurationStatus.Runnable )
            {
                _finalConfigStartableStatus = new FinalConfigStartableStatus( this );
            }
        }

        public PluginDisabledReason GetDisableReasonForDisabledReference( DependencyRequirement req )
        {
            StartDependencyImpact impact = _configSolvedImpact;
            if( impact == StartDependencyImpact.Unknown ) impact = StartDependencyImpact.Minimal;

            switch( req )
            {
                case DependencyRequirement.Running: return PluginDisabledReason.ByRunningReference;
                case DependencyRequirement.RunnableTryStart: return PluginDisabledReason.ByRunnableTryStartReference;
                case DependencyRequirement.Runnable: return PluginDisabledReason.ByRunnableReference;
                case DependencyRequirement.OptionalTryStart:
                    if( impact >= StartDependencyImpact.StartRecommended )
                    {
                        return PluginDisabledReason.ByOptionalTryStartReference;
                    }
                    break;
                case DependencyRequirement.Optional:
                    if( impact == StartDependencyImpact.FullStart )
                    {
                        return PluginDisabledReason.ByOptionalReference;
                    }
                    break;
            }
            return PluginDisabledReason.None;
        }

        public IEnumerable<ServiceData> GetIncludedServices( StartDependencyImpact impact, bool forRunnableStatus )
        {
            if( _runningIncludedServices == null )
            {
                var running = new HashSet<ServiceData>();
                var g = Service;
                while( g != null )
                {
                    running.Add( g );
                    g = g.Generalization;
                }
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    if( sRef.Requirement == DependencyRequirement.Running ) running.Add( _solver.FindExistingService( sRef.Reference.ServiceFullName ) );
                }
                _runningIncludedServices = running.ToReadOnlyList();
                bool newRunnableAdded = false;
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    if( sRef.Requirement == DependencyRequirement.Runnable ) newRunnableAdded |= running.Add( _solver.FindExistingService( sRef.Reference.ServiceFullName ) );
                }
                _runnableIncludedServices = newRunnableAdded ? running.ToReadOnlyList() : _runningIncludedServices;
            }

            if( impact == StartDependencyImpact.Minimal ) return forRunnableStatus ? _runnableIncludedServices : _runningIncludedServices;

            if( _inclServices == null ) _inclServices = new IReadOnlyList<ServiceData>[8];
            int iImpact = (int)impact;
            if( impact > StartDependencyImpact.Minimal ) --impact;
            if( forRunnableStatus ) iImpact *= 2;
            --iImpact;

            IReadOnlyList < ServiceData > i = _inclServices[iImpact];
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
                        case DependencyRequirement.RunnableTryStart:
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
                        case DependencyRequirement.OptionalTryStart:
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
            if( _exclServices == null ) _exclServices = new IReadOnlyList<ServiceData>[5];
            IReadOnlyList<ServiceData> e = _exclServices[(int)impact-1];
            if( e == null )
            {
                HashSet<ServiceData> excl = new HashSet<ServiceData>();
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
                        case DependencyRequirement.RunnableTryStart:
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
                                else if( impact <= StartDependencyImpact.StopOptionalAndRunnable )
                                {
                                    excl.Add( sr );
                                }
                                break;
                            }
                        case DependencyRequirement.OptionalTryStart:
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
                                else if( impact <= StartDependencyImpact.StopOptionalAndRunnable )
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
    }
}
