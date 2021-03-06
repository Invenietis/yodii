#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\Dynamic\DynamicSolvedConfiguration.cs) is part of CiviKey. 
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
    class DynamicSolvedConfiguration : IDynamicSolvedConfiguration
    {
        readonly IReadOnlyList<IDynamicSolvedPlugin> _plugins;
        readonly IReadOnlyList<IDynamicSolvedService> _services;
        readonly IReadOnlyList<IDynamicSolvedYodiiItem> _items;

        internal DynamicSolvedConfiguration( IReadOnlyList<IDynamicSolvedPlugin> plugins, IReadOnlyList<IDynamicSolvedService> services )
        {
            Debug.Assert(plugins != null && services != null);
            _plugins = plugins;
            _services = services;
            _items = _services.Cast<IDynamicSolvedYodiiItem>().Concat( _plugins ).ToReadOnlyList();
        }

        public IReadOnlyList<IDynamicSolvedPlugin> Plugins
        {
            get { return _plugins; }
        }

        public IReadOnlyList<IDynamicSolvedService> Services
        {
            get { return _services; }
        }

        public IReadOnlyList<IDynamicSolvedYodiiItem> YodiiItems
        {
            get { return _items; }
        }

        public IDynamicSolvedYodiiItem FindItem( string fullName )
        {
            return (IDynamicSolvedYodiiItem)FindService( fullName ) ?? FindPlugin( fullName );
        }

        public IDynamicSolvedService FindService( string serviceFullName )
        {
            return _services.FirstOrDefault( s => s.ServiceInfo.ServiceFullName == serviceFullName);
        }

        public IDynamicSolvedPlugin FindPlugin( string pluginFullName )
        {
            return _plugins.FirstOrDefault( p => p.PluginInfo.PluginFullName == pluginFullName );
        }
    }
}
