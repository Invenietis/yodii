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
    public sealed class PluginDiscoverer : IPluginDiscoverer
    {
        AssemblyDefinition _assembly;
        AssemblyInfo _assemblyInfo;

        //  DiscoveredInfo _discoveredInfo;
        List<AssemblyInfo> _allAssemblies;
        Collection<TypeDefinition> _allModules;
        IEnumerable<TypeDefinition> _taggedModules;
        List<PluginInfo> _foundPlugins;
        YodiiPluginCollection _plugins;
        YodiiServiceCollection _services;

        List<TypeDefinition> _pluginTypes;
        List<TypeDefinition> _serviceTypes;

        public PluginDiscoverer()
        {
            _plugins = new YodiiPluginCollection( this );
            _services = new YodiiServiceCollection( this );
            //AssemblyDefinition.ReadAssembly( Path.GetFullPath( "Yodii.Model.dll" ) );
        }

        public void ReadAssembly( string path )
        {
            _assembly = AssemblyDefinition.ReadAssembly( path );
            _assemblyInfo = new AssemblyInfo( new System.Uri( path ) );
            _allModules = _assembly.MainModule.Types;
        }

        private TypeReference ImportType<T>()
        {
            return _assembly.MainModule.Import( typeof( T ) );
        }

        public bool Discover()
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
            //Set specialization/generalization for each service families.
            foreach( TypeDefinition serviceType in _serviceTypes )
            {
                SetGeneralization( serviceType );           
                AddImplementation( serviceType );
            }

            foreach(TypeDefinition pluginType in _pluginTypes )
            {
                SetServiceReferences( pluginType );
            }

            //Handle conf

            //Not constructor methods
            IEnumerable<MethodDefinition> typeMethods = _allModules.SelectMany( x => x.Methods ).Where( x => x.Name.StartsWith( ".ctor" ) );
            //Constructor methods
            IEnumerable<MethodDefinition> typeConstructors = _allModules.SelectMany( x => x.Methods ).Where( x => x.Name.StartsWith( ".ctor" ) );
            
            return true;
        }


        public class YodiiPluginCollection 
        {
            private CKObservableSortedArrayKeyList<PluginInfo, string> _plugins;

            private PluginDiscoverer _parent;

            internal YodiiPluginCollection(PluginDiscoverer parent)
            {
                _parent = parent;
                _plugins = new CKObservableSortedArrayKeyList<PluginInfo, string>( e => e.PluginFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: false );
                //Set events here later if needed.
            }
            
            public IPluginInfo this[string key]
            {
                get { return _plugins.GetByKey( key ); }
            }

            //public CKObservableSortedArrayKeyList<PluginInfo, string> Plugins
            //{
            //    get { return _plugins; }
            //}

            internal void Add( PluginInfo pluginInfo )
            {
                _plugins.Add( pluginInfo );
            }

            internal CKObservableSortedArrayKeyList<PluginInfo, string> All
            {
                get { return _plugins; }
            }
        }

        public class YodiiServiceCollection 
        {
            private CKObservableSortedArrayKeyList<ServiceInfo, string> _services;

            private PluginDiscoverer _parent;

            internal YodiiServiceCollection( PluginDiscoverer parent )
            {
                _parent = parent;
                _services = new CKObservableSortedArrayKeyList<ServiceInfo, string>( e => e.ServiceFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: false );
                //Set events here later if needed.
            }

            public IServiceInfo this[string key]
            {
                get { return _services.GetByKey( key ); }
            }

            internal CKObservableSortedArrayKeyList<ServiceInfo, string> All
            {
                get { return _services; }
            }

            internal void Add( ServiceInfo serviceInfo )
            {
                _services.Add( serviceInfo );
            }
        }

        private void SetGeneralization( TypeDefinition serviceType )
        {
            IEnumerable<TypeReference> parent =
                    from i in serviceType.Interfaces
                    where !i.FullName.Equals( typeof( IYodiiService ).FullName ) && IsYodiiService( i.Resolve() ) // && IsTheOnlyGeneralization
                    select i;
            
            if( parent.Any() )
                _services[serviceType.Name].Generalization = _services[parent.ElementAt( 0 ).Name];
        }

        private void SetServiceReferences( TypeDefinition pluginType )
        {
            throw new NotImplementedException();
        }

        private void AddImplementation( TypeDefinition serviceType )
        {
            throw new NotImplementedException();
        }

        internal bool IsYodiiPlugin(TypeDefinition type)
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

        internal bool IsYodiiService(TypeDefinition type)
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

        private TypeReference GetReference( Type type )
        {
            TypeReference typeReference;
            _assembly.MainModule.TryGetTypeReference( type.Name, out typeReference );
            return typeReference;
        }

        //Service family specialization/generalization
        bool IsSubType(TypeDefinition type, TypeReference superType)
        {
            return
                IsType( type, superType ) ||
                ( type.BaseType != null && IsSubType( type.BaseType.Resolve(), superType ) ) ||
                ( type.Interfaces.Select( i => i.Resolve() ).Any( i => IsSubType( i, superType ) ) );
        }

        bool IsType(TypeReference a, TypeReference b)
        {
            return a.Namespace == b.Namespace && a.Name == b.Name;
        }

        private TypeReference GetReference<T>()
        {
            return GetReference( typeof( T ) );
        }

        public string CurrentAssemblyLocation
        {
            get { return _assembly == null ? String.Empty : _assembly.MainModule.FullyQualifiedName; }
        }

        private bool FileFilter( FileInfo f )
        {
            return !f.Name.EndsWith( ".resources.dll" )
                    && f.Name != "Yodii.Model.dll"
                    && f.Name != "nunit.framework.dll";
        }

        public int CurrentVersion { get; set; }

        public event EventHandler DiscoverBegin;

#region Properties

        public IReadOnlyCollection<IAssemblyInfo> AllAssemblies
        {
            get { return _allAssemblies.AsReadOnlyList(); }
        }

        public IReadOnlyCollection<IAssemblyInfo> PluginOrServiceAssemblies
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyCollection<IPluginInfo> Plugins
        {
            get { return _plugins.All.AsReadOnlyList(); }
        }

        public IReadOnlyCollection<IServiceInfo> Services
        {
            get { return _services.All.AsReadOnlyList(); }
        }
        
#endregion

        public IPluginInfo FindPlugin( string pluginFullName )
        {
            return _plugins[pluginFullName];
        }

        public IServiceInfo FindService( string serviceFullName )
        {
            return _services[serviceFullName];
        }
    }
}
