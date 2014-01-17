using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using CK.Core;
using Yodii.Model;
using System.Runtime.CompilerServices;

namespace Yodii.Engine.Tests
{
    static class TestExtensions
    {

        public static void Trace( this IYodiiEngineResult @this, IActivityMonitor m )
        {
            if( @this.Success )
            {
                m.Trace().Send( "Success!" );
            }
            else
            {
                m.Trace().Send( "Failed!" );
                if( @this.StaticFailureResult != null ) @this.StaticFailureResult.Trace( m );
            }
        }

        public static void Trace( this IStaticFailureResult @this, IActivityMonitor m )
        {
            m.Trace().Send( "Blocking Plugins: {0}", String.Join( ", ", @this.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName + " (" + p.WantedConfigSolvedStatus + "/" + p.DisabledReason + ")" ) ) );
            m.Trace().Send( "Blocking Services: {0}", String.Join( ", ", @this.BlockingServices.Select( s => s.ServiceInfo.ServiceFullName + " (" + s.WantedConfigSolvedStatus + "/" + s.DisabledReason + ")" ) ) );
        }

        public static void FullStaticResolutionOnly( this YodiiEngine @this, Action<IYodiiEngineStaticOnlyResult> tests, [CallerMemberName]string callerName = null )
        {
            IActivityMonitor m = TestHelper.ConsoleMonitor;
            IYodiiEngineStaticOnlyResult[] result = new IYodiiEngineStaticOnlyResult[4];
            
            using( m.OpenInfo().Send( "FullStaticResolutionOnly for {0}.", callerName ) )
            {
                using( m.OpenInfo().Send( "StaticResolutionOnly()." ) )
                {
                    result[0] = @this.StaticResolutionOnly( false, false );
                    result[0].Trace( m );
                }
                using( m.OpenInfo().Send( "StaticResolutionOnly( revertServices )." ) )
                {
                    result[1] = @this.StaticResolutionOnly( true, false );
                    result[1].Trace( m );
                }
                using( m.OpenInfo().Send( "StaticResolutionOnly( revertPlugins )." ) )
                {
                    result[2] = @this.StaticResolutionOnly( false, true );
                    result[2].Trace( m );
                }
                using( m.OpenInfo().Send( "StaticResolutionOnly( revertServices, revertPlugins )." ) )
                {
                    result[3] = @this.StaticResolutionOnly( true, true );
                    result[3].Trace( m );
                }
                int comparingErrorCount = 0;
                int comparingWarningCount = 0;
                using( m.CatchCounter( ( f, e, w ) => { comparingErrorCount = f + e; comparingWarningCount = w; } ) )
                using( m.OpenInfo().Send( "Comparing results." ) )
                {
                    if( result[0].Success )
                    {
                        if( !result[1].Success ) m.Error().Send( "revertServices has failed." );
                        if( !result[2].Success ) m.Error().Send( "revertPlugins has failed." );
                        if( !result[3].Success ) m.Error().Send( "revertPlugins & revertServices has failed." );
                    }
                    else
                    {
                        var refItems = String.Join( ", ", result[0].StaticFailureResult.BlockingItems.Select( i => i.FullName ).OrderBy( Util.FuncIdentity ) );
                        if( result[1].Success ) m.Error().Send( "revertServices succeeded." );
                        else
                        {
                            var items = String.Join( ", ", result[1].StaticFailureResult.BlockingItems.Select( i => i.FullName ).OrderBy( Util.FuncIdentity ) );
                            if( items != refItems ) m.Warn().Send( "revertServices found blocking items: '{1}' where default found: {0}.", refItems, items );
                        }
                        if( result[2].Success ) m.Error().Send( "revertPlugins succeeded." );
                        else
                        {
                            var items = String.Join( ", ", result[2].StaticFailureResult.BlockingItems.Select( i => i.FullName ).OrderBy( Util.FuncIdentity ) );
                            if( items != refItems ) m.Warn().Send( "revertServices found blocking items: '{1}' where default found: {0}.", refItems, items );
                        }
                        if( result[3].Success ) m.Error().Send( "revertPlugins & revertServices succeeded." );
                        else
                        {
                            var items = String.Join( ", ", result[3].StaticFailureResult.BlockingItems.Select( i => i.FullName ).OrderBy( Util.FuncIdentity ) );
                            if( items != refItems ) m.Warn().Send( "revertPlugins & revertServices found blocking items: '{1}' where default found: {0}.", refItems, items );
                        }
                    }
                }
                using( m.OpenInfo().Send( "Executing tests predicates." ) )
                {
                    tests( result[0] );
                    tests( result[1] );
                    tests( result[2] );
                    tests( result[3] );
                }
                if( comparingErrorCount == 0 )
                {
                    if( comparingWarningCount == 0 )
                        m.CloseGroup( "No difference between plugin/service ordering." );
                    else m.CloseGroup( "Plugin/service ordering leads to different blocking detection. See logs for details." );
                }
                else
                {
                    Assert.Fail( "Plugin/service ordering leads to different result! (See logs for details.)" );
                }
            }
        }

        public static void CheckSuccess( this IYodiiEngineResult @this )
        {
            Assert.That( @this.Success, Is.True );
            Assert.That( @this.StaticFailureResult, Is.Null );
            Assert.That( @this.HostFailureResult, Is.Null );
            Assert.That( @this.ConfigurationFailureResult, Is.Null );
            Assert.That( @this.PluginCulprits, Is.Empty );
            Assert.That( @this.ServiceCulprits, Is.Empty );
        }

        public static void CheckWantedConfigSolvedStatusIs( this IYodiiEngineResult @this, string pluginOrServiceFullName, SolvedConfigurationStatus wantedStatus )
        {
            if( @this.Success )
            {
                Assert.Fail( "Not implemented ==> TODO: IYodiiEngineResult SHOULD have a 'IYodiiEngine Engine' property!" );
            }
            else
            {
                var service = @this.StaticFailureResult.StaticSolvedConfiguration.Services.FirstOrDefault( s => s.ServiceInfo.ServiceFullName == pluginOrServiceFullName );
                if( service != null ) Assert.That( service.WantedConfigSolvedStatus, Is.EqualTo( wantedStatus ), String.Format( "Service '{0}' has a WantedConfigSolvedStatus = '{1}'. It must be '{2}'.", pluginOrServiceFullName, service.WantedConfigSolvedStatus, wantedStatus ) );
                else
                {
                    var plugin = @this.StaticFailureResult.StaticSolvedConfiguration.Plugins.FirstOrDefault( p => p.PluginInfo.PluginFullName == pluginOrServiceFullName );
                    if( plugin != null ) Assert.That( plugin.WantedConfigSolvedStatus, Is.EqualTo( wantedStatus ), String.Format( "Plugin '{0}' has a WantedConfigSolvedStatus = '{1}'. It must be '{2}'.", pluginOrServiceFullName, plugin.WantedConfigSolvedStatus, wantedStatus ) );
                    else Assert.Fail( String.Format( "Plugin or Service '{0}' not found.", pluginOrServiceFullName ) );
                }
            }
        }

        public static void CheckNoBlockingPlugins( this IYodiiEngineResult @this )
        {
            Assert.That( (@this.StaticFailureResult != null ? @this.StaticFailureResult.BlockingPlugins : Enumerable.Empty<IStaticSolvedPlugin>()), Is.Empty );
        }

        public static void CheckNoBlockingServices( this IYodiiEngineResult @this )
        {
            Assert.That( (@this.StaticFailureResult != null ? @this.StaticFailureResult.BlockingServices : Enumerable.Empty<IStaticSolvedService>()), Is.Empty );
        }

        public static void CheckAllBlockingPluginsAre( this IYodiiEngineResult @this, string names )
        {
            string[] n = names.Split( new[]{','}, StringSplitOptions.RemoveEmptyEntries );
            Assert.That( !@this.Success && @this.StaticFailureResult != null, String.Format( "{0} blocking plugins expected. No error found.", n.Length ) );
            CheckContainsAllWithAlternative( n, @this.StaticFailureResult.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName ) );
        }

        public static void CheckAllBlockingServicesAre( this IYodiiEngineResult @this, string names )
        {
            string[] n = names.Split( new[]{','}, StringSplitOptions.RemoveEmptyEntries );
            Assert.That( !@this.Success && @this.StaticFailureResult != null, String.Format( "{0} blocking services expected. No error found.", n.Length ) );
            CheckContainsAllWithAlternative( n, @this.StaticFailureResult.BlockingServices.Select( s => s.ServiceInfo.ServiceFullName ) );
        }

        internal static void CheckContainsAllWithAlternative( string[] expected, IEnumerable<string> actual )
        {
            CheckContainsWithAlternative( expected, actual, false );
        }

        internal static void CheckContainsWithAlternative( string[] expected, IEnumerable<string> actual )
        {
            CheckContainsWithAlternative( expected, actual, true );
        }

        internal static void CheckContainsWithAlternative( string[] expected, IEnumerable<string> actual, bool expectedIsPartial )
        {
            foreach( var segment in expected )
            {
                var opt = segment.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() );
                if( !actual.Any( s => opt.Contains( s ) ) ) Assert.Fail( String.Format( "Expected '{0}' but it is missing in '{1}'.", segment, String.Join( ", ", actual ) ) );
            }
            if( !expectedIsPartial )
            {
                if( expected.Length < actual.Count() ) Assert.Fail( String.Format( "Expected '{0}' but was '{1}'.", String.Join( ", ", expected ), String.Join( ", ", actual ) ) );
            }
        }

    }
}
