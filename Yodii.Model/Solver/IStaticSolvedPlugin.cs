using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{

    public interface IStaticSolvedPlugin
    {
        IPluginInfo PluginInfo { get; }

        PluginDisabledReason DisabledReason { get; }

        ConfigurationStatus ConfigOriginalStatus { get; }

        SolvedConfigurationStatus ConfigSolvedStatus { get; }

        bool IsBlocking { get; }
    }
}
