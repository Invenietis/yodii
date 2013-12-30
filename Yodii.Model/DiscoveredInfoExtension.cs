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
        /// - IPluginInfo contains valid service references (cannot reference a service of its own family, outside its own tree).
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

                foreach( var serviceRef in plugin.ServiceReferences )
                {
                    if( !plugin.CanReference( serviceRef.Reference ) )
                    {
                        return false; // Service reference points to a service that cannot be run (same family, different branch).
                    }
                }
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
        /// <param name="this">This plugin.</param>
        /// <returns>ServiceRoot, or null if plugin has no Service.</returns>
        public static IServiceInfo GetServiceFamilyRoot( this IPluginInfo @this )
        {
            IServiceInfo s = @this.Service;
            while( s != null )
            {
                if( s.Generalization != null )
                {
                    s = s.Generalization;
                }
                else
                {
                    break;
                }
            }

            return s;
        }

        /// <summary>
        /// Gets the service root, at the top of the service family of a given service.
        /// </summary>
        /// <param name="this">This service.</param>
        /// <returns>Service root. Can be this service.</returns>
        public static IServiceInfo GetServiceFamilyRoot( this IServiceInfo @this )
        {
            IServiceInfo s = @this;
            while( s != null )
            {
                if( s.Generalization != null )
                {
                    s = s.Generalization;
                }
                else
                {
                    break;
                }
            }

            return s;
        }

    }
}
