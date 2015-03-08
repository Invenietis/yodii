#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Mock model\MockAssemblyInfo.cs) is part of CiviKey. 
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
using CK.Core;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    internal class MockAssemblyInfo : IAssemblyInfo
    {
        readonly string _assemblyFileName;
        readonly List<IPluginInfo> _plugins;
        readonly List<IServiceInfo> _services;

        internal MockAssemblyInfo( string assemblyFileName )
        {
            Debug.Assert( !String.IsNullOrEmpty( assemblyFileName ) );

            _assemblyFileName = assemblyFileName;
            _plugins = new List<IPluginInfo>();
            _services = new List<IServiceInfo>();
        }

        #region IAssemblyInfo Members

        public string AssemblyFileName
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasPluginsOrServices
        {
            get { return _plugins.Count > 0 || _services.Count > 0; }
        }

        public IReadOnlyList<IPluginInfo> Plugins
        {
            get { return _plugins.AsReadOnlyList(); }
        }

        public IReadOnlyList<IServiceInfo> Services
        {
            get { return _services.AsReadOnlyList(); }
        }

        public AssemblyName AssemblyName 
        { 
            get { return null; } 
        }
        #endregion

        public Uri AssemblyLocation
        {
            // TODO
            get { throw new NotImplementedException(); }
        }
    }
}
