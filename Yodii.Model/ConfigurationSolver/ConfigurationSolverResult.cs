using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;
using Yodii.Model.CoreModel;


namespace Yodii.Model.ConfigurationSolver
{
    public class ConfigurationSolverResult : IConfigurationSolverResult
    {
        ICKReadOnlyCollection<IPluginInfo> _blockingPlugins;
        ICKReadOnlyCollection<IServiceInfo> _blockingServices;
        
        ICKReadOnlyCollection<IPluginInfo> _disabledPlugins;
        ICKReadOnlyCollection<IPluginInfo> _stoppedPlugins;
        ICKReadOnlyCollection<IPluginInfo> _runningPlugins;

        public ConfigurationSolverResult( List<IPluginInfo> blockingPlugins, List<IServiceInfo> blockingServices )
        {
            Debug.Assert( blockingPlugins != null || blockingServices != null );
            if( blockingPlugins != null )
            {
                _blockingPlugins = blockingPlugins.ToReadOnlyCollection();
            }
            if( blockingServices != null )
            {
                _blockingServices = blockingServices.ToReadOnlyCollection();
            }
        }

        public ConfigurationSolverResult( List<IPluginInfo> disabledPlugins, List<IPluginInfo> stoppedPlugins, List<IPluginInfo> runningPlugins )
        {
            ConfigurationSuccess = true;
            _disabledPlugins = disabledPlugins.ToReadOnlyCollection();
            _stoppedPlugins = stoppedPlugins.ToReadOnlyCollection();
            _runningPlugins = runningPlugins.ToReadOnlyCollection();
        }

        public bool ConfigurationSuccess { get; private set; }

        public ICKReadOnlyCollection<IPluginInfo> BlockingPlugins 
        { 
            get { return _blockingPlugins; } 
        }

        public ICKReadOnlyCollection<IServiceInfo> BlockingServices 
        {
            get { return _blockingServices; } 
        }

        public ICKReadOnlyCollection<IPluginInfo> DisabledPlugins
        {
            get { return _disabledPlugins; }
        }

        public ICKReadOnlyCollection<IPluginInfo> StoppedPlugins
        {
            get { return _stoppedPlugins; }
        }

        public ICKReadOnlyCollection<IPluginInfo> RunningPlugins
        {
            get { return _runningPlugins; }
        }

        internal void ApplyToLiveConfiguration( LiveConfiguration config )
        {
            Debug.Assert( ConfigurationSuccess );            
        }
    }
}
