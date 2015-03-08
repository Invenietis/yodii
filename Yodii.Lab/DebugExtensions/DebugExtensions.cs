#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\DebugExtensions\DebugExtensions.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
