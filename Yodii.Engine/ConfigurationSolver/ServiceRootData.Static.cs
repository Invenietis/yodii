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
        PluginData _runningPluginByConfig;

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
            get { return _runningPluginByConfig; }
        }

        internal void InitializeRunningService()
        {
            Debug.Assert( !Disabled );
            var s = GetRunningService();
            Debug.Assert( ConfigOriginalStatus != ConfigurationStatus.Running  || (s != null || s.Disabled), "ConfigOriginalStatus == ConfigurationStatus.Running  ==>  s != null || s.Disabled" );
        }

        internal override void OnAllPluginsAdded()
        {
            Debug.Assert( !Disabled );
            base.OnAllPluginsAdded();
            if( !Disabled && _runningPluginByConfig != null )
            {
                _runningPluginByConfig.Service.DoSetAsRunningService( null );
                _theOnlyPlugin = _runningPluginByConfig;
            }
        }

        internal override void SetDisabled( ServiceDisabledReason r )
        {
            base.SetDisabled( r );
            _runningPluginByConfig = null;
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
            if( _runningPluginByConfig == null )
            {
                _runningPluginByConfig = p;
            }
            else
            {
                p.SetDisabled( PluginDisabledReason.AnotherPluginAlreadyExistsForTheSameService );
            }
        }
    }
}
