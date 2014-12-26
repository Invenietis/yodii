using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using CK.Core;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    static class DynamicTestExtensions
    {
        public static void FullStart( this YodiiEngine @this, Action<YodiiEngine,IYodiiEngineResult> tests, [CallerMemberName]string callerName = null )
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
        public static void CheckAllDisabled( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, RunningStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllStopped( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, RunningStatus.Stopped, pluginOrServiceNames, false );
        }

        public static void CheckAllRunning( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, RunningStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckAllRunningLocked( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, RunningStatus.RunningLocked, pluginOrServiceNames, false );
        }

        public static void CheckDisabled( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, RunningStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckStopped( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, RunningStatus.Stopped, pluginOrServiceNames, true );
        }

        public static void CheckRunning( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, RunningStatus.Running, pluginOrServiceNames, true );
        }

        public static void CheckRunningLocked( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckStatus( @this, RunningStatus.RunningLocked, pluginOrServiceNames, true );
        }

        static void CheckStatus( this YodiiEngine @this, RunningStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Plugins.Where( p => p.RunningStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            withTheStatus = withTheStatus.Concat( @this.LiveInfo.Services.Where( s => s.RunningStatus == status ).Select( s => s.ServiceInfo.ServiceFullName ) );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

        #region Plugins only
        public static void CheckAllPluginsDisabled( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, RunningStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllPluginsStopped( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, RunningStatus.Stopped, pluginOrServiceNames, false );
        }

        public static void CheckAllPluginsRunning( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, RunningStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckAllPluginsRunningLocked( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, RunningStatus.RunningLocked, pluginOrServiceNames, false );
        }

        public static void CheckPluginsDisabled( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, RunningStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckPluginsStopped( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, RunningStatus.Stopped, pluginOrServiceNames, true );
        }

        public static void CheckPluginsRunning( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, RunningStatus.Running, pluginOrServiceNames, true );
        }

        public static void CheckPluginsRunningLocked( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckPluginsStatus( @this, RunningStatus.RunningLocked, pluginOrServiceNames, true );
        }

        static void CheckPluginsStatus( this YodiiEngine @this, RunningStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Plugins.Where( p => p.RunningStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

        #region Services only
        public static void CheckAllServicesDisabled( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, RunningStatus.Disabled, pluginOrServiceNames, false );
        }

        public static void CheckAllServicesStopped( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, RunningStatus.Stopped, pluginOrServiceNames, false );
        }

        public static void CheckAllServicesRunning( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, RunningStatus.Running, pluginOrServiceNames, false );
        }

        public static void CheckAllServicesRunningLocked( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, RunningStatus.RunningLocked, pluginOrServiceNames, false );
        }

        public static void CheckServicesDisabled( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, RunningStatus.Disabled, pluginOrServiceNames, true );
        }

        public static void CheckServicesStopped( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, RunningStatus.Stopped, pluginOrServiceNames, true );
        }

        public static void CheckServicesRunning( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, RunningStatus.Running, pluginOrServiceNames, true );
        }

        public static void CheckServicesRunningLocked( this YodiiEngine @this, string pluginOrServiceNames )
        {
            CheckServicesStatus( @this, RunningStatus.RunningLocked, pluginOrServiceNames, true );
        }

        static void CheckServicesStatus( this YodiiEngine @this, RunningStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Services.Where( s => s.RunningStatus == status ).Select( s => s.ServiceInfo.ServiceFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
        }
        #endregion

    }
}
