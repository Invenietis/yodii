using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    [ExcludeFromCodeCoverage]
    static class ConfigurationManagerExtensions
    {
        public static IYodiiEngineResult AddSuccess( this IConfigurationItemCollection @this, string name, ConfigurationStatus status, ConfigurationStatus? solvedStatusInFinalConfig = null )
        {
            IYodiiEngineResult r = @this.Set( name, status );
            r.CheckSuccess();
            if( solvedStatusInFinalConfig != null ) Assert.That( @this.ParentLayer.ConfigurationManager.FinalConfiguration.GetStatus( name ), Is.EqualTo( solvedStatusInFinalConfig ) );
            return r;
        }

        public static void CheckFinalConfigurationItemStatus( this IConfigurationManager @this, params string[] segment )
        {
            for( int i = 0; i < segment.Length ; i++) segment[i] += ",";
            CheckFinalConfigurationItemStatus( @this, String.Concat( segment ) );
        }

        public static void CheckFinalConfigurationItemStatus( this IConfigurationManager @this, string segment )
        {
            var items = segment.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() );
            foreach( var i in items )
            {
                var split = i.Split( new[] { '=' }, StringSplitOptions.RemoveEmptyEntries );
                if( split.Length > 2 ) Assert.Fail( "expected [serviceOrPluginName]=[status]" );
                Assert.That( @this.FinalConfiguration.GetStatus( split[0] ).ToString(), Is.EqualTo( split[1] ) );
            }
        }

        public static void CheckFinalConfigurationItemImpact( this IConfigurationManager @this, params string[] segment )
        {
            for( int i = 0; i < segment.Length; i++ ) segment[i] += ",";
            CheckFinalConfigurationItemImpact( @this, String.Concat( segment ) );
        }

        public static void CheckFinalConfigurationItemImpact( this IConfigurationManager @this, string segment )
        {
            var items = segment.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() );
            foreach( var i in items )
            {
                var split = i.Split( new[] { '=' }, StringSplitOptions.RemoveEmptyEntries );
                if( split.Length > 2 ) Assert.Fail( "expected [serviceOrPluginName]=[impact]" );
                Assert.That( @this.FinalConfiguration.GetImpact( split[0] ).ToString(), Is.EqualTo( split[1] ) );
            }
        }
    }
}
