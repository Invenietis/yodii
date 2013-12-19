using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Static solved service data.
    /// </summary>
    public interface IStaticSolvedService
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
        /// Service status as set by the initial configuration.
        /// </summary>
        ConfigurationStatus ConfigOriginalStatus { get; }

        /// <summary>
        /// Dependency impact as set by initial configuration.
        /// </summary>
        StartDependencyImpact ConfigOriginalImpact { get; }

        /// <summary>
        /// Final dependency impact (the Generalization's one if this service specializes another one and 
        /// this <see cref="ConfigOriginalImpact"/> is <see cref="StartDependencyImpact.Unknown"/>).
        /// </summary>
        StartDependencyImpact ConfigSolvedImpact { get; }

        /// <summary>
        /// Final configuration status based on requirements from other participants.
        /// </summary>
        ConfigurationStatus WantedConfigSolvedStatus { get; }

        /// <summary>
        /// Whether this service blocks static resolution.
        /// </summary>
        bool IsBlocking { get; }
    }
}
