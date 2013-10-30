using System;
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    public interface IDiscoveredInfo
    {
        /// <summary>
        /// Gets the list of service.
        /// </summary>
        IReadOnlyList<IServiceInfo> ServiceInfos { get; }

        /// <summary>
        /// Gets the list of plugins.
        /// </summary>
        IReadOnlyList<IPluginInfo> PluginInfos { get; }

    }
}
