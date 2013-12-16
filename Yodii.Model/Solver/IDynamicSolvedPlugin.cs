using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Solved dynamic plugin data.
    /// </summary>
    public interface IDynamicSolvedPlugin
    {
        /// <summary>
        /// Static plugin information.
        /// </summary>
        IPluginInfo PluginInfo { get; }

        /// <summary>
        /// Reason behind this plugin's disabled status.
        /// </summary>
        string DisabledReason { get; }

        /// <summary>
        /// Status as set in the initial configuration.
        /// </summary>
        ConfigurationStatus ConfigOriginalStatus { get; }

        /// <summary>
        /// Status as solved during the resolution.
        /// </summary>
        ConfigurationStatus ConfigSolvedStatus { get; }

        /// <summary>
        /// Dependency impact as set by initial configuration.
        /// </summary>
        StartDependencyImpact ConfigOriginalImpact { get; }

        /// <summary>
        /// Final dependency impact (the Service's one if this plugin implements a Service and 
        /// this <see cref="ConfigOriginalImpact"/> is <see cref="StartDependencyImpact.Unknown"/>).
        /// </summary>
        StartDependencyImpact ConfigSolvedImpact { get; }
        
        /// <summary>
        /// Running status.
        /// </summary>
        RunningStatus RunningStatus { get; }
    }
}
