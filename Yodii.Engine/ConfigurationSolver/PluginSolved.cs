using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class PluginSolved : IPluginSolved
    {
        readonly IPluginInfo _pluginInfo;
        readonly Exception _runtimeError; //Deal with it later
        readonly PluginDisabledReason _disabledReason;
        readonly ConfigurationStatus _configurationStatus;
        readonly RunningStatus? _runningStatus;
        readonly RunningRequirement _configSolvedStatus;

        public PluginSolved( IPluginInfo p, PluginDisabledReason disabledReason, RunningRequirement configSolvedStatus, ConfigurationStatus configurationStatus, RunningStatus? runningStatus )
        {
            Debug.Assert( configSolvedStatus != null );

            _pluginInfo = p;
            _disabledReason = disabledReason;
            _runningStatus = runningStatus;
            _configurationStatus = configurationStatus;
            _configSolvedStatus = configSolvedStatus;
        }

        public bool IsBlocking { get { return _configSolvedStatus >= RunningRequirement.Runnable && _disabledReason != PluginDisabledReason.None; } }

        public bool IsDisabled { get { return _disabledReason != PluginDisabledReason.None; } }

        public IPluginInfo PluginInfo { get { return _pluginInfo; } }

        public PluginDisabledReason DisabledReason { get { return _disabledReason; } }

        public ConfigurationStatus ConfigurationStatus { get { return _configurationStatus; } }

        public RunningRequirement ConfigSolvedStatus { get { return _configSolvedStatus; } }

        public RunningStatus? RunningStatus { get { return _runningStatus; } }

        public Exception RuntimeError { get { return _runtimeError; } }

        public bool IsCulprit { get { return _disabledReason != PluginDisabledReason.None || _runtimeError != null; } }
    }
}
