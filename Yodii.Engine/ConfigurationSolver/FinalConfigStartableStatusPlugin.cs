#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\FinalConfigStartableStatusPlugin.cs) is part of CiviKey. 
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
