using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    partial class ServiceRootData : ServiceData
    {
        internal ServiceRootData( Dictionary<string, ServiceData> allServices, IServiceInfo s, ConfigurationStatus serviceStatus, Func<IServiceInfo,bool> isExternalServiceAvailable )
            : base( allServices, s, null, serviceStatus, isExternalServiceAvailable )
        {
        }

        public PluginData TheOnlyPlugin
        {
            get { return _theOnlyPlugin; }
        }

        public PluginData RunningPluginByConfig
        {
            get { return _theOnlyPlugin != null && _theOnlyPlugin.ConfigOriginalStatus == ConfigurationStatus.Running ? _theOnlyPlugin : null; }
        }

        internal void InitializeRunningService()
        {
            Debug.Assert( !Disabled );
            var s = GetRunningService();
            Debug.Assert( s == null || !s.Disabled, "s != null => !s.Disabled" );
            Debug.Assert( ConfigOriginalStatus != ConfigurationStatus.Running || s != null || Disabled, "ConfigOriginalStatus == ConfigurationStatus.Running && s == null ==> this.Disabled" );

            if( !Disabled )
            {
                if( s != null ) s.BuildDirectExcludedServices();
                else BuildDirectExcludedServices();
            }
        }

        internal override void OnAllPluginsAdded()
        {
            Debug.Assert( !Disabled );
            base.OnAllPluginsAdded();
            if( !Disabled && _theOnlyPlugin != null )
            {
                _theOnlyPlugin.Service.DoSetAsRunningService( null );
            }
        }

        /// <summary>
        /// Called by ServiceData.PluginData during plugin registration.
        /// This does not immediately call ServiceData.SetAsRunningService() in order to offer PluginDisabledReason.AnotherPluginAlreadyExistForTheSameService reason
        /// rather than PluginDisabledReason.ServiceSpecializationMustRun for next conflicting plugins.
        /// </summary>
        internal void SetRunningPluginByConfig( PluginData p )
        {
            Debug.Assert( !Disabled );
            Debug.Assert( p.ConfigSolvedStatus >= SolvedConfigurationStatus.Runnable );
            Debug.Assert( p.Service == null || p.Service.GeneralizationRoot == this, "When called from PluginData constructor, Service is not yet set." );
            if( _theOnlyPlugin == null )
            {
                _theOnlyPlugin = p;
            }
            else
            {
                p.SetDisabled( PluginDisabledReason.AnotherPluginAlreadyExistsForTheSameService );
            }
        }
    }
}
