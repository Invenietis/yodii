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

        ConfigurationSolver _currentSolver;
        Queue<Action<IYodiiEngineExternal>> _postActions;
        bool _isStopping;

        class YodiiCommandList : ObservableCollection<YodiiCommand>, IObservableReadOnlyList<YodiiCommand>
        {
            public void Merge( IReadOnlyList<YodiiCommand> newCommands )
            {
                if( newCommands.Count == 0 )
                {
                    this.Clear();
                    return;
                }
                if( this.Count == 0 )
                {
                    this.AddRange( newCommands );
                    return;
                }

                if( this[0] != newCommands[0] ) this.InsertItem( 0, newCommands[0] );
                for( int i = 1; i < newCommands.Count; i++ )
                {
                    if( newCommands[i] != this[i] ) this.RemoveAt( i-- );
                }
                while( this.Count > newCommands.Count ) this.RemoveAt( Count - 1 );
                Debug.Assert( this.Count == newCommands.Count );
            }
        }

        /// <summary>
        /// Raised whenever <see cref="IsRunning"/> or <see cref="IsStopping"/> changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public YodiiEngine( IYodiiEngineHost host )
        {
            if( host == null ) throw new ArgumentNullException( "host" );
            _successResult = new SuccessYodiiEngineResult( this );
            _host = host;
            host.Engine = this;
            _manager = new ConfigurationManager( this );
            _yodiiCommands = new YodiiCommandList();
            _liveInfo = new LiveInfo( this );
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

        internal IYodiiEngineResult DoDynamicResolution( ConfigurationSolver solver, Func<YodiiCommand, bool> existingCommandFilter, YodiiCommand cmd, Action onPreSuccess = null )
        {
            bool isRootStart = false;
            Action<Action<IYodiiEngineExternal>> postActionCollector = null;
            if( !_isStopping )
            {
                if( _postActions == null )
                {
                    _postActions = new Queue<Action<IYodiiEngineExternal>>();
                    isRootStart = true;
                }
                postActionCollector = _postActions.Enqueue;
            }

            var dynResult = solver.DynamicResolution( existingCommandFilter != null ? _yodiiCommands.Where( existingCommandFilter ) : _yodiiCommands, cmd );
            IYodiiEngineHostApplyResult hostResult = null;
            try
            {
                hostResult = _host.Apply( dynResult.Disabled, dynResult.Stopped, dynResult.Running, postActionCollector );
            }
            catch
            {
                _liveInfo.Clear();
                _currentSolver = null;
                RaisePropertyChanged( "IsRunning" );
                throw;
            }

            Debug.Assert( hostResult != null && hostResult.CancellationInfo != null );
            if( hostResult.CancellationInfo.Any() )
            {
                IYodiiEngineResult result =  solver.CreateDynamicFailureResult( hostResult.CancellationInfo );
                _liveInfo.UpdateRuntimeErrors( hostResult.CancellationInfo, solver.FindExistingPlugin );
                return result;
            }
            // Success:
            if( onPreSuccess != null ) onPreSuccess();
            bool wasStopped = _currentSolver == null;
            if( _currentSolver != solver ) _currentSolver = solver;

            _liveInfo.UpdateFrom( _currentSolver );

            _yodiiCommands.Merge( dynResult.Commands );
            if( wasStopped ) RaisePropertyChanged( "IsRunning" );

            if( isRootStart )
            {
                Debug.Assert( _postActions != null );
                while( _postActions.Count > 0 )
                {
                    _postActions.Dequeue()( this );
                }
                _postActions = null;
            }
            return _successResult;
        }

        public IConfigurationManager Configuration
        {
            get { return _manager; }
        }

        public bool IsRunning
        {
            get { return _currentSolver != null; }
        }

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

        public void StopEngine()
        {
            if( IsRunning )
            {
                IsStopping = true;
                try
                {
                    // Stopping the engine disables all plugins.
                    // No post action here: this is the hint for the host that we are stopping.
                    _host.Apply( _manager.DiscoveredInfo.PluginInfos, Enumerable.Empty<IPluginInfo>(), Enumerable.Empty<IPluginInfo>(), null );
                }
                finally
                {
                    _liveInfo.Clear();
                    _currentSolver = null;
                    IsStopping = false;
                }
                RaisePropertyChanged( "IsRunning" );
            }
        }


        internal IYodiiEngineResult OnConfigurationChanging( ConfigurationSolver temporarySolver )
        {
            Debug.Assert( IsRunning, "Cannot call this function when the engine is not running" );
            return DoDynamicResolution( temporarySolver, null, null );
        }

        internal IYodiiEngineResult AddYodiiCommand( YodiiCommand cmd )
        {
            Debug.Assert( IsRunning, "Cannot call this function when the engine is not running" );
            return DoDynamicResolution( _currentSolver, null, cmd );
        }

        internal IYodiiEngineResult RevokeYodiiCommandCaller( string callerKey )
        {
            Debug.Assert( callerKey != null );
            if( IsRunning )
            {
                return DoDynamicResolution( _currentSolver, c => c.CallerKey != callerKey, null );
            }
            else
            {
                _yodiiCommands.RemoveWhereAndReturnsRemoved( c => c.CallerKey == callerKey ).Count();
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
        /// Triggers the static resolution of the graph (with the current <see cref="DiscoveredInfo"/> and <see cref="Configuration"/>).
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
            if( !pluginOrService.Capability.CanStartWith( impact ) )
            {
                throw new InvalidOperationException( "You must call Capability.CanStart with the wanted impact and ensure that it returns true before calling Start." );
            }
            var cmd = new YodiiCommand( true, pluginOrService.FullName, pluginOrService.IsPlugin, impact, callerKey );
            return AddYodiiCommand( cmd );
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
            if( !pluginOrService.Capability.CanStop )
            {
                throw new InvalidOperationException( "You must call Capability.CanStop and ensure that it returns true before calling Stop." );
            }
            var cmd = new YodiiCommand( false, pluginOrService.FullName, pluginOrService.IsPlugin, StartDependencyImpact.Unknown, callerKey );
            return AddYodiiCommand( cmd );
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
            if( !IsRunning ) throw new InvalidOperationException();
            var p = _liveInfo.FindPlugin( pluginFullName );
            if( p == null ) throw new ArgumentException();
            return StartItem( p, impact, callerKey );
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
            if( !IsRunning ) throw new InvalidOperationException();
            var s = _liveInfo.FindService( serviceFullName );
            if( s == null ) throw new ArgumentException();
            return StartItem( s, impact, callerKey );
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
            if( p == null ) throw new ArgumentException();
            return StopItem( p, callerKey );
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
            if( s == null ) throw new ArgumentException();
            return StopItem( s, callerKey );
        }

        #endregion

        /// <summary>
        /// Triggers the static resolution of the graph (with the current <see cref="DiscoveredInfo"/> and <see cref="Configuration"/>).
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
            _yodiiCommands.Clear();
            if( persistedCommands != null ) _yodiiCommands.AddRange( persistedCommands );
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

        public ILiveInfo LiveInfo
        {
            get { return _liveInfo; }
        }

        public IYodiiEngineHost Host
        {
            get { return _host; }
        }

    }
}
