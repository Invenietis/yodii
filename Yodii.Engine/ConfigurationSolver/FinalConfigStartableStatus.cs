#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\FinalConfigStartableStatus.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
