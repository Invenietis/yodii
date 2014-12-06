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
    public class PluginHost : IYodiiEngineHost
    {
        static ILog _log = LogManager.GetLogger( typeof( PluginHost ) );
        readonly ServiceHost _serviceHost;
        readonly Dictionary<string, PluginProxy> _plugins;
        Func<IPluginInfo,object[],IYodiiPlugin> _pluginCreator;

        public PluginHost()
            : this( CatchExceptionGeneration.HonorIgnoreExceptionAttribute )
        {
        }

        internal PluginHost( CatchExceptionGeneration catchMode )
        {
            _plugins = new Dictionary<string, PluginProxy>();
            _serviceHost = new ServiceHost( catchMode );
            _pluginCreator = DefaultPluginCreator;
        }

        static IYodiiPlugin DefaultPluginCreator( IPluginInfo pluginInfo, object[] ctorParameters )
        {
            var tPlugin = Assembly.Load( pluginInfo.AssemblyInfo.AssemblyName ).GetType( pluginInfo.PluginFullName, true );
            var ctor = tPlugin.GetConstructors().OrderBy( c => c.GetParameters().Length ).Last();
            return (IYodiiPlugin)ctor.Invoke( ctorParameters );
        }

        /// <summary>
        /// Gets or sets a function that is in charge of obtaining concrete plugin instances.
        /// The dynamic services parameters is available in the order 
        /// of <see cref="IServiceReferenceInfo.ConstructorParameterIndex">ConstructorParameterIndex</see> property 
        /// of <see cref="IPluginInfo.ServiceReferences">PluginInfo.ServiceReferences</see> objects.
        /// </summary>
        public Func<IPluginInfo, object[], IYodiiPlugin> PluginCreator
        {
            get { return _pluginCreator; }
            set { _pluginCreator = value ?? DefaultPluginCreator; }
        }

        class Result : IYodiiEngineHostApplyResult
        {
            public Result( IReadOnlyList<IPluginHostApplyCancellationInfo> errors, IReadOnlyList<Action<IYodiiEngine>> actions )
            {
                CancellationInfo = errors;
                PostStartActions = actions;
            }

            public IReadOnlyList<IPluginHostApplyCancellationInfo> CancellationInfo { get; private set; }

            public IReadOnlyList<Action<IYodiiEngine>> PostStartActions { get; private set; }
        }

        /// <summary>
        /// Attempts to execute a plan.
        /// </summary>
        /// <param name="disabledPlugins">Plugins that must be disabled.</param>
        /// <param name="stoppedPlugins">Plugins that must be stopped.</param>
        /// <param name="runningPlugins">Plugins that must be running.</param>
        /// <returns>A <see cref="IYodiiEngineHostApplyResult"/> that details the error if any.</returns>
        public IYodiiEngineHostApplyResult Apply( IEnumerable<IPluginInfo> disabledPlugins, IEnumerable<IPluginInfo> stoppedPlugins, IEnumerable<IPluginInfo> runningPlugins )
        {
            if( PluginCreator == null ) throw new InvalidOperationException( R.PluginCreatorIsNull );

            var errors = new List<CancellationInfo>();
            var postStartActions = new List<Action<IYodiiEngine>>();

            ServiceManager serviceManager = new ServiceManager( _serviceHost );
            List<PluginProxy> toDisable = new List<PluginProxy>();

            // The toStart and toStop list are lists of PreStart/StopContext instead of list of the simple PluginProxy.
            // With the help of the ServiceManager, this resolves the issue to find swapped plugins (and their most specialized common service).
            List<PreStopContext> toStop = new List<PreStopContext>();

            // To be able to initialize PreStartContext objects, we need to instanciate the shared memory now.
            Dictionary<object, object> sharedMemory = new Dictionary<object, object>();

            foreach( IPluginInfo k in disabledPlugins )
            {
                PluginProxy p = EnsureProxy( k );
                if( p.Status == PluginStatus.Disabled ) throw new ArgumentException( String.Format( R.HostApplyPluginAlreadyDisabled, k.PluginFullName ), "disabledPlugins" );
                toDisable.Add( p );
                if( p.Status != PluginStatus.Stopped )
                {
                    var preStop = new PreStopContext( p, sharedMemory );
                    if( k.Service != null ) serviceManager.AddToStop( k.Service, preStop );
                    toStop.Add( preStop );
                }
            }
            foreach( IPluginInfo k in stoppedPlugins )
            {
                PluginProxy p = EnsureProxy( k );
                if( p.Status == PluginStatus.Stopped ) throw new ArgumentException( String.Format( R.HostApplyStopPluginAlreadyStopped, k.PluginFullName ), "stoppedPlugins" );
                if( p.Status == PluginStatus.Disabled ) throw new ArgumentException( String.Format( R.HostApplyStopPluginAlreadyDisabled, k.PluginFullName ), "stoppedPlugins" );
                var preStop = new PreStopContext( p, sharedMemory );
                if( k.Service != null ) serviceManager.AddToStop( k.Service, preStop );
                toStop.Add( preStop );
            }
            // The lists toDisable and toStop are correctly filled: a plugin is in both lists if it must be stopped and then disabled.

            // Now, we attempt to activate the plugins that must run: if an error occurs,
            // we leave and return the error since we did not change anything.

            List<StStartContext> toStart = new List<StStartContext>();
            foreach( IPluginInfo k in runningPlugins )
            {
                PluginProxy p = EnsureProxy( k );
                if( p.Status == PluginStatus.Started ) throw new ArgumentException( String.Format( R.HostApplyStartPluginAlreadyStarted, k.PluginFullName ), "runningPlugins" );
                if( !p.IsLoaded )
                {
                    if( !p.TryLoad( _serviceHost, PluginCreator ) )
                    {
                        Debug.Assert( p.LoadError != null, "Error is catched by the PluginHost itself." );
                        _serviceHost.LogMethodError( PluginCreator.Method, p.LoadError );
                        // Unable to load the plugin: leave now.
                        errors.Add( new CancellationInfo( p.PluginKey, true ) { Error = p.LoadError, ErrorMessage = R.ErrorWhileCreatingPluginInstance } );
                    }
                    Debug.Assert( p.LoadError == null );
                    Debug.Assert( p.Status == PluginStatus.Disabled );
                }
                var preStart = new StStartContext( p, sharedMemory );
                if( k.Service != null )
                {
                    var impact = serviceManager.AddToStart( k.Service, preStart );
                    do
                    {
                        if( impact.Service.Status == ServiceStatus.Disabled )
                        {
                            impact.Service.Status = ServiceStatus.Stopped;
                            impact.Service.RaiseStatusChanged( postStartActions );
                        }
                        impact = impact.ServiceGeneralization;
                    } 
                    while( impact != null );
                }
                toStart.Add( preStart );
            }
            if( errors.Count > 0 )
            {
                return new Result( errors.AsReadOnlyList(), postStartActions.AsReadOnlyList() );
            }

            // The toStart list of PreStartContext is ready (and plugins inside are loaded without error).
            // Now starts the actual PreStop/PreStart/Stop/Start/Disable phase:

            // Calling PreStop for all "toStop" plugins.
            foreach( var c in toStop )
            {
                Debug.Assert( c.Plugin.Status == PluginStatus.Started );
                c.Plugin.RealPlugin.PreStop( c );
                if( c.HandleSuccess( errors, false ) ) c.Plugin.Status = PluginStatus.Stopping;
            }
            // If at least one failed, cancel the start for the successful ones
            // and stops here.
            if( errors.Count > 0 )
            {
                CancelSuccessfulPreStop( sharedMemory, toStop );
                return new Result( errors.AsReadOnlyList(), postStartActions.AsReadOnlyList() );
            }

            // PreStop has been successfully called: we must now call the PreStart.
            foreach( var c in toStart )
            {
                Debug.Assert( c.Plugin.Status != PluginStatus.Started );
                c.Plugin.RealPlugin.PreStart( c );
                if( c.HandleSuccess( errors, true ) )
                {
                    c.Plugin.Status = PluginStatus.Starting;
                }
            }
            // If a PreStart failed, we cancel everything and stop.
            if( errors.Count > 0 )
            {
                CancelSuccessfulPreStop( sharedMemory, toStop );
                var cStop = new CancelPreStartContext( sharedMemory );
                foreach( var c in toStart )
                {
                    if( c.Success )
                    {
                        c.Plugin.Status = PluginStatus.Stopped;
                        var rev = c.RollbackAction;
                        if( rev != null ) rev( cStop );
                        else c.Plugin.RealPlugin.Stop( cStop );
                    }
                }
                return new Result( errors.AsReadOnlyList(), postStartActions.AsReadOnlyList() );
            }

            // Sending Stopping & Starting events...

            // Time to call Stop and Start. 
            foreach( var c in toStop )
            {
                Debug.Assert( c.Success );
                c.Plugin.Status = PluginStatus.Stopped;
                c.Plugin.RealPlugin.Stop( c );
            }
            foreach( var c in toStart )
            {
                Debug.Assert( c.Success );
                c.Plugin.Status = PluginStatus.Started;
                c.Plugin.RealPlugin.Start( c );
            }

            // Sending Stopped & Started events...

            foreach( PluginProxy p in toDisable )
            {
                p.Status = PluginStatus.Disabled;
                try
                {
                    p.DisposeIfDisposable();
                }
                catch( Exception ex )
                {
                    _serviceHost.LogMethodError( p.GetImplMethodInfoDispose(), ex );
                }
            }
            return new Result( errors.AsReadOnlyList(), postStartActions.AsReadOnlyList() );
        }

        static void CancelSuccessfulPreStop( Dictionary<object, object> sharedMemory, List<PreStopContext> successfulPreStop )
        {
            var cStart = new CancelPresStopContext( sharedMemory );
            foreach( var c in successfulPreStop )
            {
                if( c.Success )
                {
                    c.Plugin.Status = PluginStatus.Started;
                    var rev = c.RollbackAction;
                    if( rev != null ) rev( cStart );
                    else c.Plugin.RealPlugin.Start( cStart );
                }
            }
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
            if( _plugins.TryGetValue( pluginInfo.PluginFullName, out result ) )
            {
                // Updates the pluginInfo reference (when discovered again, IPluginInfo instances change).
                if( result.PluginKey != pluginInfo )
                {
                    result.PluginKey = pluginInfo;
                }
            }
            else
            {
                result = new PluginProxy( pluginInfo );
                _plugins.Add( pluginInfo.PluginFullName, result );
            }
            return result;
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
