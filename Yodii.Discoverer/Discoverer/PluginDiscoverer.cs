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
    public sealed partial class PluginDiscoverer : IPluginDiscoverer
    {
        readonly Dictionary<string,CachedAssemblyInfo> _assemblies;
        readonly Dictionary<TypeDefinition,ServiceInfo> _allServices;
        readonly Dictionary<TypeDefinition,PluginInfo> _allPlugins;

        readonly AssemblyDefinition _yodiiModel;
        readonly TypeDefinition _tDefIYodiiService;
        readonly TypeDefinition _tDefIYodiiPlugin;
        readonly TypeDefinition _tDefIService;
        readonly TypeDefinition _tDefIRunningService;
        readonly TypeDefinition _tDefIRunnableRecoService;
        readonly TypeDefinition _tDefIRunnableService;
        readonly TypeDefinition _tDefIOptionalRecoService;
        readonly TypeDefinition _tDefIOptionalService;

        //  DiscoveredInfo _discoveredInfo;
        IList<TypeDefinition> _allModules;
        CKSortedArrayKeyList<PluginInfo, string> _plugins;
        CKSortedArrayKeyList<ServiceInfo, string> _services;

        List<TypeDefinition> _pluginTypes;
        List<TypeDefinition> _serviceTypes;

        class AssemblyResolver : DefaultAssemblyResolver
        {
            readonly PluginDiscoverer _discoverer;

            public AssemblyResolver( PluginDiscoverer d )
            {
                _discoverer = d;
            }

        }

        readonly AssemblyResolver _resolver;
        readonly ReaderParameters _readerParameters;
        readonly Func<string,string> _mapAssemblyPath;

        public PluginDiscoverer( Func<string,string> mapAssemblyPath = null )
        {
            _assemblies = new Dictionary<string, CachedAssemblyInfo>();
            _mapAssemblyPath = mapAssemblyPath;
            _resolver = new AssemblyResolver( this );

            _plugins = new CKSortedArrayKeyList<PluginInfo, string>( p => p.PluginFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: false );
            _services = new CKSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: false );
            
            _pluginTypes = new List<TypeDefinition>();
            _serviceTypes = new List<TypeDefinition>();
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
        }

        public IAssemblyInfo ReadAssembly( string path )
        {
            if( _mapAssemblyPath != null ) path = _mapAssemblyPath( path );
            if( path != null )
            {
                CachedAssemblyInfo result;
                if( !_assemblies.TryGetValue( path, out result ) )
                {
                    try
                    {
                        AssemblyDefinition.ReadAssembly( path, _readerParameters );
                    }
                    catch( Exception ex )
                    {
                        _assemblies.Add( path, new CachedAssemblyInfo( path, ex ) );
                    }
                    result = _assemblies[path];
                }
                return result.YodiiInfo;
            }
            return null;
        }

        public IDiscoveredInfo GetDiscoveredInfo( bool withAssembliesOnError = false )
        {
            return null;
        }

        bool Discover()
        {    
            foreach( TypeDefinition type in _allModules )
            {
                if( IsYodiiPlugin( type ) )
                {
                    _pluginTypes.Add( type );
                    _plugins.Add( new PluginInfo( type.Name, _assemblyInfo ) );
                }
                else if( IsYodiiService( type ) )
                {
                    _serviceTypes.Add( type );
                    _services.Add( new ServiceInfo( type.Name, _assemblyInfo ) );
                }
            }

            foreach( TypeDefinition pluginType in _pluginTypes )
            {
                SetServiceReferences( pluginType );
                SetPluginAttribute( pluginType );
            }

            foreach( TypeDefinition serviceType in _serviceTypes )
            {
                SetGeneralization( serviceType );           
            }

            return true;
        }

        internal void SetGeneralization( TypeDefinition serviceType )
        {
            IEnumerable<TypeReference> parent =
                    from i in serviceType.Interfaces
                    where IsYodiiService( i.Resolve() )
                    select i;
            
            if( parent.Any() )
                _services.GetByKey( serviceType.Name ).Generalization = _services.GetByKey( parent.ElementAt( 0 ).Name );
        }

        internal void SetServiceReferences( TypeDefinition pluginType )
        {
            var properties = pluginType.Properties
                                .Where( p => p.PropertyType.Resolve().Interfaces.Any( r => r.Resolve() == _tDefIYodiiService ))
                                .Where( p => p.CustomAttributes.Where( a => a.AttributeType.FullName == typeof(DependencyRequirementAttribute).FullName
                                                                            && a.ConstructorArguments.Count == 1 
                                                                            && a.ConstructorArguments[0].Type.FullName == typeof(DependencyRequirement).FullName )
                                                                .Select( a => (DependencyRequirement)a.ConstructorArguments[0].Value );


            //Retrieves the DependencyRequirement value of a service reference.
            foreach(MethodDefinition method in pluginType.Methods)
            {
                if( method.HasCustomAttributes )
                {
                    var query = method.CustomAttributes
                        .Where(ca => ca.ConstructorArguments != null)
                        .Select( ca => ca.ConstructorArguments );
                    
                    if( query.Any() && method.IsGetter )
                    {
                        ServiceInfo service = _services.GetByKey( method.ReturnType.Name );
                        if( service != null )
                        {
                            ServiceReferenceInfo serviceRefInfo =
                            new ServiceReferenceInfo( _plugins.GetByKey( pluginType.Name ), service, (DependencyRequirement)query.ElementAt( 0 ).ElementAt( 0 ).Value );
                            _plugins.GetByKey( pluginType.Name ).BindServiceRequirement( serviceRefInfo );
                        }
                    }
                }
            }
            //Set Service
            string error;
            TypeReference target = GetService( pluginType, out error );
            if( target != null )
                _plugins.GetByKey( pluginType.Name ).Service = _services.GetByKey( target.Name );
            if( !String.IsNullOrEmpty( error ) )
                _plugins.GetByKey( pluginType.Name ).ErrorMessage = error;
        }

        internal void SetPluginAttribute( TypeDefinition pluginType )
        {
            //CustomAttribute attr = pluginType.Methods[0].DeclaringType.CustomAttributes[0];
            //PluginAttribute Constructor argument : Guid.ToString()
            //attr.ConstructorArguments[0].Value

            //Field attribute (Public Name, Version, Description, etc.)
            //attr.Properties[0].Name

            //Value attribute
            //attr.Properties[0].Argument.Value
            //_plugins.GetByKey( pluginType.Name ).Id = attr.ConstructorArguments//Set ID, PublicName, Description, Version?
        }

        internal TypeReference GetService( TypeDefinition pluginType, out string error )
        {
            error = "";
            IEnumerable<TypeReference> query = from TypeReference i in pluginType.Interfaces
                                               where IsYodiiService( i.Resolve() )
                                               select i;
            if( query.Any() )
            {
                if( query.Count() > 1 )
                    error = "A plugin cannot have more than 2 services";
                return query.ElementAt( 0 );
            }
            return null;                
        }

        internal bool IsYodiiPlugin( TypeDefinition type )
        {
            if( type.IsClass && !type.IsAbstract )
            {
                IEnumerable<TypeReference> target =
                    from i in type.Interfaces
                    where i.FullName.Equals( typeof( IYodiiPlugin ).FullName )
                    select i;
                if( target.Any() ) return true;
            }
            return false;
        }

        internal bool IsYodiiService( TypeDefinition type )
        {
            if( type.IsInterface )
            {
                IEnumerable<TypeReference> target =
                    from i in type.Interfaces
                    where i.FullName.Equals( typeof( IYodiiService ).FullName )
                    select i;
                if( target.Any() ) return true;
            }
            return false;
        }

        //Helper methods
        internal TypeReference GetReference<T>()
        {
            return GetReference( typeof( T ) );
        }

        internal TypeReference GetReference( Type type )
        {
            TypeReference typeReference;
            _assembly.MainModule.TryGetTypeReference( type.Name, out typeReference );
            return typeReference;
        }

        internal TypeReference ImportType<T>()
        {
            return _assembly.MainModule.Import( typeof( T ) );
        }

        internal bool IsType( TypeReference a, TypeReference b )
        {
            return a.Namespace == b.Namespace && a.Name == b.Name;
        }

        internal bool IsSubType( TypeDefinition type, TypeReference superType )
        {
            return
                IsType( type, superType ) ||
                ( type.BaseType != null && IsSubType( type.BaseType.Resolve(), superType ) ) ||
                ( type.Interfaces.Select( i => i.Resolve() ).Any( i => IsSubType( i, superType ) ) );
        }

        public string CurrentAssemblyLocation
        {
            get { return _assembly == null ? String.Empty : _assembly.MainModule.FullyQualifiedName; }
        }

        public int CurrentVersion { get; set; }

        public event EventHandler DiscoverBegin;

#region Properties

        public IReadOnlyCollection<IAssemblyInfo> PluginOrServiceAssemblies
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyCollection<IPluginInfo> Plugins
        {
            get { return _plugins.AsReadOnlyList(); }
        }

        public IReadOnlyCollection<IServiceInfo> Services
        {
            get { return _services.AsReadOnlyList(); }
        }
        
#endregion

        public IPluginInfo FindPlugin( string pluginFullName )
        {
            return _plugins.GetByKey( pluginFullName );
        }

        public IServiceInfo FindService( string serviceFullName )
        {
            return _services.GetByKey( serviceFullName );
        }
    }
}
