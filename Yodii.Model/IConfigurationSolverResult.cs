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

        //IReadOnlyList<IPluginSolved> BlkPlugins { get; }

        //IReadOnlyList<IServiceSolved> BlkServices { get; }

        IReadOnlyList<IPluginInfo> BlockingPlugins { get; }

        IReadOnlyList<IServiceInfo> BlockingServices { get; }

        ////

        IReadOnlyCollection<IPluginInfo> DisabledPlugins { get; }

        IReadOnlyCollection<IPluginInfo> StoppedPlugins { get; }

        IReadOnlyCollection<IPluginInfo> RunningPlugins { get; }
    }
}
