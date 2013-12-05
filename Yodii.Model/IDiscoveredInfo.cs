using System;
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    /// <summary>
    /// Interface containing all ServiceInfos discovered on a system.
    /// </summary>
    public interface IDiscoveredInfo
    {
        /// <summary>
        /// Lists of all discovered services on this system.
        /// </summary>
        IReadOnlyList<IServiceInfo> ServiceInfos { get; }

        /// <summary>
        /// List of all discovered plugins on this system.
        /// </summary>
        IReadOnlyList<IPluginInfo> PluginInfos { get; }

    }
}
