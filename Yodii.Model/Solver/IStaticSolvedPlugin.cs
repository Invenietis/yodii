using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Static solved plugin data.
    /// </summary>
    public interface IStaticSolvedPlugin
    {
        /// <summary>
        /// Static plugin information.
        /// </summary>
        IPluginInfo PluginInfo { get; }

        /// <summary>
        /// Reason behind this plugin's disabled status.
        /// </summary>
        PluginDisabledReason DisabledReason { get; }

        /// <summary>
        /// Status as set by initial configuration.
        /// </summary>
        ConfigurationStatus ConfigOriginalStatus { get; }

        /// <summary>
        /// Desired configuration status.
        /// </summary>
        SolvedConfigurationStatus WantedConfigSolvedStatus { get; }

        /// <summary>
        /// Whether this plugin blocks static resolution.
        /// </summary>
        bool IsBlocking { get; }
    }
}
