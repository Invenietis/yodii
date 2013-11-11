using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    public interface IConfigurationSolverResult
    {
        bool ConfigurationSuccess { get; }

        IReadOnlyList<IPluginSolved> BlockingPlugins { get; }

        IReadOnlyList<IServiceSolved> BlockingServices { get; }

        ////

        IReadOnlyCollection<IPluginSolved> DisabledPlugins { get; }

        //IReadOnlyCollection<IPluginInfo> StoppedPlugins { get; }

        IReadOnlyCollection<IPluginSolved> RunningPlugins { get; }
    }
}
