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
        readonly IServiceDependentObject _obj;
        int _finalStartableStatus;

        public FinalConfigStartableStatus( IServiceDependentObject pluginOrServicePropagation )
        {
            _obj = pluginOrServicePropagation;

            Debug.Assert( _obj.FinalConfigSolvedStatus == ConfigurationStatus.Optional || _obj.FinalConfigSolvedStatus == ConfigurationStatus.Runnable );
            Debug.Assert( ComputeStartable( StartDependencyImpact.Minimal ) );
            Debug.Assert( ComputeStartable( _obj.ConfigSolvedImpact == StartDependencyImpact.Unknown ? StartDependencyImpact.Minimal : _obj.ConfigSolvedImpact ) );
        }

        public bool IsStartable( StartDependencyImpact impact )
        {
            if( impact == StartDependencyImpact.Unknown || impact == StartDependencyImpact.Minimal || impact == _obj.ConfigSolvedImpact ) return true;
            int iImpact = (int)impact;
            if( (_finalStartableStatus & (256 << iImpact)) == 0 )
            {
                _finalStartableStatus |= (256 << iImpact);
                if( ComputeStartable( impact ) ) _finalStartableStatus |= (1 << iImpact);
            }
            return (_finalStartableStatus & (1 << iImpact)) != 0;
        }

        bool ComputeStartable( StartDependencyImpact impact )
        {
            foreach( var s in _obj.GetIncludedServices( impact ) )
            {
                if( s.Disabled ) return false;
            }
            foreach( var s in _obj.GetExcludedServices( impact ) )
            {
                if( s.FinalConfigSolvedStatus == ConfigurationStatus.Running ) return false;
            }
            return true;
        }
    }

}
