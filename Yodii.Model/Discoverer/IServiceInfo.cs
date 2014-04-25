using System;
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    /// <summary>
    /// Service information.
    /// </summary>
    public interface IServiceInfo : IDiscoveredItem
    {
        /// <summary>
        /// Gets the unique full name of the service (namespace and interface name).
        /// </summary>
        string ServiceFullName { get; }

        /// <summary>
        /// Gets the <see cref="IServiceInfo"/> that generalizes this one if it exists.
        /// </summary>
        IServiceInfo Generalization { get; set; }

        /// <summary>
        /// Gets the assembly info that contains (defines) this interface.
        /// If the service interface itself has not been found, this is null.
        /// </summary>
        IAssemblyInfo AssemblyInfo { get; }

        /// <summary>
        /// Gets the different <see cref="IPluginInfo"/> that implement this service.
        /// </summary>
        IReadOnlyList<IPluginInfo> Implementations { get; }

    }
}
