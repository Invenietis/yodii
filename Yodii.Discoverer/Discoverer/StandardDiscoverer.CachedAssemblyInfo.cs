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
            public StandardDiscoverer _discoverer { get; private set; }

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
                _discoverer = d;

                try
                {
                    CKSortedArrayKeyList<PluginInfo, string> _plugins = new CKSortedArrayKeyList<PluginInfo, string>( p => p.PluginFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: false );
                    CKSortedArrayKeyList<ServiceInfo, string> _services = new CKSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: false );             
                    List<TypeDefinition> _serviceTypes = new List<TypeDefinition>();

                    foreach( TypeDefinition service in d._allModules )
                    {
                        if( IsYodiiService( service ) )
                        {
                            _services.Add( new ServiceInfo( service.Name, new AssemblyInfo( new Uri( service.Module.FullyQualifiedName.ToString() ) ) ) );
                            _serviceTypes.Add( service );
                        }
                    }

                    foreach( TypeDefinition typeDef in _serviceTypes )
                    {
                        IEnumerable<TypeReference> parent =
                            from i in typeDef.Interfaces
                            where IsYodiiService( i.Resolve() )
                            select i;

                        if( parent.Any() )
                            _services.GetByKey( typeDef.Name ).Generalization = _services.GetByKey( parent.ElementAt( 0 ).Name );
                    }

                    foreach( TypeDefinition plugin in d._allModules )
                    {
                        if( IsYodiiPlugin( plugin ) )
                        {
                            _plugins.Add( new PluginInfo( plugin.Name, new AssemblyInfo( new Uri( plugin.Module.FullyQualifiedName.ToString() ) ) ) );
                            //SetServiceReferences( plugin );
                            TypeReference s = GetService( plugin );
                            if( s != null )
                                _plugins.GetByKey( plugin.Name ).Service = _services.GetByKey( s.Name );

                            if( HasYodiiServiceReferences( plugin ) )
                            {
                                MethodDefinition ctor = plugin.Methods.Where( m => m.IsConstructor && m.HasCustomAttributes ).ElementAt( 0 );

                                var attr = ctor.CustomAttributes.Where( ca => ca.ConstructorArguments[0].Type.FullName == _discoverer._tDefDependencyRequirement.FullName ).Select( ca => ca.ConstructorArguments );
                                for( int i = 0; i < attr.Count(); ++i )
                                {
                                    DependencyRequirement req = (DependencyRequirement)attr.ElementAt( i ).ElementAt( 0 ).Value;
                                    string paramName = attr.ElementAt( i ).ElementAt( 1 ).Value.ToString();
                                    int paramIndex = ctor.Parameters.IndexOf( p => p.Name == paramName );

                                    ServiceInfo service = _services.GetByKey( ctor.Parameters.ElementAt( paramIndex ).ParameterType.Name );
                                    PluginInfo pl = _plugins.GetByKey( plugin.Name );
                                    ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( pl, service, req, paramName, paramIndex, req == DependencyRequirement.Running );
                                    pl.BindServiceRequirement( serviceRef );
                                }
                            }
                        }
                    }

                    YodiiInfo = new AssemblyInfo( new Uri( path ), _services.ToArray(), _plugins.ToArray() );
                }
                catch( Exception ex )
                {
                    YodiiInfo = new AssemblyInfo( new Uri( path ), ex.Message );
                    Error = ex;
                }
            }

            private bool HasYodiiServiceReferences( TypeDefinition plugin )
            {
                if( plugin.Methods.Where( m => m.IsConstructor && m.HasCustomAttributes ).Any() ) return true;
                return false;
            }

            bool IsYodiiPlugin( TypeDefinition type )
            {
                if( type.IsClass && !type.IsAbstract )
                {
                    IEnumerable<TypeReference> target =
                        from i in type.Interfaces
                        where i.Resolve().FullName.Equals( _discoverer._tDefIYodiiPlugin.FullName )
                        select i;
                    if( target.Any() ) return true;
                }
                return false;
            }

            bool IsYodiiService( TypeDefinition type )
            {
                if( type.IsInterface )
                {
                    IEnumerable<TypeReference> target =
                        from i in type.Interfaces
                        where i.Resolve().FullName.Equals( _discoverer._tDefIYodiiService.FullName )
                        select i;
                    if( target.Any() ) return true;
                }
                return false;
            }

            TypeReference GetService( TypeDefinition plugin )
            {
                IEnumerable<TypeReference> query = from TypeReference i in plugin.Interfaces
                                                   where IsYodiiService( i.Resolve() )
                                                   select i;
                if( query.Any() )
                    return query.ElementAt( 0 );
                return null;
            }
        }

        public int CurrentVersion { get; set; }
    }
}
