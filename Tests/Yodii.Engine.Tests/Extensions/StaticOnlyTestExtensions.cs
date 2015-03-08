#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Extensions\StaticOnlyTestExtensions.cs) is part of CiviKey. 
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
using System.Linq.Expressions;
using System.Text;
using CK.Core;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii
{
    static class StaticOnlyTestExtensions
    {

        public static IYodiiEngineStaticOnlyResult Trace( this IYodiiEngineStaticOnlyResult @this, IActivityMonitor m )
        {
            if( @this.Success )
            {
                m.Trace().Send( "Success!" );
            }
            else
            {
                m.Trace().Send( "Failed!" );
                @this.StaticFailureResult.Trace( m );
            }
            return @this;
        }

        #region Plugins and Services
        public static IYodiiEngineStaticOnlyResult CheckAllDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            return CheckStatus( @this, SolvedConfigurationStatus.Disabled, pluginOrServiceNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckAllRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            return CheckStatus( @this, SolvedConfigurationStatus.Runnable, pluginOrServiceNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckAllRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            return CheckStatus( @this, SolvedConfigurationStatus.Running, pluginOrServiceNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            return CheckStatus( @this, SolvedConfigurationStatus.Disabled, pluginOrServiceNames, true );
        }

        public static IYodiiEngineStaticOnlyResult CheckRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            return CheckStatus( @this, SolvedConfigurationStatus.Runnable, pluginOrServiceNames, true );
        }

        public static IYodiiEngineStaticOnlyResult CheckRunning( this IYodiiEngineStaticOnlyResult @this, string pluginOrServiceNames )
        {
            return CheckStatus( @this, SolvedConfigurationStatus.Running, pluginOrServiceNames, true );
        }

        static IYodiiEngineStaticOnlyResult CheckStatus( this IYodiiEngineStaticOnlyResult @this, SolvedConfigurationStatus status, string pluginOrServiceNames, bool expectedIsPartial )
        {
            string[] expected = pluginOrServiceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Plugins.Where( p => p.FinalConfigSolvedStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            withTheStatus = withTheStatus.Concat( @this.StaticSolvedConfiguration.Services.Where( s => s.FinalConfigSolvedStatus == status ).Select( s => s.ServiceInfo.ServiceFullName ) );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

        #region Plugins only
        public static IYodiiEngineStaticOnlyResult CheckAllPluginsDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, SolvedConfigurationStatus.Disabled, pluginNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckAllPluginsRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, SolvedConfigurationStatus.Runnable, pluginNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckAllPluginsRunning( this IYodiiEngineStaticOnlyResult @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, SolvedConfigurationStatus.Running, pluginNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckPluginsDisabled( this IYodiiEngineStaticOnlyResult @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, SolvedConfigurationStatus.Disabled, pluginNames, true );
        }

        public static IYodiiEngineStaticOnlyResult CheckPluginsRunnable( this IYodiiEngineStaticOnlyResult @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, SolvedConfigurationStatus.Runnable, pluginNames, true );
        }

        public static IYodiiEngineStaticOnlyResult CheckPluginsRunning( this IYodiiEngineStaticOnlyResult @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, SolvedConfigurationStatus.Running, pluginNames, true );
        }

        static IYodiiEngineStaticOnlyResult CheckPluginsStatus( this IYodiiEngineStaticOnlyResult @this, SolvedConfigurationStatus status, string pluginNames, bool expectedIsPartial )
        {
            string[] expected = pluginNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Plugins.Where( p => p.FinalConfigSolvedStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

        #region Services only
        public static IYodiiEngineStaticOnlyResult CheckAllServicesDisabled( this IYodiiEngineStaticOnlyResult @this, string serviceNames )
        {
            return CheckServicesStatus( @this, SolvedConfigurationStatus.Disabled, serviceNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckAllServicesRunnable( this IYodiiEngineStaticOnlyResult @this, string serviceNames )
        {
            return CheckServicesStatus( @this, SolvedConfigurationStatus.Runnable, serviceNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckAllServicesRunning( this IYodiiEngineStaticOnlyResult @this, string serviceNames )
        {
            return CheckServicesStatus( @this, SolvedConfigurationStatus.Running, serviceNames, false );
        }

        public static IYodiiEngineStaticOnlyResult CheckServicesDisabled( this IYodiiEngineStaticOnlyResult @this, string serviceNames )
        {
            return CheckServicesStatus( @this, SolvedConfigurationStatus.Disabled, serviceNames, true );
        }

        public static IYodiiEngineStaticOnlyResult CheckServicesRunnable( this IYodiiEngineStaticOnlyResult @this, string serviceNames )
        {
            return CheckServicesStatus( @this, SolvedConfigurationStatus.Runnable, serviceNames, true );
        }

        public static IYodiiEngineStaticOnlyResult CheckServicesRunning( this IYodiiEngineStaticOnlyResult @this, string serviceNames )
        {
            return CheckServicesStatus( @this, SolvedConfigurationStatus.Running, serviceNames, true );
        }

        static IYodiiEngineStaticOnlyResult CheckServicesStatus( this IYodiiEngineStaticOnlyResult @this, SolvedConfigurationStatus status, string serviceNames, bool expectedIsPartial )
        {
            string[] expected = serviceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.StaticSolvedConfiguration.Services.Where( s => s.FinalConfigSolvedStatus == status ).Select( s => s.ServiceInfo.ServiceFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

    }
}
