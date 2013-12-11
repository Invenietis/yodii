using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{   
    internal partial class ServiceData
    {
        readonly Dictionary<string,ServiceData> _allServices;
        ServiceData[] _directExcludedServices;
        ServiceDisabledReason _configDisabledReason;
        ConfigurationStatus _configSolvedStatus;
        ServiceSolvedConfigStatusReason _configSolvedStatusReason;
        ServiceData _configRunningSpecialization;

        class BackReference
        {
            public readonly PluginData PluginData;
            public readonly DependencyRequirement Requirement;
            public readonly BackReference Next;

            public BackReference( BackReference next, PluginData p, DependencyRequirement req )
            {
                PluginData = p;
                Requirement = req;
                Next = next;
            }

        }
        BackReference _firstBackRunnableReference;

        public readonly IServiceInfo ServiceInfo;

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
        public readonly ConfigurationStatus ConfigOriginalStatus;

        internal ServiceData( Dictionary<string, ServiceData> allServices, IServiceInfo s, ServiceData generalization, ConfigurationStatus serviceStatus, Func<IServiceInfo,bool> isExternalServiceAvailable )
        {
            _allServices = allServices;
            _directExcludedServices = Util.EmptyArray<ServiceData>.Empty;
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
            if ( (ConfigOriginalStatus = serviceStatus) == ConfigurationStatus.Disabled )
            {
                _configDisabledReason = ServiceDisabledReason.Config;
            }
            else if( s.HasError )
            {
                _configDisabledReason = ServiceDisabledReason.ServiceInfoHasError;
            }
            else if( Generalization != null && Generalization.Disabled )
            {
                _configDisabledReason = ServiceDisabledReason.GeneralizationIsDisabledByConfig;
            }
            //else if( !s.IsDynamicService && !isExternalServiceAvailable( s ) )
            //{
            //    _disabledReason = ServiceDisabledReason.ExternalServiceUnavailable;
            //}            
            _configSolvedStatusReason = ServiceSolvedConfigStatusReason.Config;
            if ( !Disabled )
            {
                _configSolvedStatus = serviceStatus;
            }
        }

        /// <summary>
        /// Gets whether this service is disabled. 
        /// </summary>
        public bool Disabled
        {
            get { return _configDisabledReason != ServiceDisabledReason.None; }
        }

        public ServiceData MustExistSpecialization
        {
            get { return _configRunningSpecialization; }
        }

        private bool IsStrictGeneralizationOf( ServiceData d )
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
            get { return _configDisabledReason; }
        }

        internal void SetDisabled( ServiceDisabledReason r )
        {
            Debug.Assert( r != ServiceDisabledReason.None );
            Debug.Assert( _configDisabledReason == ServiceDisabledReason.None );
            Debug.Assert( !GeneralizationRoot.Disabled ||
                (GeneralizationRoot.DisabledReason == ServiceDisabledReason.MultipleSpecializationsRunningByConfig
                || GeneralizationRoot.DisabledReason == ServiceDisabledReason.GeneralizationIsDisabled)
                && r == ServiceDisabledReason.GeneralizationIsDisabled, "A root is necessarily not disabled if one of its specialization is not disabled except if we are propagating a MultipleSpecializationsRunningByConfig to the specialized Services." );
            _configDisabledReason = r;
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
            // The _mustExistReferencer list contains plugins that has at least a Runnable reference to this service
            // and have been initialized when this Service was not yet disabled.
            BackReference br = _firstBackRunnableReference;
            while( br != null )
            {
                if( !br.PluginData.Disabled ) br.PluginData.SetDisabled( PluginDisabledReason.MustExistReferenceIsDisabled );
                br = br.Next;
            }
            _configRunningSpecialization = null;
            _theOnlyPlugin = null;
        }

        /// <summary>
        /// Gets the minimal running requirement. It is initialized by the configuration, but may evolve.
        /// </summary>
        public ConfigurationStatus ConfigSolvedStatus
        {
            get { return _configRunningSpecialization != null ? _configRunningSpecialization._configSolvedStatus : _configSolvedStatus; }
        }

        /// <summary>
        /// Gets the minimal running requirement for this service (not the one of MustExistSpecialization if it exists).
        /// </summary>
        public ConfigurationStatus ThisConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        /// <summary>
        /// Gets the strongest reason that explains this service ThisMinimalRunningRequirement. 
        /// </summary>
        public ServiceSolvedConfigStatusReason ThisConfigSolvedStatusReason 
        {
            get { return _configSolvedStatusReason; }
        }

        /// <summary>
        /// This can be called on an already disabled service and may trigger changes on the whole system.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="reason"></param>
        /// <returns>True if the requirement can be satisfied at this level. False otherwise.</returns>
        internal bool SetSolvedConfigurationStatus( ConfigurationStatus s, ServiceSolvedConfigStatusReason reason )
        {
            if( _configRunningSpecialization != null && _configRunningSpecialization != this )
            {
                Debug.Assert( _configRunningSpecialization._configSolvedStatus == ConfigurationStatus.Running );
                return _configRunningSpecialization.SetSolvedConfigurationStatus( s, reason );
            }
            if( s <= _configSolvedStatus )
            {
                if( s >= ConfigurationStatus.Runnable ) return !Disabled;
                return true;
            }
            // New requirement is stronger than the previous one.
            // We now try to honor the requirement at the service level.
            // If we fail, this service will be disabled, but we set the requirement to prevent reentrancy.
            // Reentrancy can nevertheless be caused by subsequent requirements RunnableTryStart or Running:
            // we allow this (there will be at most 3 reentrant calls to this method). 
            // Note that we capture the reason only on the first call, not on each failing call: the reason is not necessarily 
            // associated to the running requirement.
            var current = _configSolvedStatus;
            _configSolvedStatus = s;
            // Is it compliant with a Disabled service? If yes, it is always satisfied.
            if( s < ConfigurationStatus.Runnable )
            {
                _configSolvedStatusReason = reason;
                return true;
            }
            // The new requirement is at least Runnable.
            // If this service is already disabled, there is nothing to do.
            if( Disabled ) return false;

            _configSolvedStatusReason = reason;

            // Call SetAsRunningService only if this Service becomes Running.
            if( _configSolvedStatus == ConfigurationStatus.Running && current < ConfigurationStatus.Running )
            {
                if( !SetAsRunningService() ) return false;
            }
            Debug.Assert( !Disabled );
            // Now, if the OnlyPlugin exists, propagate the Runnable, Runnable or Running requirement to it.
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
        /// Called by SetSolvedConfigurationStatus whenever the Requirement becomes Running, or by ServiceRootData.OnAllPluginsAdded
        /// if a RunningPluginByConfig exists for the root.
        /// </summary>
        /// <returns></returns>
        internal bool SetAsRunningService( bool fromRunningPlugin = false )
        {
            if( fromRunningPlugin )
            {
                Debug.Assert( GeneralizationRoot.RunningPluginByConfig.ConfigSolvedStatus == ConfigurationStatus.Running );
                _configSolvedStatus = ConfigurationStatus.Running;
            }
            Debug.Assert( _configSolvedStatus == ConfigurationStatus.Running );

            Debug.Assert( _configRunningSpecialization != this, "SetSolvedConfigurationStatus filters this thanks to the _configSolvedStatus." );

            var currentRunning = GeneralizationRoot._configRunningSpecialization;
            if( currentRunning != null )
            {
                // If there is already a current specialized service, we ONLY accept a more specialized 
                // running service than the current one: if this service is not a specialization, we reject the change.
                if( !currentRunning.IsStrictGeneralizationOf( this ) ) return false;
            }
            DoSetAsRunningService( currentRunning );
            return !Disabled;
        }

        /// <summary>
        /// Called by SetSolvedConfigurationStatus whenever the Requirement becomes Running, or by ServiceRootData.OnAllPluginsAdded
        /// if a RunningPluginByConfig exists for the root.
        /// </summary>
        /// <returns></returns>
        internal void DoSetAsRunningService( ServiceData prevCurrentRunning )
        {
            // We first set the new specialization from this service up to the root.
            var g = this;
            while( g != null )
            {
                g._configRunningSpecialization = this;
                g = g.Generalization;
            }

            // We must now disable all sibling services (and plugins) from this up to the currently running one and 
            // when the current one is null, up to the root.
            g = Generalization;
            var specThatMustExist = this;
            while( g != prevCurrentRunning )
            {
                var spec = g.FirstSpecialization;
                while( spec != null )
                {
                    if( spec != specThatMustExist && !spec.Disabled ) spec.SetDisabled( ServiceDisabledReason.AnotherSpecializationMustRun );
                    spec = spec.NextSpecialization;
                }
                PluginData p = g.FirstPlugin;
                while( p != null )
                {
                    if( !p.Disabled ) p.SetDisabled( PluginDisabledReason.ServiceSpecializationMustRun );
                    p = p.NextPluginForService;
                }
                specThatMustExist = g;
                g = g.Generalization;
            }
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
        /// Never null.
        /// Should be a IReadOnlyList in .Net 4.5.
        /// </summary>
        internal ServiceData[] DirectExcludedServices 
        { 
            get { return _directExcludedServices; } 
        }

        /// <summary>
        /// First step after object construction: called by ServiceRootData.InitializeRunningService.
        /// </summary>
        /// <returns>The deepest specialization that must run or null if no running service or a conflict exists.</returns>
        protected ServiceData GetRunningService()
        {
            Debug.Assert( !Disabled, "Must NOT be called on already disabled service." );
            // Handles direct specializations that MustExist.
            ServiceData directSpecThatMustRun = null;
            ServiceData specRunning = null;
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                if( !spec.Disabled )
                {
                    var running = spec.GetRunningService();
                    // We may stop as soon as a conflict is detected, but we continue the loop to let any branches detect other potential conflicts.
                    if( !Disabled )
                    {
                        if( spec.DisabledReason == ServiceDisabledReason.MultipleSpecializationsRunningByConfig )
                        {
                            Debug.Assert( running == null, "Since a conflict has been detected below, returned running is null." );
                            SetDisabled( ServiceDisabledReason.MultipleSpecializationsRunningByConfig );
                            directSpecThatMustRun = specRunning = null;
                        }
                        else
                        {
                            Debug.Assert( spec.Disabled == false, "Since it was not disabled before calling GetRunningService, it could only be ServiceDisabledReason.MultipleSpecializationsMustExist." );
                            if( running != null )
                            {
                                if( specRunning != null )
                                {
                                    SetDisabled( ServiceDisabledReason.MultipleSpecializationsRunningByConfig );
                                    directSpecThatMustRun = specRunning = null;
                                }
                                else
                                {
                                    specRunning = running;
                                    directSpecThatMustRun = spec;
                                }
                            }
                        }
                    }
                }
                spec = spec.NextSpecialization;
            }
            Debug.Assert( !Disabled || specRunning == null, "(Conflict below <==> Disabled) => specRunning has been set to null." );
            Debug.Assert( (specRunning != null) == (directSpecThatMustRun != null) );
            if( !Disabled )
            {
                // No specialization is required to exist, is it our case?
                if( specRunning == null )
                {
                    if( ConfigOriginalStatus == ConfigurationStatus.Running ) specRunning = _configRunningSpecialization = this;
                }
                else
                {
                    // A specialization must be running: it must be the only one, others are disabled.
                    spec = FirstSpecialization;
                    while( spec != null )
                    {
                        if( spec != directSpecThatMustRun && !spec.Disabled )
                        {
                            spec.SetDisabled( ServiceDisabledReason.AnotherSpecializationMustExistByConfig );
                        }
                        spec = spec.NextSpecialization;
                    }
                    _configRunningSpecialization = specRunning;
                }
                Debug.Assert( !Disabled, "The fact that this service (or a specialization) must exist, can not disable this service." );
            }
            return specRunning;
        }

        /// <summary>
        /// Called by ServiceRootData.InitializeRunningService after GetRunningService
        /// to build the direct excluded services (for non already disabled services).
        /// </summary>
        internal void BuildDirectExcludedServices()
        {
            Debug.Assert( !Disabled );
            if( Generalization != null )
            {
                List<ServiceData> excl = null;
                if( Generalization._directExcludedServices.Length > 0 ) excl = new List<ServiceData>( Generalization._directExcludedServices );
                ServiceData sibling = Generalization.FirstSpecialization;
                while( sibling != null )
                {
                    if( sibling != this && !sibling.Disabled )
                    {
                        if( excl == null ) excl = new List<ServiceData>();
                        excl.Add( sibling );
                    }
                    sibling = sibling.NextSpecialization;
                }
                if( excl != null ) _directExcludedServices = excl.ToArray();
            }
            ServiceData child = FirstSpecialization;
            while( child != null )
            {
                if( !child.Disabled ) child.BuildDirectExcludedServices();
                child = child.NextSpecialization;
            }
        }

        PluginData FindFirstPluginData( Func<PluginData, bool> filter )
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
            // Consider its ConfigSolvedStatus to detect a trivial case: the fact that another plugin 
            // must run for the same Generalization service.
            // The less trivially case when this running plugin conflicts with some other Running at the services level
            // is already handled in PluginData constructor thanks to service.MustExistSpecialization being not null that 
            // immediately disables the plugin.
            if( p.ConfigSolvedStatus == ConfigurationStatus.Running )
            {
                Debug.Assert( !p.Disabled );
                GeneralizationRoot.SetRunningPluginByConfig( p );
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

        /// <summary>
        /// Adds a plugin that requires this service with Runnable, RunnableTryStart or Running requirement.
        /// </summary>
        /// <param name="plugin">The plugin that references us.</param>
        internal void AddRunnableReferencer( PluginData plugin, DependencyRequirement req )
        {
            Debug.Assert( !Disabled );
            Debug.Assert( req >= DependencyRequirement.Runnable );
            _firstBackRunnableReference = new BackReference( _firstBackRunnableReference, plugin, req );
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
                if( !spec.Disabled ) spec.OnAllPluginsAdded();
                spec = spec.NextSpecialization;
            }
            // For raw Service (from Service container) we have nothing to do... 
            // they are available or not (and then they are Disabled).
            if( !Disabled )
            {
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
            Debug.Assert( (p.Service == this || IsStrictGeneralizationOf( p.Service )) && p.Disabled );
            if( p.Service == this ) ++DisabledPluginCount;
            ++TotalDisabledPluginCount;
            if( !Disabled )
            {
                int nbAvailable = TotalAvailablePluginCount;
                if ( nbAvailable == 0 )
                {
                    _theOnlyPlugin = null;
                    _commonReferences = null;
                    SetDisabled( ServiceDisabledReason.AllPluginsAreDisabled );
                }
                else InitializePropagation( nbAvailable, fromConfig: false );
            }
            // We must update and check the total number of plugins.
            if( Generalization != null ) Generalization.OnPluginDisabled( p );
        }

        public override string ToString()
        {
            return String.Format( "{0} - {1} - {2} - {4} plugins => ((Dynamic: {3})", ServiceInfo.ServiceFullName, Disabled ? DisabledReason.ToString() : "!Disabled", ConfigSolvedStatus, _dynamicStatus, TotalAvailablePluginCount );
        }
    }
}
