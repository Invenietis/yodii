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
        /// Requested service status.
        /// </summary>
        ConfigurationStatus WantedConfigSolvedStatus { get; }

        /// <summary>
        /// Whether this service blocks static resolution.
        /// </summary>
        bool IsBlocking { get; }
    }
}
