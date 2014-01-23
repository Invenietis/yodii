using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    /// <summary>
    /// Common abstraction of Service propagation and plugin.
    /// </summary>
    interface IServiceDependentObject
    {
        /// <summary>
        /// The final configuration status (Disabled if the item is actually disabled).
        /// </summary>
        SolvedConfigurationStatus FinalConfigSolvedStatus { get; }

        /// <summary>
        /// The solved impact for the static resolution: never IsTryOnly nor unknown.
        /// </summary>
        StartDependencyImpact ConfigSolvedImpact { get; }

        /// <summary>
        /// Gets the services that, given a StartDependencyImpact, must also be running if this object must run
        /// or be at least be runnable if this object must be able to run.
        /// </summary>
        /// <param name="impact">The dependency impact.</param>
        /// <param name="forRunnableStatus">True for runnable propagation.</param>
        /// <returns>Set of services that must be run or be able to run.</returns>
        IEnumerable<ServiceData> GetIncludedServices( StartDependencyImpact impact, bool forRunnableStatus );

        /// <summary>
        /// Gets the services that, given a StartDependencyImpact, must be stopped for this object to be running.
        /// </summary>
        /// <param name="impact">The dependency impact.</param>
        /// <returns>Set of services that can not run when this object runs.</returns>
        IEnumerable<ServiceData> GetExcludedServices( StartDependencyImpact impact );

    }
}
