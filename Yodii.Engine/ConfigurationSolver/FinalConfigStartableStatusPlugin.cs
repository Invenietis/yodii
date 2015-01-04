using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    sealed class FinalConfigStartableStatusPlugin : FinalConfigStartableStatus
    {
        readonly PluginData _plugin;

        public FinalConfigStartableStatusPlugin( PluginData p )
            : base( p.ConfigSolvedImpact )
        {
            _plugin = p;
            Debug.Assert( !p.Disabled );
            Debug.Assert( ComputeStartableFor( StartDependencyImpact.Minimal ) );
            Debug.Assert( ComputeStartableFor( p.ConfigSolvedImpact ) );
        }

        protected override bool ComputeStartableFor( StartDependencyImpact impact )
        {
            var allIncluded = _plugin.GetIncludedServicesClosure( impact );
            foreach( var s in _plugin.GetExcludedServices( impact ) )
            {
                if( s.FinalConfigSolvedStatus == SolvedConfigurationStatus.Running ) return false;
                if( allIncluded.Contains( s ) ) return false;
            }
            foreach( var s in allIncluded )
            {
                if( s.Disabled ) return false;
            }
            return true;
        }

    }

}
