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
    class ConfigurationSolver : IConfigurationSolver
    {
        readonly YodiiEngine _engine;
        readonly Dictionary<string,ServiceData> _services;
        readonly List<ServiceData.ServiceFamily> _serviceFamilies;
        readonly Dictionary<string,PluginData> _plugins;
        readonly HashSet<ServiceData> _deferedPropagation;
        readonly bool _revertServicesOrder;
        readonly bool _revertPluginsOrder;
        ServiceData[] _orderedServices;
        PluginData[] _orderedPlugins;
        int _independentPluginsCount;

        internal static Tuple<IYodiiEngineStaticOnlyResult, ConfigurationSolver> CreateAndApplyStaticResolution( YodiiEngine engine, FinalConfiguration finalConfiguration, IDiscoveredInfo discoveredInfo, bool revertServices, bool revertPlugins, bool createStaticSolvedConfigOnSuccess )
        {
            ConfigurationSolver temporarySolver = new ConfigurationSolver(  engine,revertServices, revertPlugins );
            IYodiiEngineStaticOnlyResult result =  temporarySolver.StaticResolution( finalConfiguration, discoveredInfo, createStaticSolvedConfigOnSuccess );
            // StaticResolution returns null on success.
            // If there is a result, it is either an error or createStaticSolvedConfigOnSuccess is true and this is a StaticResolutionOnly: in both 
            // case we do not need to keep the temporary solver.
            if( result != null ) temporarySolver = null;
            return Tuple.Create( result, temporarySolver );
        }

        ConfigurationSolver( YodiiEngine engine, bool revertServices = false, bool revertPlugins = false )
        {
            _engine = engine;
            _services = new Dictionary<string, ServiceData>();
            _serviceFamilies = new List<ServiceData.ServiceFamily>();
            _plugins = new Dictionary<string, PluginData>();
            _deferedPropagation = new HashSet<ServiceData>();
            _revertServicesOrder = revertServices;
            _revertPluginsOrder = revertPlugins;
        }

        public ConfigurationSolverStep Step { get; private set; }

        public ServiceData FindExistingService( string serviceFullName )
        {
            return _services[serviceFullName];
        }

        public PluginData FindExistingPlugin( string pluginFullName )
        {
            return _plugins[pluginFullName];
        }

        public ServiceData FindService( string serviceFullName )
        {
            return _services.GetValueWithDefault( serviceFullName, null );
        }

        public PluginData FindPlugin( string pluginFullName )
        {
            return _plugins.GetValueWithDefault( pluginFullName, null );
        }

        void IConfigurationSolver.DeferPropagation( ServiceData s )
        {
            _deferedPropagation.Add( s );
        }

        public IEnumerable<ServiceData> AllServices { get { return _orderedServices; } }
        
        public IEnumerable<PluginData> AllPlugins { get { return _orderedPlugins; } }

        IYodiiEngineStaticOnlyResult StaticResolution( FinalConfiguration finalConfig, IDiscoveredInfo info, bool createStaticSolvedConfigOnSuccess )
        {
            // Registering all Services.
            Step = Engine.ConfigurationSolverStep.RegisterServices;
            {
                // In order to be deterministic, works on an ordered list of IServiceInfo to build the graph.
                List<IServiceInfo> orderedServicesInfo = info.ServiceInfos.Where( s => s != null && !String.IsNullOrWhiteSpace( s.ServiceFullName ) ).OrderBy( s => s.ServiceFullName ).ToList();
                if( _revertServicesOrder ) orderedServicesInfo.Reverse();
                foreach( IServiceInfo sI in orderedServicesInfo )
                {
                    // This creates services and applies solved configuration to them: directly disabled services
                    // and specializations disabled by their generalizations' configuration are handled.
                    RegisterService( finalConfig, sI );
                }
                _orderedServices = orderedServicesInfo.Select( s => _services[s.ServiceFullName] ).ToArray();
            }

            // Service trees have been built.
            // We can now instantiate plugin data. 
            Step = Engine.ConfigurationSolverStep.RegisterPlugins;
            {
                // In order to be deterministic, works on an ordered list of IPluginInfo to build the graph.
                List<IPluginInfo> orderedPluginsInfo = info.PluginInfos.Where( p => p != null ).OrderBy( p => p.PluginFullName ).ToList();
                if( _revertPluginsOrder ) orderedPluginsInfo.Reverse();
                foreach( IPluginInfo p in orderedPluginsInfo )
                {
                    RegisterPlugin( finalConfig, p );
                }
                _orderedPlugins = orderedPluginsInfo.Select( p => _plugins[p.PluginFullName] ).ToArray();
            }
            // All possible plugins are registered. Services without any available plugins are de facto disabled.
            // Propagation for each service is deferred.
            Step = Engine.ConfigurationSolverStep.OnAllPluginsAdded;
            {
                foreach( var f in _serviceFamilies )
                {
                    f.OnAllPluginsAdded();
                }
                ProcessDeferredPropagations();
            }

            // Now, we apply ServiceReference Running constraints from every plugins to their referenced services.
            Step = ConfigurationSolverStep.PropagatePluginStatus;
            {
                foreach( PluginData p in _orderedPlugins )
                {
                    if( p.FinalConfigSolvedStatus >= ConfigurationStatus.Runnable )
                    {
                        p.PropagateSolvedStatus();
                    }
                }
                ProcessDeferredPropagations();
            }

            // Finalizes static resolution by computing final Runnable statuses per impact for Optional and Runnable plugins or services.
            Step = ConfigurationSolverStep.InitializeFinalStartableStatus;
            {
                foreach( ServiceData s in _orderedServices ) s.InitializeFinalStartableStatus();
                foreach( PluginData p in _orderedPlugins ) p.InitializeFinalStartableStatus();
            }

            List<PluginData> blockingPlugins = null;
            List<ServiceData> blockingServices = null;
            Step = ConfigurationSolverStep.BlockingDetection;
            {
                // Time to conclude about configuration and to initialize dynamic resolution.
                // Any Plugin that has a ConfigOriginalStatus greater or equal to Runnable and is Disabled leads to an impossible configuration.
                foreach( PluginData p in _orderedPlugins )
                {
                    if( p.Disabled )
                    {
                        if( p.ConfigOriginalStatus >= ConfigurationStatus.Runnable )
                        {
                            if( blockingPlugins == null ) blockingPlugins = new List<PluginData>();
                            blockingPlugins.Add( p );
                        }
                    }
                }

                // Any Service that has a ConfigOriginalStatus greater or equal to Runnable and is Disabled leads to an impossible configuration.
                foreach( ServiceData s in _orderedServices )
                {
                    if( s.Disabled )
                    {
                        if( s.ConfigOriginalStatus >= ConfigurationStatus.Runnable )
                        {
                            if( blockingServices == null ) blockingServices = new List<ServiceData>();
                            blockingServices.Add( s );
                        }
                    }
                }
            }
            if( blockingPlugins != null || blockingServices != null )
            {
                Step = ConfigurationSolverStep.StaticError;
                return new YodiiEngineResult( this, blockingPlugins, blockingServices, _engine );
            }
            Step = ConfigurationSolverStep.WaitingForDynamicResolution;
            if( createStaticSolvedConfigOnSuccess )
            {
                return new YodiiEngineResult( this, _engine );
            }
            return null;
        }

        void ProcessDeferredPropagations()
        {
            while( _deferedPropagation.Count > 0 )
            {
                ServiceData s = _deferedPropagation.First();
                _deferedPropagation.Remove( s );
                s.PropagateSolvedStatus();
            }
        }

        /// <summary>
        /// Solves undetermined status based on commands.
        /// </summary>
        /// <param name="commands"></param>
        /// <returns>This method returns a Tuple <IEnumerable<IPluginInfo>,IEnumerable<IPluginInfo>,IEnumerable<IPluginInfo>> to the host.
        /// Plugins are either disabled, stopped (but can be started) or running (locked or not).</returns>
        internal DynamicSolverResult DynamicResolution( IEnumerable<YodiiCommand> pastCommands, YodiiCommand newOne = null )
        {
            foreach( var f in _serviceFamilies ) f.DynamicResetState();
            foreach( var p in _plugins.Values ) p.DynamicResetState();
            foreach( var f in _serviceFamilies )
            {
                Debug.Assert( !f.Root.Disabled || f.Root.FindFirstPluginData( p => !p.Disabled ) == null );
                f.DynamicOnAllPluginsStateInitialized();
            }
            List<YodiiCommand> commands = new List<YodiiCommand>();
            if( newOne != null )
            {
                bool alwaysTrue = ApplyAndTellMeIfCommandMustBeKept( newOne );
                Debug.Assert( alwaysTrue, "The newly added command is necessarily okay." );
                commands.Add( newOne );
            }

            foreach( var previous in pastCommands )
            {
                if( newOne == null || newOne.ServiceFullName != previous.ServiceFullName || newOne.PluginFullName != previous.PluginFullName )
                {
                    if( ApplyAndTellMeIfCommandMustBeKept( previous ) )
                    {
                        commands.Add( previous );
                    }
                }
            }
            foreach( var f in _serviceFamilies )
            {
                Debug.Assert( !f.Root.Disabled || f.Root.FindFirstPluginData( p => !p.Disabled ) == null, "All plugins must be disabled." );
                if( !f.Root.Disabled ) f.DynamicFinalDecision( true );
            }
            foreach( var f in _serviceFamilies )
            {
                Debug.Assert( !f.Root.Disabled || f.Root.FindFirstPluginData( p => !p.Disabled ) == null, "All plugins must be disabled." );
                if( !f.Root.Disabled ) f.DynamicFinalDecision( false );
            }

            List<IPluginInfo> disabled = new List<IPluginInfo>();
            List<IPluginInfo> stopped = new List<IPluginInfo>();
            List<IPluginInfo> running = new List<IPluginInfo>();

            foreach( var p in _plugins.Values )
            {
                if( p.DynamicStatus != null )
                {
                    if( p.DynamicStatus.Value == RunningStatus.Disabled ) disabled.Add( p.PluginInfo );
                    else if( p.DynamicStatus.Value == RunningStatus.Stopped ) stopped.Add( p.PluginInfo );
                    else running.Add( p.PluginInfo );
                }
                else
                {
                    Debug.Assert( p.Service == null );
                    p.DynamicStopBy( PluginRunningStatusReason.StoppedByFinalDecision );
                    stopped.Add( p.PluginInfo );
                }
            }
            return new DynamicSolverResult( disabled.AsReadOnlyList(), stopped.AsReadOnlyList(), running.AsReadOnlyList(), commands.AsReadOnlyList() );
        }
        
        bool ApplyAndTellMeIfCommandMustBeKept( YodiiCommand cmd )
        {
            if( cmd.ServiceFullName != null )
            {
                // If the service does not exist, we keep the command.
                ServiceData s;
                if( _services.TryGetValue( cmd.ServiceFullName, out s ) )
                {
                    if ( cmd.Start ) return s.DynamicStartByCommand( cmd.Impact );
                    return s.DynamicStopByCommand();
                }
                return true;
            }
            // Starts or stops the plugin.
            // If the plugin does not exist, we keep the command.
            PluginData p;
            if( _plugins.TryGetValue(cmd.PluginFullName, out p) )
            {
                if ( cmd.Start ) return p.DynamicStartByCommand( cmd.Impact );
                else return p.DynamicStopByCommand();
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
                data = new ServiceData( this, s, serviceStatus );
                _serviceFamilies.Add( data.Family );
            }
            else
            {
                data = new ServiceData( s, dataGen, serviceStatus );
            }
            _services.Add( s.ServiceFullName, data );
            return data;
        }

        PluginData RegisterPlugin( FinalConfiguration finalConfig, IPluginInfo p )
        {
            PluginData data;
            if( _plugins.TryGetValue( p.PluginFullName, out data ) ) return data;

            ConfigurationStatus pluginStatus = finalConfig.GetStatus( p.PluginFullName );
            ServiceData service = p.Service != null ? _services[p.Service.ServiceFullName] : null;
            if( service == null ) ++_independentPluginsCount;
            data = new PluginData( this, p, service, pluginStatus );
            _plugins.Add( p.PluginFullName, data );
            return data;
        }

        internal IYodiiEngineResult CreateDynamicFailureResult( IEnumerable<Tuple<IPluginInfo, Exception>> errors )
        {
            return new YodiiEngineResult( this, errors, _engine );
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat( "{0} service families.", _serviceFamilies.Count );
            foreach( var f in _serviceFamilies )
            {
                b.AppendLine();
                f.Root.ToString( b, "   " );
            }
            b.AppendLine();
            b.AppendFormat( "{0} independent plugins.", _independentPluginsCount );
            foreach( var p in _plugins.Values.Where( p => p.Service == null ) )
            {
                b.AppendLine();
                b.Append( "   " ).Append( p.ToString() );
            }
            return b.ToString();
        }
    }
}