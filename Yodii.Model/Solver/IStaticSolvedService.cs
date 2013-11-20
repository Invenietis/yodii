using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{

    public interface IStaticSolvedService
    {
        IServiceInfo ServiceInfo { get; }

        ServiceDisabledReason DisabledReason { get; }

        ConfigurationStatus ConfigOriginalStatus { get; }

        SolvedConfigurationStatus ConfigSolvedStatus { get; }

        bool IsBlocking { get; }
    }
}
