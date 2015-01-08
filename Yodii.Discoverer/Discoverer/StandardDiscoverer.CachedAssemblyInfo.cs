#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Discoverer\Discoverer\StandardDiscoverer.CachedAssemblyInfo.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;
using Mono.Collections.Generic;
using Mono.Cecil;

namespace Yodii.Discoverer
{
    public sealed partial class StandardDiscoverer : IDiscoverer
    {
        class CachedAssemblyInfo
        {
            public readonly AssemblyDefinition CecilInfo;
            public readonly AssemblyInfo YodiiInfo;
            public Exception Error { get; private set; }

            public CachedAssemblyInfo( Exception ex )
            {
                Error = ex;
                YodiiInfo = new AssemblyInfo( new Uri( "assembly://error" ), ex.Message );
            }

            public CachedAssemblyInfo( AssemblyDefinition cecilInfo )
            {
                CecilInfo = cecilInfo;
                YodiiInfo = new AssemblyInfo( cecilInfo.FullName, new Uri( "assembly://test.dll/" + cecilInfo.FullName ) );
            }

            public void Discover( StandardDiscoverer d )
            {
                try
                {
                    List<PluginInfo> plugins = new List<PluginInfo>();
                    List<ServiceInfo> services = new List<ServiceInfo>();
                    foreach( TypeDefinition t in CecilInfo.Modules.SelectMany( m => m.Types ) )
                    {
                        if( d.IsYodiiService( t ) )
                        {
                            ServiceInfo s = d.FindOrCreateService( t );
                            services.Add( s );
                        }
                        else if( d.IsYodiiPlugin( t ) )
                        {
                            PluginInfo p = d.FindOrCreatePlugin( t );
                            plugins.Add( p );
                        }
                    }

                    YodiiInfo.SetResult( services.ToArray(), plugins.ToArray() );
                }
                catch( Exception ex )
                {
                    YodiiInfo.SetError( ex.Message );
                    Error = ex;
                }
            }

        }
    }
}
