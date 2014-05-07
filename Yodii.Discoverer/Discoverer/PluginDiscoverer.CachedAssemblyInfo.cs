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
    public sealed partial class PluginDiscoverer
    {
        class CachedAssemblyInfo
        {
            public readonly AssemblyDefinition CecilInfo;
            public readonly AssemblyInfo YodiiInfo;
            public readonly Exception Error;

            public CachedAssemblyInfo( string path, Exception ex )
            {
                Error = ex;
                YodiiInfo = new AssemblyInfo( new Uri( path ), ex.Message );
            }

            public CachedAssemblyInfo( PluginDiscoverer d, string path, AssemblyDefinition cecilInfo )
            {
                CecilInfo = cecilInfo;
                var r = Discover( path, d );
                YodiiInfo = r.Key;
                Error = r.Value;
            }

            KeyValuePair<AssemblyInfo,Exception> Discover( string path, PluginDiscoverer d )
            {
                try
                {
                    List<ServiceInfo> _thisServices = new List<ServiceInfo>();
                    List<PluginInfo> _thisPlugins = new List<PluginInfo>();

                    return new KeyValuePair<AssemblyInfo,Exception>( new AssemblyInfo( new Uri( path ), _thisServices.ToArray(), _thisPlugins.ToArray() ), null );
                }
                catch( Exception ex )
                {
                    return new KeyValuePair<AssemblyInfo,Exception>( new AssemblyInfo( new Uri( path ), ex.Message ), ex );
                }
            }


        }
    }
}
