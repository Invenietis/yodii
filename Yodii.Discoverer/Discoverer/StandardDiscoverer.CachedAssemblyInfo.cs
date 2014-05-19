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
            public IAssemblyInfo YodiiInfo { get; private set; }
            public Exception Error { get; private set; }

            public CachedAssemblyInfo( string path, Exception ex )
            {
                Error = ex;
                YodiiInfo = new AssemblyInfo( new Uri( path ), ex.Message );
            }

            public CachedAssemblyInfo( AssemblyDefinition cecilInfo )
            {
                CecilInfo = cecilInfo;
            }

            public void Discover( string path, StandardDiscoverer d )
            {
                Debug.Assert( !String.IsNullOrEmpty( path ) && d != null );
                d._pathTest = path;
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

                    YodiiInfo = new AssemblyInfo( new Uri( path ), services.ToArray(), plugins.ToArray() );
                }

                catch( Exception ex )
                {
                    YodiiInfo = new AssemblyInfo( new Uri( path ), ex.Message );
                    Error = ex;
                }
            }

                    
            //        CKSortedArrayKeyList<PluginInfo, string> _plugins = new CKSortedArrayKeyList<PluginInfo, string>( p => p.PluginFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: false );
            //        CKSortedArrayKeyList<ServiceInfo, string> _services = new CKSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: false );             
            //        List<TypeDefinition> _serviceTypes = new List<TypeDefinition>();

            //        foreach( TypeDefinition service in d._allModules )
            //        {
            //            if( IsYodiiService( service ) )
            //            {
            //                _services.Add( new ServiceInfo( service.Name, new AssemblyInfo( new Uri( service.Module.FullyQualifiedName.ToString() ) ) ) );
            //                _serviceTypes.Add( service );
            //            }
            //        }

            //        foreach( TypeDefinition typeDef in _serviceTypes )
            //        {
            //            IEnumerable<TypeReference> parent =
            //                from i in typeDef.Interfaces
            //                where IsYodiiService( i.Resolve() )
            //                select i;

            //            if( parent.Any() )
            //                _services.GetByKey( typeDef.Name ).Generalization = _services.GetByKey( parent.ElementAt( 0 ).Name );
            //        }

            //        foreach( TypeDefinition plugin in d._allModules )
            //        {
            //            if( !IsYodiiPlugin( plugin ) ) continue;

            //            PluginInfo pluginInfo = new PluginInfo( plugin.FullName, new AssemblyInfo( new Uri( plugin.Module.FullyQualifiedName.ToString() ) ) );
            //            _plugins.Add( pluginInfo );
            //            TypeReference s = GetService( plugin );
            //            if( s != null ) pluginInfo.Service = _services.GetByKey( s.FullName );

            //            var ctors = plugin.Methods.Where( m => m.IsConstructor );
            //            var longerCtor = ctors.OrderBy( c => c.Parameters.Count ).LastOrDefault();
            //            if( longerCtor != null )
            //            {
            //                foreach( ParameterDefinition param in longerCtor.Parameters ) 
            //                {
            //                    var paramType = param.ParameterType.Resolve();
            //                    if( !paramType.IsInterface ) continue;
            //                    if( paramType.HasGenericParameters )
            //                    {
            //                        if( paramType.GenericParameters.Count > 1 ) continue;
            //                        TypeDefinition wrappedService = paramType.GenericParameters[0].Resolve();
            //                        if( !IsYodiiService( wrappedService ) ) continue;
            //                        TypeDefinition wrapper = wrappedService.DeclaringType;
            //                        DependencyRequirement req;
            //                        if( !IsDependencyRequirement( wrapper, out req ) ) continue;

            //                        ServiceInfo sRef = _services.GetByKey( wrappedService.FullName );
            //                        ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( pluginInfo, sRef, req, param.Name, param.Index, false );
            //                    }
            //                    else
            //                    {
            //                        if( !IsYodiiService( paramType ) ) continue;
            //                        ServiceInfo sRef = _services.GetByKey( paramType.FullName );
            //                        ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( pluginInfo, sRef, DependencyRequirement.Running, param.Name, param.Index, true );
            //                    }
            //                }

            //            }
            //        }
            //        YodiiInfo = new AssemblyInfo( new Uri( path ), _services.ToArray(), _plugins.ToArray() );
            //    }
        }
    }
}
