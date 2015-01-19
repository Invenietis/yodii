#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Discoverer\Discoverer\StandardDiscoverer.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
        readonly TypeDefinition _tDefIYodiiEngine;
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

            public AssemblyDefinition ReadAssembly( string path )
            {
                CachedAssemblyInfo cached;
                if( !_discoverer._assemblies.TryGetValue( path, out cached ) )
                {
                    AssemblyDefinition a = AssemblyDefinition.ReadAssembly( path, _discoverer._readerParameters );
                    RegisterAssembly( a );
                    if( !_discoverer._assemblies.TryGetValue( a.FullName, out cached ) )
                    {
                        cached = _discoverer.RegisterNewAssembly( a );
                        _discoverer._assemblies.Add( path, cached );
                    }
                }
                return cached.CecilInfo;
            }
             
            public override AssemblyDefinition Resolve( AssemblyNameReference name )
            {
                AssemblyDefinition assembly = base.Resolve( name );
                CachedAssemblyInfo info;
                if( !_discoverer._assemblies.TryGetValue( assembly.FullName, out info ) )
                {
                    _discoverer.RegisterNewAssembly( assembly );
                }   
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
            _readerParameters = new ReaderParameters() { AssemblyResolver = _resolver };
            
            var pathYodiiModel = new Uri( typeof( IYodiiService ).Assembly.CodeBase ).LocalPath;
            _yodiiModel = _resolver.ReadAssembly( pathYodiiModel );

            _tDefIYodiiEngine = _yodiiModel.MainModule.Types.First( t => t.FullName == typeof( IYodiiEngineBase ).FullName );
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
            _resolver.ReadAssembly( path );
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
                    result = new CachedAssemblyInfo( ex );
                    _assemblies[path] = result;
                }
            }
            Debug.Assert( result.YodiiInfo != null, "result.YodiiInfo is NEVER null." );
            return result.YodiiInfo;
        }

        private CachedAssemblyInfo RegisterNewAssembly( AssemblyDefinition a )
        {
            Debug.Assert( a != null );
            CachedAssemblyInfo info = new CachedAssemblyInfo( a );
            _assemblies.Add( a.FullName, info );
            if( _yodiiModel != null ) info.Discover( this );
            else
            {
                foreach( var dependency in a.MainModule.AssemblyReferences )
                {
                    _resolver.Resolve( dependency );
                }
                info.YodiiInfo.SetResult( CKReadOnlyListEmpty<ServiceInfo>.Empty, CKReadOnlyListEmpty<PluginInfo>.Empty );
            }
            return info;
        }

        public IDiscoveredInfo GetDiscoveredInfo( bool withAssembliesOnError = false )
        {
            List<IAssemblyInfo> assemblyInfos = new List<IAssemblyInfo>();
            foreach( CachedAssemblyInfo info in _assemblies.Values )
            {
                if( !withAssembliesOnError && info.Error != null ) continue;
                if( assemblyInfos.Contains( info.YodiiInfo ) ) continue;
                assemblyInfos.Add( info.YodiiInfo );
            }
            return new DiscoveredInfo( assemblyInfos.ToReadOnlyList() );
        }

        ServiceInfo FindOrCreateService( TypeDefinition t )
        {
            ServiceInfo s;
            if( _services.TryGetValue( t, out s ) ) return s;

            s = new ServiceInfo( t.FullName, _assemblies[t.Module.Assembly.FullName].YodiiInfo );
            _services.Add( t, s );
            ServiceInfo gen = t.Interfaces
                                    .Select( i => i.Resolve() )
                                    .Where( i => IsYodiiService( i ) )
                                    .Select( i => FindOrCreateService( i ) )
                                    .OrderByDescending( super => super.Depth )
                                    .FirstOrDefault();
            if( gen != null ) s.Depth = gen.Depth + 1;      
            s.Generalization = gen;
            return s;         
        }

        PluginInfo FindOrCreatePlugin( TypeDefinition t )
        {
            PluginInfo p;
            if( _plugins.TryGetValue( t, out p ) ) return p;

            IAssemblyInfo assembly = _assemblies[t.Module.Assembly.FullName].YodiiInfo;
            try
            {
                ServiceInfo implService = GetDirectImplementedService( t );
                int paramCount = 0;
                List<PluginInfoKnownParameter> knownParameters = null;
                List<ServiceReferenceInfo> services = null;

                var ctors = t.Methods.Where( m => m.IsConstructor );
                var longerCtor = ctors.OrderBy( c => c.Parameters.Count ).LastOrDefault();
                if( longerCtor != null )
                {
                    var parameters = longerCtor.Parameters;
                    paramCount = parameters.Count;
                    foreach( ParameterDefinition param in longerCtor.Parameters )
                    {
                        var paramType = param.ParameterType.Resolve();
                        if( !paramType.IsInterface ) continue;

                        PluginInfoKnownParameter knownParameter = null;
                        ServiceReferenceInfo serviceRef = null;
                        if( paramType.HasGenericParameters )
                        {
                            if( paramType.GenericParameters.Count > 1 ) continue;

                            TypeReference actualServiceref = ((GenericInstanceType)param.ParameterType).GenericArguments[0];
                            if( actualServiceref == null ) continue;
                            TypeDefinition actualService = actualServiceref.Resolve();
                            if( !IsYodiiService( actualService ) ) continue;
                            TypeDefinition wrapper = paramType.GenericParameters[0].DeclaringType.Resolve();
                            DependencyRequirement req;
                            if( !IsDependencyRequirement( wrapper, out req ) ) continue;

                            ServiceInfo sRef = FindOrCreateService( actualService );
                            serviceRef = new ServiceReferenceInfo( sRef, req, param.Name, param.Index, false );
                        }
                        else
                        {
                            if( IsYodiiService( paramType ) )
                            {
                                ServiceInfo sRef = FindOrCreateService( paramType );
                                serviceRef = new ServiceReferenceInfo( sRef, DependencyRequirement.Running, param.Name, param.Index, true );
                            }
                            else
                            {
                                if( paramType.Resolve().Equals( _tDefIYodiiEngine ) )
                                {
                                    knownParameter = new PluginInfoKnownParameter( param.Name, param.Index, "IYodiiEngine" );
                                }
                                else if( paramType.FullName == "CK.Core.IActivityMonitor" )
                                {
                                    knownParameter = new PluginInfoKnownParameter( param.Name, param.Index, "IActivityMonitor" );
                                }
                            }
                        }
                        if( serviceRef != null )
                        {
                            if( services == null ) services = new List<ServiceReferenceInfo>();
                            services.Add( serviceRef );
                        }
                        if( knownParameter != null )
                        {
                            if( knownParameters == null ) knownParameters = new List<PluginInfoKnownParameter>();
                            knownParameters.Add( knownParameter );
                        }
                    }
                }
                p = new PluginInfo( t.FullName, assembly, implService, services, paramCount, knownParameters );
            }
            catch( Exception ex )
            {
                p = new PluginInfo( t.FullName, assembly, ex.Message );
            }
            _plugins.Add( t, p );
            return p;
        }

        ServiceInfo GetDirectImplementedService( TypeDefinition pluginType )
        {
            return pluginType.Interfaces
                            .Select( i => i.Resolve() )
                            .Where( i => IsYodiiService( i ) )
                            .Select( i => FindOrCreateService( i ) )
                            .FirstOrDefault();
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
                return type.Interfaces.Any( i => i.Resolve().Equals( _tDefIYodiiService ) );
            }
            return false;
        }

        bool IsYodiiPlugin( TypeDefinition type )
        {
            if( type.IsClass && !type.IsAbstract )
            {
                return HasIYodiiPluginInterface(type) || BaseIsYodiiPlugin( type );
            }
            return false;
        }

        bool BaseIsYodiiPlugin( TypeDefinition type )
        {
            Debug.Assert( type != null );
            var baseType = type.BaseType;

            if( baseType == null ) return false;
            var resolvedBaseType = baseType.Resolve();
            return HasIYodiiPluginInterface( resolvedBaseType ) || BaseIsYodiiPlugin( resolvedBaseType );
        }

        bool HasIYodiiPluginInterface( TypeDefinition type )
        {
            return type.Interfaces.Any( i => i.Resolve().Equals( _tDefIYodiiPlugin ) );
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

        private IAssemblyInfo currentIfNotYetLoaded { get; set; }
    }
}
