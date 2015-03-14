#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\YodiiHost.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yodii.Model;
using System.Reflection;
using System.Text;

namespace Yodii.Host
{
    public partial class YodiiHost : IYodiiEngineHost
    {
        readonly IActivityMonitor _monitor;
        readonly ServiceHost _serviceHost;
        readonly Dictionary<string, PluginProxy> _plugins;
        Func<IPluginInfo,object[],IYodiiPlugin> _pluginCreator;
        IYodiiEngineExternal _engine;
        bool _catchPreStartOrPreStopExceptions;

        public YodiiHost()
            : this( null, CatchExceptionGeneration.HonorIgnoreExceptionAttribute )
        {
        }

        public YodiiHost( IActivityMonitor monitor, CatchExceptionGeneration exceptionGeneration = CatchExceptionGeneration.HonorIgnoreExceptionAttribute )
        {
            if( monitor == null ) monitor = new ActivityMonitor( "Yodii.Host.YodiiHost");
            _monitor = monitor;
            _plugins = new Dictionary<string, PluginProxy>();
            _serviceHost = new ServiceHost( exceptionGeneration );
            _pluginCreator = DefaultPluginCreator;
        }

        IYodiiPlugin DefaultPluginCreator( IPluginInfo pluginInfo, object[] ctorParameters )
        {
            using( _monitor.OpenTrace().Send( "Using DefaultCreator for {0}.", pluginInfo.PluginFullName ) )
            {
                var tPlugin = Assembly.Load( pluginInfo.AssemblyInfo.AssemblyName ).GetType( pluginInfo.PluginFullName, true );
                var ctor = tPlugin.GetConstructors().OrderBy( c => c.GetParameters().Length ).LastOrDefault();
                if( ctor == null || ctor.GetParameters().Length != pluginInfo.ConstructorInfo.ParameterCount )
                {
                    throw new CKException( R.DefaultPluginCreatorUnableToFindCtor, pluginInfo.ConstructorInfo.ParameterCount, pluginInfo.PluginFullName );
                }
                if( ctorParameters.Any( p => p == null ) )
                {
                    throw new CKException( R.DefaultPluginCreatorUnresolvedParams );
                }
                return (IYodiiPlugin)ctor.Invoke( ctorParameters );
            }
        }

        /// <summary>
        /// Gets or sets the associated <see cref="IYodiiEngineExternal"/>. 
        /// It must be set before starting the engine and only once.
        /// </summary>
        public IYodiiEngineExternal Engine
        {
            get { return _engine; }
            set
            {
                if( _plugins.Count > 0 ) throw new InvalidOperationException( R.HostEngineMustBeSetBeforeStartingTheEngine );
                if( _engine != null && _engine != value ) throw new InvalidOperationException( R.HostEngineMustBeSetOnlyOnce );
                _engine = value;
            }
        }

        /// <summary>
        /// Gets or sets whether exceptions that occurred during calls to <see cref="IYodiiPlugin.PreStart"/> 
        /// or <see cref="IYodiiPlugin.PreStop"/> are intercepted and considered as if the PreStart/Stop rejected the transition.
        /// Defaults to false. Can be changed at any moment.
        /// </summary>
        public bool CatchPreStartOrPreStopExceptions
        {
            get { return _catchPreStartOrPreStopExceptions; }
            set { _catchPreStartOrPreStopExceptions = value; }
        }

        /// <summary>
        /// Gets or sets a function that is in charge of obtaining concrete plugin instances.
        /// The dynamic services parameters are available in the order 
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
            public Result( IReadOnlyList<IPluginHostApplyCancellationInfo> errors )
            {
                CancellationInfo = errors;
            }

            public IReadOnlyList<IPluginHostApplyCancellationInfo> CancellationInfo { get; private set; }

        }

        /// <summary>
        /// Attempts to execute a plan.
        /// </summary>
        /// <param name="solvedConfiguration">Configuration to apply.</param>
        /// <returns>A <see cref="IYodiiEngineHostApplyResult"/> that details the error if any.</returns>
        public IYodiiEngineHostApplyResult Apply(
            IReadOnlyList<KeyValuePair<IPluginInfo, RunningStatus>> solvedConfiguration, 
            Action<Action<IYodiiEngineExternal>> postStartActionsCollector )
        {
            CheckArguments( solvedConfiguration, postStartActionsCollector );
            if( PluginCreator == null ) throw new InvalidOperationException( R.PluginCreatorIsNull );

            using( _monitor.OpenInfo().Send( "Applying plan..." ) )
            {
                var s = _monitor.Info();
                if( !s.IsRejected )
                {
                    var b = new StringBuilder();
                    foreach( var c in solvedConfiguration )
                    {
                        b.Append( c.Key.PluginFullName ).Append( " - " ).Append( c.Value ).AppendLine();
                    }
                    s.Send( b.ToString() );
                }
                // To be able to share information if hard stopping must be called, we need to instanciate the shared memory now.
                Dictionary<object, object> sharedMemory = new Dictionary<object, object>();
                try
                {
                    return DoApply( solvedConfiguration, postStartActionsCollector, sharedMemory );
                }
                catch( Exception ex )
                {
                    _monitor.Fatal().Send( ex );
                    HardStop( sharedMemory );
                    throw;
                }
            }
        }

        IYodiiEngineHostApplyResult DoApply(
            IReadOnlyList<KeyValuePair<IPluginInfo, RunningStatus>> solvedConfiguration,
            Action<Action<IYodiiEngineExternal>> postStartActionsCollector,
            Dictionary<object, object> sharedMemory )
        {
            bool isEngineStopping = postStartActionsCollector == null;
            var errors = new List<CancellationInfo>();

            ServiceManager serviceManager = new ServiceManager( _serviceHost );
            // The toStart and toStop list are lists of StStart/StStopContext.
            // With the help of the ServiceManager, this resolves the issue to find swapped plugins (and their most specialized common service).
            List<StStopContext> toStop = new List<StStopContext>();
            List<StStartContext> toStart = new List<StStartContext>();

            using( _monitor.OpenTrace().Send( "Computing plugins to Stop from disabled ones: " ) )
            {
                #region Disabled Plugins
                foreach( var kp in solvedConfiguration )
                {
                    if( kp.Value == RunningStatus.Disabled )
                    {
                        IPluginInfo info = kp.Key;
                        PluginProxy p = EnsureProxy( info );
                        Debug.Assert( p.Status != PluginStatus.Stopping && p.Status != PluginStatus.Starting );
                        if( p.Status != PluginStatus.Null )
                        {
                            var preStop = new StStopContext( p, RunningStatus.Disabled, sharedMemory, p.Status == PluginStatus.Stopped, isEngineStopping );
                            if( info.Service != null ) serviceManager.AddToStop( info.Service, preStop );
                            toStop.Add( preStop );
                        }
                    }
                }
                _monitor.CloseGroup( toStop.Select( c => c.Plugin.PluginInfo.PluginFullName ).Concatenate() );
                #endregion
            }
            using( _monitor.OpenTrace().Send( "Adding plugins to Stop from stopping plugins: " ) )
            {
                #region Stopped Plugins
                foreach( var kp in solvedConfiguration )
                {
                    if( kp.Value == RunningStatus.Stopped )
                    {
                        IPluginInfo info = kp.Key;
                        PluginProxy p = EnsureProxy( info );
                        Debug.Assert( p.Status != PluginStatus.Stopping && p.Status != PluginStatus.Starting );
                        if( p.Status == PluginStatus.Started )
                        {
                            var preStop = new StStopContext( p, RunningStatus.Stopped, sharedMemory, false, isEngineStopping );
                            if( info.Service != null ) serviceManager.AddToStop( info.Service, preStop );
                            toStop.Add( preStop );
                        }
                        _monitor.CloseGroup( toStop.Select( c => c.Plugin.PluginInfo.PluginFullName ).Concatenate() );
                    }
                }
                #endregion
            }

            // This memorizes the plugins that are already running and needs to run but with a 
            // change in their IsRunningLocked status.
            List<PluginProxy> alreadyRunningPlugins = null;

            // Now, we attempt to activate the plugins that must run: if an error occurs,
            // we leave and return the error since we did not change anything.
            using( _monitor.OpenTrace().Send( "Registering running plugins: " ) )
            {
                #region Running Plugins. Leave on first Load error.
                #region Calling Constructors: Service calls are blocked.
                using( _serviceHost.BlockServiceCall( calledServiceType => new ServiceCallBlockedException( calledServiceType, R.CallingServiceFromCtor ) ) )
                {
                    foreach( var kp in solvedConfiguration )
                    {
                        if( kp.Value >= RunningStatus.Running )
                        {
                            IPluginInfo info = kp.Key;
                            PluginProxy p = EnsureProxy( info );
                            if( p.Status != PluginStatus.Started )
                            {
                                if( p.RealPluginObject == null )
                                {
                                    using( _monitor.OpenTrace().Send( "Instanciating '{0}'.", info.PluginFullName ) )
                                    {
                                        if( !p.TryLoad( _serviceHost, PluginCreator ) )
                                        {
                                            Debug.Assert( p.LoadError != null, "Error is catched by the PluginHost itself." );
                                            _monitor.Error().Send( p.LoadError, "Instanciation failed" );
                                            // Unable to load the plugin: leave now.
                                            errors.Add( new CancellationInfo( p.PluginInfo, true ) { Error = p.LoadError, ErrorMessage = R.ErrorWhileCreatingPluginInstance } );
                                            // Breaks the loop: stop on the first failure.
                                            // It is useless to pre load next plugins as long as we can be sure that they will not run now. 
                                            break;
                                        }
                                        Debug.Assert( p.Status == PluginStatus.Null );
                                    }
                                }
                                var preStart = new StStartContext( p, kp.Value, sharedMemory, p.Status == PluginStatus.Null );
                                p.Status = PluginStatus.Stopped;
                                if( info.Service != null ) serviceManager.AddToStart( info.Service, preStart );
                                toStart.Add( preStart );
                            }
                            else if( (kp.Value == RunningStatus.RunningLocked) != p.IsRunningLocked )
                            {
                                if( alreadyRunningPlugins == null ) alreadyRunningPlugins = new List<PluginProxy>();
                                alreadyRunningPlugins.Add( p );
                            }
                        }
                    }
                }
                #endregion
                if( errors.Count > 0 )
                {
                    // Restores Disabled states.
                    CancelSuccessfulStartForDisabled( toStart );
                    return new Result( errors.AsReadOnlyList() );
                }
                _monitor.CloseGroup( toStart.Select( c => c.Plugin.PluginInfo.PluginFullName ).Concatenate() );
                #endregion
            }

            // The toStart list of StStartContext is ready (and plugins inside are loaded without error).
            // Now starts the actual PreStop/PreStart/Stop/Start/Disable phase.
            // Service calls are allowed.
            using( _monitor.OpenTrace().Send( "Calling PreStop." ) )
            {
                #region Calling PreStop for all "toStop" plugins: calls to Services are allowed.
                foreach( var stopC in toStop )
                {
                    if( !stopC.IsDisabledOnly )
                    {
                        using( _monitor.OpenTrace().Send( "Plugin: {0}.", stopC.Plugin.PluginInfo.PluginFullName ) )
                        {
                            Debug.Assert( stopC.Plugin.Status == PluginStatus.Started );
                            try
                            {
                                stopC.Plugin.RealPluginObject.PreStop( stopC );
                            }
                            catch( Exception ex )
                            {
                                _monitor.Error().Send( ex );
                                if( !_catchPreStartOrPreStopExceptions ) throw;
                                if( postStartActionsCollector != null ) stopC.Cancel( ex.Message, ex );
                            }
                            if( stopC.HandleSuccess( errors, isPreStart: false ) )
                            {
                                stopC.Plugin.Status = PluginStatus.Stopping;
                            }
                        }
                    }
                }
                #endregion
            }
            // If at least one failed, cancel the start for the successful ones
            // and stops here.
            if( errors.Count > 0 )
            {
                CancelSuccessfulPreStop( sharedMemory, toStop );
                // Restores Disabled states.
                CancelSuccessfulStartForDisabled( toStart );
                return new Result( errors.AsReadOnlyList() );
            }

            // PreStop has been successfully called: we must now call the PreStart.
            // Calls to Services are not allowed during PreStart.
            using( _monitor.OpenTrace().Send( "Calling PreStart." ) )
            using( _serviceHost.BlockServiceCall( calledServiceType => new ServiceCallBlockedException( calledServiceType, R.CallingServiceFromPreStart ) ) )
            {
                #region Calling PreStart.
                foreach( var startC in toStart )
                {
                    Debug.Assert( startC.Plugin.Status == PluginStatus.Stopped );
                    try
                    {
                        startC.Plugin.RealPluginObject.PreStart( startC );
                    }
                    catch( Exception ex )
                    {
                        _monitor.Error().Send( ex );
                        if( !_catchPreStartOrPreStopExceptions ) throw;
                        startC.Cancel( ex.Message, ex );
                    }
                    if( startC.HandleSuccess( errors, true ) )
                    {
                        startC.Plugin.Status = PluginStatus.Starting;
                    }
                }
                #endregion
            }
            // If a PreStart failed, we cancel everything and stop.
            // Calls to Services are not allowed during Stop or rollback actions.
            if( errors.Count > 0 )
            {
                CancelSuccessfulPreStop( sharedMemory, toStop );
                using( _serviceHost.BlockServiceCall( calledServiceType => new ServiceCallBlockedException( calledServiceType, R.CallingServiceFromPreStartRollbackAction ) ) )
                {
                    var cStop = new CancelPreStartContext( sharedMemory );
                    foreach( var c in toStart )
                    {
                        if( c.Success )
                        {
                            c.Plugin.Status = PluginStatus.Stopped;
                            var rev = c.RollbackAction;
                            if( rev != null ) rev( cStop );
                            else c.Plugin.RealPluginObject.Stop( cStop );
                        }
                    }
                }
                // Restores Disabled states.
                CancelSuccessfulStartForDisabled( toStart );
                return new Result( errors.AsReadOnlyList() );
            }

            // Setting ServiceStatus & sending events: StoppingSwapped, Stopping, StartingSwapped, Starting. 
            SetServiceSatus( postStartActionsCollector, toStop, toStart, true );

            // Time to call Stop. While Stopping, calling Services is not allowed.
            using( _monitor.OpenTrace().Send( "Calling Stop." ) )
            using( _serviceHost.BlockServiceCall( calledServiceType => new ServiceCallBlockedException( calledServiceType, R.CallingServiceFromStop ) ) )
            {
                // Calling Stop must not throw any exceptions: we let the exception bubble here.
                foreach( var stopC in toStop )
                {
                    if( !stopC.IsDisabledOnly )
                    {
                        Debug.Assert( stopC.Plugin.Status == PluginStatus.Stopping );
                        stopC.Plugin.SetRunningLocked( false );
                        stopC.Plugin.Status = PluginStatus.Stopped;
                        stopC.Plugin.RealPluginObject.Stop( stopC );
                    }
                }
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
                    Debug.Assert( impact.Implementation.Plugin == startC.Plugin || impact.SwappedImplementation.Plugin == startC.Plugin );
                    impact.Service.SetPluginImplementation( startC.Plugin );
                    impact = impact.ServiceGeneralization;
                }
            }
            // Now that all services are bound, starts the plugin.
            // Calling Start must not throw any exceptions: we let the exception buble here.
            foreach( var startC in toStart )
            {
                startC.Plugin.SetRunningLocked( startC.RunningStatus == RunningStatus.RunningLocked );
                startC.Plugin.Status = PluginStatus.Started;
                startC.Plugin.RealPluginObject.Start( startC );
            }
            if( alreadyRunningPlugins != null )
            {
                foreach( var p in alreadyRunningPlugins ) p.SetRunningLocked( !p.IsRunningLocked );
            }
            // Setting services status & sending events...
            SetServiceSatus( postStartActionsCollector, toStop, toStart, false );

            // Disabling plugins that need to.
            // Calls to services are disabled.
            using( _monitor.OpenTrace().Send( "Disabling plugins (calling Dispose for IDisposable)." ) )
            using( _serviceHost.BlockServiceCall( calledServiceType => new ServiceCallBlockedException( calledServiceType, R.CallingServiceFromDisable ) ) )
            {
                foreach( var disableC in toStop )
                {
                    if( disableC.MustDisable )
                    {
                        disableC.Plugin.Disable( _monitor );
                        var impact = disableC.ServiceImpact;
                        while( impact != null && !impact.Starting )
                        {
                            if( impact.Service.Implementation == disableC.Plugin )
                            {
                                impact.Service.SetPluginImplementation( null );
                            }
                            impact = impact.ServiceGeneralization;
                        }
                    }
                }
            }
            return new Result( errors.AsReadOnlyList() );
        }

        private static void CheckArguments( IReadOnlyList<KeyValuePair<IPluginInfo, RunningStatus>> configuration, Action<Action<IYodiiEngineExternal>> postStartActionsCollector )
        {
            HashSet<IPluginInfo> uniqueCheck = new HashSet<IPluginInfo>();
            foreach( var p in configuration )
            {
                if( !uniqueCheck.Add( p.Key ) )
                {
                    throw new ArgumentException( String.Format( R.HostApplyPluginMustBeInOneList, p.Key.PluginFullName ) );
                }
                if( p.Value != RunningStatus.Disabled )
                {
                    if( postStartActionsCollector == null ) throw new ArgumentException( R.HostApplyHasStoppedOrRunningPluginWhileEngineIsStopping );
                }
            }
        }

        private void CancelSuccessfulStartForDisabled( List<StStartContext> toStart )
        {
            foreach( var disableC in toStart )
            {
                if( disableC.WasDisabled ) disableC.Plugin.Disable( _monitor );
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
                    else c.Plugin.RealPluginObject.Start( cStart );
                }
            }
        }

        void SetServiceSatus(
            Action<Action<IYodiiEngineExternal>> postStartActionsCollector, 
            List<StStopContext> toStop, 
            List<StStartContext> toStart, 
            bool isTransition )
        {
            using( _monitor.OpenInfo().Send( isTransition ? "Sending Stopping service events." : "Sending Stopped service events." ) )
            {
                foreach( var stopC in toStop )
                {
                    Debug.Assert( stopC.Success );
                    var impact = stopC.ServiceImpact;
                    while( impact != null )
                    {
                        if( impact.SwappedImplementation != null )
                        {
                            Debug.Assert( impact.Implementation == stopC );
                            if( !impact.Implementation.HotSwapped && isTransition )
                            {
                                impact.Service.Status = ServiceStatus.StoppingSwapped;
                                impact.Service.RaiseStatusChanged( postStartActionsCollector, impact.SwappedImplementation.Plugin );
                            }
                        }
                        else
                        {
                            impact.Service.Status = isTransition ? ServiceStatus.Stopping : ServiceStatus.Stopped;
                            impact.Service.RaiseStatusChanged( postStartActionsCollector );
                        }
                        impact = impact.ServiceGeneralization;
                    }
                }
            }
            using( _monitor.OpenInfo().Send( isTransition ? "Sending Starting service events." : "Sending Started service events." ) )
            {
                foreach( var startC in toStart )
                {
                    Debug.Assert( startC.Success );
                    var impact = startC.ServiceImpact;
                    while( impact != null )
                    {
                        if( impact.SwappedImplementation != null )
                        {
                            Debug.Assert( impact.SwappedImplementation == startC );
                            if( !impact.Implementation.HotSwapped )
                            {
                                if( isTransition )
                                {
                                    impact.Service.Status = ServiceStatus.StartingSwapped;
                                    impact.Service.RaiseStatusChanged( postStartActionsCollector, impact.Implementation.Plugin );
                                }
                                else
                                {
                                    impact.Service.Status = ServiceStatus.StartedSwapped;
                                    impact.Service.RaiseStatusChanged( postStartActionsCollector );
                                }
                            }
                        }
                        else
                        {
                            impact.Service.Status = isTransition ? ServiceStatus.Starting : ServiceStatus.Started;
                            impact.Service.RaiseStatusChanged( postStartActionsCollector );
                        }
                        impact = impact.ServiceGeneralization;
                    }
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
                result = new PluginProxy( _engine, pluginInfo );
                _plugins.Add( pluginInfo.PluginFullName, result );
            }
            return result;
        }


    }
}
