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
            public AssemblyInfo YodiiInfo { get; private set; }
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
                    List<ServiceInfo> _thisServices = new List<ServiceInfo>();
                    List<PluginInfo> _thisPlugins = new List<PluginInfo>();

                    //List<TypeDefinition> _pluginTypes = new List<TypeDefinition>();
                    //List<TypeDefinition> _serviceTypes = new List<TypeDefinition>(); 

                    //Instantiate service infos 
                    foreach( TypeDefinition service in d._allModules )
                    {
                        if( IsYodiiService( service ) )
                        {
                            _thisServices.Add( new ServiceInfo( service.Name, new AssemblyInfo( new Uri( service.Module.FullyQualifiedName.ToString() ) ) ) );
                            d._serviceTypes.Add( service );
                            d._services.Add( new ServiceInfo( service.Name, new AssemblyInfo( new Uri( service.Module.FullyQualifiedName.ToString() ) ) ) );
                        }
                    }

                    //Set generalization on services
                    foreach( TypeReference typeRef in _discoverer._serviceTypes )
                    {
                        IEnumerable<TypeReference> parent =
                            from i in typeRef.Resolve().Interfaces
                            where IsYodiiService( i.Resolve() )
                            select i;

                        if( parent.Any() )
                            _discoverer._services.GetByKey( typeRef.Name ).Generalization = _discoverer._services.GetByKey( parent.ElementAt( 0 ).Name );
                    }

                    //Instantiate plugins and set their service references
                    foreach( TypeDefinition plugin in d._allModules )
                    {
                        if( IsYodiiPlugin( plugin ) )
                        {
                            _thisPlugins.Add( new PluginInfo( plugin.Name, new AssemblyInfo( new Uri( plugin.Module.FullyQualifiedName.ToString() ) ) ) );
                            d._pluginTypes.Add( plugin );
                            d._plugins.Add( new PluginInfo( plugin.Name, new AssemblyInfo( new Uri( plugin.Module.FullyQualifiedName.ToString() ) ) ) );
                            SetServiceReferences( plugin );
                        }
                    }

                    YodiiInfo = new AssemblyInfo( new Uri( path ), _thisServices.ToArray(), _thisPlugins.ToArray() );
                }
                catch( Exception ex )
                {
                    YodiiInfo = new AssemblyInfo( new Uri( path ), ex.Message );
                    Error = ex;
                }
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

            //Ugly as fuck, needs refactoring, fast.
            void SetServiceReferences( TypeDefinition plugin )
            {
                //check for service 
                var s = GetService( plugin );
                if( s != null )
                    _discoverer._plugins.GetByKey( plugin.Name ).Service = _discoverer._services.GetByKey( s.Name );
                
                //check for references
                MethodDefinition ctor = plugin.Methods.Where( m => m.IsConstructor && m.HasCustomAttributes ).ElementAt( 0 );

                var attr = ctor.CustomAttributes.Where( ca => ca.ConstructorArguments[0].Type.Resolve() == _discoverer._tDefDependencyRequirement ).Select( ca => ca.ConstructorArguments );
                DependencyRequirement req = (DependencyRequirement)attr.ElementAt( 0 ).ElementAt( 0 ).Value;
                string paramName = attr.ElementAt( 0 ).ElementAt( 1 ).Value.ToString();

                int paramIndex = ctor.Parameters.IndexOf( p => p.Name == paramName );
                ServiceInfo service = _discoverer._services.GetByKey( ctor.Parameters.ElementAt( paramIndex ).ParameterType.Name );
                PluginInfo pl = _discoverer._plugins.GetByKey( plugin.Name );
                ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( pl, service, req, paramName, paramIndex, req == DependencyRequirement.Running );
                pl.BindServiceRequirement( serviceRef );             
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
