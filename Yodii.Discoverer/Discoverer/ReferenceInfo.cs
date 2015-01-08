#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Discoverer\Discoverer\ReferenceInfo.cs) is part of CiviKey. 
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
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Discoverer
{
    internal sealed class ServiceReferenceInfo : IServiceReferenceInfo
    {
        readonly IPluginInfo _owner;
        readonly IServiceInfo _reference;
        readonly DependencyRequirement _requirement;
        readonly string _ctorParamName;
        readonly int _ctorParamIndex;
        readonly bool _isNakedRunningService;

        internal ServiceReferenceInfo( IPluginInfo ownerPlugin, IServiceInfo referencedService, DependencyRequirement requirement, string paramName, int paramIndex, bool isNakedService )
        {
            Debug.Assert( ownerPlugin != null );
            Debug.Assert( referencedService != null );

            _owner = ownerPlugin;
            _reference = referencedService;
            _requirement = requirement;
            _ctorParamName = paramName;
            _ctorParamIndex = paramIndex;
            _isNakedRunningService = isNakedService;
        }

        public IPluginInfo Owner
        {
            get { return _owner; }
        }

        public IServiceInfo Reference
        {
            get { return _reference; }
        }

        public DependencyRequirement Requirement
        {
            get { return _requirement; }
        }

        public string ConstructorParameterName
        {
            get { return _ctorParamName; }
        }

        public int ConstructorParameterIndex
        {
            get { return _ctorParamIndex; }
        }

        public bool IsNakedRunningService
        {
            get { return _isNakedRunningService; }
        }
    }
}
