using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal abstract class SolvedItemSnapshot
    {
        readonly string _disabledReason;
        readonly ConfigurationStatus _configOriginalStatus;
        readonly RunningStatus? _runningStatus;
        readonly SolvedConfigurationStatus _configSolvedStatus;
        readonly StartDependencyImpact _configOriginalImpact;
        readonly StartDependencyImpact _configSolvedImpact;

        public SolvedItemSnapshot( IYodiiItemData item )
        {
            _disabledReason = item.DisabledReason;
            _runningStatus = item.DynamicStatus;
            _configOriginalStatus = item.ConfigOriginalStatus;
            _configSolvedStatus = item.ConfigSolvedStatus;
            _configOriginalImpact = item.ConfigOriginalImpact;
            _configSolvedImpact = item.RawConfigSolvedImpact;
        }

        public abstract string FullName { get; }
        
        public string DisabledReason { get { return _disabledReason; } }

        public ConfigurationStatus ConfigOriginalStatus { get { return _configOriginalStatus; } }

        public SolvedConfigurationStatus WantedConfigSolvedStatus 
        { 
            get { return _configSolvedStatus; } 
        }

        public SolvedConfigurationStatus FinalConfigSolvedStatus 
        { 
            get { return _disabledReason == null ? _configSolvedStatus : SolvedConfigurationStatus.Disabled; } 
        }

        public StartDependencyImpact ConfigOriginalImpact { get { return _configOriginalImpact; } }

        public StartDependencyImpact ConfigSolvedImpact { get { return _configSolvedImpact; } }

        public bool IsBlocking 
        { 
            get { return _configOriginalStatus >= ConfigurationStatus.Runnable && _disabledReason != null; } 
        }

        public RunningStatus RunningStatus
        { 
            get 
            {
                Debug.Assert( _runningStatus.HasValue, "After dynamic resolution: running status is known." );
                return _runningStatus.Value; 
            } 
        }

        public override string ToString()
        {
            if( _disabledReason != null )
            {
                if( IsBlocking )
                {
                    return String.Format( "{0} - Blocking, Disabled: {1}, Expected: {2}", FullName, _disabledReason, _configSolvedStatus.ToString() );
                }
                return String.Format( "{0} - Disabled ({1}), Wanted: {2}", FullName, _disabledReason, _configSolvedStatus.ToString() );
            }
            if( _runningStatus.HasValue )
            {
                return String.Format( "{0} - {1} - Dynamic: {2}", FullName, _configSolvedStatus.ToString(), _runningStatus.ToString() );
            }
            return String.Format( "{0} - {1}", FullName, _configSolvedStatus.ToString() );
        }
    }
}
