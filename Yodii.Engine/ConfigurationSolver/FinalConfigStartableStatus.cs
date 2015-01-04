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
    /// that are Optional or Runnable: it computes for each of the 16 available possible way to start the object whether
    /// it is possible or not.
    /// </summary>
    abstract class FinalConfigStartableStatus
    {
        readonly bool?[] _all;
        
        public FinalConfigStartableStatus( StartDependencyImpact solvedConfiguration )
        {
            _all = new bool?[16];
            Debug.Assert( _all.All( s => !s.HasValue ) );
            FillFromKnownGood( solvedConfiguration );
            Debug.Assert( _all[0].Value );
        }

        void FillFromKnownGood( StartDependencyImpact known )
        {
            for( int i = 0; i < 16; ++i )
            {
                StartDependencyImpact flags = (StartDependencyImpact)(i << 1);
                if( (flags & known) == flags )
                {
                    Debug.Assert( !_all[i].HasValue || _all[i].Value, "Either not yet known or already possible start." );
                    _all[i] = true;
                }
            }
        }

        public bool CanStartWith( StartDependencyImpact impact )
        {
            impact = impact.ClearAllTryBits();
            int i = (int)impact >> 1;
            var known = _all[i];
            if( !known.HasValue )
            {
                known = ComputeStartableFor( impact );
                if( known.Value )
                {
                    FillFromKnownGood( impact );
                    Debug.Assert( _all[i].Value );
                }
                else _all[i] = known;
            }
            return known.Value;
        }

        protected abstract bool ComputeStartableFor( StartDependencyImpact impact );

    }

}
