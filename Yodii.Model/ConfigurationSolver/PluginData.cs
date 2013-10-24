using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model.CoreModel;
using Yodii.Model;

namespace Yodii.Model.ConfigurationSolver
{
    partial class PluginData
    {
        readonly Dictionary<IServiceInfo,ServiceData> _allServices;
        PluginDisabledReason _configurationDisabledReason;
        RunningRequirement _configurationRunningRequirement;
        PluginRunningRequirementReason _configurationRunningRequirementReason;

        internal static readonly PluginData[] EmptyArray = new PluginData[0];

        internal PluginData( Dictionary<IServiceInfo,ServiceData> allServices, IPluginInfo p, ServiceData service, ConfigurationStatus pluginStatus )
        {
            _allServices = allServices;
            PluginInfo = p;
            // Updates disabled state first so that AddPlugin can take disabled state into account.
            if( (PluginStatus = pluginStatus) == ConfigurationStatus.Disable )
            {
                _configurationDisabledReason = PluginDisabledReason.Config;
            }
            else if( p.HasError )
            {
                _configurationDisabledReason = PluginDisabledReason.PluginInfoHasError;
            }
            else if( service != null )
            {
                if( service.Disabled )
                {
                    _configurationDisabledReason = PluginDisabledReason.ServiceIsDisabled;
                }
                else if( service.MustExistSpecialization != null && service.MustExistSpecialization != service )
                {
                    _configurationDisabledReason = PluginDisabledReason.ServiceSpecializationMustExist;
                }
            }
            if( !Disabled )
            {
                // Register MustExist references to Services from this plugin.
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    if( sRef.Requirement >= RunningRequirement.Runnable )
                    {
                        // If the required service is already disabled, we immediately disable this plugin.
                        // If the required service is not yet disabled, we register this plugin data:
                        // whenever the service is disabled, it will disable the plugin.
                        if( sRef.Reference.HasError )
                        {
                            SetDisabled( PluginDisabledReason.MustExistReferenceServiceIsOnError );
                            break;
                        }
                        ServiceData sr = allServices[sRef.Reference];
                        if( sr.Disabled )
                        {
                            SetDisabled( PluginDisabledReason.MustExistReferenceIsDisabled );
                            break;
                        }
                        sr.AddMustExistReferencer( this );
                    }
                }
            }
            // Updates RunningRequirement so that AddPlugin can take MustExist into account.
            _configurationRunningRequirementReason = PluginRunningRequirementReason.Config;
            if( !Disabled ) _configurationRunningRequirement = (RunningRequirement)pluginStatus;
            if( service != null )
            {
                service.AddPlugin( this );
                // Sets Service after AddPlugin call to avoid calling Service.OnPluginDisabled 
                // if the AddPlugin or references checks above disables it.
                Service = service;
            }
        }

        public readonly IPluginInfo PluginInfo;

        /// <summary>
        /// The ServiceData if this plugin implements a Service, Null otherwise.
        /// </summary>
        public readonly ServiceData Service;

        /// <summary>
        /// The ConfigurationStatus of the plugin itself.
        /// </summary>
        public readonly ConfigurationStatus PluginStatus;

        /// <summary>
        /// Gets whether this plugin must exist or run. It is initialized by the configuration, but may evolve
        /// if this plugin implements a service.
        /// </summary>
        public RunningRequirement MinimalRunningRequirement
        {
            get { return _configurationRunningRequirement; }
        }

        /// <summary>
        /// Gets the strongest reason that explains this plugin MinimalRunningRequirement. 
        /// </summary>
        public PluginRunningRequirementReason MinimalRunningRequirementReason
        {
            get { return _configurationRunningRequirementReason; }
        }
        
        /// <summary>
        /// Link to the next element in the list of sibling PluginData that implement the same Service.
        /// </summary>
        public PluginData NextPluginForService;

        /// <summary>
        /// Gets whether this plugin is disabled. 
        /// </summary>
        public bool Disabled
        {
            get { return _configurationDisabledReason != PluginDisabledReason.None; }
        }

        /// <summary>
        /// Gets the first reason why this plugin is disabled. 
        /// </summary>
        public PluginDisabledReason DisabledReason
        {
            get { return _configurationDisabledReason; }
        }

        internal void SetDisabled( PluginDisabledReason r )
        {
            Debug.Assert( r != PluginDisabledReason.None );
            Debug.Assert( _configurationDisabledReason == PluginDisabledReason.None );
            _configurationDisabledReason = r;
            if( Service != null ) Service.OnPluginDisabled( this );
        }

        /// <summary>
        /// Called by ServiceData.RetrieveTheOnlyPlugin and ServiceData.SetRunningRequirement.
        /// In both cases, it is to propagate the current service requirement to the plugin.
        /// When called by RetrieveTheOnlyPlugin, it when the plugin became the only plugin.
        /// When called by SetRunningRequirement, it is because this plugin is the only plugin.
        /// </summary>
        internal bool SetRunningRequirement( RunningRequirement r, PluginRunningRequirementReason reason )
        {
            if( r <= _configurationRunningRequirement )
            {
                if( r >= RunningRequirement.Runnable) return !Disabled;
                return true;
            }
            // New requirement is stronger than the previous one.
            _configurationRunningRequirement = r;
            // Is it compliant with a Disabled plugin? If yes, it is always satisfied.
            if( r < RunningRequirement.Runnable )
            {
                _configurationRunningRequirementReason = reason;
                // The new requirement is OptionalTryStart. This can always be satisfied.
                return true;
            }
            // The new requirement is at least MustExist.
            // If this is already disabled, there is nothing to do.
            if( Disabled ) return false;

            _configurationRunningRequirementReason = reason;

            // We are always called by our service: it is useless to upgrade the running
            // requirement of our service here: once the plugin is created, its MustExist configuration
            // is immediately taken into account => this plugin requirement never "pops up" without beeing 
            // driven by one of its Services.

            return CheckReferencesWhenMustExist();
        }

        /// <summary>
        /// This is called by SetRunningRequirement and by ConfigurationSolver.
        /// </summary>
        /// <returns></returns>
        internal bool CheckReferencesWhenMustExist()
        {
            Debug.Assert( !Disabled && _configurationRunningRequirement >= RunningRequirement.Runnable );
            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                RunningRequirement propagation = sRef.Requirement;
                if( _configurationRunningRequirement < propagation ) propagation = _configurationRunningRequirement;

                ServiceData sr = _allServices[sRef.Reference];
                if( !sr.SetRunningRequirement( propagation, ServiceRunningRequirementReason.FromMustExistReference ) )
                {
                    if( !Disabled ) SetDisabled( PluginDisabledReason.RequirementPropagationToReferenceFailed );
                    break;
                }
            }
            return !Disabled;
        }

        public override string ToString()
        //_status is obtained through the PluginData.Dynamic partial class
        {
            return String.Format( "{0} - {1} - {2} => (Dynamic: {3})", PluginInfo.PluginFullName, Disabled ? DisabledReason.ToString() : "!Disabled", MinimalRunningRequirement, _status );
        }

    }

}
