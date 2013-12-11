using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{
    partial class PluginData
    {
        readonly Dictionary<string,ServiceData> _allServices;
        ServiceData[] _excludedServices;
        PluginDisabledReason _configDisabledReason;
        ConfigurationStatus _configSolvedStatus;
        PluginRunningRequirementReason _configSolvedStatusReason;

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
        /// Never null.
        /// Should be a IReadOnlyList in .Net 4.5.
        /// </summary>
        internal ServiceData[] ExcludedServices
        {
            get { return _excludedServices; }
        }

        internal PluginData( Dictionary<string,ServiceData> allServices, IPluginInfo p, ServiceData service, ConfigurationStatus pluginStatus )
        {
            _allServices = allServices;
            _excludedServices = Util.EmptyArray<ServiceData>.Empty;
            PluginInfo = p;
            // Updates disabled state first so that AddPlugin can take disabled state into account.
            if( (ConfigOriginalStatus = pluginStatus) == ConfigurationStatus.Disabled )
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
                else if( service.MustExistSpecialization != null && service.MustExistSpecialization != service )
                {
                    _configDisabledReason = PluginDisabledReason.ServiceSpecializationMustRun;
                }
            }
            if( !Disabled )
            {
                HashSet<ServiceData> excludedServices = null;
                // Register MustExist references to Services from this plugin.
                foreach( var sRef in PluginInfo.ServiceReferences )
                {
                    if( sRef.Requirement >= DependencyRequirement.Runnable )
                    {
                        // If the required service is already disabled, we immediately disable this plugin.
                        // If the required service is not yet disabled, we register this plugin data:
                        // whenever the service is disabled, it will disable the plugin.
                        if( sRef.Reference.HasError )
                        {
                            SetDisabled( PluginDisabledReason.RunnableReferenceServiceIsOnError );
                            break;
                        }
                        ServiceData sr = allServices[sRef.Reference.ServiceFullName];
                        if( sr.Disabled )
                        {
                            SetDisabled( PluginDisabledReason.MustExistReferenceIsDisabled );
                            break;
                        }
                        sr.AddRunnableReferencer( this, sRef.Requirement );
                        if( sr.DirectExcludedServices.Length > 0 )
                        {
                            if( excludedServices == null ) excludedServices = new HashSet<ServiceData>( sr.DirectExcludedServices );
                            else excludedServices.UnionWith( sr.DirectExcludedServices );
                        }
                    }
                }
                if( excludedServices != null ) _excludedServices = excludedServices.ToArray();
            }
            // Updates SolvedConfigurationStatus so that AddPlugin can take Runnable into account.
            _configSolvedStatusReason = PluginRunningRequirementReason.Config;
            if ( !Disabled )
            {
                _configSolvedStatus = pluginStatus;
            }
            Debug.Assert( !Disabled || _configSolvedStatus == ConfigurationStatus.Optional, "Disabled => status is Optional." );
            if( service != null )
            {
                service.AddPlugin( this );
                // Sets Service after AddPlugin call to avoid calling Service.OnPluginDisabled 
                // if the AddPlugin or references checks above disables it.
                Service = service;
            }
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

        /// <summary>
        /// Called by ServiceData.RetrieveTheOnlyPlugin and ServiceData.SetSolvedConfigurationStatus.
        /// In both cases, it is to propagate the current service solved status to the plugin.
        /// When called by RetrieveTheOnlyPlugin, it when the plugin became the only plugin.
        /// When called by SetSolvedConfigurationStatus, it is because this plugin is the only plugin.
        /// </summary>
        internal bool SetSolvedConfigurationStatus( ConfigurationStatus s, PluginRunningRequirementReason reason )
        {
            if( s <= _configSolvedStatus )
            {
                if( s >= ConfigurationStatus.Runnable) return !Disabled;
                return true;
            }
            // New requirement is stronger than the previous one.
            _configSolvedStatus = s;
            // Is it compliant with a Disabled plugin? If yes, it is always satisfied.
            if ( s < ConfigurationStatus.Runnable )
            {
                _configSolvedStatusReason = reason;
                // The new requirement is OptionalTryStart. This can always be satisfied.
                return true;
            }
            // The new requirement is at least MustExist.
            // If this is already disabled, there is nothing to do.
            if( Disabled ) return false;

            _configSolvedStatusReason = reason;

            // We are always called by our service: it is useless to upgrade the running
            // requirement of our service here: once the plugin is created, its MustExist configuration
            // is immediately taken into account => this plugin requirement never "pops up" without beeing 
            // driven by one of its Services.

            return PropagateSolvedConfigurationWhenRunnable();
        }

        /// <summary>
        /// This is called by SetRunningRequirement and by ConfigurationSolver.
        /// </summary>
        /// <returns></returns>
        internal bool PropagateSolvedConfigurationWhenRunnable()
        {
            Debug.Assert( !Disabled && _configSolvedStatus >= ConfigurationStatus.Runnable );

            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                if( sRef.Requirement >= DependencyRequirement.Runnable )
                {
                    var propagation =  sRef.Requirement == DependencyRequirement.Running ? _configSolvedStatus : ConfigurationStatus.Runnable;
                    ServiceData sr = _allServices[sRef.Reference.ServiceFullName];
                    if ( !sr.SetSolvedConfigurationStatus( propagation, ServiceSolvedConfigStatusReason.FromRunnableReference ) )
                    {
                        if( !Disabled ) SetDisabled( PluginDisabledReason.RequirementPropagationToReferenceFailed );
                        else _configDisabledReason = PluginDisabledReason.RequirementPropagationToReferenceFailed;
                        break;
                    }
                }
            }
            return !Disabled;
        }
                  
        public override string ToString()
        {
            //_status is obtained through the PluginData.Dynamic partial class
            return String.Format( "{0} - {1} - {2} => (Dynamic: {3})", PluginInfo.PluginFullName, Disabled ? DisabledReason.ToString() : "!Disabled", ConfigSolvedStatus, _dynamicStatus );
        }
    }
}
