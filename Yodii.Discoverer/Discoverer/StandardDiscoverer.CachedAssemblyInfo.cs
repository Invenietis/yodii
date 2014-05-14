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
                        if( !IsYodiiPlugin( plugin ) )
                            continue;

                        PluginInfo pluginInfo = new PluginInfo( plugin.Name, new AssemblyInfo( new Uri( plugin.Module.FullyQualifiedName.ToString() ) ) );
                        _plugins.Add( pluginInfo );
                        TypeReference s = GetService( plugin );
                        if( s != null )
                            _plugins.GetByKey( plugin.Name ).Service = _services.GetByKey( s.Name );

                        var ctors = plugin.Methods.Where( m => m.IsConstructor );
                        var longerCtor = ctors.OrderBy( c => c.Parameters.Count ).LastOrDefault();
                        if( longerCtor != null )
                        {
                            foreach( ParameterDefinition param in longerCtor.Parameters ) 
                            {
                                var paramType = param.ParameterType.Resolve();
                                if( !paramType.IsInterface ) continue;
                                DependencyRequirement req;
                                if( GetReq( paramType.GenericParameters.FirstOrDefault().DeclaringType, out req ) )
                                {
                                    ServiceInfo sRef = _services.GetByKey( ( (GenericInstanceType)param.ParameterType ).GenericParameters[param.Index].Name );
                                    ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( pluginInfo, sRef, req, param.Name, param.Index, DependencyRequirement.Running == req );
                                }
                                //foreach( GenericParameter generic in paramType.GenericParameters )
                                //{
                                //    TypeReference typeDef = generic.DeclaringType;
                                //}
                                //if( paramType.Interfaces.Contains(

                                //DependencyRequirement req = GetReq( param.ParameterType );
                                //ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( p, sRef, req, paramName, param.Index, req == DependencyRequirement.Running );                               
                            }

                        }

                        var ctorParameters = plugin.Methods.Where( m => m.IsConstructor && ContainsYodiiReferences( m.Parameters ) ).SelectMany( m => m.Parameters);
                            //var ctorParameters = plugin.Methods.Where( m => m.IsConstructor && HasYodiiServiceReferences( m ) ).Where(p => p..ElementAt( 0 ).Parameters.Where( p => IsYodiiServiceReference( p.ParameterType ) );
                            
                        foreach( ParameterDefinition param in ctorParameters ) 
                        {
                            PluginInfo p = _plugins.GetByKey( plugin.Name );
                            //DependencyRequirement req = GetReq( param.ParameterType );
                            ServiceInfo sRef = _services.GetByKey( ( (GenericInstanceType)param.ParameterType ).GenericParameters[param.Index].Name );
                            //ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( p, sRef, req, paramName, param.Index, req == DependencyRequirement.Running );                               
                        }
                    }
                
                    //HasYodiiServiceReferences(ctor) && 
                        //var attr = ctor.CustomAttributes.Where( ca => ca.ConstructorArguments[0].Type.FullName == _discoverer._tDefDependencyRequirement.FullName ).Select( ca => ca.ConstructorArguments );
                        //for( int i = 0; i < attr.Count(); ++i )
                        //{
                        //    DependencyRequirement req = (DependencyRequirement)attr.ElementAt( i ).ElementAt( 0 ).Value;
                        //    string paramName = attr.ElementAt( i ).ElementAt( 1 ).Value.ToString();
                        //    int paramIndex = ctor.Parameters.IndexOf( p => p.Name == paramName );

                        //    ServiceInfo service = _services.GetByKey( ctor.Parameters.ElementAt( paramIndex ).ParameterType.Name );
                        //    PluginInfo pl = _plugins.GetByKey( plugin.Name );
                        //    ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( pl, service, req, paramName, paramIndex, req == DependencyRequirement.Running );
                        //    pl.BindServiceRequirement( serviceRef );
                        //}
                    //}

                    YodiiInfo = new AssemblyInfo( new Uri( path ), _services.ToArray(), _plugins.ToArray() );
                }
                catch( Exception ex )
                {
                    YodiiInfo = new AssemblyInfo( new Uri( path ), ex.Message );
                    Error = ex;
                }
            }

            private bool GetReq( TypeReference param, out DependencyRequirement req )
            {
                req = DependencyRequirement.Optional;

                if( param == _discoverer._tDefIOptionalService )
                {
                    req = DependencyRequirement.Optional;
                    return true;
                }
                else if( param == _discoverer._tDefIOptionalRecoService )
                {
                    req = DependencyRequirement.OptionalRecommended;
                    return true;
                }
                else if( param == _discoverer._tDefIRunnableService )
                {
                    req = DependencyRequirement.Runnable;
                    return true;
                }
                else if( param == _discoverer._tDefIRunnableRecoService )
                {
                    req = DependencyRequirement.RunnableRecommended;
                    return true;
                }
                else if( param == _discoverer._tDefIRunningService )
                {
                    req = DependencyRequirement.Running;
                    return true;
                }
                return false;
            }

            private bool ContainsYodiiReferences( Collection<ParameterDefinition> parameters )
            {
                foreach( ParameterDefinition param in parameters )
                {
                    string name = param.ParameterType.Name;
                    //Collection<GenericParameter> generics = param.Resolve().Name
                        //ParameterType.Resolve().GenericParameters;

                    if( param.ParameterType.Name == _discoverer._tDefIOptionalRecoService.Name
                    || param.ParameterType.Name == _discoverer._tDefIOptionalService.Name
                    || param.ParameterType.Name == _discoverer._tDefIRunnableRecoService.Name
                    || param.ParameterType.Name == _discoverer._tDefIRunnableService.Name
                    || param.ParameterType.Name == _discoverer._tDefIRunningService.Name ) return true;
                }
                return false;
            }      

            private bool IsYodiiDependencyService( TypeReference typeReference )
            {
                return true;
            }

            //private IEnumerable<ParameterDefinition> HasYodiiServiceReferences( TypeDefinition plugin )
            //{
            //    var query = plugin.Methods.Where( m => m.IsConstructor ).SelectMany( m => m.Parameters.Where( p => IsDependencyService( p.ParameterType ) ) );
            //    var test = plugin.Methods.Where( m => m.Parameters != null );
            //    if( query.Any() ) return query;
            //    return null;
            //}
            private bool HasYodiiServiceReferences( MethodDefinition method )
            {
                var query = method.Parameters.Select( p => IsYodiiService( p.ParameterType.Resolve() ) );
                return query.Any();
            }
    
            private bool IsDependencyService( TypeReference typeReference )
            {
                if( !typeReference.HasGenericParameters ) return false;
                return true;
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
    }
}
