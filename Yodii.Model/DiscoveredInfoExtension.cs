using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
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
    }
}
