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

        //  DiscoveredInfo _discoveredInfo;
        IList<TypeDefinition> _allModules;
        CKSortedArrayKeyList<PluginInfo, string> _plugins;
        CKSortedArrayKeyList<ServiceInfo, string> _services;

        List<TypeDefinition> _pluginTypes;
        List<TypeDefinition> _serviceTypes;

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
                if( !_discoverer._assemblies.TryGetValue( fullName, out info ) )
                {
                    _discoverer.RegisterNewAssembly( a );
                }
                return a;
            }

        }

        readonly CustomAssemblyResolver _resolver;
        readonly ReaderParameters _readerParameters;

        public StandardDiscoverer( params string[] directories )
        {
            _assemblies = new Dictionary<string, CachedAssemblyInfo>();

            _resolver = new CustomAssemblyResolver( this );
            foreach( var d in directories ) _resolver.AddSearchDirectory( d );
            _readerParameters = new ReaderParameters() { AssemblyResolver = _resolver };

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
            _tDefDependencyRequirement = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( DependencyRequirement ).FullName );
        }

        public IAssemblyInfo ReadAssembly( string path )
        {
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
                }
                catch( Exception ex )
                {
                    result = new CachedAssemblyInfo( path, ex );
                    _assemblies.Add( path, result );
                }
            }
            return result.YodiiInfo;
        }

        private CachedAssemblyInfo RegisterNewAssembly( AssemblyDefinition a )
        {
            string path = a.MainModule.FullyQualifiedName;
            CachedAssemblyInfo info = new CachedAssemblyInfo( a );
            _assemblies.Add( a.FullName, info );
            _assemblies.Add( path, info );
            info.Discover( path, this );
            return info;
        }

        public IDiscoveredInfo GetDiscoveredInfo( bool withAssembliesOnError = false )
        {
            return null;
        }

        internal void SetServiceReferences( TypeDefinition pluginType )
        {
            throw new NotImplementedException();
            //foreach(MethodDefinition method in pluginType.Methods)
            //{
            //    if( method.HasCustomAttributes )
            //    {
            //        var query = method.CustomAttributes
            //            .Where(ca => ca.ConstructorArguments != null)
            //            .Select( ca => ca.ConstructorArguments );
                    
            //        if( query.Any() && method.IsGetter )
            //        {
            //            ServiceInfo service = _services.GetByKey( method.ReturnType.Name );
            //            if( service != null )
            //            {
            //                ServiceReferenceInfo serviceRefInfo =
            //                new ServiceReferenceInfo( _plugins.GetByKey( pluginType.Name ), service, (DependencyRequirement)query.ElementAt( 0 ).ElementAt( 0 ).Value );
            //                _plugins.GetByKey( pluginType.Name ).BindServiceRequirement( serviceRefInfo );
            //            }
            //        }
            //    }
            //}
            //Set Service
            //string error;
            //TypeReference target = GetService( pluginType, out error );
            //if( target != null )
            //    _plugins.GetByKey( pluginType.Name ).Service = _services.GetByKey( target.Name );
            //if( !String.IsNullOrEmpty( error ) )
            //    _plugins.GetByKey( pluginType.Name ).ErrorMessage = error;
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
    }
}
