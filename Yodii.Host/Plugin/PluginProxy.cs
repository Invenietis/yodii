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
            PluginInfo = pluginKey;
        }

        public IPluginInfo PluginInfo { get; internal set; }

        internal bool TryLoad( ServiceHost serviceHost, Func<IPluginInfo, object[], IYodiiPlugin> pluginCreator )
        {
            var serviceReferences = PluginInfo.ServiceReferences;

            int paramCount = 0;

            // Fixes potential Array Out of Bounds for constructors with params ordered like ( MyType, IYodiiService ).
            // Unknown constructor parameters (here index 0) will be null.
            // Non-service parameters present after the last service (like ( IYodiiService, MyType )) will still be out of bounds :
            // we don't know the actual size or contents of the constructor.
            // So if you're injecting and/or allow unknown types, pluginCreator should check that it's neither null nor out-of-bounds.
            if( serviceReferences.Count > 0 ) paramCount = serviceReferences.Max( x => x.ConstructorParameterIndex ) + 1;

            object[] ctorParameters = new object[paramCount];

            for( int i = 0; i < serviceReferences.Count; ++i )
            {
                ctorParameters[serviceReferences[i].ConstructorParameterIndex] = serviceHost.EnsureProxyForDynamicService( serviceReferences[i].Reference );
            }
            return TryLoad( serviceHost, () => pluginCreator( PluginInfo, ctorParameters ), PluginInfo.PluginFullName );
        }

    }
}
