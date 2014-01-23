using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    /// <summary>
    /// Read only generalization of PluginData and ServiceData.
    /// </summary>
    interface IYodiiItemData
    {
        ConfigurationStatus ConfigOriginalStatus { get; }

        SolvedConfigurationStatus ConfigSolvedStatus { get; }

        SolvedConfigurationStatus FinalConfigSolvedStatus { get; }

        string DisabledReason { get; }

        FinalConfigStartableStatus FinalStartableStatus { get; }

        RunningStatus? DynamicStatus { get; }

        StartDependencyImpact ConfigOriginalImpact { get; }

        StartDependencyImpact RawConfigSolvedImpact { get; }
    }
}
