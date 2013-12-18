using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class FinalConfigStartableStatus
    {
        public readonly bool CallableWithStartFullStop;
        public readonly bool CallableWithStopOptionalAndRunnable;
        public readonly bool CallableWithStartRecommended;
        public readonly bool CallableWithFullStart;

        public FinalConfigStartableStatus( IServiceDependentObject pluginOrServicePropagation )
        {
            Debug.Assert( pluginOrServicePropagation.FinalConfigSolvedStatus == ConfigurationStatus.Optional || pluginOrServicePropagation.FinalConfigSolvedStatus == ConfigurationStatus.Runnable );
            Debug.Assert( ComputeStartableFor( pluginOrServicePropagation, StartDependencyImpact.Minimal ) );
            
            StartDependencyImpact config = pluginOrServicePropagation.ConfigSolvedImpact == StartDependencyImpact.Unknown ? StartDependencyImpact.Minimal : pluginOrServicePropagation.ConfigSolvedImpact;
            Debug.Assert( ComputeStartableFor( pluginOrServicePropagation, config ) );

            CallableWithStartFullStop = config != StartDependencyImpact.FullStop ? ComputeStartableFor( pluginOrServicePropagation, StartDependencyImpact.FullStop ) : true;
            CallableWithStopOptionalAndRunnable = config != StartDependencyImpact.StopOptionalAndRunnable ? ComputeStartableFor( pluginOrServicePropagation, StartDependencyImpact.StopOptionalAndRunnable ) : true;
            CallableWithStartRecommended = config != StartDependencyImpact.StartRecommended ? ComputeStartableFor( pluginOrServicePropagation, StartDependencyImpact.StartRecommended ) : true;
            CallableWithFullStart = config != StartDependencyImpact.FullStart ? ComputeStartableFor( pluginOrServicePropagation, StartDependencyImpact.FullStart ) : true;
        }

        bool ComputeStartableFor( IServiceDependentObject o, StartDependencyImpact impact )
        {
            foreach( var s in o.GetIncludedServices( impact, false ) )
            {
                if( s.Disabled ) return false;
            }
            foreach( var s in o.GetExcludedServices( impact ) )
            {
                if( s.FinalConfigSolvedStatus == ConfigurationStatus.Running ) return false;
            }
            return true;
        }
    }

}
