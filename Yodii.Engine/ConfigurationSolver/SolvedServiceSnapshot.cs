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
        readonly string _serviceDisabledReason;
        readonly ConfigurationStatus _configSolvedStatus;
        readonly IServiceInfo _serviceInfo;
        readonly ConfigurationStatus _configurationStatus;
        readonly RunningStatus? _runningStatus;
        readonly StartDependencyImpact _configOriginalImpact;
        readonly StartDependencyImpact _configSolvedImpact;

        public SolvedServiceSnapshot( ServiceData s )
        {
            _serviceInfo = s.ServiceInfo;
            _serviceDisabledReason = s.DisabledReason;
            _configSolvedStatus = s.ConfigSolvedStatus;
            _configurationStatus = s.ConfigOriginalStatus;
            _runningStatus = s.DynamicStatus;
            _configOriginalImpact = s.ConfigOriginalImpact;
            _configSolvedImpact = s.RawConfigSolvedImpact;
        }

        public string FullName { get { return _serviceInfo.ServiceFullName; } }
        
        public IServiceInfo ServiceInfo { get { return _serviceInfo; } }

        public string DisabledReason { get { return _serviceDisabledReason; } }

        public ConfigurationStatus ConfigOriginalStatus { get { return _configurationStatus; } }

        public StartDependencyImpact ConfigOriginalImpact { get { return _configOriginalImpact; } }

        public StartDependencyImpact ConfigSolvedImpact { get { return _configSolvedImpact; } }

        ConfigurationStatus IStaticSolvedYodiiItem.WantedConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }

        ConfigurationStatus IDynamicSolvedYodiiItem.ConfigSolvedStatus
        {
            get { return _configSolvedStatus; }
        }


        bool IStaticSolvedYodiiItem.IsBlocking 
        { 
            get {  return _configSolvedStatus >= ConfigurationStatus.Runnable && _serviceDisabledReason != null; } 
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
            return String.Format( "{0} - {1} - {2}", _serviceInfo.ServiceFullName, _serviceDisabledReason.ToString(), _configSolvedStatus.ToString() );
        }

    }
}
