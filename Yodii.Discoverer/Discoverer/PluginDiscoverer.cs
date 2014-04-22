using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Discoverer
{
    public sealed class PluginDiscoverer /*: IPluginDiscoverer*/
    {
        AssemblyDefinition _assembly;
        //  DiscoveredInfo _discoveredInfo;
        List<IAssemblyInfo> _allAssemblies;

        List<PluginInfo> _foundPlugins;
        List<PluginInfo> _notFoundPlugins;
        List<PluginInfo> _oldPlugins;
        List<PluginInfo> _allPlugins;

        List<ServiceInfo> _foundServices;
        List<ServiceInfo> _notFoundServices;
        List<ServiceInfo> _oldServices;
        List<ServiceInfo> _allServices;

        public PluginDiscoverer()
        {
            _foundPlugins = new List<PluginInfo>();
            _notFoundPlugins = new List<PluginInfo>();
            _allPlugins = new List<PluginInfo>();
            _oldPlugins = new List<PluginInfo>();

            _allAssemblies = new List<IAssemblyInfo>();

            _foundServices = new List<ServiceInfo>();
            _notFoundServices = new List<ServiceInfo>();
            _allServices = new List<ServiceInfo>();
            _oldServices = new List<ServiceInfo>();
        }

        public void ReadAssembly( string path )
        {
            _assembly = AssemblyDefinition.ReadAssembly( path );
            Discover();
        }

        /// <summary>
        /// First discover classes,
        /// Then interfaces,
        /// Pass all the data to a Builder functions (PluginInfo/ServiceInfo objects + links) 
        /// And fill the DiscoveredInfo object with it
        /// </summary>
        /// <returns></returns>
        internal bool Discover()
        {
            var typeDef = _assembly.MainModule.Types;
            //Not constructor methods
            var typeMethod = typeDef.SelectMany( x => x.Methods ).Where( x => x.Name != ".ctor" );
            foreach( TypeDefinition t in _assembly.MainModule.Types )
            {
                if( t.IsClass && !t.IsAbstract )
                {
       
                    //TypeReference typeRef = t.BaseType();
                    //t.CustomAttributes[0].ConstructorArguments[0].Value;
                    //typeRef.Resolve();
                }
            }
            return true;
        }

        private IEnumerable<TypeDefinition> GetTypesToWeave()
        {
            foreach( TypeDefinition type in _assembly.MainModule.Types )
            {
                foreach( CustomAttribute attribute in type.CustomAttributes )
                {
                    if( attribute.Constructor.DeclaringType.FullName == typeof( IYodiiPlugin ).FullName ||
                        attribute.Constructor.DeclaringType.FullName == typeof( IYodiiService ).FullName )
                    {
                        yield return type;
                        break;
                    }
                }
            }
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

        //public string[] GetModule( string fileName )
        //{
        //    foreach( TypeDefinition type in _assembly.MainModule.Types )
        //    {
        //        if( type.FullName.Equals( fileName ) )
        //            return new string[] { type.BaseType.ToString(), type.Fields.ToString(), type.Interfaces.ToString() };
        //    }
           
        //}

        public int CurrentVersion { get; set; }

        public event EventHandler DiscoverBegin;

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
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyCollection<IPluginInfo> AllPlugins
        {
            get { return _allPlugins.AsReadOnlyList(); }
        }

        public IReadOnlyCollection<IPluginInfo> OldVersionnedPlugins
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyCollection<IServiceInfo> Services
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyCollection<IServiceInfo> AllServices
        {
            get { return _allServices.AsReadOnlyList(); }
        }

        public IReadOnlyCollection<IServiceInfo> NotFoundServices
        {
            get { throw new NotImplementedException(); }
        }

        public IPluginInfo FindPlugin( Guid pluginId )
        {
            throw new NotImplementedException();
        }

        public IServiceInfo FindService( string assemblyQualifiedName )
        {
            throw new NotImplementedException();
        }

        public string[] GetModule( string p )
        {
            throw new NotImplementedException();
        }
    }
}
