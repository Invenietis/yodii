using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    public class ConfigurationSolverResult : IConfigurationSolverResult
    {
        readonly List<IPluginSolved> _blockingPlugins;
        readonly List<IServiceSolved> _blockingServices;

        readonly IReadOnlyCollection<IPluginSolved> _disabledPlugins;
        readonly int _availablePluginsCount;
        readonly IReadOnlyCollection<IPluginSolved> _runningPlugins;

        internal ConfigurationSolverResult( List<IPluginSolved> blockingPlugins, List<IServiceSolved> blockingServices )
        {
            Debug.Assert( blockingPlugins != null || blockingServices != null );
            _blockingPlugins = blockingPlugins;
            _blockingServices = blockingServices;

            //_blockingPlugins = blockingPlugins != null ? blockingPlugins.ToReadOnlyCollection() : CKReadOnlyListEmpty<IPluginInfo>.Empty;
            //_blockingServices = blockingServices != null ? blockingServices.ToReadOnlyCollection() : CKReadOnlyListEmpty<IServiceInfo>.Empty;
        }

        public ConfigurationSolverResult( List<IPluginSolved> disabledPlugins, int availablePluginsCount, List<IPluginSolved> runningPlugins )
        {
            _disabledPlugins = disabledPlugins.ToReadOnlyCollection();
            _availablePluginsCount = availablePluginsCount;
            _runningPlugins = runningPlugins.ToReadOnlyCollection();
        }

        public bool ConfigurationSuccess { get { return _blockingPlugins == null; } }

        public IReadOnlyList<IPluginSolved> BlockingPlugins { get { return _blockingPlugins.ToReadOnlyList(); } }

        public IReadOnlyList<IServiceSolved> BlockingServices { get { return _blockingServices.ToReadOnlyList(); } }

       //////

        public IReadOnlyCollection<IPluginSolved> DisabledPlugins
        {
            get { return _disabledPlugins; }
        }

        public int AvailablePluginsCount
        {
            get { return _availablePluginsCount; }
        }

        public IReadOnlyCollection<IPluginSolved> RunningPlugins
        {
            get { return _runningPlugins; }
        }
    }
}
