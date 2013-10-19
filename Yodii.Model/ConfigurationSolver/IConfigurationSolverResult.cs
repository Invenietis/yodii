using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model.CoreModel;


namespace Yodii.Model.ConfigurationSolver
{
    public interface IConfigurationSolverResult
    {
        bool ConfigurationSuccess { get; }

        ICKReadOnlyCollection<IPluginInfo> BlockingPlugins { get; }

        ICKReadOnlyCollection<IServiceInfo> BlockingServices { get; }

        ICKReadOnlyCollection<IPluginInfo> DisabledPlugins { get; }

        ICKReadOnlyCollection<IPluginInfo> StoppedPlugins { get; }

        ICKReadOnlyCollection<IPluginInfo> RunningPlugins { get; }
    }
}
