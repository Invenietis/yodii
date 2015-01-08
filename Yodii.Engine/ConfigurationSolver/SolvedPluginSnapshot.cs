#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\SolvedPluginSnapshot.cs) is part of CiviKey. 
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

namespace Yodii.Engine
{
    internal class SolvedPluginSnapshot : SolvedItemSnapshot, IStaticSolvedPlugin, IDynamicSolvedPlugin
    {
        readonly IPluginInfo _pluginInfo;

        public SolvedPluginSnapshot( PluginData plugin )
            : base( plugin )
        {
            _pluginInfo = plugin.PluginInfo;
        }

        public bool IsPlugin { get { return true; } }

        public override string FullName { get { return _pluginInfo.PluginFullName; } }
        
        public IPluginInfo PluginInfo { get { return _pluginInfo; } }
    }
}
