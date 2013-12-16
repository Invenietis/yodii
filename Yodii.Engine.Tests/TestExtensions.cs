using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    static class TestExtensions
    {
        public static void BlockingPluginsAre( this IYodiiEngineResult @this, string names )
        {
            string[] n = names.Split( new[]{','}, StringSplitOptions.RemoveEmptyEntries );
            Assert.That( !@this.Success && @this.StaticFailureResult != null, String.Format( "{0} blocking plugins expected. No error found.", n.Length ) );
            CheckOrContains( n, @this.StaticFailureResult.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName ) );
        }

        public static void BlockingServicesAre( this IYodiiEngineResult @this, string names )
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
