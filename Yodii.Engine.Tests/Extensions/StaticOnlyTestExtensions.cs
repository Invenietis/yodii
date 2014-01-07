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
            CheckStatus( @this, SolvedConfigurationStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, SolvedConfigurationStatus.Runnable, pluginOrServiceNames, false );
        }

        public static void CheckAllRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, SolvedConfigurationStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, SolvedConfigurationStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, SolvedConfigurationStatus.Runnable, pluginOrServiceNames, true );
        }

        public static void CheckRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, SolvedConfigurationStatus.Running, pluginOrServiceNames, true );
        }

        static void CheckStatus( this IYodiiEngineStaticOnlyResult @this, SolvedConfigurationStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Plugins.Where( p => p.FinalConfigSolvedStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            withTheStatus = withTheStatus.Concat( @this.StaticSolvedConfiguration.Services.Where( s => s.FinalConfigSolvedStatus == status ).Select( s => s.ServiceInfo.ServiceFullName ) );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

        #region Plugins only
        public static void CheckAllPluginsDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, SolvedConfigurationStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllPluginsRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, SolvedConfigurationStatus.Runnable, pluginOrServiceNames, false );
        }

        public static void CheckAllPluginsRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, SolvedConfigurationStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckPluginsDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, SolvedConfigurationStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckPluginsRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, SolvedConfigurationStatus.Runnable, pluginOrServiceNames, true );
        }

        public static void CheckPluginsRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, SolvedConfigurationStatus.Running, pluginOrServiceNames, true );
        }

        static void CheckPluginsStatus( this IYodiiEngineStaticOnlyResult @this, SolvedConfigurationStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Plugins.Where( p => p.FinalConfigSolvedStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

        #region Services only
        public static void CheckAllServicesDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, SolvedConfigurationStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllServicesRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, SolvedConfigurationStatus.Runnable, pluginOrServiceNames, false );
        }

        public static void CheckAllServicesRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, SolvedConfigurationStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckServicesDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, SolvedConfigurationStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckServicesRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, SolvedConfigurationStatus.Runnable, pluginOrServiceNames, true );
        }

        public static void CheckServicesRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, SolvedConfigurationStatus.Running, pluginOrServiceNames, true );
        }

        static void CheckServicesStatus( this IYodiiEngineStaticOnlyResult @this, SolvedConfigurationStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Services.Where( s => s.FinalConfigSolvedStatus == status ).Select( s => s.ServiceInfo.ServiceFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

    }
}
