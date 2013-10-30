using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;


namespace Yodii.Model.ConfigurationSolver
{
    public class ConfigurationSolverResult : IConfigurationSolverResult
    {
        readonly IReadOnlyCollection<IPluginInfo> _blockingPlugins;
        readonly IReadOnlyCollection<IServiceInfo> _blockingServices;

        readonly IReadOnlyCollection<IPluginInfo> _disabledPlugins;
        readonly IReadOnlyCollection<IPluginInfo> _stoppedPlugins;
        readonly IReadOnlyCollection<IPluginInfo> _runningPlugins;

        public ConfigurationSolverResult( List<IPluginInfo> blockingPlugins, List<IServiceInfo> blockingServices )
        {
            Debug.Assert( blockingPlugins != null || blockingServices != null );
            _blockingPlugins = blockingPlugins != null ? blockingPlugins.ToReadOnlyCollection() : CKReadOnlyListEmpty<IPluginInfo>.Empty;
            _blockingServices = blockingServices != null ? blockingServices.ToReadOnlyCollection() : CKReadOnlyListEmpty<IServiceInfo>.Empty;
        }

        public ConfigurationSolverResult( List<IPluginInfo> disabledPlugins, List<IPluginInfo> stoppedPlugins, List<IPluginInfo> runningPlugins )
        {
            _disabledPlugins = disabledPlugins.ToReadOnlyCollection();
            _stoppedPlugins = stoppedPlugins.ToReadOnlyCollection();
            _runningPlugins = runningPlugins.ToReadOnlyCollection();
        }

        public bool ConfigurationSuccess { get { return _blockingPlugins == null; } }

        public IReadOnlyCollection<IPluginInfo> BlockingPlugins 
        { 
            get { return _blockingPlugins; } 
        }

        public IReadOnlyCollection<IServiceInfo> BlockingServices 
        {
            get { return _blockingServices; } 
        }

        public IReadOnlyCollection<IPluginInfo> DisabledPlugins
        {
            get { return _disabledPlugins; }
        }

        public IReadOnlyCollection<IPluginInfo> StoppedPlugins
        {
            get { return _stoppedPlugins; }
        }

        public IReadOnlyCollection<IPluginInfo> RunningPlugins
        {
            get { return _runningPlugins; }
        }
    }
}
