using System;
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    public interface IServiceInfo : IDiscoveredErrorInfo
    {
        /// <summary>
        /// Gets the full name of the service (namespace and interface name).
        /// </summary>
        string ServiceFullName { get; }

        /// <summary>
        /// Gets the <see cref="IServiceInfo"/> that generalizes this one if it exists.
        /// </summary>
        IServiceInfo Generalization { get; }

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
