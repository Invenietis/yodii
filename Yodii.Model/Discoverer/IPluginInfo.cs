using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Plugin information.
    /// </summary>
    public interface IPluginInfo : IDiscoveredItem
    {
        /// <summary>
        /// Gets the unique full name of the plugin (namespace and class name).
        /// </summary>
        string PluginFullName { get; }

        /// <summary>
        /// Gets the assembly info that contains this plugin.
        /// </summary>
        IAssemblyInfo AssemblyInfo { get; }

        /// <summary>
        /// Gets the services that this plugin references.
        /// </summary>
        IReadOnlyList<IServiceReferenceInfo> ServiceReferences { get; }

        /// <summary>
        /// Gets the service that this plugin implements. Null if the plugin does not implement any service.
        /// </summary>
        IServiceInfo Service { get; }
    }
}
