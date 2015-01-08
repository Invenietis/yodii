#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\Mocks\DiscoveredInfo.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;
using CK.Core;

namespace Yodii.Host.Tests.Mocks
{
    class DiscoveredInfo : IDiscoveredInfo
    {
        readonly List<PluginInfo> _plugins;
        readonly List<ServiceInfo> _services;

        public DiscoveredInfo()
        {
            _plugins = new List<PluginInfo>();
            _services = new List<ServiceInfo>();
            DefaultAssembly = new AssemblyInfo( "file://DefaultAssembly" );
        }

        public AssemblyInfo DefaultAssembly;

        public List<ServiceInfo> ServiceInfos
        {
            get { return _services; }
        }

        public ServiceInfo FindService( string serviceFullName )
        {
            return _services.FirstOrDefault( s => s.ServiceFullName == serviceFullName );
        }

        public PluginInfo FindPlugin( string pluginFullName )
        {
            return _plugins.FirstOrDefault( p => p.PluginFullName == pluginFullName );
        }

        public List<PluginInfo> PluginInfos
        {
            get { return _plugins; }
        }

        IReadOnlyList<IPluginInfo> IDiscoveredInfo.PluginInfos
        {
            get { return _plugins.AsReadOnlyList(); }
        }

        IReadOnlyList<IServiceInfo> IDiscoveredInfo.ServiceInfos
        {
            get { return _services.AsReadOnlyList(); }
        }

        public IReadOnlyList<IAssemblyInfo> AssemblyInfos
        {
            get { throw new NotImplementedException(); }
        }
    }
}
