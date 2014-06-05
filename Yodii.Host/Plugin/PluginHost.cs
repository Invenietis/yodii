#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Plugin\PluginHost.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using CK.Core;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yodii.Model;
using System.Reflection;

namespace Yodii.Host
{
    public class PluginHost : IPluginHost, IYodiiEngineHost
    {
        static ILog _log = LogManager.GetLogger( typeof( PluginHost ) );
        readonly ServiceHost _serviceHost;
        readonly Dictionary<string, PluginProxy> _plugins;
        readonly Dictionary<string, PluginProxy> _loadedPlugins;
        readonly List<PluginProxy> _newlyLoadedPlugins;
        Func<IPluginInfo,object[],IYodiiPlugin> _pluginCreator;

        public PluginHost()
            : this( CatchExceptionGeneration.HonorIgnoreExceptionAttribute )
        {
        }

        internal PluginHost( CatchExceptionGeneration catchMode )
        {
            _plugins = new Dictionary<string, PluginProxy>();
            _loadedPlugins = new Dictionary<string, PluginProxy>();
            _serviceHost = new ServiceHost( catchMode );
            _newlyLoadedPlugins = new List<PluginProxy>();
            _pluginCreator = DefaultPluginCreator;
        }

        static IYodiiPlugin DefaultPluginCreator( IPluginInfo pluginInfo, object[] ctorParameters )
        {
            var tPlugin = Assembly.Load( pluginInfo.AssemblyInfo.AssemblyName ).GetType( pluginInfo.PluginFullName, true );
            var ctor = tPlugin.GetConstructors().OrderBy( c => c.GetParameters().Length ).Last();
            return (IYodiiPlugin)ctor.Invoke( ctorParameters );
        }

        /// <summary>
        /// Gets the loaded plugins. This contains also the plugins that are currently disabled but have been loaded at least once.
        /// </summary>
        public IReadOnlyCollection<IPluginProxy> LoadedPlugins { get { return _loadedPlugins.Values.ToList().ToReadOnlyCollection(); } }

        /// <summary>
        /// Gets or sets a function that is in charge of obtaining concrete plugin instances.
        /// The dynamic services parameters is available in the order 
        /// of <see cref="IServiceReferenceInfo.ConstructorParameterIndex">ConstructorParameterIndex</see> property 
        /// of <see cref="IPluginInfo.ServiceReferences">PluginInfo.ServiceReferences</see> objects.
        /// </summary>
        public Func<IPluginInfo,object[],IYodiiPlugin> PluginCreator 
        {
            get { return _pluginCreator; }
            set { _pluginCreator = value ?? DefaultPluginCreator; } 
        }
        /*
        /// <summary>
        /// Gets the <see cref="IPluginProxy"/> corresponding to the <see cref="IPluginInfo"/>.
        /// </summary>
        /// <param name="pluginInfo">Plugin info (typically provided by the <see cref="IDiscoverer"/>.</param>
        /// <returns>The plugin proxy (may be stopped or even not loaded).</returns>
        public IPluginProxy FindPluginProxy( IPluginInfo pluginInfo )//A VIRER
        {
            return _plugins.GetValueWithDefault( pluginInfo, null );
        }
        */
        /// <summary>
        /// Gets the <see cref="IPluginProxy"/> corresponding to the Plugin Guid set as parameter.
        /// </summary>
        /// <param name="pluginId">The Guid of the plugin</param>
        /// <param name="checkCurrentlyLoading">set to yes if you want to look for the right IPluginProxy in the plugin currently being loaded</param>
        /// <returns>The plugin proxy (may be stopped or even not loaded).</returns>
        public IPluginProxy FindLoadedPlugin( string pluginFullName, bool checkCurrentlyLoading )
        {
            var p = _loadedPlugins.GetValueWithDefault( pluginFullName, null );
            if( p == null && checkCurrentlyLoading ) p = _newlyLoadedPlugins.FirstOrDefault( n => n.PluginKey.PluginFullName == pluginFullName  );
            return p;
        }

        public bool IsPluginRunning( IPluginInfo pluginInfo )
        {
            PluginProxy result;
            if( !_plugins.TryGetValue( pluginInfo.PluginFullName, out result ) ) return false; 
            return result.Status == InternalRunningStatus.Started;
        }

        /// <summary>
        /// Attempts to execute a plan.
        /// </summary>
        /// <param name="disabledPluginKeys">Plugins that must be disabled.</param>
        /// <param name="stoppedPluginKeys">Plugins that must be stopped.</param>
        /// <param name="runningPluginKeys">Plugins that must be running.</param>
        /// <returns>A <see cref="IExecutionPlanError"/> that details the error if any.</returns>
        public IEnumerable<Tuple<IPluginInfo, Exception>> Apply( IEnumerable<IPluginInfo> disabledPluginKeys, IEnumerable<IPluginInfo> stoppedPluginKeys, IEnumerable<IPluginInfo> runningPluginKeys )    
        {
            if( PluginCreator == null ) throw new InvalidOperationException( R.PluginCreatorIsNull );

            IEnumerable<Tuple<IPluginInfo, Exception>> executionPlanResult = new List<Tuple<IPluginInfo, Exception>>();
            int nbIntersect;
            nbIntersect = disabledPluginKeys.Intersect( stoppedPluginKeys ).Count();
            if( nbIntersect != 0 ) throw new CKException( R.DisabledAndStoppedPluginsIntersect, nbIntersect );
            nbIntersect = disabledPluginKeys.Intersect( runningPluginKeys ).Count();
            if( nbIntersect != 0 ) throw new CKException( R.DisabledAndRunningPluginsIntersect, nbIntersect );
            nbIntersect = stoppedPluginKeys.Intersect( runningPluginKeys ).Count();
            if( nbIntersect != 0 ) throw new CKException( R.StoppedAndRunningPluginsIntersect, nbIntersect );

            List<PluginProxy> toDisable = new List<PluginProxy>();
            List<PluginProxy> toStop = new List<PluginProxy>();
            List<PluginProxy> toStart = new List<PluginProxy>();

            foreach( IPluginInfo k in disabledPluginKeys )
            {
                PluginProxy p = EnsureProxy( k );
                if( p.Status != InternalRunningStatus.Disabled )
                {
                    toDisable.Add( p );
                    if( p.Status != InternalRunningStatus.Stopped )
                    {
                        toStop.Add( p );
                    }
                }
            }
            foreach( IPluginInfo k in stoppedPluginKeys )
            {
                PluginProxy p = EnsureProxy( k );
                if( p.Status != InternalRunningStatus.Stopped )
                {
                    toStop.Add( p );
                }
            }
            // The lists toDisable and toStop are correctly filled.
            // A plugin can be in both lists if it must be stopped and then disabled.

            // Now, we attempt to activate the plugins that must run: if an error occurs,
            // we leave and return the error since we did not change anything.
            foreach( IPluginInfo k in runningPluginKeys )
            {
                PluginProxy p = EnsureProxy( k );
                if( !p.IsLoaded )
                {
                    if( !p.TryLoad( _serviceHost, PluginCreator ) )
                    {
                        Debug.Assert( p.LoadError != null, "Error is catched by the PluginHost itself." );
                        _serviceHost.LogMethodError( PluginCreator.Method, p.LoadError );
                        // Unable to load the plugin: leave now.
                        return executionPlanResult.Append(new Tuple<IPluginInfo, Exception>(p.PluginKey,p.LoadError));
                    }
                    Debug.Assert( p.LoadError == null );
                    Debug.Assert( p.Status == InternalRunningStatus.Disabled );
                    _newlyLoadedPlugins.Add( p );
                }
                if( p.Status != InternalRunningStatus.Started )
                {
                    toStart.Add( p );
                }
            }
            // The toStart list is ready: plugins inside are loaded without error.

            // We stop all "toStop" plugin.
            // Their "stop" methods will be called.
            foreach( PluginProxy p in toStop )
            {
                if( p.Status > InternalRunningStatus.Stopped )
                {
                    try
                    {
                        SetPluginStatus( p, InternalRunningStatus.Stopping );
                        p.RealPlugin.Stop();
                        _log.Debug( String.Format( "The {0} plugin has been successfully stopped.", p.PublicName ) );
                    }
                    catch( Exception ex )
                    {
                        _log.ErrorFormat( "There has been a problem when stopping the {0} plugin.", ex, p.PublicName );
                        _serviceHost.LogMethodError( p.GetImplMethodInfoStop(), ex );
                        executionPlanResult.Append( new Tuple<IPluginInfo, Exception>( p.PluginKey, ex ) );
                    }
                }
            }

            // We un-initialize all "toStop" plugin.
            // Their "Teardown" methods will be called.
            // After that, they are all "stopped".
            foreach( PluginProxy p in toStop )
            {
                try
                {
                    if( p.Status > InternalRunningStatus.Stopped )
                    {
                        SetPluginStatus( p, InternalRunningStatus.Stopped );
                        p.RealPlugin.Teardown();
                        _log.Debug( String.Format( "The {0} plugin has been successfully torn down.", p.PublicName ) );
                    }
                }
                catch( Exception ex )
                {
                    _log.ErrorFormat( "There has been a problem when tearing down the {0} plugin.", ex, p.PublicName );
                    _serviceHost.LogMethodError( p.GetImplMethodInfoTeardown(), ex );
                    executionPlanResult.Append( new Tuple<IPluginInfo, Exception>( p.PluginKey, ex ) );
                }
            }
            Debug.Assert( toStop.All( p => p.Status <= InternalRunningStatus.Stopped ) );


            // Prepares the plugins to start so that they become the implementation
            // of their Service and are at least stopped (instead of disabled).
            foreach( PluginProxy p in toStart )
            {
                ServiceProxyBase service = p.Service;
                // The call to service.SetImplementation, sets the implementation and takes
                // the _status of the service into account: this status is at most Stopped
                // since we necessarily stopped the previous implementation (if any) above.
                if( service != null )
                {
                    Debug.Assert( service.Status <= InternalRunningStatus.Stopped );
                    service.SetPluginImplementation( p );
                }
                // This call will trigger an update of the service status.
                if( p.Status == InternalRunningStatus.Disabled ) SetPluginStatus( p, InternalRunningStatus.Stopped );
            }

            // Now that services have been associated to their new implementation (in Stopped status), we
            // can disable the plugins that must be disabled.
            foreach( PluginProxy p in toDisable )
            {
                SetPluginStatus( p, InternalRunningStatus.Disabled );
                try
                {
                    p.DisposeIfDisposable();
                }
                catch( Exception ex )
                {
                    _log.ErrorFormat( "There has been a problem when disposing the {0} plugin.", ex, p.PublicName );
                    _serviceHost.LogMethodError( p.GetImplMethodInfoDispose(), ex );
                    executionPlanResult.Append( new Tuple<IPluginInfo, Exception>( p.PluginKey, ex ) );
                }
            }

            // Before starting 
            for( int i = 0; i < toStart.Count; i++ )
            {
                PluginProxy p = toStart[i];

                SetPluginStatus( p, InternalRunningStatus.Starting );
                PluginSetupInfo info = new PluginSetupInfo();
                try
                {
                    p.RealPlugin.Setup( info );
                    info.Clear();
                    _log.Debug( String.Format( "The {0} plugin has been successfully set up.", p.PublicName ) );
                }
                catch( Exception ex )
                {
                    _log.ErrorFormat( "There has been a problem when setting up the {0} plugin.", ex, p.PublicName );
                    _serviceHost.LogMethodError( p.GetImplMethodInfoSetup(), ex );

                    // Revoking the call to Setup for all plugins that haven't been started yet.
                    // Will pass the plugin to states : Stopping and then Stopped
                    for( int j = 0; j <= i; j++ )
                    {
                        RevokeSetupCall( toStart[j] );
                    }

                    info.Error = ex;
                   return executionPlanResult.Append( new Tuple<IPluginInfo, Exception>( p.PluginKey, ex ) );
                }
            }

            // Since we are now ready to start new plugins, it is now time to make the external world
            // aware of the existence of any new plugins and configure them to run.
            foreach( PluginProxy p in _newlyLoadedPlugins )
            {
                _loadedPlugins.Add( p.PluginKey.PluginFullName, p );
            }
            _newlyLoadedPlugins.Clear();

            for( int i = 0; i < toStart.Count; i++ )
            {
                PluginProxy p = toStart[i];
                try
                {
                    SetPluginStatus( p, InternalRunningStatus.Started );
                    p.RealPlugin.Start();
                    _log.Debug( String.Format( "The {0} plugin has been successfully started.", p.PublicName ) );
                }
                catch( Exception ex )
                {
                    // Emitted as low level log.
                    _log.ErrorFormat( "There has been a problem when starting the {0} plugin.", ex, p.PublicName );

                    // Emitted as a log event.
                    _serviceHost.LogMethodError( p.GetImplMethodInfoStart(), ex );

                    //All the plugins already started  when the exception was thrown have to be stopped + teardown (including this one in exception)
                    for( int j = 0; j <= i; j++ )
                    {
                        RevokeStartCall( toStart[j] );
                    }

                    // Revoking the call to Setup for all plugins that hadn't been started when the exception occured.
                    for( int j = i + 1; j < toStart.Count; j++ )
                    {
                        RevokeSetupCall( toStart[j] );
                    }

                    return executionPlanResult.Append( new Tuple<IPluginInfo, Exception>( p.PluginKey, ex ) );
                }
            }
            return executionPlanResult;
        }

        private void RevokeStartCall( PluginProxy p )
        {
            try
            {
                p.RealPlugin.Stop();
            }
            catch( Exception exStop )
            {
                // 2.1 - Should be emitted as an external log event.
                _serviceHost.LogMethodError( p.GetImplMethodInfoTeardown(), exStop );
            }
            RevokeSetupCall( p );
        }

        private void RevokeSetupCall( PluginProxy p )
        {
            // 2 - Stops the plugin status.
            SetPluginStatus( p, InternalRunningStatus.Stopping, true );
            // 3 - Safe call to TearDown.
            try
            {
                p.RealPlugin.Teardown();
            }
            catch( Exception exTeardown )
            {
                // 2.1 - Should be emitted as an external log event.
                _serviceHost.LogMethodError( p.GetImplMethodInfoTeardown(), exTeardown );
            }
            SetPluginStatus( p, InternalRunningStatus.Stopped, true );
        }

        /// <summary>
        /// Gets or sets the object that sends <see cref="IServiceHost.EventCreating"/> and <see cref="IServiceHost.EventCreated"/>.
        /// </summary>
        public object EventSender
        {
            get { return _serviceHost.EventSender; }
            set { _serviceHost.EventSender = value; }
        }

        PluginProxy EnsureProxy( IPluginInfo pluginInfo )
        {
            PluginProxy result;
            if(_plugins.TryGetValue( pluginInfo.PluginFullName, out result ))
            {
                if( result.PluginKey != pluginInfo )//If SetDiscoveredInfo is called, the pluginInfo will be new even if it is the same.
                {
                    result.PluginKey = pluginInfo; //TODO : figure out how to know and reload if the plugin really changes (example : different version)
                }
            }
            else
            {
                result = new PluginProxy( pluginInfo );
                _plugins.Add( pluginInfo.PluginFullName, result );
            }
            return result;
        }

        public event EventHandler<PluginStatusChangedEventArgs> StatusChanged;

        void SetPluginStatus( PluginProxy plugin, InternalRunningStatus newOne )
        {
            SetPluginStatus( plugin, newOne, false );
        }

        void SetPluginStatus( PluginProxy plugin, InternalRunningStatus newOne, bool allowErrorTransition )
        {
            InternalRunningStatus previous = plugin.Status;
            Debug.Assert( previous != newOne );
            if( newOne > previous )
            {
                // New status is greater than the previous one.
                // We first set the plugin (and raise the event) and then raise the service event (if any).
                DoSetPluginStatus( plugin, newOne, previous );
                if( plugin.IsCurrentServiceImplementation )
                {
                    if( newOne == InternalRunningStatus.Stopped && plugin.Service.Status == InternalRunningStatus.Stopped )
                    {
                        // This is an consequence of the fact that we disable plugins after 
                        // starting the new ones.
                        // When pA (stopping) implements sA and pB implements sA (starting), sA remains "Stopped".
                    }
                    else plugin.Service.SetStatusChanged( newOne, allowErrorTransition );
                }
            }
            else
            {
                // New status is lower than the previous one.
                // We first raise the service event (if any) and then the plugin event.
                if( plugin.IsCurrentServiceImplementation ) plugin.Service.SetStatusChanged( newOne, allowErrorTransition );
                DoSetPluginStatus( plugin, newOne, previous );
            }
        }

        private void DoSetPluginStatus( PluginProxy plugin, InternalRunningStatus newOne, InternalRunningStatus previous )
        {
            plugin.Status = newOne;
            var h = StatusChanged;
            if( h != null )
            {
                h( this, new PluginStatusChangedEventArgs( previous, plugin ) );
            }
        }

        public IServiceHost ServiceHost
        {
            get { return _serviceHost; }
        }

        public ILogCenter LogCenter
        {
            get { return _serviceHost; }
        }

        
    }
}
