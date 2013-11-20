using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class SolvedServiceSnapshot : IStaticSolvedService, IDynamicSolvedService
    {
        readonly ServiceDisabledReason _serviceDisabledReason;
        readonly SolvedConfigurationStatus _configSolvedStatus;
        readonly IServiceInfo _serviceInfo;
        readonly ConfigurationStatus _configurationStatus;
        readonly RunningStatus? _runningStatus;

        public SolvedServiceSnapshot( ServiceData s )
        {
            _serviceInfo = s.ServiceInfo;
            _serviceDisabledReason = s.DisabledReason;
            _configSolvedStatus = s.ConfigSolvedStatus;
            _configurationStatus = s.ConfigOriginalStatus;
            _runningStatus = s.DynamicStatus;
        }

        public bool IsBlocking { get { return _configSolvedStatus >= SolvedConfigurationStatus.Runnable && _serviceDisabledReason != ServiceDisabledReason.None; } }
        public bool IsDisabled { get { return _serviceDisabledReason != ServiceDisabledReason.None; } }

        public IServiceInfo ServiceInfo { get { return _serviceInfo; } }

        public ServiceDisabledReason ConfigDisabledReason { get { return _serviceDisabledReason; } }

        public ConfigurationStatus ConfigurationStatus { get { return _configurationStatus; } }

        public SolvedConfigurationStatus ConfigSolvedStatus { get { return _configSolvedStatus; } }

        public RunningStatus? RunningStatus { get { return _runningStatus; } }

        public override string ToString()
        {
            return String.Format( "{0} - {1} - {2}", _serviceInfo.ServiceFullName, IsDisabled ? _serviceDisabledReason.ToString() : "!Disabled", ConfigSolvedStatus.ToString() );
        }
    }
}
