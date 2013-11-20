using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface IDynamicSolvedService
    {
        IServiceInfo ServiceInfo { get; }

        ServiceDisabledReason DisabledReason { get; }

        ConfigurationStatus ConfigOriginalStatus { get; }

        SolvedConfigurationStatus ConfigSolvedStatus { get; }

        RunningStatus RunningStatus { get; }
    }
}
