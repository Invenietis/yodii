using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;


namespace Yodii.Model.ConfigurationSolver
{
    public interface IConfigurationSolverResult
    {
        bool ConfigurationSuccess { get; }

        IReadOnlyCollection<IPluginInfo> BlockingPlugins { get; }

        IReadOnlyCollection<IServiceInfo> BlockingServices { get; }

        IReadOnlyCollection<IPluginInfo> DisabledPlugins { get; }

        IReadOnlyCollection<IPluginInfo> StoppedPlugins { get; }

        IReadOnlyCollection<IPluginInfo> RunningPlugins { get; }
    }
}
