#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\DiscoveredInfoExtension.cs) is part of CiviKey. 
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
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Extensions for discoverable items.
    /// </summary>
    public static class DiscoveredInfoExtension
    {
        /// <summary>
        /// Gets all the services that are supported by this plugin (its <see cref="IPluginInfo.Service"/> and all its <see cref="IServiceInfo.Generalization"/>.
        /// </summary>
        /// <param name="this">This plugin information.</param>
        /// <returns>The Service and its generalizations.</returns>
        public static IEnumerable<IServiceInfo> AllServices( this IPluginInfo @this )
        {
            IServiceInfo s = @this.Service;
            while( s != null )
            {
                yield return s;
                s = s.Generalization;
            }
        }

        /// <summary>
        /// Checks that this IDiscoveredInfo obeys is globally valid: plugins and services have no error, plugins and services 
        /// have unique names and there is no loop in any service generalizations.
        /// </summary>
        /// <param name="this">This discovered information.</param>
        /// <returns>True if data is valid, false otherwise.</returns>
        public static bool IsValid( this IDiscoveredInfo @this )
        {
            HashSet<string> pluginOrServiceIds = new HashSet<string>();
            List<IServiceInfo> visitedServices = new List<IServiceInfo>();
            foreach( var service in @this.ServiceInfos )
            {
                if( service.HasError ) return false;
                if( !pluginOrServiceIds.Add( service.ServiceFullName ) ) return false;
                // Checks generalization loop.
                visitedServices.Clear();
                IServiceInfo g = service.Generalization;
                visitedServices.Add( service );
                while( g != null )
                {
                    if( visitedServices.Contains( g ) ) return false;
                    visitedServices.Add( g );
                    g = g.Generalization;
                }
            }
            foreach( var plugin in @this.PluginInfos )
            {
                if( plugin.HasError ) return false;
                if( !pluginOrServiceIds.Add( plugin.PluginFullName ) ) return false; 
            }

            return true;
        }

        /// <summary>
        /// Gets whether this plugin can reference the given service.
        /// </summary>
        /// <param name="this">This plugin.</param>
        /// <param name="service">Service to reference.</param>
        /// <returns>True if this plugin can have a reference to the service.</returns>
        public static bool CanReference( this IPluginInfo @this, IServiceInfo service )
        {
            var thisPluginRoot = @this.GetServiceFamilyRoot();
            var referenceRoot = service.GetServiceFamilyRoot();

            return thisPluginRoot != referenceRoot || @this.AllServices().Contains( service );
        }

        /// <summary>
        /// Gets the service root, at the top of the service family of a given plugin.
        /// </summary>
        /// <param name="this">This plugin. Can not be null.</param>
        /// <returns>ServiceRoot, or null if plugin has no Service.</returns>
        public static IServiceInfo GetServiceFamilyRoot( this IPluginInfo @this )
        {
            return @this.Service.GetServiceFamilyRoot();
        }

        /// <summary>
        /// Gets the service root, at the top of the service family of a given service.
        /// </summary>
        /// <param name="this">This service. Can be null.</param>
        /// <returns>Service root. Can be this service.</returns>
        public static IServiceInfo GetServiceFamilyRoot( this IServiceInfo @this )
        {
            IServiceInfo s = @this;
            while( s != null )
            {
                if( s.Generalization == null ) break;
                s = s.Generalization;
            }
            return s;
        }

    }
}
