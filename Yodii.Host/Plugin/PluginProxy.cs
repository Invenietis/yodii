#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Plugin\PluginProxy.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;
using Yodii.Model;
using System.Reflection;

namespace Yodii.Host
{

    class PluginProxy : PluginProxyBase, IPluginProxy
    {
        public PluginProxy( IPluginInfo pluginKey )
        {
            PluginKey = pluginKey;
        }

        public IPluginInfo PluginKey { get; internal set; }

        public string PublicName { get { return PluginKey.PluginFullName; } }

        internal bool TryLoad( ServiceHost serviceHost, Func<IPluginInfo,object[],IYodiiPlugin> pluginCreator )
        {
            var serviceReferences = PluginKey.ServiceReferences;
            object[] ctorParameters = new object[serviceReferences.Count];
            for( int i = 0; i < serviceReferences.Count; ++i )
            {
                ctorParameters[serviceReferences[i].ConstructorParameterIndex] = serviceHost.EnsureProxyForDynamicService( serviceReferences[i].Reference );
            }
            return TryLoad( serviceHost, () => pluginCreator( PluginKey, ctorParameters ), PluginKey );
        }

    }
}
