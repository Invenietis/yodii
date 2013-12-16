using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class SolvedPluginSnapshot : IStaticSolvedPlugin, IDynamicSolvedPlugin
    {
        readonly IPluginInfo _pluginInfo;
        readonly PluginDisabledReason _disabledReason;
        readonly ConfigurationStatus _configurationStatus;
        readonly RunningStatus? _runningStatus;
        readonly ConfigurationStatus _configSolvedStatus;

        public SolvedPluginSnapshot( PluginData plugin )
        {
            _pluginInfo = plugin.PluginInfo;
            _disabledReason = plugin.DisabledReason;
            _runningStatus = plugin.DynamicStatus;
            _configurationStatus = plugin.ConfigOriginalStatus;
            _configSolvedStatus = plugin.ConfigSolvedStatus;
        }

        public IPluginInfo PluginInfo { get { return _pluginInfo; } }

        public string DisabledReason { get { return _disabledReason.ToString(); } }

        public ConfigurationStatus ConfigOriginalStatus { get { return _configurationStatus; } }

        ConfigurationStatus IStaticSolvedPlugin.WantedConfigSolvedStatus 
        { 
            get { return _configSolvedStatus; } 
        }

        ConfigurationStatus IDynamicSolvedPlugin.ConfigSolvedStatus 
        { 
            get { return _configSolvedStatus; } 
        }

        bool IStaticSolvedPlugin.IsBlocking 
        { 
            get 
            { 
                return _configSolvedStatus >= ConfigurationStatus.Runnable && _disabledReason != PluginDisabledReason.None; 
            } 
        }

        RunningStatus IDynamicSolvedPlugin.RunningStatus
        { 
            get 
            {
                Debug.Assert( _runningStatus.HasValue, "After dynamic resolution: running status is known." );
                return _runningStatus.Value; 
            } 
        }

        public override string ToString()
        {
            return String.Format( "{0} - {1} - {2}", _pluginInfo.PluginFullName, _disabledReason.ToString(), _configSolvedStatus.ToString() );
        }
    }
}
