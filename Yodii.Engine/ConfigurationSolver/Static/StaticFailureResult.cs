#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\Static\StaticFailureResult.cs) is part of CiviKey. 
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
using CK.Core;

namespace Yodii.Engine
{
    public class StaticFailureResult : IStaticFailureResult
    {
        readonly IReadOnlyList<IStaticSolvedPlugin> _blockingPlugins;
        readonly IReadOnlyList<IStaticSolvedService> _blockingServices;
        readonly IReadOnlyList<IStaticSolvedYodiiItem> _blockingItems;
        readonly IStaticSolvedConfiguration _solvedConfiguration;

        internal StaticFailureResult( IStaticSolvedConfiguration solvedConfiguration, IReadOnlyList<IStaticSolvedPlugin> blockedPlugins, IReadOnlyList<IStaticSolvedService> blockedServices )
        {
            Debug.Assert( solvedConfiguration != null && blockedPlugins != null && blockedServices != null );
            _blockingPlugins = blockedPlugins;
            _blockingServices = blockedServices;
            _blockingItems = _blockingServices.Cast<IStaticSolvedYodiiItem>().Concat( _blockingPlugins ).ToReadOnlyList();

            _solvedConfiguration = solvedConfiguration;
        }

        public IStaticSolvedConfiguration StaticSolvedConfiguration
        {
            get { return _solvedConfiguration; }
        }
        
        public IReadOnlyList<IStaticSolvedPlugin> BlockingPlugins
        {
            get { return _blockingPlugins; }
        }

        public IReadOnlyList<IStaticSolvedService> BlockingServices
        {
            get { return _blockingServices; }
        }
        
        public IReadOnlyList<IStaticSolvedYodiiItem> BlockingItems
        {
            get { return _blockingItems; }
        }
    }
}
