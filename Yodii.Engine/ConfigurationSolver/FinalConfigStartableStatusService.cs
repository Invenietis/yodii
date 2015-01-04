using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    /// <summary>
    /// Concrete FinalConfigStartableStatus for service: consider that the available StartDependencyImpact are the union
    /// of the available StartDependencyImpact of the available plugins.
    /// </summary>
    sealed class FinalConfigStartableStatusService : FinalConfigStartableStatus
    {
        readonly ServiceData _service;

        public FinalConfigStartableStatusService( ServiceData s )
            : base( s.ConfigSolvedImpact )
        {
            _service = s;
            Debug.Assert( !s.Disabled );
            Debug.Assert( ComputeStartableFor( StartDependencyImpact.Minimal ) );
            Debug.Assert( ComputeStartableFor( s.ConfigSolvedImpact ) );
        }

        protected override bool ComputeStartableFor( StartDependencyImpact impact )
        {
            return _service.FindFirstPluginData( p => p.FinalStartableStatus != null && p.FinalStartableStatus.CanStartWith( impact ) ) != null;
        }

    }
}
