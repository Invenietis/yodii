#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Mocks\AssemblyInfo.cs) is part of CiviKey. 
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
using System.Reflection;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    public class AssemblyInfo : IAssemblyInfo
    {
        readonly Uri _location;
        readonly AssemblyName _assemblyName;

        internal AssemblyInfo( string assemblyUri )
        {
            _location = new Uri( assemblyUri );
        }
        internal AssemblyInfo( string assemblyFullName, Uri location )
        {
            Debug.Assert( location != null );
            _location = location;
            _assemblyName = new AssemblyName( assemblyFullName );
        }
        public Uri AssemblyLocation
        {
	        get { return _location; }
        }

        public AssemblyName AssemblyName { get { return _assemblyName; } }

        public IReadOnlyList<IPluginInfo> Plugins
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IServiceInfo> Services
        {
            get { throw new NotImplementedException(); }
        }
    }
}
