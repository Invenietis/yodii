using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Wpf
{
    internal static class DebugExtensions
    {
        public static string Describe( this IYodiiEngineResult result )
        {
            StringBuilder sb = new StringBuilder();
            // ConfigurationFailureResult
            if( result.ConfigurationFailureResult != null && result.ConfigurationFailureResult.FailureReasons.Count > 0 )
            {
                sb.AppendLine( "Configuration error:" );
                foreach( var reason in result.ConfigurationFailureResult.FailureReasons )
                {
                    sb.AppendLine( String.Format( "- {0}", reason ) );
                }
            }

            // StaticFailureResult
            if( result.StaticFailureResult != null )
            {
                sb.AppendLine( "Error during static resolution:" );
                if( result.StaticFailureResult.BlockingServices.Count > 0 )
                {
                    sb.AppendLine( "* These services could not be started:" );
                    foreach( var s in result.StaticFailureResult.BlockingServices )
                    {
                        sb.AppendLine(
                            String.Format( "  - Service '{0}':\n    {1}", s.ServiceInfo.ServiceFullName, s.DisabledReason )
                        );
                    }
                }
                if( result.StaticFailureResult.BlockingPlugins.Count > 0 )
                {
                    sb.AppendLine( "* These plugins could not be started:" );
                    foreach( var p in result.StaticFailureResult.BlockingPlugins )
                    {
                        sb.AppendLine(
                            String.Format( "  - Plugin '{0}':\n    {1}", p.PluginInfo.PluginFullName, p.DisabledReason )
                        );
                    }
                }
            }

            if( result.HostFailureResult != null )
            {
                sb.AppendLine( "There was a runtime error on the plugin host:" );
                foreach( var r in result.HostFailureResult.ErrorPlugins )
                {
                    sb.AppendLine(
                        String.Format( "For plugin '{0}':\n  * {1}", r.Plugin.PluginInfo.PluginFullName, r.CancellationInfo.ErrorMessage )
                        );
                }
            }

            if( result.ServiceCulprits.Count > 0 )
            {
                sb.AppendLine( "These services caused the error:" );
                foreach( var pc in result.ServiceCulprits )
                {
                    sb.AppendLine(
                            String.Format( "- '{0}'", pc.ServiceFullName )
                        );
                }
            }

            if( result.PluginCulprits.Count > 0 )
            {
                sb.AppendLine( "These plugins caused the error:" );
                foreach( var pc in result.PluginCulprits )
                {
                    sb.AppendLine(
                            String.Format( "- '{0}'", pc.PluginFullName )
                        );
                }
            }

            return sb.ToString();
        }
    }
}
