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
        readonly Dictionary<string, CachedAssemblyInfo> _assemblies;
        readonly Dictionary<TypeDefinition, ServiceInfo> _services;
        readonly Dictionary<TypeDefinition, PluginInfo> _plugins;
        
        readonly AssemblyDefinition _yodiiModel;
        readonly TypeDefinition _tDefIYodiiService;
        readonly TypeDefinition _tDefIYodiiPlugin;
        readonly TypeDefinition _tDefIService;
        readonly TypeDefinition _tDefIRunningService;
        readonly TypeDefinition _tDefIRunnableRecoService;
        readonly TypeDefinition _tDefIRunnableService;
        readonly TypeDefinition _tDefIOptionalRecoService;
        readonly TypeDefinition _tDefIOptionalService;
        readonly TypeDefinition _tDefDependencyRequirement;

        class CustomAssemblyResolver : DefaultAssemblyResolver
        {
            readonly StandardDiscoverer _discoverer;

            public CustomAssemblyResolver( StandardDiscoverer d )
            {
                _discoverer = d;
            }
             
            public override AssemblyDefinition Resolve( string fullName )
            {
                AssemblyDefinition a = base.Resolve( fullName );
                CachedAssemblyInfo info;
                if( !_discoverer._assemblies.TryGetValue( fullName /*a.FullName*/, out info ) )
                {
                    _discoverer.RegisterNewAssembly( a );
                }
                return a;
            }
            public override AssemblyDefinition Resolve( AssemblyNameReference name )
            {
                AssemblyDefinition assembly;
                assembly = base.Resolve( name );
                if( assembly.FullName.Equals( _discoverer._yodiiModel.FullName ) )
                {
                    return _discoverer._yodiiModel;
                }
                CachedAssemblyInfo info;
                if( !_discoverer._assemblies.TryGetValue( name.FullName /*assembly.FullName*/, out info ) )
                {
                    _discoverer.RegisterNewAssembly( assembly );
                }   
                //info.YodiiInfo.
                return assembly;
            }

        }

        readonly CustomAssemblyResolver _resolver;
        readonly ReaderParameters _readerParameters;

        public StandardDiscoverer( params string[] directories )
        {
            _assemblies = new Dictionary<string, CachedAssemblyInfo>();
            _services = new Dictionary<TypeDefinition, ServiceInfo>();
            _plugins = new Dictionary<TypeDefinition, PluginInfo>();

            _resolver = new CustomAssemblyResolver( this );
            foreach( var d in directories ) _resolver.AddSearchDirectory( d );
            //IAssemblyResolver restest = _resolver;//TESTE
            _readerParameters = new ReaderParameters/*()*/ { AssemblyResolver = _resolver };
            
            var pathYodiiModel = new Uri( typeof( IYodiiService ).Assembly.CodeBase ).LocalPath;
            _yodiiModel = AssemblyDefinition.ReadAssembly( pathYodiiModel );

            _tDefIYodiiService = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IYodiiService ).FullName );
            _tDefIYodiiPlugin = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IYodiiPlugin ).FullName );

            _tDefIService = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IService<> ).FullName );
            _tDefIRunningService = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IRunningService<> ).FullName );
            _tDefIRunnableRecoService = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IRunnableRecommendedService<> ).FullName );
            _tDefIRunnableService = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IRunnableService<> ).FullName );
            _tDefIOptionalRecoService = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IOptionalRecommendedService<> ).FullName );
            _tDefIOptionalService = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IOptionalService<> ).FullName );
            _tDefDependencyRequirement = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( DependencyRequirement ).FullName );
        }

        public IAssemblyInfo ReadAssembly( string path )
        {
            _pathTest = path;
            if( String.IsNullOrEmpty( path ) ) throw new ArgumentNullException( "path" );
            CachedAssemblyInfo result;
            if( !_assemblies.TryGetValue( path, out result ) )
            {
                try
                {
                    AssemblyDefinition a = AssemblyDefinition.ReadAssembly( path, _readerParameters );
                    if( !_assemblies.TryGetValue( a.FullName, out result ) )
                    {
                        result = RegisterNewAssembly( a );
                    }
                                Debug.Assert( result.YodiiInfo != null, "result.YodiiInfo is null" );
                }
                catch( Exception ex )
                {
                    result = new CachedAssemblyInfo( path, ex );
                    CachedAssemblyInfo info;
                    if(_assemblies.TryGetValue(path, out info))
                    {
                        _assemblies.Remove( path );
                    }
                    _assemblies.Add( path, result );
                }
            }

            return result.YodiiInfo;
        }

        //private CachedAssemblyInfo RegisterNewAssembly( AssemblyDefinition a, string path )
        //{
        //    //string path = a.MainModule.FullyQualifiedName;
        //    CachedAssemblyInfo info = new CachedAssemblyInfo( a );
        //    _assemblies.Add( a.FullName, info );
        //    _assemblies.Add( path, info );
        //    info.Discover( path, this );
        //    return info;
        //}
        string _pathTest;
        private CachedAssemblyInfo RegisterNewAssembly( AssemblyDefinition a )
        {
            //string path = a.MainModule.FullyQualifiedName;
            string path = _pathTest;
            CachedAssemblyInfo info = new CachedAssemblyInfo( a );
            Debug.Assert( info != null, "info is null" );
            _assemblies.Add( a.FullName, info );
            _assemblies.Add( path, info ); // /!\
           // _assemblies.TryGetValue(
            info.Discover( path, this );
            return info;
        }
        public IDiscoveredInfo GetDiscoveredInfo( bool withAssembliesOnError = false )
        {
            List<IAssemblyInfo> assemblyInfos = new List<IAssemblyInfo>();
            foreach( CachedAssemblyInfo info in _assemblies.Values )
            {
                if( !withAssembliesOnError && info.Error != null ) continue;
                Debug.Assert( info.YodiiInfo != null, "YodiiInfo of the CachedAssembly info in the StandardDiscoverer's _assembly is null" );
                if( assemblyInfos.Contains( info.YodiiInfo ) ) continue;
                assemblyInfos.Add( info.YodiiInfo );
            }
            return new DiscoveredInfo( assemblyInfos.ToReadOnlyList() );
        }

        ServiceInfo FindOrCreateService( TypeDefinition t )
        {
            ServiceInfo s;
            if( _services.TryGetValue( t, out s ) ) return s;
            //_services.Add( t, new ServiceInfo( t.FullName, new AssemblyInfo( new Uri( t.Module.FullyQualifiedName.ToString() ) ) ) );
            _services.Add( t, new ServiceInfo( t.FullName, new AssemblyInfo( new Uri( _pathTest ) ) ) );
           
            IEnumerable<TypeReference> parent =
                            from i in t.Interfaces
                            where IsYodiiService( i.Resolve() )
                            select i;

            if( parent.Any() )
                _services[t].Generalization = FindOrCreateService( parent.First().Resolve() );

            return _services[t];         
        }

        PluginInfo FindOrCreatePlugin( TypeDefinition t )
        {
            PluginInfo tryP;
            if( _plugins.TryGetValue( t, out tryP ) ) return tryP;
            //PluginInfo p = new PluginInfo( t.FullName, new AssemblyInfo( new Uri( t.Module.FullyQualifiedName.ToString() ) ) );
            PluginInfo p = new PluginInfo( t.FullName, new AssemblyInfo( new Uri( _pathTest) ) );
            _plugins.Add( t, p );

            ServiceInfo service = null;
            TypeDefinition serviceType = GetService( t );
            if( serviceType != null )
            {
                service = FindOrCreateService( serviceType );
                if( service != null )
                    _plugins[t].Service = service;
            }      

            var ctors = t.Methods.Where( m => m.IsConstructor );
            var longerCtor = ctors.OrderBy( c => c.Parameters.Count ).LastOrDefault();
            if( longerCtor != null )
            {
                foreach( ParameterDefinition param in longerCtor.Parameters )
                {
                    if( param.ParameterType.Namespace.StartsWith( "System" ) ) continue;
                    var paramType = param.ParameterType.Resolve();
                    if( !paramType.IsInterface ) continue;
                    if( paramType.HasGenericParameters )
                    {
                        if( paramType.GenericParameters.Count > 1 ) continue;
                        //ServiceInfo sRef = FindOrCreateService( ( (GenericInstanceType)param.ParameterType ).GenericParameters[0].Resolve() );
                        
                        TypeReference refWrapped = ( (GenericInstanceType)param.ParameterType ).GenericArguments.FirstOrDefault();
                        if( refWrapped == null ) continue;
                        TypeDefinition wrappedService = refWrapped.Resolve();
                        TypeDefinition wrappedService2 = paramType.GenericParameters[0].DeclaringType.Resolve();
                        if( !IsYodiiService( wrappedService ) ) continue;
                        //TypeDefinition genericForReq = paramType.GenericParameters[0].DeclaringType;
                        //TypeDefinition wrapper = wrappedService2.DeclaringType;
                        //TypeDefinition wrapper = wrappedService;
                        DependencyRequirement req;
                        if( !IsDependencyRequirement( wrappedService2, out req ) ) continue;
                                    
                        ServiceInfo sRef = FindOrCreateService( wrappedService );
                        ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( p, sRef, req, param.Name, param.Index, false );
                    }
                    else
                    {
                        if( !IsYodiiService( paramType ) ) continue;
                        ServiceInfo sRef = FindOrCreateService( paramType );
                        ServiceReferenceInfo serviceRef = new ServiceReferenceInfo( p, sRef, DependencyRequirement.Running, param.Name, param.Index, true );
                    }
                }
            }
            return p;
        }

    

        //TypeReference GetService( TypeDefinition plugin )
        //{
        //    IEnumerable<TypeReference> query = from TypeReference i in plugin.Interfaces
        //                                       where IsYodiiService( i.Resolve() )
        //                                       select i;
        //    if( query.Any() )
        //        return query.ElementAt( 0 );
        //    return null;
        //}

        TypeDefinition GetService( TypeDefinition plugin )
        {
            /*IEnumerable<TypeReference> query = from TypeReference i in plugin.Interfaces
                                               where IsYodiiService( i.Resolve() ) && i.FullName != _tDefIYodiiService.FullName
                                               select i;*/
            IEnumerable<TypeReference> query = from TypeReference i in plugin.Interfaces
                                               where IsYodiiService( i.Resolve() ) 
                                               select i;
            if( query.Any() )
                return query.ElementAt( 0 ).Resolve();
            //ElementAt( 0 ).Resolve();
            return null;
        }

        /// <summary>
        /// Checks whether the Interface implements IYodiiService.
        /// If the interface itself is IYodiiService, returns false.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsYodiiService( TypeDefinition type )
        {
            if( type.IsInterface )
            {
                IEnumerable<TypeReference> target =
                    from i in type.Interfaces
                    where i.Resolve().Equals( _tDefIYodiiService )
                    select i;
                if( target.Any() ) return true;
            }
            return false;
        }

        bool IsYodiiPlugin( TypeDefinition type )
        {
            if( type.IsClass && !type.IsAbstract )
            {
                IEnumerable<TypeReference> target =
                    from i in type.Interfaces
                    where i.Resolve().Equals( _tDefIYodiiPlugin)
                    select i;
                if( target.Any() ) return true;
            }
            return false;
        }

        private bool IsDependencyRequirement( TypeDefinition wrapper, out DependencyRequirement req )
        {
            req = DependencyRequirement.Optional;

            if( wrapper == _tDefIOptionalService ) return true;

            if( wrapper == _tDefIOptionalRecoService )
            {
                req = DependencyRequirement.OptionalRecommended;
                return true;
            }
            if( wrapper == _tDefIRunnableService )
            {
                req = DependencyRequirement.Runnable;
                return true;
            }
            if( wrapper == _tDefIRunnableRecoService )
            {
                req = DependencyRequirement.RunnableRecommended;
                return true;
            }
            if( wrapper == _tDefIRunningService )
            {
                req = DependencyRequirement.Running;
                return true;
            }
            return false;
        }
    }
}
