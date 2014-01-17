using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    static class ConfigurationManagerExtensions
    {
        public static IYodiiEngineResult AddSuccess( this IConfigurationItemCollection @this, string name, ConfigurationStatus status, ConfigurationStatus? solvedStatusInFinalConfig = null )
        {
            IYodiiEngineResult r = @this.Add( name, status );
            r.CheckSuccess();
            if( solvedStatusInFinalConfig != null ) Assert.That( @this.ParentLayer.ConfigurationManager.FinalConfiguration.GetStatus( name ), Is.EqualTo( solvedStatusInFinalConfig ) );
            return r;
        }

        public static void CheckFinalConfigurationItems( this IConfigurationManager @this, params string[] segment )
        {
            for( int i = 0; i < segment.Length ; i++) segment[i] += ",";
            CheckFinalConfigurationItems( @this, String.Concat( segment ) );
        }

        public static void CheckFinalConfigurationItems( this IConfigurationManager @this, string segment )
        {
            var items = segment.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() );
            foreach( var i in items )
            {
                var split = i.Split( new[] { '=' }, StringSplitOptions.RemoveEmptyEntries );
                if( split.Length > 2 ) Assert.Fail( "expected [serviceOrPluginName]=[status]" );
                Assert.That( @this.FinalConfiguration.GetStatus( split[0] ).ToString(), Is.EqualTo( split[1] ) );
            }
        }

        public static void ClearAllLayers( this YodiiEngine @this, string pluginOrServiceFullName )
        {
            foreach( ConfigurationLayer layer in @this.Configuration.Layers )
            {
                layer.Items.Remove( pluginOrServiceFullName );
            }
        }
    }
}
