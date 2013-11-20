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

        public IServiceInfo ServiceInfo { get { return _serviceInfo; } }

        public ServiceDisabledReason ConfigDisabledReason { get { return _serviceDisabledReason; } }

        public ConfigurationStatus ConfigOriginalStatus { get { return _configurationStatus; } }

        SolvedConfigurationStatus IStaticSolvedService.WantedConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        SolvedConfigurationStatus IDynamicSolvedService.ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }


        bool IStaticSolvedService.IsBlocking 
        { 
            get 
            { 
                return _configSolvedStatus >= SolvedConfigurationStatus.Runnable 
                        && _serviceDisabledReason != ServiceDisabledReason.None; 
            } 
        }
        
        RunningStatus IDynamicSolvedService.RunningStatus
        {
            get
            {
                Debug.Assert( _runningStatus.HasValue, "After dynamic resolution: running status is known." );
                return _runningStatus.Value;
            }
        }

        public override string ToString()
        {
            return String.Format( "{0} - {1} - {2}", _serviceInfo.ServiceFullName, IsDisabled ? _serviceDisabledReason.ToString() : "!Disabled", ConfigSolvedStatus.ToString() );
        }
    }
}
