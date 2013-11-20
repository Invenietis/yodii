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
        ServiceData _mustExistService;
        PluginData _mustExistPluginByConfig;

        internal ServiceRootData( Dictionary<IServiceInfo, ServiceData> allServices, IServiceInfo s, ConfigurationStatus serviceStatus, Func<IServiceInfo,bool> isExternalServiceAvailable )
            : base( allServices, s, null, serviceStatus, isExternalServiceAvailable )
        {
        }

        public ServiceData MustExistService
        {
            get { return _mustExistService; }
        }

        public PluginData TheOnlyPlugin
        {
            get { return _theOnlyPlugin; }
        }

        public PluginData RunnablePluginByConfig
        {
            get { return _mustExistPluginByConfig; }
        }

        internal void InitializeMustExistService()
        {
            Debug.Assert( !Disabled );
            _mustExistService = GetMustExistService();
            if( _mustExistService == null && ConfigOriginalStatus >= ConfigurationStatus.Runnable ) _mustExistService = this;
        }

        internal override void OnAllPluginsAdded()
        {
            Debug.Assert( !Disabled );
            base.OnAllPluginsAdded();
            if( !Disabled && _mustExistPluginByConfig != null )
            {
                _mustExistPluginByConfig.Service.SetAsRunnableService( fromRunnablePlugin: true );
            }
        }

        internal override void SetDisabled( ServiceDisabledReason r )
        {
            base.SetDisabled( r );
            _mustExistService = null;
            _mustExistPluginByConfig = null;
        }

        internal void MustExistServiceChanged( ServiceData s )
        {
            Debug.Assert( !Disabled );
            _mustExistService = s;
        }

        /// <summary>
        /// Called by ServiceData.PluginData during plugin registration.
        /// This does not immediatly call ServiceData.SetAsMustExistService() in order to offer PluginDisabledReason.AnotherPluginAlreadyExistForTheSameService reason
        /// rather than PluginDisabledReason.ServiceSpecializationMustExist for next conflicting plugins.
        /// </summary>
        internal void SetMustExistPluginByConfig( PluginData p )
        {
            Debug.Assert( !Disabled );
            Debug.Assert( p.ConfigSolvedStatus >= DependencyRequirement.Runnable );
            Debug.Assert( p.Service == null || p.Service.GeneralizationRoot == this, "When called from PluginData ctor, Service is not yet set." );
            if( _mustExistPluginByConfig == null )
            {
                _mustExistPluginByConfig = p;
            }
            else
            {
                p.SetDisabled( PluginDisabledReason.AnotherPluginAlreadyExistsForTheSameService );
            }
        }
    }
}
