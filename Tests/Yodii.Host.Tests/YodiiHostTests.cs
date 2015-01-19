#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\YodiiHostTests.cs) is part of CiviKey. 
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
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Yodii.Model;
using Yodii.Engine;
using Yodii.Discoverer;
using CK.Core;
using System.IO;
using Yodii;

namespace Yodii.Host.Tests
{

    [TestFixture]
    public class YodiiHostTests
    {
        [Test]
        public void start_is_canceled_on_unresolved_pluginCtor_parameters()
        {
            YodiiHost host = new YodiiHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.Configuration.SetDiscoveredInfo( TestHelper.GetDiscoveredInfoInThisAssembly() );
            IYodiiEngineResult result = engine.StartEngine();
            Assert.That( result.Success );

            var trackerLive = engine.LiveInfo.FindService( "Yodii.Host.Tests.ITrackMethodCallsPluginService" );
            var trackerProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );
            var pluginLive = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ParameterTestPlugin" );
            var pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.ParameterTestPlugin" );
            Assert.That( pluginLive, Is.Not.Null );
            Assert.That( trackerLive.Capability.CanStart );
            Assert.That( trackerProxy.Status == PluginStatus.Null );
            Assert.That( pluginProxy.Status == PluginStatus.Null );

            // Try to start when MyCustomClass cannot be resolved
            result = engine.StartItem( pluginLive );
            Assert.That( result.Success, Is.False );

            Assert.That( pluginLive.IsRunning, Is.False );
            Assert.That( pluginLive.CurrentError.IsLoadError );
            Assert.That( pluginLive.CurrentError.Error, Is.InstanceOf( typeof( CKException ) ) );
            Assert.That( !trackerLive.IsRunning );
            Assert.That( trackerProxy.Status == PluginStatus.Null );
            Assert.That( pluginProxy.Status == PluginStatus.Null );

            // Set a PluginCreator able to resolve MyCustomClass
            host.PluginCreator = CustomPluginCreator;

            // Try to start when MyCustomClass can be resolved
            result = engine.StartItem( pluginLive );

            Assert.That( result.Success );
            Assert.That( pluginLive.IsRunning, Is.True );
            Assert.That( trackerLive.IsRunning, "Tracker plugin has started." );

            result = engine.StopItem( pluginLive );
            Assert.That( result.Success );
            Assert.That( pluginLive.IsRunning, Is.False );
            Assert.That( trackerLive.IsRunning, Is.False, "Tracker plugin has stopped." );
        }

        [Test]
        public void simple_start_stop_of_a_plugin()
        {
            YodiiHost host = new YodiiHost();
            IService<ITrackMethodCallsPluginService> serviceS = host.ServiceHost.EnsureProxyForDynamicService<ITrackMethodCallsPluginService>();
            ITrackMethodCallsPluginService service = serviceS.Service;
            Assert.Throws<ServiceNotAvailableException>( (delegate() { int i = service.CalledMethods.Count; }), "Since the service has not implementation yet: ServiceNotAvailableException." );

            YodiiEngine engine = new YodiiEngine( host );
            engine.Configuration.SetDiscoveredInfo( TestHelper.GetDiscoveredInfoInThisAssembly() );

            var result = engine.StartEngine();
            Assert.That( result.Success );

            var pLive = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );
            result = engine.StartItem( pLive );
            Assert.That( result.Success );

            Assert.That( service.CalledMethods.Count, Is.EqualTo( 3 ), "The service is started, we can call its methods." );

            // This is a direct access to the plugin (bypassing tha Service proxy):
            var proxy = host.FindLoadedPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );
            Assert.That( proxy.Status == PluginStatus.Started );
            TrackMethodCallsPlugin choucroute = (TrackMethodCallsPlugin)proxy.RealPluginObject;
            Assert.That( choucroute.CalledMethods.Count == 3 );

            Assert.That( pLive.Capability.CanStop == true );
            result = engine.StopItem( pLive );
            Assert.That( result.Success );
            
            Assert.That( proxy.Status == PluginStatus.Stopped );
            Assert.That( choucroute.CalledMethods.Count, Is.EqualTo( 5 ), "Plugin is stopped but we can still directly access it." );
            Assert.Throws<ServiceStoppedException>( (delegate() { int i = service.CalledMethods.Count; }), "Since the service is stopped: ServiceNotAvailableException." );

            result = engine.StartItem( pLive );
            Assert.That( result.Success );
            Assert.That( service.CalledMethods.Count == 7, "The service is started, we can call its methods." );

            StandardDiscoverer discoverer = new StandardDiscoverer();
            discoverer.ReadAssembly( Assembly.GetExecutingAssembly().Location );
            IDiscoveredInfo info2 = discoverer.GetDiscoveredInfo();
            engine.Configuration.SetDiscoveredInfo( info2 );
            // Test that the pluginproxy hasn't changed after a getDiscoveredInfo
            IPluginProxy proxy2 = host.FindLoadedPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );
            var pLive2 = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );

            Assert.That( proxy == proxy2, "With a new DiscoveredInfo, proxy is the same." );
            Assert.That( pLive == pLive2, "With a new DiscoveredInfo, PluginLiveInfo is the same." );
            TrackMethodCallsPlugin choucroute2 = (TrackMethodCallsPlugin)proxy.RealPluginObject;
            Assert.That( choucroute == choucroute2, "The plugin itself is the same object." );
        }

        [Test]
        public void when_PreSart_or_PreStop_fails()
        {
            YodiiHost host = new YodiiHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.Configuration.SetDiscoveredInfo( TestHelper.GetDiscoveredInfoInThisAssembly() );
            engine.Configuration.Layers.Default.Items.Set( "Yodii.Host.Tests.ITrackMethodCallsPluginService", ConfigurationStatus.Running );

            var result = engine.StartEngine();
            Assert.That( result.Success, Is.True );
            var pShouldRun = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );
            Assert.That( pShouldRun.IsRunning, Is.True, "It is running by configuration." );

            var anotherServiceLive = engine.LiveInfo.FindService( "Yodii.Host.Tests.IAnotherService" );
            var trackerServiceLive = engine.LiveInfo.FindService( "Yodii.Host.Tests.ITrackerService" );
            var pLive = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.FailureTransitionPlugin" );
            IPluginProxy proxy = host.FindLoadedPlugin( "Yodii.Host.Tests.FailureTransitionPlugin" );

            Assert.That( !pLive.IsRunning && pLive.Capability.CanStart );
            Assert.That( proxy.Status, Is.EqualTo( PluginStatus.Null ) );
            Assert.That( !anotherServiceLive.IsRunning && anotherServiceLive.Capability.CanStart );
            Assert.That( !trackerServiceLive.IsRunning && trackerServiceLive.Capability.CanStart );

            FailureTransitionPlugin.CancelPreStart = true;
            FailureTransitionPlugin.CancelPreStop = false;

            Assert.That( pLive.IsRunning, Is.False );
            result = engine.StartItem( pLive );
            Assert.That( result.Success, Is.False );
            Assert.That( result.HostFailureResult.ErrorPlugins[0].CancellationInfo.ErrorMessage, Is.EqualTo( "Canceled!" ) );
            engine.CheckStopped( "Yodii.Host.Tests.IFailureTransitionPluginService" );

            Assert.That( pShouldRun.IsRunning, Is.True, "It is still running by configuration." );
            Assert.That( proxy.Status, Is.EqualTo( PluginStatus.Stopped ), "It has been suceesfully loaded and since it does not support IDisposable, the instance is kept." );
            Assert.That( !anotherServiceLive.IsRunning && anotherServiceLive.Capability.CanStart, "IAnotherService has not started." );
            Assert.That( !trackerServiceLive.IsRunning && trackerServiceLive.Capability.CanStart, "ITrackerService has not started." );

            FailureTransitionPlugin.CancelPreStart = false;
            result = engine.StartItem( pLive );
            Assert.That( result.Success, Is.True );
            Assert.That( proxy.Status, Is.EqualTo( PluginStatus.Started ), "Now Started: it will then be Stopped." );
            Assert.That( pShouldRun.IsRunning, Is.True, "It is still running by configuration." );
            Assert.That( anotherServiceLive.IsRunning, "IAnotherService is now running." );
            Assert.That( trackerServiceLive.IsRunning, "ITrackerService is now running." );

            result = engine.StopItem( pLive );
            Assert.That( result.Success, Is.True );
            Assert.That( proxy.Status, Is.EqualTo( PluginStatus.Stopped ) );
            Assert.That( pShouldRun.IsRunning, Is.True, "It is still running by configuration." );
            Assert.That( !anotherServiceLive.IsRunning, "IAnotherService is stopped." );
            Assert.That( !trackerServiceLive.IsRunning, "ITrackerService is stopped." );

            result = engine.StartItem( pLive );
            Assert.That( result.Success, Is.True );
            Assert.That( proxy.Status, Is.EqualTo( PluginStatus.Started ) );
            Assert.That( pShouldRun.IsRunning, Is.True, "It is still running by configuration." );
            Assert.That( anotherServiceLive.IsRunning, "IAnotherService is now running again." );
            Assert.That( trackerServiceLive.IsRunning, "ITrackerService is now running again." );

            FailureTransitionPlugin.CancelPreStop = true;
            result = engine.StopItem( pLive );
            Assert.That( result.Success, Is.False );
            Assert.That( result.HostFailureResult.ErrorPlugins[0].CancellationInfo.ErrorMessage, Is.EqualTo( "Canceled!" ) );
            Assert.That( proxy.Status, Is.EqualTo( PluginStatus.Started ), "Still running!" );
            Assert.That( pShouldRun.IsRunning, Is.True, "It is still running by configuration." );
            Assert.That( anotherServiceLive.IsRunning, "IAnotherService is still running." );
            Assert.That( trackerServiceLive.IsRunning, "ITrackerService is still running." );

            // When the engine stops, cancelling PreStop is forbidden.
            Assert.Throws<InvalidOperationException>( () => engine.StopEngine() );
            
            FailureTransitionPlugin.CancelPreStop = false;
            engine.StopEngine();
        }

        [Test]
        public void when_PreSart_or_PreStop_fails_with_a_disposable_plugin()
        {
            YodiiHost host = new YodiiHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.Configuration.SetDiscoveredInfo( TestHelper.GetDiscoveredInfoInThisAssembly() );

            var p = new PluginWrapper<FailureTransitionPluginDisposable>( engine, host );

            engine.StartEngine().CheckSuccess();
            p.CheckState( PluginStatus.Null );

            FailureTransitionPlugin.CancelPreStart = true;
            FailureTransitionPlugin.CancelPreStop = false;

            engine.StartItem( p.Live ).CheckHostFailure();
            p.CheckState( PluginStatus.Null );

            FailureTransitionPlugin.CancelPreStart = false;
            engine.StartItem( p.Live ).CheckSuccess();
            p.CheckState( PluginStatus.Started );

            FailureTransitionPlugin.CancelPreStop = true;
            engine.StopItem( p.Live ).CheckHostFailure();
            p.CheckState( PluginStatus.Started );

            engine.Configuration.Layers.Default.Items.Set( p.Live.PluginInfo.PluginFullName, ConfigurationStatus.Disabled ).CheckHostFailure();
            p.CheckState( PluginStatus.Started );

            FailureTransitionPlugin.CancelPreStop = false;
            engine.StopItem( p.Live ).CheckSuccess();
            p.CheckState( PluginStatus.Stopped );

            engine.StartItem( p.Live ).CheckSuccess();
            p.CheckState( PluginStatus.Started );

            engine.Configuration.Layers.Default.Items.Set( p.Live.PluginInfo.PluginFullName, ConfigurationStatus.Disabled ).CheckSuccess();
            p.CheckState( PluginStatus.Null );

        }

        static object ResolveUnknownType( Type t )
        {
            if( t == typeof( MyCustomClass ) )
            {
                return new MyCustomClass();
            }
            return null;
        }

        static IYodiiPlugin CustomPluginCreator( IPluginInfo pluginInfo, object[] ctorServiceParameters )
        {
            // Get the plugin Type
            var tPlugin = Assembly.Load( pluginInfo.AssemblyInfo.AssemblyName ).GetType( pluginInfo.PluginFullName, true );

            // Pick the constructor used (always the lengthiest one)
            var ctor = tPlugin.GetConstructors().OrderBy( c => c.GetParameters().Length ).Last();

            ParameterInfo[] parameters = ctor.GetParameters();

            // Create the parameters array, copying resolved services as needed from ctorServiceParameters
            object[] ctorParameters = new object[parameters.Length];
            Debug.Assert( ctorParameters.Length >= ctorServiceParameters.Length );

            for( int i = 0; i < parameters.Length; i++ )
            {
                ParameterInfo p = parameters[i];
                object serviceInstance = ctorServiceParameters.Length >= (i + 1) ? ctorServiceParameters[i] : null;

                if( serviceInstance != null )
                {
                    ctorParameters[i] = serviceInstance;
                }
                else
                {
                    ctorParameters[i] = ResolveUnknownType( p.ParameterType );
                }
            }

            return (IYodiiPlugin)ctor.Invoke( ctorParameters );
        }
    }
}
