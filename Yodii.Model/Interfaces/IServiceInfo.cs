using System;
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    public interface IServiceInfo
    {
        string AssemblyQualifiedName { get; }

        /// <summary>
        /// Gets the full name of the service (namespace and interface name).
        /// </summary>
        string ServiceFullName { get; }

        /// <summary>
        /// Gets whether the service is a <see cref="IDynamicService"/>.
        /// </summary>
        bool IsDynamicService { get; }

        /// <summary>
        /// Gets the assembly info that contains (defines) this interface.
        /// If the service interface itself has not been found, this is null.
        /// </summary>
        IAssemblyInfo AssemblyInfo { get; }

        /// <summary>
        /// Gets the different <see cref="IPluginInfo"/> that implement this service.
        /// </summary>
        IReadOnlyList<IPluginInfo> Implementations { get; }

        /// <summary>
        /// Gets the collection of <see cref="ISimpleMethodInfo"/> that this service exposes.
        /// </summary>
        IReadOnlyCollection<ISimpleMethodInfo> MethodsInfoCollection { get; }

        /// <summary>
        /// Gets the collection of <see cref="ISimpleEventInfo"/> that this service exposes.
        /// </summary>
        IReadOnlyCollection<ISimpleEventInfo> EventsInfoCollection { get; }

        /// <summary>
        /// Gets the collection of <see cref="ISimplePropertyInfo"/> that this service exposes.
        /// </summary>
        IReadOnlyCollection<ISimplePropertyInfo> PropertiesInfoCollection { get; }
    }
}
