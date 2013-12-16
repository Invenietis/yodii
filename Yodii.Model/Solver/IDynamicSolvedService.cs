using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Solved dynamic service data.
    /// </summary>
    public interface IDynamicSolvedService
    {
        /// <summary>
        /// Static service information.
        /// </summary>
        IServiceInfo ServiceInfo { get; }

        /// <summary>
        /// Reason behind this service's disabled status.
        /// </summary>
        string DisabledReason { get; }

        /// <summary>
        /// This service's status as set in the initial configuration.
        /// </summary>
        ConfigurationStatus ConfigOriginalStatus { get; }

        /// <summary>
        /// Status as set in the resolved configuration.
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
