using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Generalizes static solved plugin or service information.
    /// </summary>
    public interface IStaticSolvedYodiiItem
    {
        /// <summary>
        /// Gets the <see cref="IPluginInfo.PluginFullName"/> or the <see cref="IServiceInfo.ServiceFullName"/>.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets a string that describes the reason for a disabled status.
        /// Null when this item is not disabled.
        /// </summary>
        string DisabledReason { get; }

        /// <summary>
        /// Status as set by initial configuration.
        /// </summary>
        ConfigurationStatus ConfigOriginalStatus { get; }

        /// <summary>
        /// Dependency impact as set by initial configuration.
        /// </summary>
        StartDependencyImpact ConfigOriginalImpact { get; }

        /// <summary>
        /// Final dependency impact. 
        /// <para>
        /// For a plugin it is the Service's one if this plugin implements a Service and 
        /// this <see cref="ConfigOriginalImpact"/> is <see cref="StartDependencyImpact.Unknown"/>.
        /// </para>
        /// <para>
        /// For a Service, it is the Generalization's one if this service specializes another one and 
        /// this <see cref="ConfigOriginalImpact"/> is <see cref="StartDependencyImpact.Unknown"/>.
        /// </para>
        /// </summary>
        StartDependencyImpact ConfigSolvedImpact { get; }

        /// <summary>
        /// Final configuration status based on requirements from other participants.
        /// </summary>
        ConfigurationStatus WantedConfigSolvedStatus { get; }

        /// <summary>
        /// Gets whether this item blocks static resolution.
        /// </summary>
        bool IsBlocking { get; }
    }
}
