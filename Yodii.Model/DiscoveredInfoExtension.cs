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
        /// Checks that this IDiscoveredInfo obeys to integrity constraints (see remarks).
        /// </summary>
        /// <param name="this">This discovered information.</param>
        /// <returns>True if data is valid, false otherwise.</returns>
        /// <remarks>
        /// IDiscoveredInfo constraints checked here :
        /// - IServiceInfo and IPluginInfo names are unique, in both collections (A service cannot have a plugin name, and vice-versa).
        /// - IServiceInfo generalizations do not specialize this IServiceInfo (Generalization loop).
        /// </remarks>
        public static bool IsValid( this IDiscoveredInfo @this )
        {
            // Check plugin and service identifiers
            HashSet<string> pluginOrServiceIds = new HashSet<string>();

            foreach( var service in @this.ServiceInfos )
            {
                if( !pluginOrServiceIds.Add( service.ServiceFullName ) )
                {
                    return false; // Service name appears twice.
                }

                // Check if service is contained in its generalization tree (generalization loop)
                List<IServiceInfo> visitedServices = new List<IServiceInfo>();

                IServiceInfo g = service.Generalization;
                visitedServices.Add( service );
                while( g != null )
                {
                    if( visitedServices.Contains( g ) )
                    {
                        return false; // This service is contained in its generalization tree.
                    }
                    g = g.Generalization;
                    visitedServices.Add( g );
                }
            }

            foreach( var plugin in @this.PluginInfos )
            {
                if( !pluginOrServiceIds.Add( plugin.PluginFullName ) )
                {
                    return false; // Plugin name is already used.
                }
            }

            return true;
        }

    }
}
