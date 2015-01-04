using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using CK.Core;
using NUnit.Framework;
using Yodii.Engine;
using Yodii.Model;

namespace Yodii
{
    static class DynamicTestExtensions
    {
        public static void FullStartAndStop( this YodiiEngine @this, Action<YodiiEngine,IYodiiEngineResult> tests, [CallerMemberName]string callerName = null )
        {
            IActivityMonitor m = TestHelper.ConsoleMonitor;
            IYodiiEngineResult result;

            using( m.OpenInfo().Send( "FullStart for {0}.", callerName ) )
            {
                using( m.OpenInfo().Send( "FullStart()." ) )
                {
                    result = @this.StartEngine( false, false );
                    result.Trace( m );
                    tests( @this, result );
                    @this.StopEngine();
                }
                using( m.OpenInfo().Send( "FullStart( revertServices )." ) )
                {
                    result = @this.StartEngine( true, false );
                    result.Trace( m );
                    tests( @this, result );
                    @this.StopEngine();
                }
                using( m.OpenInfo().Send( "FullStart( revertPlugins )." ) )
                {
                    result = @this.StartEngine( false, true );
                    result.Trace( m );
                    tests( @this, result );
                    @this.StopEngine();
                }
                using( m.OpenInfo().Send( "FullStart( revertServices, revertPlugins )." ) )
                {
                    result = @this.StartEngine( true, true );
                    result.Trace( m );
                    tests( @this, result );
                    @this.StopEngine();
                }
            }
        }

        #region Plugins and Services
        public static IYodiiEngine CheckAllDisabled( this IYodiiEngine @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Disabled, serviceOrPluginNames, false );
        }

        public static IYodiiEngine CheckAllStopped( this IYodiiEngine @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Stopped, serviceOrPluginNames, false );
        }

        public static IYodiiEngine CheckAllRunning( this IYodiiEngine @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Running, serviceOrPluginNames, false );
        }

        public static IYodiiEngine CheckAllRunningLocked( this IYodiiEngine @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.RunningLocked, serviceOrPluginNames, false );
        }

        public static IYodiiEngine CheckDisabled( this IYodiiEngine @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Disabled, serviceOrPluginNames, true );
        }

        public static IYodiiEngine CheckStopped( this IYodiiEngine @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Stopped, serviceOrPluginNames, true );
        }

        public static IYodiiEngine CheckRunning( this IYodiiEngine @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Running, serviceOrPluginNames, true );
        }

        public static IYodiiEngine CheckRunningLocked( this IYodiiEngine @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.RunningLocked, serviceOrPluginNames, true );
        }

        static IYodiiEngine CheckStatus( this IYodiiEngine @this, RunningStatus status, string serviceOrPluginNames, bool expectedIsPartial )
        {
            string[] expected = serviceOrPluginNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Plugins.Where( p => p.RunningStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            withTheStatus = withTheStatus.Concat( @this.LiveInfo.Services.Where( s => s.RunningStatus == status ).Select( s => s.ServiceInfo.ServiceFullName ) );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

        #region Plugins only
        public static IYodiiEngine CheckAllPluginsDisabled( this IYodiiEngine @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Disabled, pluginNames, false );
        }

        public static IYodiiEngine CheckAllPluginsStopped( this IYodiiEngine @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Stopped, pluginNames, false );
        }

        public static IYodiiEngine CheckAllPluginsRunning( this IYodiiEngine @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Running, pluginNames, false );
        }

        public static IYodiiEngine CheckAllPluginsRunningLocked( this IYodiiEngine @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.RunningLocked, pluginNames, false );
        }

        public static IYodiiEngine CheckPluginsDisabled( this IYodiiEngine @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Disabled, pluginNames, true );
        }

        public static IYodiiEngine CheckPluginsStopped( this IYodiiEngine @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Stopped, pluginNames, true );
        }

        public static IYodiiEngine CheckPluginsRunning( this IYodiiEngine @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Running, pluginNames, true );
        }

        public static IYodiiEngine CheckPluginsRunningLocked( this IYodiiEngine @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.RunningLocked, pluginNames, true );
        }

        static IYodiiEngine CheckPluginsStatus( this IYodiiEngine @this, RunningStatus status, string pluginNames, bool expectedIsPartial )
        {
            string[] expected = pluginNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Plugins.Where( p => p.RunningStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

        #region Services only
        public static IYodiiEngine CheckAllServicesDisabled( this IYodiiEngine @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Disabled, serviceNames, false );
        }

        public static IYodiiEngine CheckAllServicesStopped( this IYodiiEngine @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Stopped, serviceNames, false );
        }

        public static IYodiiEngine CheckAllServicesRunning( this IYodiiEngine @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Running, serviceNames, false );
        }

        public static IYodiiEngine CheckAllServicesRunningLocked( this IYodiiEngine @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.RunningLocked, serviceNames, false );
        }

        public static IYodiiEngine CheckServicesDisabled( this IYodiiEngine @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Disabled, serviceNames, true );
        }

        public static IYodiiEngine CheckServicesStopped( this IYodiiEngine @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Stopped, serviceNames, true );
        }

        public static IYodiiEngine CheckServicesRunning( this IYodiiEngine @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Running, serviceNames, true );
        }

        public static IYodiiEngine CheckServicesRunningLocked( this IYodiiEngine @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.RunningLocked, serviceNames, true );
        }

        static IYodiiEngine CheckServicesStatus( this IYodiiEngine @this, RunningStatus status, string serviceNames, bool expectedIsPartial )
        {
            string[] expected = serviceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Services.Where( s => s.RunningStatus == status ).Select( s => s.ServiceInfo.ServiceFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

    }
}
