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
            if( ctorParameters.Length != ctor.GetParameters().Length || ctorParameters.Any( p => p == null ) )
            {
                throw new CKException( R.DefaultPluginCreatorUnresolvedParams );
            }
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

        public IServiceHost ServiceHost
        {
            get { return _serviceHost; }
        }

        public ILogCenter LogCenter
        {
            get { return _serviceHost; }
        }

        /// <summary>
        /// Gets the <see cref="IPluginProxy"/> for the plugin identifier. 
        /// It may find plugins that are currently disabled but have been loaded at least once.
        /// </summary>
        /// <param name="pluginId">Plugin identifier.</param>
        /// <returns>Null if not found.</returns>
        public IPluginProxy FindLoadedPlugin( string pluginFullName )
        {
            return _plugins.GetValueWithDefault( pluginFullName, null );
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
            if( disabledPlugins == null ) throw new ArgumentNullException( "disabledPlugins" );
            if( stoppedPlugins == null ) throw new ArgumentNullException( "stoppedPlugins" );
            if( runningPlugins == null ) throw new ArgumentNullException( "runningPlugins" );
            if( PluginCreator == null ) throw new InvalidOperationException( R.PluginCreatorIsNull );

            HashSet<IPluginInfo> uniqueCheck = new HashSet<IPluginInfo>();
            foreach( var input in disabledPlugins.Concat( stoppedPlugins ).Concat( runningPlugins ) )
            {
                if( !uniqueCheck.Add( input ) )
                {
                    throw new ArgumentException( String.Format( R.HostApplyPluginMustBeInOneList, input.PluginFullName ) );
                }
            }

            var errors = new List<CancellationInfo>();
            var postStartActions = new List<Action<IYodiiEngine>>();

            ServiceManager serviceManager = new ServiceManager( _serviceHost );
            // The toStart and toStop list are lists of StStart/StStopContext.
            // With the help of the ServiceManager, this resolves the issue to find swapped plugins (and their most specialized common service).
            List<StStopContext> toStop = new List<StStopContext>();
            List<StStartContext> toStart = new List<StStartContext>();

            // To be able to initialize StContext objects, we need to instanciate the shared memory now.
            Dictionary<object, object> sharedMemory = new Dictionary<object, object>();

            foreach( IPluginInfo k in disabledPlugins )
            {
                PluginProxy p = EnsureProxy( k );
                Debug.Assert( p.Status != PluginStatus.Stopping && p.Status != PluginStatus.Starting );
                if( p.Status != PluginStatus.Disabled )
                {
                    var preStop = new StStopContext( p, sharedMemory, true, p.Status == PluginStatus.Stopped );
                    if( k.Service != null ) serviceManager.AddToStop( k.Service, preStop );
                    toStop.Add( preStop );
                }
            }
            foreach( IPluginInfo k in stoppedPlugins )
            {
                PluginProxy p = EnsureProxy( k );
                Debug.Assert( p.Status != PluginStatus.Stopping && p.Status != PluginStatus.Starting );
                if( p.Status == PluginStatus.Started )
                {
                    var preStop = new StStopContext( p, sharedMemory, false, false );
                    if( k.Service != null ) serviceManager.AddToStop( k.Service, preStop );
                    toStop.Add( preStop );
                }
            }

            // Now, we attempt to activate the plugins that must run: if an error occurs,
            // we leave and return the error since we did not change anything.
            _serviceHost.CallServiceBlocker = calledServiceType => new ServiceCallBlockedException( calledServiceType, R.CallingServiceFromCtor );
            foreach( IPluginInfo k in runningPlugins )
            {
                PluginProxy p = EnsureProxy( k );
                if( p.Status != PluginStatus.Started )
                {
                    if( !p.IsLoaded )
                    {
                        if( !p.TryLoad( _serviceHost, PluginCreator ) )
                        {
                            Debug.Assert( p.LoadError != null, "Error is catched by the PluginHost itself." );
                            _serviceHost.LogMethodError( PluginCreator.Method, p.LoadError );
                            // Unable to load the plugin: leave now.
                            errors.Add( new CancellationInfo( p.PluginInfo, true ) { Error = p.LoadError, ErrorMessage = R.ErrorWhileCreatingPluginInstance } );
                            // Breaks the loop: stop on the first failure.
                            // It is useless to pre load next plugins as long as we can be sure that they will not run now. 
                            break;
                        }
                        Debug.Assert( p.Status == PluginStatus.Disabled );
                    }
                    var preStart = new StStartContext( p, sharedMemory, p.Status == PluginStatus.Disabled );
                    p.Status = PluginStatus.Stopped;
                    if( k.Service != null ) serviceManager.AddToStart( k.Service, preStart );
                    toStart.Add( preStart );
                }
            }
            _serviceHost.CallServiceBlocker = null;
            if( errors.Count > 0 )
            {
                // Restores Disabled states.
                CancelSuccessfulStartForDisabled( toStart );
                return new Result( errors.AsReadOnlyList(), postStartActions.AsReadOnlyList() );
            }

            // The toStart list of StStartContext is ready (and plugins inside are loaded without error).
            // Now starts the actual PreStop/PreStart/Stop/Start/Disable phase:

            // Calling PreStop for all "toStop" plugins: calls to Services are allowed.
            foreach( var stopC in toStop )
            {
                if( !stopC.IsDisabledOnly )
                {
                    Debug.Assert( stopC.Plugin.Status == PluginStatus.Started );
                    try
                    {
                        stopC.Plugin.RealPlugin.PreStop( stopC );
                    }
                    catch( Exception ex )
                    {
                        stopC.Cancel( ex.Message, ex );
                    }
                    if( stopC.HandleSuccess( errors, false ) )
                    {
                        stopC.Plugin.Status = PluginStatus.Stopping;
                    }
                }
            }
            // If at least one failed, cancel the start for the successful ones
            // and stops here.
            if( errors.Count > 0 )
            {
                CancelSuccessfulPreStop( sharedMemory, toStop );
                // Restores Disabled states.
                CancelSuccessfulStartForDisabled( toStart );
                return new Result( errors.AsReadOnlyList(), postStartActions.AsReadOnlyList() );
            }

            // PreStop has been successfully called: we must now call the PreStart.
            // Calls to Services are not allowed during PreStart.
            _serviceHost.CallServiceBlocker = calledServiceType => new ServiceCallBlockedException( calledServiceType, R.CallingServiceFromPreStart );
            foreach( var startC in toStart )
            {
                Debug.Assert( startC.Plugin.Status == PluginStatus.Stopped );
                try
                {
                    startC.Plugin.RealPlugin.PreStart( startC );
                }
                catch( Exception ex )
                {
                    startC.Cancel( ex.Message, ex );
                }
                if( startC.HandleSuccess( errors, true ) )
                {
                    startC.Plugin.Status = PluginStatus.Starting;
                }
            }
            _serviceHost.CallServiceBlocker = null;
            // If a PreStart failed, we cancel everything and stop.
            if( errors.Count > 0 )
            {
                CancelSuccessfulPreStop( sharedMemory, toStop );
                _serviceHost.CallServiceBlocker = calledServiceType => new ServiceCallBlockedException( calledServiceType, R.CallingServiceFromPreStartRollbackAction );
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
                _serviceHost.CallServiceBlocker = null;
                // Restores Disabled states.
                CancelSuccessfulStartForDisabled( toStart );
                return new Result( errors.AsReadOnlyList(), postStartActions.AsReadOnlyList() );
            }

            // Setting ServiceStatus & sending events: StoppingSwapped, Stopping, StartingSwapped, Starting. 
            SetServiceSatus( postStartActions, toStop, toStart, true );


            // Time to call Stop. While Stopping, calling Services is not allowed.
            string callingPluginName = null;
            _serviceHost.CallServiceBlocker = calledServiceType => new ServiceCallBlockedException( calledServiceType, String.Format( R.CallingServiceFromStop, callingPluginName ) );
            // Even if we throw the exception here, we always clear the CallServiceBlocker on error.
            try
            {
                foreach( var stopC in toStop )
                {
                    if( !stopC.IsDisabledOnly )
                    {
                        Debug.Assert( stopC.Plugin.Status == PluginStatus.Stopping );
                        callingPluginName = stopC.Plugin.PluginInfo.PluginFullName;
                        stopC.Plugin.Status = PluginStatus.Stopped;
                        stopC.Plugin.RealPlugin.Stop( stopC );
                    }
                }
            }
            finally
            {
                _serviceHost.CallServiceBlocker = null;
            }
            // Before calling Start, we must set the implementations on Services
            // to be the starting plugin.
            foreach( var startC in toStart )
            {
                Debug.Assert( startC.Plugin.Status == PluginStatus.Starting );
                var impact = startC.ServiceImpact;
                while( impact != null )
                {
                    Debug.Assert( impact.Service.Status == ServiceStatus.Starting || impact.Service.Status == ServiceStatus.StartingSwapped );
                    Debug.Assert( (!impact.Implementation.HotSwapped && impact.Implementation.Plugin == startC.Plugin) 
                                  || (impact.Implementation.HotSwapped && impact.SwappedImplementation.Plugin == startC.Plugin) );
                    impact.Service.SetPluginImplementation( startC.Plugin );
                    impact = impact.ServiceGeneralization;
                }
            }
            // Now that all 
            foreach( var startC in toStart )
            {
                startC.Plugin.Status = PluginStatus.Started;
                startC.Plugin.RealPlugin.Start( startC );
            }

            // Setting services status & sending events...
            SetServiceSatus( postStartActions, toStop, toStart, false );

            // Disabling plugins that need to.
            foreach( var disableC in toStop )
            {
                if( disableC.MustDisable )
                {
                    disableC.Plugin.Disable( _serviceHost );
                    var impact = disableC.ServiceImpact;
                    while( impact != null && !impact.Starting )
                    {
                        impact.Service.SetPluginImplementation( null );
                        impact = impact.ServiceGeneralization;
                    }
                }
            }
            return new Result( errors.AsReadOnlyList(), postStartActions.AsReadOnlyList() );
        }

        private void CancelSuccessfulStartForDisabled( List<StStartContext> toStart )
        {
            foreach( var disableC in toStart )
            {
                if( disableC.WasDisabled ) disableC.Plugin.Disable( _serviceHost );
            }
        }

        static void CancelSuccessfulPreStop( Dictionary<object, object> sharedMemory, List<StStopContext> successfulPreStop )
        {
            var cStart = new CancelPresStopContext( sharedMemory );
            foreach( var c in successfulPreStop )
            {
                if( !c.IsDisabledOnly && c.Success )
                {
                    c.Plugin.Status = PluginStatus.Started;
                    var rev = c.RollbackAction;
                    if( rev != null ) rev( cStart );
                    else c.Plugin.RealPlugin.Start( cStart );
                }
            }
        }

        static void SetServiceSatus( 
            List<Action<IYodiiEngine>> postStartActions, 
            List<StStopContext> toStop, 
            List<StStartContext> toStart, 
            bool isTransition )
        {
            foreach( var stopC in toStop )
            {
                Debug.Assert( stopC.Success );
                var impact = stopC.ServiceImpact;
                while( impact != null )
                {
                    if( impact.SwappedImplementation != null )
                    {
                        if( !impact.Implementation.HotSwapped )
                        {
                            if( isTransition )
                            {
                                impact.Service.Status = ServiceStatus.StoppingSwapped;
                                impact.Service.RaiseStatusChanged( postStartActions, impact.SwappedImplementation.Plugin );
                            }
                            else
                            {
                                impact.Service.Status = ServiceStatus.Stopped;
                                impact.Service.RaiseStatusChanged( postStartActions );
                            }
                        }
                    }
                    else
                    {
                        impact.Service.Status = isTransition ? ServiceStatus.Stopping : ServiceStatus.Stopped;
                        impact.Service.RaiseStatusChanged( postStartActions );
                    }
                    impact = impact.ServiceGeneralization;
                }
            }
            // Sending Starting events...
            foreach( var startC in toStart )
            {
                Debug.Assert( startC.Success );
                var impact = startC.ServiceImpact;
                while( impact != null )
                {
                    if( impact.SwappedImplementation != null )
                    {
                        if( !impact.Implementation.HotSwapped )
                        {
                            if( isTransition )
                            {
                                impact.Service.Status = ServiceStatus.StartingSwapped;
                                impact.Service.RaiseStatusChanged( postStartActions, impact.Implementation.Plugin );
                            }
                            else
                            {
                                impact.Service.Status = ServiceStatus.StartedSwapped;
                                impact.Service.RaiseStatusChanged( postStartActions );
                            }
                        }
                    }
                    else
                    {
                        impact.Service.Status = isTransition ? ServiceStatus.Starting : ServiceStatus.Started;
                        impact.Service.RaiseStatusChanged( postStartActions );
                    }
                    impact = impact.ServiceGeneralization;
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
                if( result.PluginInfo != pluginInfo )
                {
                    result.PluginInfo = pluginInfo;
                }
            }
            else
            {
                result = new PluginProxy( pluginInfo );
                _plugins.Add( pluginInfo.PluginFullName, result );
            }
            return result;
        }


    }
}
