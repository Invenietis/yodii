using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    static class TestExtensions
    {

        public static void FullStart( this YodiiEngine @this, Action<IYodiiEngineResult> tests )
        {
            var result = @this.Start();
            tests( result );
            result = @this.Start( true, false );
            tests( result );
            result = @this.Start( false, true );
            tests( result );
            result = @this.Start( true, true );
            tests( result );
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

        public static void CheckWantedConfigSolvedStatusIs( this IYodiiEngineResult @this, string pluginOrServiceFullName, ConfigurationStatus wantedStatus )
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

        public static void CheckBlockingPluginsAre( this IYodiiEngineResult @this, string names )
        {
            string[] n = names.Split( new[]{','}, StringSplitOptions.RemoveEmptyEntries );
            Assert.That( !@this.Success && @this.StaticFailureResult != null, String.Format( "{0} blocking plugins expected. No error found.", n.Length ) );
            CheckOrContains( n, @this.StaticFailureResult.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName ) );
        }

        public static void CheckBlockingServicesAre( this IYodiiEngineResult @this, string names )
        {
            string[] n = names.Split( new[]{','}, StringSplitOptions.RemoveEmptyEntries );
            Assert.That( !@this.Success && @this.StaticFailureResult != null, String.Format( "{0} blocking services expected. No error found.", n.Length ) );
            CheckOrContains( n, @this.StaticFailureResult.BlockingServices.Select( s => s.ServiceInfo.ServiceFullName ) );
        }

        static void CheckOrContains( string[] expected, IEnumerable<string> actual )
        {
            foreach( var segment in expected )
            {
                string[] opt = segment.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                if( !actual.Any( s => opt.Contains( s ) ) ) Assert.Fail( String.Format( "Expected '{0}' but it is missing in '{1}'.", segment, String.Join( ", ", actual ) ) );
            }
            if( expected.Length < actual.Count() ) Assert.Fail( String.Format( "Expected {0} items in '{1}'.", expected.Length, String.Join( ", ", actual ) ) );
        }


    }
}
