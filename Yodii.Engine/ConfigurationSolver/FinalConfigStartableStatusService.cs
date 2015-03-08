#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\FinalConfigStartableStatusService.cs) is part of CiviKey. 
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
