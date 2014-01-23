using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Generalizes dynamic solved plugin or service information.
    /// </summary>
    public interface IDynamicSolvedYodiiItem
    {
        /// <summary>
        /// Gets the <see cref="IPluginInfo.PluginFullName"/> or the <see cref="IServiceInfo.ServiceFullName"/>.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the final running status.
        /// </summary>
        RunningStatus RunningStatus { get; }

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
        /// Gets the solved configuration status, it is the wanted result: it can be <see cref="SolvedConfigurationStatus.Running"/>
        /// even if the <see cref="DisabledReason"/> is not null.
        /// Use <see cref="FinalConfigSolvedStatus"/> to get a status that integrates the fact that the item is disabled.
        /// </summary>
        SolvedConfigurationStatus WantedConfigSolvedStatus { get; }

        /// <summary>
        /// Gets the final configuration status, it is the result of the static resolution phase.
        /// </summary>
        SolvedConfigurationStatus FinalConfigSolvedStatus { get; }

        /// <summary>
        /// Gets the dependency impact as set by initial configuration.
        /// </summary>
        StartDependencyImpact ConfigOriginalImpact { get; }

        /// <summary>
        /// Gets the final dependency impact. 
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
    }
}
