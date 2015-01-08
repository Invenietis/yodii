#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\Mocks\ServiceReferenceInfo.cs) is part of CiviKey. 
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


namespace Yodii.Host.Tests.Mocks
{
    public class ServiceReferenceInfo : IServiceReferenceInfo
    {
        IPluginInfo _owner;
        IServiceInfo _reference;
        DependencyRequirement _requirement;
        string _ctorParamOrPropertyName;
        int _ctorParamIndex;
        bool _isServiceWrapped;

        internal ServiceReferenceInfo( IPluginInfo ownerPlugin, IServiceInfo referencedService, DependencyRequirement requirement )
        {
            Debug.Assert( ownerPlugin != null );
            Debug.Assert( referencedService != null );

            _owner = ownerPlugin;
            _reference = referencedService;
            _requirement = requirement;
        }

        public IPluginInfo Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public IServiceInfo Reference
        {
            get { return _reference; }
            set { _reference = value; }
        }

        public DependencyRequirement Requirement
        {
            get { return _requirement; }
            set { _requirement = value; }
        }

        public string ConstructorParameterName
        {
            get { return _ctorParamOrPropertyName; }
            set { _ctorParamOrPropertyName = value; }
        }


        public int ConstructorParameterIndex
        {
            get { return _ctorParamIndex; }
            set { _ctorParamIndex = value; }
        }

        public bool IsNakedRunningService
        {
            get { return _isServiceWrapped; }
            set { _isServiceWrapped = value; }
        }
    }
}
