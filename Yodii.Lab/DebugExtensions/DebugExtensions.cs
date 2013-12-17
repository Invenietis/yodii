using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Lab
{
    internal static class DebugExtensions
    {
        public static string Describe( this IYodiiEngineResult result )
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( "IYodiiEngineResult: " );
            if( result.Success )
            {
                sb.AppendLine( "Success" );
            }
            else
            {
                sb.AppendLine( "Failure" );
            }

            // ConfigurationFailureResult
            if( result.ConfigurationFailureResult != null && result.ConfigurationFailureResult.FailureReasons.Count > 0 )
            {
                sb.AppendLine( "ConfigurationFailureResult:" );
                foreach( var reason in result.ConfigurationFailureResult.FailureReasons )
                {
                    sb.AppendLine( String.Format( "- {0}", reason ) );
                }
            }

            // StaticFailureResult
            if( result.StaticFailureResult != null )
            {
                sb.AppendLine( "StaticFailureResult:" );
                if( result.StaticFailureResult.BlockingServices.Count > 0 )
                {
                    sb.AppendLine( "BlockingServices:" );
                    foreach( var s in result.StaticFailureResult.BlockingServices )
                    {
                        sb.AppendLine(
                            String.Format( "- {0}: {1}", s.ServiceInfo.ServiceFullName, s.DisabledReason.ToString() )
                        );
                    }
                }
                if( result.StaticFailureResult.BlockingPlugins.Count > 0 )
                {
                    sb.AppendLine( "BlockingPlugins:" );
                    foreach( var p in result.StaticFailureResult.BlockingPlugins )
                    {
                        sb.AppendLine(
                            String.Format( "- {0}/{1}: {2}", p.PluginInfo.PluginFullName, p.PluginInfo.PluginFullName, p.DisabledReason.ToString() )
                        );
                    }
                }
            }

            if( result.HostFailureResult != null )
            {
                sb.AppendLine("HostFailureResult:");
                foreach( var r in result.HostFailureResult.ErrorPlugins )
                {
                    sb.AppendLine(
                        String.Format("- {0}: {1}", r.Plugin.PluginInfo.PluginFullName, r.Error.Message)
                        );
                }
            }

            if( result.ServiceCulprits.Count > 0 )
            {
                sb.AppendLine( "ServiceCulprits:" );
                foreach( var pc in result.ServiceCulprits )
                {
                    sb.AppendLine(
                        String.Format( "- {0}: {1}", pc.ServiceFullName, pc.ErrorMessage )
                        );
                }
            }

            if( result.PluginCulprits.Count > 0 )
            {
                sb.AppendLine( "PluginCulprits:" );
                foreach( var pc in result.PluginCulprits )
                {
                    sb.AppendLine(
                        String.Format( "- {0}: {1}", pc.PluginFullName, pc.ErrorMessage )
                        );
                }
            }

            return sb.ToString();
        }
    }
}
