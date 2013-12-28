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
        readonly StartDependencyImpact _configOriginalImpact;
        readonly StartDependencyImpact _configSolvedImpact;

        public SolvedPluginSnapshot( PluginData plugin )
        {
            _pluginInfo = plugin.PluginInfo;
            _disabledReason = plugin.DisabledReason;
            _runningStatus = plugin.DynamicStatus;
            _configurationStatus = plugin.ConfigOriginalStatus;
            _configSolvedStatus = plugin.ConfigSolvedStatus;
            _configOriginalImpact = plugin.ConfigOriginalImpact;
            _configSolvedImpact = plugin.RawConfigSolvedImpact;
        }

        public string FullName { get { return _pluginInfo.PluginFullName; } }
        
        public IPluginInfo PluginInfo { get { return _pluginInfo; } }

        public string DisabledReason { get { return _disabledReason == PluginDisabledReason.None ? null : _disabledReason.ToString(); } }

        public ConfigurationStatus ConfigOriginalStatus { get { return _configurationStatus; } }

        ConfigurationStatus IStaticSolvedYodiiItem.WantedConfigSolvedStatus 
        { 
            get { return _configSolvedStatus; } 
        }

        ConfigurationStatus IDynamicSolvedYodiiItem.ConfigSolvedStatus 
        { 
            get { return _configSolvedStatus; } 
        }

        public StartDependencyImpact ConfigOriginalImpact { get { return _configOriginalImpact; } }

        public StartDependencyImpact ConfigSolvedImpact { get { return _configSolvedImpact; } }

        bool IStaticSolvedYodiiItem.IsBlocking 
        { 
            get 
            { 
                return _configSolvedStatus >= ConfigurationStatus.Runnable && _disabledReason != PluginDisabledReason.None; 
            } 
        }

        RunningStatus IDynamicSolvedYodiiItem.RunningStatus
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
