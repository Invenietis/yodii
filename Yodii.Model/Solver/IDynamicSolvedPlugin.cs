using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface IDynamicSolvedPlugin
    {
        IPluginInfo PluginInfo { get; }

        PluginDisabledReason DisabledReason { get; }

        ConfigurationStatus ConfigOriginalStatus { get; }

        SolvedConfigurationStatus ConfigSolvedStatus { get; }
        
        RunningStatus RunningStatus { get; }
    }
}
