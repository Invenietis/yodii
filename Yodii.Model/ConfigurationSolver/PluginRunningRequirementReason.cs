using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Model.ConfigurationSolver
{
    enum PluginRunningRequirementReason
    {
        /// <summary>
        /// Initialized by PluginData constructor.
        /// </summary>
        Config,
        
        /// <summary>
        /// Sets by ServiceData.RetrieveTheOnlyPlugin.
        /// </summary>
        FromServiceConfigToSinglePlugin,

        /// <summary>
        /// Sets by ServiceData.RetrieveTheOnlyPlugin and ServiceData.SetRunningRequirement.
        /// </summary>
        FromServiceToSinglePlugin,

    }

}
