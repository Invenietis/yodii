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
                YodiiInfo = new AssemblyInfo( new Uri( "assembly://test.dll/" + cecilInfo.FullName ) );
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
