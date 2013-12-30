using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CK.Core;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    static class StaticOnlyTestExtensions
    {

        public static void Trace( this IYodiiEngineStaticOnlyResult @this, IActivityMonitor m )
        {
            if( @this.Success )
            {
                m.Trace().Send( "Success!" );
            }
            else
            {
                m.Trace().Send( "Failed!" );
                @this.StaticFailureResult.Trace( m );
            }
        }

        #region Plugins and Services
        public static void CheckAllDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, ConfigurationStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllOptional( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, ConfigurationStatus.Optional, pluginOrServiceNames, false );
        }

        public static void CheckAllRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, ConfigurationStatus.Runnable, pluginOrServiceNames, false );
        }

        public static void CheckAllRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, ConfigurationStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, ConfigurationStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckOptional( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, ConfigurationStatus.Optional, pluginOrServiceNames, true );
        }

        public static void CheckRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, ConfigurationStatus.Runnable, pluginOrServiceNames, true );
        }

        public static void CheckRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, ConfigurationStatus.Running, pluginOrServiceNames, true );
        }

        static void CheckStatus( this IYodiiEngineStaticOnlyResult @this, ConfigurationStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Plugins.Where( p => p.FinalConfigurationStatus() == status ).Select( p => p.PluginInfo.PluginFullName );
            withTheStatus = withTheStatus.Concat( @this.StaticSolvedConfiguration.Services.Where( s => s.FinalConfigurationStatus() == status ).Select( s => s.ServiceInfo.ServiceFullName ) );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

        #region Plugins only
        public static void CheckAllPluginsDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, ConfigurationStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllPluginsOptional( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, ConfigurationStatus.Optional, pluginOrServiceNames, false );
        }

        public static void CheckAllPluginsRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, ConfigurationStatus.Runnable, pluginOrServiceNames, false );
        }

        public static void CheckAllPluginsRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, ConfigurationStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckPluginsDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, ConfigurationStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckPluginsOptional( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, ConfigurationStatus.Optional, pluginOrServiceNames, true );
        }

        public static void CheckPluginsRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, ConfigurationStatus.Runnable, pluginOrServiceNames, true );
        }

        public static void CheckPluginsRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, ConfigurationStatus.Running, pluginOrServiceNames, true );
        }

        static void CheckPluginsStatus( this IYodiiEngineStaticOnlyResult @this, ConfigurationStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Plugins.Where( p => p.FinalConfigurationStatus() == status ).Select( p => p.PluginInfo.PluginFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

        #region Services only
        public static void CheckAllServicesDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, ConfigurationStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllServicesOptional( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, ConfigurationStatus.Optional, pluginOrServiceNames, false );
        }

        public static void CheckAllServicesRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, ConfigurationStatus.Runnable, pluginOrServiceNames, false );
        }

        public static void CheckAllServicesRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, ConfigurationStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckServicesDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, ConfigurationStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckServicesOptional( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, ConfigurationStatus.Optional, pluginOrServiceNames, true );
        }

        public static void CheckServicesRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, ConfigurationStatus.Runnable, pluginOrServiceNames, true );
        }

        public static void CheckServicesRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, ConfigurationStatus.Running, pluginOrServiceNames, true );
        }

        static void CheckServicesStatus( this IYodiiEngineStaticOnlyResult @this, ConfigurationStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Services.Where( s => s.FinalConfigurationStatus() == status ).Select( s => s.ServiceInfo.ServiceFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

    }
}
