#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\YodiiEngine.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;
using CK.Core;
using System.Collections.ObjectModel;

namespace Yodii.Engine
{
    /// <summary>
    /// Yodii engine implementation.
    /// </summary>
    public class YodiiEngine : IYodiiEngineExternal
    {
        readonly ConfigurationManager _manager;
        readonly IYodiiEngineHost _host;
        readonly LiveInfo _liveInfo;
        readonly YodiiCommandList _yodiiCommands;
        readonly SuccessYodiiEngineResult _successResult;
        readonly Queue<Action<IYodiiEngineExternal>> _postActions;

        ConfigurationSolver _currentSolver;
        bool _isExternalWorking;
        bool _isInternalWorking;
        bool _isStopping;

        /// <summary>
        /// Raised whenever <see cref="IsRunning"/> or <see cref="IsStopping"/> changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new <see cref="YodiiEngine"/>.
        /// </summary>
        /// <param name="host">The host to use.</param>
        public YodiiEngine( IYodiiEngineHost host )
        {
            if( host == null ) throw new ArgumentNullException( "host" );
            _successResult = new SuccessYodiiEngineResult( this );
            _host = host;
            host.Engine = this;
            _manager = new ConfigurationManager( this );
            _yodiiCommands = new YodiiCommandList();
            _liveInfo = new LiveInfo( this );
            _postActions = new Queue<Action<IYodiiEngineExternal>>();
        }

        /// <summary>
        /// Gets a prebuilt immutable succesful result.
        /// </summary>
        internal SuccessYodiiEngineResult SuccessResult
        {
            get { return _successResult; }
        }

        internal Tuple<IYodiiEngineStaticOnlyResult, ConfigurationSolver> StaticResolutionByConfigurationManager( IDiscoveredInfo info, FinalConfiguration finalConfiguration )
        {
            Debug.Assert( IsRunning );
            return ConfigurationSolver.CreateAndApplyStaticResolution( this, finalConfiguration, info, false, false, false );
        }

        internal IYodiiEngineResult DoDynamicResolution( ConfigurationSolver solver, Func<InternalYodiiCommand, bool> existingCommandFilter, InternalYodiiCommand cmd, Action onPreSuccess = null )
        {
            if( _isInternalWorking ) throw new InvalidOperationException( "Reentrancy detected: Engine is currently working." );
            
            bool isRootStart = true;
            if( _isExternalWorking ) isRootStart = false;
            else IsWorking = true;

            // DynamicResolution MUST NOT fail: let it outside the try.
            var dynResult = solver.DynamicResolution( _yodiiCommands, existingCommandFilter, cmd );
            _isInternalWorking = true;
            try
            {
                // 1 - Calling host.
                //     During this call, _isInternalWorking is set to true: we detect (Pre)Stop/Start attempts to 
                //     reenter the engine (configuration changes, starting/stopping a plugin or a service).
                int currentPostActions = _postActions.Count;
                Action<Action<IYodiiEngineExternal>> postActionCollector = null;
                if( !_isStopping ) postActionCollector = _postActions.Enqueue;
                IYodiiEngineHostApplyResult hostResult = _host.Apply( dynResult.SolvedConfiguration, postActionCollector );
                _isInternalWorking = false;
                
                // 2 - Handling host.Apply errors.
                Debug.Assert( hostResult != null && hostResult.CancellationInfo != null );
                if( hostResult.CancellationInfo.Any() )
                {
                    IYodiiEngineResult result =  solver.CreateDynamicFailureResult( hostResult.CancellationInfo );
                    _liveInfo.UpdateRuntimeErrors( hostResult.CancellationInfo, solver.FindExistingPlugin );
                    if( _currentSolver != solver ) _yodiiCommands.ClearBindings();
                    if( _postActions.Count > currentPostActions )
                    {
                        if( currentPostActions == 0 ) _postActions.Clear();
                        else
                        {
                            var initials = _postActions.Take( currentPostActions ).ToList();
                            _postActions.Clear();
                            foreach( var a in initials ) _postActions.Enqueue( a );
                        }
                    }
                    if( isRootStart )
                    {
                        IsWorking = false;
                    }
                    return result;
                }
                // 3 - host.Apply succeed:
                //     - Call the onPreSuccess delegate: this is used to set a new DiscoveredInfo if needed.
                //       Since we update the Engine's field here, we keep the last one: the last PostStartAction that 
                //       changes the Discoverer wins.
                //     - Detect that we are starting to set IsRunning to true.
                //     - Set the currentSolver: same as before, the last wins.
                //     - The YodiiCommand are incrementally merged: we don't (and can't) wait to be at the root start to update them. 
                //     - LiveInfo is updated on each call in order to expose up-to-date informations to post actions.
                if( onPreSuccess != null ) onPreSuccess();
                bool wasStopped = _currentSolver == null;
                Debug.Assert( !wasStopped || isRootStart, "wasStopped => isRootStart" );
                if( _currentSolver != solver ) _currentSolver = solver;
                _yodiiCommands.Merge( dynResult.Commands );
                _liveInfo.UpdateFrom( _currentSolver );

                // On root start, we dump the post actions as long as there are some.
                // Before 
                if( isRootStart )
                {
                    while( _postActions.Count > 0 )
                    {
                        _postActions.Dequeue()( this );
                    }
                    IsWorking = false;
                    if( wasStopped ) RaisePropertyChanged( "IsRunning" );
                }
                return _successResult;
            }
            catch
            {
                _isInternalWorking = false;
                if( isRootStart )
                {
                    _liveInfo.Clear();
                    _postActions.Clear();
                    _currentSolver = null;
                    IsWorking = false;
                    RaisePropertyChanged( "IsRunning" );
                }
                throw;
            }
        }

        /// <summary>
        /// Gets the <see cref="IConfigurationManager"/>.
        /// </summary>
        public IConfigurationManager Configuration
        {
            get { return _manager; }
        }

        /// <summary>
        /// Gets whether this engine is currently running.
        /// </summary>
        public bool IsRunning
        {
            get { return _currentSolver != null; }
        }

        /// <summary>
        /// Gets whether this engine is currently stopping.
        /// </summary>
        public bool IsStopping
        {
            get { return _isStopping; }
            private set
            {
                if( _isStopping != value )
                {
                    _isStopping = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets whether this engine is currently working: it is currently applying a configuration change, processing 
        /// a dynamic command or stopping.
        /// When true, this engine can not accept any subsequent commands or configuration changes.
        /// </summary>
        public bool IsWorking
        {
            get { return _isExternalWorking; }
            private set
            {
                if( _isExternalWorking != value )
                {
                    _isExternalWorking = value;
                    RaisePropertyChanged();
                }
            }
        }

        internal bool IsInternalWorking
        {
            get { return _isInternalWorking; }
        }

        /// <summary>
        /// Stops the engine: stops all plugins and stops monitoring configuration.
        /// </summary>
        public void StopEngine()
        {
            if( IsRunning )
            {
                IsStopping = true;
                IsWorking = true;
                try
                {
                    // Stopping the engine disables all plugins.
                    // No post action here: this is the hint for the host that we are stopping.
                    _isInternalWorking = true;
                    _host.Apply( _manager.DiscoveredInfo.PluginInfos.Select( p => new KeyValuePair<IPluginInfo,RunningStatus>( p, RunningStatus.Disabled ) ).ToReadOnlyList(), null );
                }
                finally
                {
                    _isInternalWorking = false;
                    _liveInfo.Clear();
                    _currentSolver = null;
                    IsWorking = false;
                    IsStopping = false;
                }
                RaisePropertyChanged( "IsRunning" );
            }
        }


        internal IYodiiEngineResult OnConfigurationChanging( ConfigurationSolver temporarySolver, Action onPreSuccess )
        {
            Debug.Assert( IsRunning, "Cannot call this function when the engine is not running" );
            return DoDynamicResolution( temporarySolver, null, null, onPreSuccess );
        }

        internal IYodiiEngineResult AddYodiiCommand( InternalYodiiCommand cmd )
        {
            Debug.Assert( IsRunning, "Cannot call this function when the engine is not running" );
            Debug.Assert( cmd.LiveItem != null, "New commands are necessarily bound to an existing live item." );
            if( !cmd.IsNewValidLiveCommand ) return new YodiiEngineResult( new CommandFailureResult( cmd ), this );
            return DoDynamicResolution( _currentSolver, null, cmd );
        }

        internal IYodiiEngineResult RevokeYodiiCommandCaller( string callerKey )
        {
            Debug.Assert( callerKey != null );
            if( IsRunning )
            {
                return DoDynamicResolution( _currentSolver, c => c.Command.CallerKey != callerKey, null );
            }
            else
            {
                _yodiiCommands.RemoveCaller( callerKey );
                return _successResult;
            }
        }

        /// <summary>
        /// Starts the engine (that must be stopped), performs all possible resolutions,
        /// and begins monitoring configuration for changes.
        /// </summary>
        /// <param name="persistedCommands">Optional list of commands that will be initialized.</param>
        /// <returns>Engine start result.</returns>
        /// <exception cref="InvalidOperationException">This engine must not be running (<see cref="IsRunning"/> must be false).</exception>
        public IYodiiEngineResult StartEngine( IEnumerable<YodiiCommand> persistedCommands = null )
        {
            return StartEngine( false, false, persistedCommands );
        }


        /// <summary>
        /// Starts the engine (that must be stopped).
        /// </summary>
        /// <param name="revertServices">True to revert the list of the services (based on their <see cref="IServiceInfo.ServiceFullName"/>).</param>
        /// <param name="revertPlugins">True to revert the list of the plugins (based on their <see cref="IPluginInfo.PluginFullName"/>).</param>
        /// <param name="persistedCommands">Optional list of commands that will be initialized.</param>
        /// <returns>The result.</returns>
        /// <exception cref="InvalidOperationException">This engine must not be running (<see cref="IsRunning"/> must be false).</exception>
        public IYodiiEngineResult StartEngine( bool revertServices, bool revertPlugins, IEnumerable<YodiiCommand> persistedCommands = null )
        {
            if( IsRunning ) throw new InvalidOperationException();
            _yodiiCommands.ResetOnStart( persistedCommands );
            var r = ConfigurationSolver.CreateAndApplyStaticResolution( this, _manager.FinalConfiguration, _manager.DiscoveredInfo, revertServices, revertPlugins, false );
            if( r.Item1 != null )
            {
                Debug.Assert( !r.Item1.Success, "Not null means necessarily an error." );
                Debug.Assert( r.Item1.Engine == this );
                return r.Item1;
            }
            return DoDynamicResolution( r.Item2, null, null );
        }
        
        /// <summary>
        /// Triggers the static resolution of the graph (with the current <see cref="Configuration"/> and its <see cref="IConfigurationManager.DiscoveredInfo"/>).
        /// This has no impact on the engine and can be called when <see cref="IsRunning"/> is false.
        /// </summary>
        /// <returns>The result with a potential non null <see cref="IYodiiEngineResult.StaticFailureResult"/> but always an available <see cref="IYodiiEngineStaticOnlyResult.StaticSolvedConfiguration"/>.</returns>
        public IYodiiEngineStaticOnlyResult StaticResolutionOnly()
        {
            return StaticResolutionOnly( false, false );
        }

        #region Start/Stop item, plugin and service methods.

        /// <summary>
        /// Attempts to start a service or a plugin. 
        /// </summary>
        /// <param name="pluginOrService">The plugin or service live object to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// The <see cref="ILiveYodiiItem.Capability">pluginOrService.Capability</see> property <see cref="ILiveRunCapability.CanStart"/> 
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </para>
        /// or
        /// <para>
        /// This engine is not running.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter <paramref name="pluginOrService"/> is null.
        /// </exception>
        /// <returns>Result detailing whether the service or plugin was successfully started or not.</returns>
        public IYodiiEngineResult StartItem( ILiveYodiiItem pluginOrService, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            if( pluginOrService == null ) throw new ArgumentNullException();
            if( !IsRunning ) throw new InvalidOperationException();
            return AddYodiiCommand( new InternalYodiiCommand( pluginOrService, true, impact, callerKey ) );
        }

        /// <summary>
        /// Attempts to stop this service or plugin.
        /// </summary>
        /// <param name="pluginOrService">The plugin or service live object to stop.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <returns>Result detailing whether the service or plugin was successfully stopped or not.</returns>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// The <see cref="ILiveYodiiItem.Capability">pluginOrService.Capability</see> property <see cref="ILiveRunCapability.CanStop"/> must be true.
        /// </para>
        /// or
        /// <para>
        /// This engine is not running.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter <paramref name="pluginOrService"/> is null.
        /// </exception>
        public IYodiiEngineResult StopItem( ILiveYodiiItem pluginOrService, string callerKey = null )
        {
            if( pluginOrService == null ) throw new ArgumentNullException();
            if( !IsRunning ) throw new InvalidOperationException();
            return AddYodiiCommand( new InternalYodiiCommand( pluginOrService, false, StartDependencyImpact.Unknown, callerKey ) );
        }

        IYodiiEngineResult ItemNotFoundFailure( string name )
        {
            return new YodiiEngineResult( new CommandFailureResult( name, false, true ), this );
        }

        /// <summary>
        /// Attempts to start a plugin. 
        /// </summary>
        /// <param name="pluginFullName">Name of the plugin to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStart"/>  property  
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The plugin must exist.</exception>
        /// <returns>Result detailing whether the plugin was successfully started or not.</returns>
        public IYodiiEngineResult StartPlugin( string pluginFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            if( pluginFullName == null ) throw new ArgumentNullException();
            if( !IsRunning ) throw new InvalidOperationException();
            var p = _liveInfo.FindPlugin( pluginFullName );
            if( p == null ) return ItemNotFoundFailure( pluginFullName );
            return AddYodiiCommand( new InternalYodiiCommand( p, true, impact, callerKey ) );
        }

        /// <summary>
        /// Attempts to start a service. 
        /// </summary>
        /// <param name="serviceFullName">Name of the service to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStart"/>  property  
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The service must exist.</exception>
        /// <returns>Result detailing whether the service was successfully started or not.</returns>
        public IYodiiEngineResult StartService( string serviceFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            if( serviceFullName == null ) throw new ArgumentNullException();
            if( !IsRunning ) throw new InvalidOperationException();
            var s = _liveInfo.FindService( serviceFullName );
            if( s == null ) return ItemNotFoundFailure( serviceFullName );
            return AddYodiiCommand( new InternalYodiiCommand( s, true, impact, callerKey ) );
        }

        /// <summary>
        /// Attempts to stop a service or a plugin. 
        /// </summary>
        /// <param name="pluginFullName">Name of the plugin to stop.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStop"/>' property must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The plugin must exist.</exception>
        /// <returns>Result detailing whether the service or plugin was successfully stopped or not.</returns>
        public IYodiiEngineResult StopPlugin( string pluginFullName, string callerKey = null )
        {
            if( !IsRunning ) throw new InvalidOperationException();
            var p = _liveInfo.FindPlugin( pluginFullName );
            if( p == null ) return ItemNotFoundFailure( pluginFullName ); 
            return AddYodiiCommand( new InternalYodiiCommand( p, false, StartDependencyImpact.Unknown, callerKey ) );
        }

        /// <summary>
        /// Attempts to stop a service. 
        /// </summary>
        /// <param name="serviceFullName">Name of the service to stop.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStop"/>' property must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The service must exist.</exception>
        /// <returns>Result detailing whether the service was successfully stopped or not.</returns>
        public IYodiiEngineResult StopService( string serviceFullName, string callerKey = null )
        {
            if( !IsRunning ) throw new InvalidOperationException();
            var s = _liveInfo.FindService( serviceFullName );
            if( s == null ) return ItemNotFoundFailure( serviceFullName ); 
            return AddYodiiCommand( new InternalYodiiCommand( s, false, StartDependencyImpact.Unknown, callerKey ) );
        }

        #endregion

        /// <summary>
        /// Triggers the static resolution of the graph (with the current <see cref="Configuration"/>).
        /// This has no impact on the engine and can be called when <see cref="IsRunning"/> is false.
        /// </summary>
        /// <param name="revertServices">True to revert the list of the services (based on their <see cref="IServiceInfo.ServiceFullName"/>).</param>
        /// <param name="revertPlugins">True to revert the list of the plugins (based on their <see cref="IPluginInfo.PluginFullName"/>).</param>
        /// <returns>
        /// The result with a potential non null <see cref="IYodiiEngineResult.StaticFailureResult"/> but always an 
        /// available <see cref="IYodiiEngineStaticOnlyResult.StaticSolvedConfiguration"/>.
        /// </returns>
        public IYodiiEngineStaticOnlyResult StaticResolutionOnly( bool revertServices, bool revertPlugins )
        {
            var r = ConfigurationSolver.CreateAndApplyStaticResolution( this, _manager.FinalConfiguration, _manager.DiscoveredInfo, revertServices, revertPlugins, createStaticSolvedConfigOnSuccess: true );
            Debug.Assert( r.Item1 != null, "Either an error or a successful static resolution." );
            return r.Item1;
        }


        /// <summary>
        /// YodiiCommands are exposed by LiveInfo, not by the IYodiiEngine itself.
        /// </summary>
        internal IObservableReadOnlyList<YodiiCommand> YodiiCommands
        {
            get { return _yodiiCommands; }
        }

        void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        /// <summary>
        /// Gets live information about the running services and plugins when the engine is started.
        /// Empty when the engine is not running.
        /// </summary>
        public ILiveInfo LiveInfo
        {
            get { return _liveInfo; }
        }

        /// <summary>
        /// Gets the <see cref="IYodiiEngineHost"/>.
        /// </summary>
        public IYodiiEngineHost Host
        {
            get { return _host; }
        }

    }
}
