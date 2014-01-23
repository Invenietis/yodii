using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    /// <summary>
    /// At the end of the static resolution an instance of this class is built for each plugin and service 
    /// that are Optional or Runnable: it computes for each of the 4 available possible way to start the object whether
    /// it is possible or not.
    /// </summary>
    class FinalConfigStartableStatus
    {
        public readonly bool CanStartWithFullStop;
        public readonly bool CallableWithStopOptionalAndRunnable;
        public readonly bool CallableWithStartRecommended;
        public readonly bool CallableWithFullStart;

        public FinalConfigStartableStatus( IServiceDependentObject o )
        {
            Debug.Assert( o.FinalConfigSolvedStatus != SolvedConfigurationStatus.Disabled );
            Debug.Assert( ComputeStartableFor( o, StartDependencyImpact.Minimal ) );
            
            StartDependencyImpact config = o.ConfigSolvedImpact;
            Debug.Assert( ComputeStartableFor( o, config ) );
            Debug.Assert( ComputeStartableFor( o, StartDependencyImpact.Minimal ) );

            CanStartWithFullStop = config != StartDependencyImpact.FullStop ? ComputeStartableFor( o, StartDependencyImpact.FullStop ) : true;
            CallableWithStopOptionalAndRunnable = config != StartDependencyImpact.StopOptionalAndRunnable ? ComputeStartableFor( o, StartDependencyImpact.StopOptionalAndRunnable ) : true;
            CallableWithStartRecommended = config != StartDependencyImpact.StartRecommended ? ComputeStartableFor( o, StartDependencyImpact.StartRecommended ) : true;
            CallableWithFullStart = config != StartDependencyImpact.FullStart ? ComputeStartableFor( o, StartDependencyImpact.FullStart ) : true;
        }

        bool ComputeStartableFor( IServiceDependentObject o, StartDependencyImpact impact )
        {
            foreach( var s in o.GetIncludedServices( impact, false ) )
            {
                if( s.Disabled ) return false;
            }
            foreach( var s in o.GetExcludedServices( impact ) )
            {
                if( s.FinalConfigSolvedStatus == SolvedConfigurationStatus.Running ) return false;
            }
            return true;
        }
    }

}
