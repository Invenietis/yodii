using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model.CoreModel;

namespace Yodii.Model.ConfigurationSolver
{
   
    internal partial class ServiceData
    {
        readonly Dictionary<IServiceInfo,ServiceData> _allServices;
        ServiceDisabledReason _disabledReason;
        RunningRequirement _runningRequirement;
        ServiceRunningRequirementReason _runningRequirementReason;
        ServiceData _mustExistSpecialization;
        ServiceData _directMustExistSpecialization;
        List<PluginData> _mustExistReferencer;

        internal ServiceData( Dictionary<IServiceInfo, ServiceData> allServices, IServiceInfo s, ServiceData generalization, SolvedConfigStatus serviceStatus, Func<IServiceInfo,bool> isExternalServiceAvailable )
        {
            _allServices = allServices;
            ServiceInfo = s;
            if( (Generalization = generalization) != null )
            {
                GeneralizationRoot = Generalization.GeneralizationRoot;
                NextSpecialization = Generalization.FirstSpecialization;
                Generalization.FirstSpecialization = this;
                ++Generalization.SpecializationCount;
            }
            else
            {
                GeneralizationRoot = (ServiceRootData)this;
            }
            if( (ServiceSolvedStatus = serviceStatus) == SolvedConfigStatus.Disabled )
            {
                _disabledReason = ServiceDisabledReason.Config;
            }
            else if( s.HasError )
            {
                _disabledReason = ServiceDisabledReason.ServiceInfoHasError;
            }
            else if( Generalization != null && Generalization.Disabled )
            {
                _disabledReason = ServiceDisabledReason.GeneralizationIsDisabledByConfig;
            }
            //else if( !s.IsDynamicService && !isExternalServiceAvailable( s ) )
            //{
            //    _disabledReason = ServiceDisabledReason.ExternalServiceUnavailable;
            //}
            if( !Disabled ) _runningRequirement = (RunningRequirement)serviceStatus;
            _runningRequirementReason = ServiceRunningRequirementReason.Config;
        }

        public readonly IServiceInfo ServiceInfo;

        /// <summary>
        /// True if this service should try to run thanks to its own configuration (<see cref="ServiceSolvedStatus"/> or because
        /// of any <see cref="Generalization"/> (if any) is configured to try to run.
        /// </summary>
        public bool IsTryStartByConfig
        {
            get
            {
                return ServiceSolvedStatus == SolvedConfigStatus.OptionalTryStart
                        || ServiceSolvedStatus == SolvedConfigStatus.RunnableTryStart
                        || (Generalization != null && Generalization.IsTryStartByConfig);
            }
        }

        /// <summary>
        /// The direct generalization if any.
        /// When null, this instance is a <see cref="ServiceRootData"/>.
        /// </summary>
        public readonly ServiceData Generalization;

        /// <summary>
        /// Root of Generalization. Never null since when this is not a specialization, this is its own root.
        /// </summary>
        public readonly ServiceRootData GeneralizationRoot;

        /// <summary>
        /// The SolvedConfigStatus of the Service. 
        /// </summary>
        public readonly SolvedConfigStatus ServiceSolvedStatus;

        /// <summary>
        /// Gets whether this service is disabled. 
        /// </summary>
        public bool Disabled
        {
            get { return _disabledReason != ServiceDisabledReason.None; }
        }

        public ServiceData MustExistSpecialization
        {
            get { return _mustExistSpecialization; }
        }

        public bool IsGeneralizationOf( ServiceData d )
        {
            var g = d.Generalization;
            if( g == null || d.GeneralizationRoot != GeneralizationRoot ) return false;
            do
            {
                if( g == this ) return true;
                g = g.Generalization;
            }
            while( g != null );
            return false;
        }

        /// <summary>
        /// Gets the first reason why this service is disabled. 
        /// </summary>
        public ServiceDisabledReason DisabledReason
        {
            get { return _disabledReason; }
        }

        internal virtual void SetDisabled( ServiceDisabledReason r )
        {
            Debug.Assert( r != ServiceDisabledReason.None );
            Debug.Assert( _disabledReason == ServiceDisabledReason.None );
            Debug.Assert( !GeneralizationRoot.Disabled, "A root is necessarily not disabled if one of its specialization is not disabled." );
            _disabledReason = r;
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
            Debug.Assert( _theOnlyPlugin == null && _commonReferences == null, "Disabling all plugins must have set them to null." );
            // The _mustExistReferencer list contains plugins that has at least a MustExist reference to this service
            // and have been initialized when this Service was not yet disabled.
            if( _mustExistReferencer != null )
            {
                foreach( PluginData p in _mustExistReferencer )
                {
                    if( !p.Disabled ) p.SetDisabled( PluginDisabledReason.MustExistReferenceIsDisabled );
                }
                // It is useless to keep them.
                _mustExistReferencer = null;
            }
            _directMustExistSpecialization = null;
            _mustExistSpecialization = null;
            if( Generalization != null && !Generalization.Disabled ) Generalization.OnSpecializationDisabled( this );
        }

        void OnSpecializationDisabled( ServiceData s )
        {
            if( _directMustExistSpecialization == s )
            {
                SetDisabled( ServiceDisabledReason.MustExistSpecializationIsDisabled );
            }
        }

        /// <summary>
        /// Gets the minimal running requirement. It is initialized by the configuration, but may evolve.
        /// </summary>
        public RunningRequirement MinimalRunningRequirement
        {
            get { return _mustExistSpecialization != null ? _mustExistSpecialization._runningRequirement : _runningRequirement; }
        }

        /// <summary>
        /// Gets the minimal running requirement for this service (not the one of MustExistSpecialization if it exists).
        /// </summary>
        public RunningRequirement ThisMinimalRunningRequirement
        {
            get { return _runningRequirement; }
        }

        /// <summary>
        /// Gets the strongest reason that explains this service ThisMinimalRunningRequirement. 
        /// </summary>
        public ServiceRunningRequirementReason ThisRunningRequirementReason 
        {
            get { return _runningRequirementReason; }
        }

        /// <summary>
        /// This can be called on an already disabled service and may trigger changes on the whole system.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="reason"></param>
        /// <returns>True if the requirement can be satisfied at this level. False otherwise.</returns>
        internal bool SetRunningRequirement( RunningRequirement r, ServiceRunningRequirementReason reason )
        {
            if( _mustExistSpecialization != null && _mustExistSpecialization != this )
            {
                return _mustExistSpecialization.SetRunningRequirement( r, reason );
            }
            if( r <= _runningRequirement )
            {
                if( r >= RunningRequirement.Runnable ) return !Disabled;
                return true;
            }
            // New requirement is stronger than the previous one.
            // We now try to honor the requirement at the service level.
            // If we fail, this service will be disabled, but we set the requirement to prevent reentrancy.
            // Reentrancy can nevertheless be caused by subsequent requirements MustExistTryStart or MustExistAndRun:
            // we allow this (there will be at most 3 reentrant calls to this method). 
            // Note that we capture the reason only on the first call, not on each failing call: the reason is not necessarily 
            // associated to the running requirement.
            var current = _runningRequirement;
            _runningRequirement = r;
            // Is it compliant with a Disabled service? If yes, it is always satisfied.
            if( r < RunningRequirement.Runnable )
            {
                _runningRequirementReason = reason;
                // Propagate TryStart.
                PropagateRunningRequirementToOnlyPluginOrCommonReferences();
                return true;
            }
            // The new requirement is at least MustExist.
            // If this is already disabled, there is nothing to do.
            if( Disabled ) return false;

            _runningRequirementReason = reason;

            if( current < RunningRequirement.Runnable )
            {
                if( !SetAsMustExistService() ) return false;
            }
            Debug.Assert( !Disabled );
            // Now, if the OnlyPlugin exists, propagate the MustExist (or more) requirement to it.
            // MustExist requirement triggers MustExist on MustExist plugins to services requirements.
            // (This can be easily propagated if there is one and only one plugin for the service.)
            //
            // If more than one plugin exist, we can actually propagate requirements to all the Services that are shared 
            // by all our non-disabled plugins: we initialize our CommonServiceReferences object.
            //
            if( TotalAvailablePluginCount > 1 ) InitializePropagation( TotalAvailablePluginCount, fromConfig:false );
            PropagateRunningRequirementToOnlyPluginOrCommonReferences();
            return !Disabled;
        }

        /// <summary>
        /// Called by SetRunningRequirement whenever the Requirement becomes MustExist, or by ServiceRootData.OnAllPluginsAdded
        /// if a MustExistPluginByConfig exists for the root.
        /// </summary>
        /// <returns></returns>
        internal bool SetAsMustExistService( bool fromMustExistPlugin = false )
        {
            if( fromMustExistPlugin )
            {
                _runningRequirement = GeneralizationRoot.MustExistPluginByConfig.MinimalRunningRequirement;
            }
            Debug.Assert( _runningRequirement >= RunningRequirement.Runnable );
            // From a non running requirement to a running requirement.
            var currentMustExist = GeneralizationRoot.MustExistService;
            //
            // Only 2 possible cases here:
            //
            // - There is no current MustExist Service for our Generalization.
            // - We specialize the current one.
            //
            Debug.Assert( currentMustExist == null || currentMustExist.IsGeneralizationOf( this ) );
            // Note: The other cases would be:
            //    - We are a Generalization of the current one. This is not possible since SetRunningRequirement is routed to the _mustExistSpecialization if it exists.
            //    - we are the current one... We would necessarily already be MustExist.
            //    - a specialization exists and we are not a specialization nor a generalization of it: this is not possible since we would have been disabled.
            //
            // When a MustExist appears below a current one, the requirement of the current one (the generalization) may be stronger than
            // the new one: the new one must be updated to honor the generalization's requirement.
            //
            // Once a MustExist is set, the _mustExistSpecialization is used to reroute the call to SetRunningRequirement and the MinimalRunningRequirement property:
            // it is useless to propagate Requirement up to the MustExist branch.
            //
            if( currentMustExist != null && currentMustExist._runningRequirement > _runningRequirement )
            {
                _runningRequirement = currentMustExist._runningRequirement;
                _runningRequirementReason = currentMustExist._runningRequirementReason;
            }

            // We must disable all sibling services (and plugins) from this up to mustExist (when mustExist is null, up to the root).
            _mustExistSpecialization = this;
            var g = Generalization;
            if( g != null )
            {
                var specThatMustExist = this;
                do
                {
                    g._mustExistSpecialization = this;
                    g._directMustExistSpecialization = specThatMustExist;
                    var spec = g.FirstSpecialization;
                    while( spec != null )
                    {
                        if( spec != specThatMustExist && !spec.Disabled ) spec.SetDisabled( ServiceDisabledReason.AnotherSpecializationMustExist );
                        spec = spec.NextSpecialization;
                    }
                    PluginData p = g.FirstPlugin;
                    while( p != null )
                    {
                        if( !p.Disabled ) p.SetDisabled( PluginDisabledReason.ServiceSpecializationMustExist );
                        p = p.NextPluginForService;
                    }
                    specThatMustExist = g;
                    g = g.Generalization;
                }
                while( g != currentMustExist );
            }
            if( Disabled ) return false;
            GeneralizationRoot.MustExistServiceChanged( this );
            return true;
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
        /// Number of direct specializations.
        /// </summary>
        public int SpecializationCount;

        /// <summary>
        /// Head of the linked list of PluginData that implement this exact Service (not specialized ones).
        /// </summary>
        public PluginData FirstPlugin;

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
        /// First step after object construction.
        /// </summary>
        /// <returns>The deepest specialization that must exist or null if none must exist or a conflict exists.</returns>
        protected ServiceData GetMustExistService()
        {
            Debug.Assert( !Disabled, "Must NOT be called on already disabled service." );
            // Handles direct specializations that MustExist.
            ServiceData directSpecThatHasMustExist = null;
            ServiceData specMustExist = null;
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                if( !spec.Disabled )
                {
                    var mustExist = spec.GetMustExistService();
                    // We may stop as soon as a conflict is detected, but we continue the loop to let any branches detect other potential conflicts.
                    if( !Disabled )
                    {
                        if( spec.DisabledReason == ServiceDisabledReason.MultipleSpecializationsMustExistByConfig )
                        {
                            Debug.Assert( mustExist == null, "Since a conflict has been detected below, returned mustExist is null." );
                            SetDisabled( ServiceDisabledReason.MultipleSpecializationsMustExistByConfig );
                            directSpecThatHasMustExist = specMustExist = null;
                        }
                        else
                        {
                            Debug.Assert( spec.Disabled == false, "Since it was not disabled before calling GetMustExistService, it could only be ServiceDisabledReason.MultipleSpecializationsMustExist." );
                            if( mustExist != null )
                            {
                                if( specMustExist != null )
                                {
                                    SetDisabled( ServiceDisabledReason.MultipleSpecializationsMustExistByConfig );
                                    directSpecThatHasMustExist = specMustExist = null;
                                }
                                else
                                {
                                    specMustExist = mustExist;
                                    directSpecThatHasMustExist = spec;
                                }
                            }
                        }
                    }
                }
                spec = spec.NextSpecialization;
            }
            Debug.Assert( !Disabled || specMustExist == null, "(Conflict below <==> Disabled) => specMustExist has been set to null." );
            Debug.Assert( (specMustExist != null) == (directSpecThatHasMustExist != null) );
            if( !Disabled )
            {
                // No specialization is required to exist, is it our case?
                if( specMustExist == null )
                {
                    Debug.Assert( ServiceSolvedStatus != SolvedConfigStatus.Disabled, "Caution: Disabled is greater than MustExist." );
                    if( ServiceSolvedStatus >= SolvedConfigStatus.Runnable ) specMustExist = _mustExistSpecialization = this;
                }
                else
                {
                    // A specialization must exist: it must be the only one, others are disabled.
                    spec = FirstSpecialization;
                    while( spec != null )
                    {
                        if( spec != directSpecThatHasMustExist && !spec.Disabled )
                        {
                            spec.SetDisabled( ServiceDisabledReason.AnotherSpecializationMustExistByConfig );
                        }
                        spec = spec.NextSpecialization;
                    }
                    _mustExistSpecialization = specMustExist;
                    _directMustExistSpecialization = directSpecThatHasMustExist;
                    // Since there is a MustExist specialization, it concentrates the running requirements
                    // of all its generalization (here from their configurations).
                    if( _runningRequirement > specMustExist._runningRequirement )
                    {
                        specMustExist._runningRequirement = _runningRequirement;
                        specMustExist._runningRequirementReason = ServiceRunningRequirementReason.FromGeneralization;
                    }
                }
                Debug.Assert( !Disabled, "The fact that this service (or a specialization) must exist, can not disable this service." );
            }
            return specMustExist;
        }

        internal void AddPlugin( PluginData p )
        {
            // Consider its RunningRequirements to detect trivial case: the fact that another plugin 
            // must exist for the same Generalization service.
            // The less trivially case when this must exist plugin conflicts with some MustExist at the services level
            // is already handled in PluginData constructor thanks to service.MustExistSpecialization beeing not null that 
            // immediately disables the plugin.
            if( p.MinimalRunningRequirement >= RunningRequirement.Runnable )
            {
                Debug.Assert( !p.Disabled );
                GeneralizationRoot.SetMustExistPluginByConfig( p );
            }
            // Adds the plugin, taking its disabled state into account.
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

        internal void AddMustExistReferencer( PluginData plugin )
        {
            Debug.Assert( !Disabled );
            if( _mustExistReferencer == null ) _mustExistReferencer = new List<PluginData>();
            _mustExistReferencer.Add( plugin );
        }

        internal virtual void OnAllPluginsAdded()
        {
            Debug.Assert( !Disabled, "Must NOT be called on already disabled service." );
            Debug.Assert( (MustExistSpecialization == null || MustExistSpecialization == this) || PluginCount == DisabledPluginCount, "If there is a must exist specialization, all our plugins are disabled." );

            // Recursive call: the only plugin or the CommonServiceReferences are
            // updated bottom up, so that this Generalization can reuse them.
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                if( !spec.Disabled ) OnAllPluginsAdded();
                spec = spec.NextSpecialization;
            }
            // For raw Service (from Service container) we have nothing to do... 
            // they are available or not (and then they are Disabled).
            if( ServiceInfo.IsDynamicService )
            {
                // Handle the case where TotalPluginCount is zero (there is no implementation).
                // or where TotalDisabledPluginCount is the same as TotalPluginCount.
                if( TotalPluginCount == 0 )
                {
                    SetDisabled( ServiceDisabledReason.NoPlugin );
                }
                else
                {
                    int nbAvailable = TotalAvailablePluginCount;
                    if( nbAvailable == 0 )
                    {
                        SetDisabled( ServiceDisabledReason.AllPluginsAreDisabled );
                    }
                    else InitializePropagation( nbAvailable, fromConfig: true );
                }
            }
        }

        internal void OnPluginDisabled( PluginData p )
        {
            Debug.Assert( p.Service == this && p.Disabled );
            ++DisabledPluginCount;
            // We must update and check the total number of plugins.
            ServiceData g = this;
            while( g != null )
            {
                ++g.TotalDisabledPluginCount;
                int nbAvailable = TotalAvailablePluginCount;
                if( nbAvailable == 0 )
                {
                    _theOnlyPlugin = null;
                    _commonReferences = null;
                    if( !g.Disabled ) SetDisabled( ServiceDisabledReason.AllPluginsAreDisabled );
                }
                else InitializePropagation( nbAvailable, fromConfig: false ); 
                g = g.Generalization;
            }
        }

        public override string ToString()
        {
            return String.Format( "{0} - {1} - {2} - {4} plugins => ((Dynamic: {3})", ServiceInfo.ServiceFullName, Disabled ? DisabledReason.ToString() : "!Disabled", MinimalRunningRequirement, _status, TotalAvailablePluginCount );
        }
    }
}
