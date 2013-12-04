using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;
using Yodii.Model;
using System.Collections.ObjectModel;

namespace Yodii.Engine
{
    class ConfigurationSolver
    {
        Dictionary<string,ServiceData> _services;
        List<ServiceRootData> _serviceRoots;
        Dictionary<Guid,PluginData> _plugins;

        public ConfigurationSolver()
        {
            _services = new Dictionary<string, ServiceData>();
            _serviceRoots = new List<ServiceRootData>();
            _plugins = new Dictionary<Guid, PluginData>();
        }

        public IYodiiEngineResult StaticResolution( FinalConfiguration finalConfig, IDiscoveredInfo info )
        {
            // Registering all Services.
            _services.Clear();
            _serviceRoots.Clear();

            foreach( IServiceInfo sI in info.ServiceInfos )
            {
                // This creates services and applies solved configuration to them: directly disabled services
                // and specializations disabled by their generalizations' configuration are handled.
                RegisterService( finalConfig, sI );
            }
            // Service trees have been built and we have the roots.
            // We can now handle MustRun services: there must be at most one such service by service 
            // root otherwise it is a configuration error.
            foreach( var root in _serviceRoots )
            {
                if( !root.Disabled ) root.InitializeMustExistService();
            }
            // We can now instantiate plugin data. 
            _plugins.Clear();
            foreach( IPluginInfo p in info.PluginInfos )
            {
                RegisterPlugin( finalConfig, p );
            }
            // Initialize services disabled state based on their available plugins:
            // roots without any available plugins are de facto disabled.
            foreach( var root in _serviceRoots )
            {
                if( !root.Disabled ) root.OnAllPluginsAdded();
            }
            // Now, we apply ServiceReference MustExist constraints from every plugins to their referenced services.
            foreach( PluginData p in _plugins.Values )
            {
                // When a plugin is disabled because of a disabled required service reference and it implements a service, the service
                // becomes disabled (if it has no more available implementations) and that triggers disabling of plugins that require
                // the service. This works because disable flag on each participant is carefully set before propagating the
                // information to others to avoid loops and because such plugins reference themselves at the required service (AddRunnableReferencer).
                if( !p.Disabled && p.ConfigSolvedStatus >= SolvedConfigurationStatus.Runnable )
                {
                    p.CheckReferencesWhenMustExist();
                }
            }
            // Time to conclude about configuration and to initialize dynamic resolution.
            // Any Plugin that has a ConfigOriginalStatus greater or equal to Runnable and is Disabled leads to an impossible configuration.
            List<PluginData> blockingPlugins = null;
            List<ServiceData> blockingServices = null;

            foreach ( PluginData p in _plugins.Values )
            {
                if ( p.Disabled )
                {
                    if ( p.ConfigOriginalStatus >= ConfigurationStatus.Runnable )
                    {
                        if ( blockingPlugins == null ) blockingPlugins = new List<PluginData>();
                        blockingPlugins.Add( p );
                    }
                }
            }
            // Any Service that has a ConfigSolvedStatus greater or equal to Runnable and is Disabled leads to an impossible configuration.
            foreach ( ServiceData s in _services.Values )
            {
                if ( s.Disabled )
                {
                    if ( s.ConfigOriginalStatus >= ConfigurationStatus.Runnable )
                    {
                        if ( blockingServices == null ) blockingServices = new List<ServiceData>();
                        blockingServices.Add( s );
                    }
                }
            }
            if ( blockingPlugins != null || blockingServices != null )
            {
                return new YodiiEngineResult(_services, _plugins, blockingPlugins, blockingServices );
            }
            return new SuccessYodiiEngineResult();
        }
        /// <summary>
        /// On the first time it is called, this function will reset the dynamic state of all plugins and services. This means the RunningStatus? is set from the SolvedConfigurationStatus.
        /// RunnablePluginsCount holds the total number of plugins that are deemed to be runnable by the Static resolution. 
        /// Their RunningStatus is either null or Stopped. We then apply all existing commands except the one at the top of the list.
        /// It'll be applied last as it must always be true.
        /// 
        /// From this moment on, all plugins/services must have a running status NOT null.
        /// 
        /// Then, this same function will be called again from the engine to dynamically start/stop plugins/services. 
        /// ResetDynamicState must not be called as we must keep the dynamic state of the objects. 
        /// </summary>
        /// <param name="commands"></param>
        /// <returns>This method returns a Tuple <IEnumerable<IPluginInfo>,IEnumerable<IPluginInfo>,IEnumerable<IPluginInfo>> to the host.
        /// Plugins are either disabled, stopped (but can be started) or running (locked or not).</returns>
        public Tuple<IEnumerable<IPluginInfo>,IEnumerable<IPluginInfo>,IEnumerable<IPluginInfo>> DynamicResolution( List<YodiiCommand> commands )
        {
            foreach( var s in _services.Values ) s.ResetDynamicState();
            foreach( var p in _plugins.Values ) p.ResetDynamicState();
            
            #if DEBUG
            foreach( var s in _services.Values ) s.OnAllPluginsDynamicStateInitialized();
            #endif
                       
            for( int i = 0; i < commands.Count; ++i )
            {
                if ( !ApplyAndTellMeIfCommandMustBeKept( commands[i] ) )
                {
                    Debug.Assert( i > 0, "First command is necessarily okay." );
                    commands.RemoveAt( i-- );
                }
            }

            //This LINQ query guarantees all plugins running status are not null just before sending them to the host for injection.
            //_plugins.Values.Where( p => p.DynamicStatus.Value == null ).ToList().ForEach(pp => pp.DynamicStatus = RunningStatus.Stopped);

            Debug.Assert( _plugins.Values.All( p => p.DynamicStatus.HasValue ) && _services.Values.All( s => s.DynamicStatus.HasValue ) );
            return Tuple.Create( _plugins.Values.Where( p => p.Disabled ).Select( p => p.PluginInfo ),
                                 _plugins.Values.Where( p => p.DynamicStatus == RunningStatus.Stopped ).Select( p => p.PluginInfo ),
                                 _plugins.Values.Where( p => p.DynamicStatus == RunningStatus.Running || p.DynamicStatus == RunningStatus.RunningLocked ).Select( p => p.PluginInfo ) );
        }
        
        /// This function retrieves persistent YodiiCommands (from XML?) and adds them to the current list.
        private List<YodiiCommand> RetrievePersistentYodiiCommands()
        {
            throw new NotImplementedException();
        }


        private bool ApplyAndTellMeIfCommandMustBeKept( YodiiCommand cmd )
        {
            if ( cmd.FullName != null )
            {
                ServiceData s = _services[cmd.FullName];
                if ( s != null )
                {
                    if ( cmd.Start ) return s.StartByCommand();
                    return s.StopByCommand();
                }
                return true;
            }
            // Starts or stops the plugin.
            PluginData p = _plugins[cmd.PluginId];
            if ( p != null )
            {
                if ( cmd.Start ) return p.StartByCommand( cmd.Impact );
                else return p.StopByCommand();
            }
            return true;
        }

        ServiceData RegisterService( FinalConfiguration finalConfig, IServiceInfo s )
        {
            ServiceData data;
            if( _services.TryGetValue( s.ServiceFullName, out data ) ) return data;

            //Set default status
            ConfigurationStatus serviceStatus = finalConfig.GetStatus( s.ServiceFullName );
            // Handle generalization.
            ServiceData dataGen = null;
            if( s.Generalization != null )
            {
                dataGen = RegisterService( finalConfig, s.Generalization );
            }
            Debug.Assert( (s.Generalization == null) == (dataGen == null) );
            if( dataGen == null )
            {
                var dataRoot = new ServiceRootData( _services, s, serviceStatus, externalService => true );
                _serviceRoots.Add( dataRoot );
                data = dataRoot;
            }
            else
            {
                data = new ServiceData( _services, s, dataGen, serviceStatus, externalService => true );
            }
            _services.Add( s.ServiceFullName, data );
            return data;
        }

        PluginData RegisterPlugin( FinalConfiguration finalConfig, IPluginInfo p )
        {
            PluginData data;
            if( _plugins.TryGetValue( p.PluginId, out data ) ) return data;

            //Set default status
            ConfigurationStatus pluginStatus = finalConfig.GetStatus( p.PluginId.ToString() );
            ServiceData service = p.Service != null ? _services[p.Service.ServiceFullName] : null;
            data = new PluginData( _services, p, service, pluginStatus );
            _plugins.Add( p.PluginId, data );
            return data;
        }

        internal IYodiiEngineResult CreateDynamicFailureResult( IEnumerable<Tuple<IPluginInfo, Exception>> errors )
        {
            return new YodiiEngineResult( _services, _plugins, errors );
        }

    }
}