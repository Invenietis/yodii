#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\ConfigurationSolver.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;
using Yodii.Model;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

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

        /// <summary>
        /// Finds a service by its name that must exist (otherwise an exception is thrown).
        /// </summary>
        /// <param name="serviceFullName">The service name.</param>
        /// <returns>The ServiceData.</returns>
        public ServiceData FindExistingService( string serviceFullName )
        {
            return _services[serviceFullName];
        }

        /// <summary>
        /// Finds a plugin by its name that must exist (otherwise an exception is thrown).
        /// </summary>
        /// <param name="pluginFullName">The plugin name.</param>
        /// <returns>The PluginData.</returns>
        public PluginData FindExistingPlugin( string pluginFullName )
        {
            return _plugins[pluginFullName];
        }

        /// <summary>
        /// Finds a service by its name. Returns null if it does not exist.
        /// </summary>
        /// <param name="serviceFullName">The service name.</param>
        /// <returns>Null if not found.</returns>
        public ServiceData FindService( string serviceFullName )
        {
            return _services.GetValueWithDefault( serviceFullName, null );
        }

        /// <summary>
        /// Finds a plugin by its name. Returns null if it does not exist.
        /// </summary>
        /// <param name="pluginFullName">The plugin name.</param>
        /// <returns>Null if not found.</returns>
        public PluginData FindPlugin( string pluginFullName )
        {
            return _plugins.GetValueWithDefault( pluginFullName, null );
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
                    if( p.FinalConfigSolvedStatus == SolvedConfigurationStatus.Running )
                    {
                        p.PropagateRunningStatus();
                    }
                }
                ProcessDeferredPropagations();
                DetectInvalidLoop();
            }
            // Finalizes static resolution by computing final Runnable statuses per impact for Optional and Runnable plugins or services.
            Step = ConfigurationSolverStep.InitializeFinalStartableStatus;
            {
                // Must first initialize plugins FinalStartableStatus since services use them.
                foreach( PluginData p in _orderedPlugins ) p.InitializeFinalStartableStatus();
                foreach( ServiceData s in _orderedServices ) s.InitializeFinalStartableStatus();
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

        /// <summary>
        /// Goes through the plugin list to check for non-suported, running (or runnable if the plugin must be running) reference loops.
        /// If a plugin is disabled during the process, it is repeated.
        /// </summary>
        private void DetectInvalidLoop()
        {
            bool atLeastOneFailed;
            do
            {
                atLeastOneFailed = false;
                foreach( var p in _orderedPlugins )
                {
                    if( !p.Disabled )
                    {
                        // If the plugin is running by configuration, we consider runnable references: we want runnable references to be able to start
                        // and their start must not stop this plugin whatever the configured impact is.
                        // If the plugin is only runnable, we take runnable references (and optional ones) into account depending on this configured impact.
                        var impact = p.ConfigSolvedImpact;
                        if( p.ConfigSolvedStatus == SolvedConfigurationStatus.Running ) impact |= StartDependencyImpact.IsStartRunnableOnly | StartDependencyImpact.IsStartRunnableRecommended;

                        var running = p.GetIncludedServicesClosure( impact, true );
                        if( running.Overlaps( p.GetExcludedServices( impact ) ) )
                        {
                            atLeastOneFailed = true;
                            p.SetDisabled( PluginDisabledReason.InvalidStructureLoop );
                            ProcessDeferredPropagations();
                        }
                    }
                }
            }
            while( atLeastOneFailed );
        }

        void IConfigurationSolver.DeferPropagation( ServiceData s )
        {
            Debug.Assert( Step < ConfigurationSolverStep.WaitingForDynamicResolution );
            _deferedPropagation.Add( s );
        }

        void ProcessDeferredPropagations()
        {
            Debug.Assert( Step < ConfigurationSolverStep.WaitingForDynamicResolution );
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
        /// <param name="pastCommands">Previously honored commands.</param>
        /// <param name="newOne">Optional new command to honor first.</param>
        /// <returns>This method returns a <see cref="DynamicSolverResult"/> that contains the plugins to start/stop or disable for the host.
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
                bool alwaysTrue = ApplyAndTellMeIfCommandMustBeKept( newOne, 0 );
                Debug.Assert( alwaysTrue, "The newly added command is necessarily okay." );
                commands.Add( newOne );
            }

            int iCommand = 0;
            foreach( var previous in pastCommands )
            {
                if( newOne == null || newOne.PluginFullName != previous.PluginFullName || newOne.ServiceFullName != previous.ServiceFullName )
                {
                    if( ApplyAndTellMeIfCommandMustBeKept( previous, ++iCommand ) )
                    {
                        commands.Add( previous );
                    }
                }
            }
            foreach( var f in _serviceFamilies )
            {
                Debug.Assert( !f.Root.Disabled || f.Root.FindFirstPluginData( p => !p.Disabled ) == null, "All plugins must be disabled." );
                if( !f.Root.Disabled )
                {
                    f.DynamicFinalDecision( true );
                }
            }
            foreach( var f in _serviceFamilies )
            {
                Debug.Assert( !f.Root.Disabled || f.Root.FindFirstPluginData( p => !p.Disabled ) == null, "All plugins must be disabled." );
                if( !f.Root.Disabled )
                {
                    f.DynamicFinalDecision( false );
                }
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
        
        bool ApplyAndTellMeIfCommandMustBeKept( YodiiCommand cmd, int idxCommand )
        {
            // If the plugin does not exist, we keep the command.
            bool success = true;
            if( cmd.ServiceFullName != null )
            {
                // If the service does not exist, we keep the command.
                ServiceData s;
                if( _services.TryGetValue( cmd.ServiceFullName, out s ) )
                {
                    success = cmd.Start ? s.DynamicStartByCommand( (cmd.Impact | s.ConfigSolvedImpact).ClearUselessTryBits(), idxCommand == 0 ) : s.DynamicStopByCommand();
                }
            }
            else
            {
                PluginData p;
                if( _plugins.TryGetValue(cmd.PluginFullName, out p) )
                {
                    success = cmd.Start ? p.DynamicStartByCommand( (cmd.Impact | p.ConfigSolvedImpact).ClearUselessTryBits(), idxCommand == 0 ) : p.DynamicStopByCommand();
                }
            }
            return success || idxCommand < 50;
        }

        ServiceData RegisterService( FinalConfiguration finalConfig, IServiceInfo s )
        {
            ServiceData data;
            if( _services.TryGetValue( s.ServiceFullName, out data ) ) return data;

            // Gets default status
            FinalConfigurationItem serviceConf = finalConfig.GetFinalConfiguration( s.ServiceFullName );
            // Handle generalization.
            ServiceData dataGen = null;
            if( s.Generalization != null )
            {
                dataGen = RegisterService( finalConfig, s.Generalization );
            }
            Debug.Assert( (s.Generalization == null) == (dataGen == null) );
            if( dataGen == null )
            {
                data = new ServiceData( this, s, serviceConf.Status, serviceConf.Impact );
                _serviceFamilies.Add( data.Family );
            }
            else
            {
                data = new ServiceData( s, dataGen, serviceConf.Status, serviceConf.Impact );
            }
            _services.Add( s.ServiceFullName, data );
            return data;
        }

        PluginData RegisterPlugin( FinalConfiguration finalConfig, IPluginInfo p )
        {
            PluginData data;
            if( _plugins.TryGetValue( p.PluginFullName, out data ) ) return data;

            FinalConfigurationItem pConf = finalConfig.GetFinalConfiguration( p.PluginFullName );
            ServiceData service = p.Service != null ? _services[p.Service.ServiceFullName] : null;
            if( service == null ) ++_independentPluginsCount;
            data = new PluginData( this, p, service, pConf.Status, pConf.Impact );
            _plugins.Add( p.PluginFullName, data );
            return data;
        }

        internal IYodiiEngineResult CreateDynamicFailureResult( IReadOnlyList<IPluginHostApplyCancellationInfo> errors )
        {
            return new YodiiEngineResult( this, errors, _engine );
        }

        [ExcludeFromCodeCoverage]
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