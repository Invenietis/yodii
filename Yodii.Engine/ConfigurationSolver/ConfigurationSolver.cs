using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;
using Yodii.Model;
using System.Collections.ObjectModel;
using Yodii.Engine.ConfigurationSolver;

namespace Yodii.Engine
{
    class ConfigurationSolver
    {
        Dictionary<IServiceInfo,ServiceData> _services;
        List<ServiceRootData> _serviceRoots;
        Dictionary<IPluginInfo,PluginData> _plugins;

        public ConfigurationSolver()
        {
            _services = new Dictionary<IServiceInfo, ServiceData>();
            _serviceRoots = new List<ServiceRootData>();
            _plugins = new Dictionary<IPluginInfo, PluginData>();
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
                // information to others to avoid loops and because such plugins reference themselves at the required service (AddMustExistReferencer).
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
                return new YodiiEngineResult( _services, _plugins, blockingPlugins, blockingServices );
            }
            return null;
        }

        public void DynamicResolution( List<YodiiCommand> commands )
        {
            foreach ( var p in _plugins.Values ) p.ResetDynamicState();
            foreach ( var s in _services.Values ) s.ResetDynamicState();

            for ( int i = 0; i < commands.Count; ++i )
            {
                if ( !ApplyAndTellMeIfCommandMustBeKept( commands[i] ) )
                {
                    Debug.Assert( i > 0 );
                    commands.RemoveAt( i-- );
                }
            }

            Debug.Assert( _plugins.Values.All( p => p.DynamicStatus.HasValue ) && _services.Values.All( s => s.DynamicStatus.HasValue ) );
        }

        public IEnumerable<Tuple<IPluginInfo,Exception>> WhatIsAHost( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> totoStart )
        {
        }

        private bool ApplyAndTellMeIfCommandMustBeKept( YodiiCommand cmd )
        {
            if ( cmd.FullName != null )
            {
                ServiceData s;
                if ( _services.TryGetValue( cmd.FullName, out s ) )
                {
                    if ( cmd.Start ) return s.Start();
                    return s.Stop();
                }
            }
            else
            {
                PluginData p;
                if ( _plugins.TryGetValue( cmd.PluginId, out p ) )
                {
                    if ( cmd.Start ) return p.Start( cmd.Impact );
                    return p.Stop();
                }
            }
            return true;
        }

        ServiceData RegisterService( FinalConfiguration finalConfig, IServiceInfo s )
        {
            ServiceData data;
            if( _services.TryGetValue( s, out data ) ) return data;

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
            _services.Add( s, data );
            return data;
        }

        PluginData RegisterPlugin( FinalConfiguration finalConfig, IPluginInfo p )
        {
            PluginData data;
            if( _plugins.TryGetValue( p, out data ) ) return data;

            //Set default status
            ConfigurationStatus pluginStatus = finalConfig.GetStatus( p.PluginId.ToString() );
            ServiceData service = p.Service != null ? _services[p.Service] : null;
            data = new PluginData( _services, p, service, pluginStatus );
            _plugins.Add( p, data );
            return data;
        }
    }
}