using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    interface IServiceDependentObject
    {
        ConfigurationStatus FinalConfigSolvedStatus { get; }

        StartDependencyImpact ConfigSolvedImpact { get; }

        IEnumerable<ServiceData> GetIncludedServices( StartDependencyImpact impact );

        IEnumerable<ServiceData> GetExcludedServices( StartDependencyImpact impact );

    }
}
