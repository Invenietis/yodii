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
using Yodii.Host.Tests.PluginParameterTests;

namespace Yodii.Host.Tests
{
    [TestFixture]
    public class YodiiHostTests
    {
        [Test]
        public void start_is_canceled_on_unresolved_pluginCtor_parameters()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );
            IYodiiEngineResult result = engine.Start();
            Assert.That( result.Success );

            var testPlugin = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.PluginParameterTests.ParameterTestPlugin" );
            Assert.That( testPlugin, Is.Not.Null );

            // Try to start when MyCustomClass cannot be resolved
            result = testPlugin.Start();
            Assert.That( result.Success, Is.False );

            Assert.That( testPlugin.IsRunning, Is.False );
            Assert.That( testPlugin.CurrentError.IsLoadError );
            Assert.That( testPlugin.CurrentError.Error, Is.InstanceOf( typeof( CKException ) ) );

            engine.Stop();

            // Set a PluginCreator able to resolve MyCustomClass
            host.PluginCreator = CustomPluginCreator;

            // Try to start when MyCustomClass can be resolved
            result = engine.Start();
            Assert.That( result.Success );
            testPlugin = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.PluginParameterTests.ParameterTestPlugin" );
            Assert.That( testPlugin, Is.Not.Null );

            result = testPlugin.Start();
            Assert.That( result.Success );
            Assert.That( testPlugin.IsRunning, Is.True );
        }

        [Test]
        public void simple_start_stop_of_a_plugin()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            IService<ITrackMethodCallsService> serviceS = host.ServiceHost.EnsureProxyForDynamicService<ITrackMethodCallsService>();
            ITrackMethodCallsService service = serviceS.Service;
            Assert.Throws<ServiceNotAvailableException>( (delegate() { int i = service.CalledMethods.Count; }), "Since the service has not implementation yet: ServiceNotAvailableException." );

            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );

            var result = engine.Start();
            Assert.That( result.Success );

            var pLive = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );
            result = pLive.Start();
            Assert.That( result.Success );

            Assert.That( service.CalledMethods.Count, Is.EqualTo( 3 ), "The service is started, we can call its methods." );

            // This is a direct access to the plugin (bypassing tha Service proxy):
            var proxy = host.FindLoadedPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );
            Assert.That( proxy.Status == PluginStatus.Started );
            TrackMethodCallsPlugin choucroute = (TrackMethodCallsPlugin)proxy.RealPluginObject;
            Assert.That( choucroute.CalledMethods.Count == 3 );

            Assert.That( pLive.Capability.CanStop == true );
            result = pLive.Stop();
            Assert.That( result.Success );
            
            Assert.That( proxy.Status == PluginStatus.Stopped );
            Assert.That( choucroute.CalledMethods.Count, Is.EqualTo( 5 ), "Plugin is stopped but we can still directly access it." );
            Assert.Throws<ServiceStoppedException>( (delegate() { int i = service.CalledMethods.Count; }), "Since the service is stopped: ServiceNotAvailableException." );

            result = pLive.Start();
            Assert.That( result.Success );
            Assert.That( service.CalledMethods.Count == 7, "The service is started, we can call its methods." );

            IDiscoveredInfo info2 = discoverer.GetDiscoveredInfo();
            engine.SetDiscoveredInfo( info );
            // Test that the pluginproxy hasn't changed after a getDiscoveredInfo
            IPluginProxy proxy2 = host.FindLoadedPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );
            var pLive2 = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.TrackMethodCallsPlugin" );

            Assert.That( proxy == proxy2, "With a new DiscoveredInfo, proxy is the same." );
            Assert.That( pLive == pLive2, "With a new DiscoveredInfo, PluginLiveInfo is the same." );
            TrackMethodCallsPlugin choucroute2 = (TrackMethodCallsPlugin)proxy.RealPluginObject;
            Assert.That( choucroute == choucroute2, "The plugin itself is the same object." );
        }


        /// <summary>
        /// When the first setup fails, the proxy isn't even made.
        /// </summary>
        [Test]
        public void TOBEREVIEWED_IfSetupFailsChoucrouteTest2()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.Host.Tests.ChoucroutePlugin2", ConfigurationStatus.Optional );

            var result = engine.Start();
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin2" ).Start();
            engine.CheckStopped( "Yodii.Host.Tests.IChoucrouteService2" );
            IPluginProxy proxy = host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin2" );
            Assert.IsNull( proxy );
        }


        /// <summary>
        /// The fact a stop fails is pretty much ignored.
        /// </summary>
        [Test]
        public void TOBEREVIEWED_IfStopFailsChoucrouteTest3()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost(); /*IYodiiEngineHost this is not enough, need access to PluginCreator & ServiceReferencesBinder*/
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.Host.Tests.ChoucroutePlugin3", ConfigurationStatus.Optional );

            var result = engine.Start();
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Start();
            IPluginProxy pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" );
            ChoucroutePlugin3 choucroute = (ChoucroutePlugin3)pluginProxy.RealPluginObject;
            Assert.That( choucroute.CalledMethods.Count == 3 );//cstor setup start
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Status == PluginStatus.Started );

            Assert.That( engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Capability.CanStop == true );
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Stop();
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Status == PluginStatus.Stopped );
            Assert.That( choucroute.CalledMethods.Count == 5 );//+stop teardown

            IChoucrouteService3 service= (IChoucrouteService3)host.ServiceHost.GetProxy( typeof( IChoucrouteService3 ) );
            Assert.Throws<ServiceStoppedException>( (delegate() { int i = service.CalledMethods.Count; }) );

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Start();
            Assert.That( service.CalledMethods.Count == 7 );//+setup start
        }
        /// <summary>
        /// If a start fails, the stop&teardown methods are called.
        /// </summary>
        [Test]
        public void TOBEREVIEWED_IfSartFailsChoucrouteTest4()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.Host.Tests.ChoucroutePlugin4", ConfigurationStatus.Optional );

            var result = engine.Start();
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin4" ).Start();
            IPluginProxy pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin4"  );
            ChoucroutePlugin4 choucroute = (ChoucroutePlugin4)pluginProxy.RealPluginObject;
            Assert.That( choucroute.CalledMethods.Count == 5 );//ctor, setup, start (execption), stop, teardown.
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin4" ).Status == PluginStatus.Stopped );
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin4" ).Stop(); //we can call stop on an allready stopped plugin.
            Assert.That( choucroute.CalledMethods.Count == 5 ); //but this call for stop does not get to the plugin.

            IChoucrouteService4 service= (IChoucrouteService4)host.ServiceHost.GetProxy( typeof( IChoucrouteService4 ) );
            engine.CheckStopped( "Yodii.Host.Tests.IChoucrouteService4" );

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin4" ).Start();
            Assert.That( choucroute.CalledMethods.Count == 9 );//+setup, start (execption), stop, teardown.
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin4" ).Status == PluginStatus.Stopped );
        }
        /// <summary>
        /// Making sure the _impl of the generalized service updates itself.
        /// </summary>
        [Test]
        public void TOBEREVIEWED_ServiceGeneralizationChoucrouteTest5()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );

            var result = engine.Start();

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.AnotherPlugin5" ).Start();
            IPluginProxy pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.AnotherPlugin5" );
            AnotherPlugin5 anotherPlugin = (AnotherPlugin5)pluginProxy.RealPluginObject;

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.SimpleChoucroutePlugin5" ).Start();
            IPluginProxy pluginProxySimple = host.FindLoadedPlugin( "Yodii.Host.Tests.SimpleChoucroutePlugin5" );
            SimpleChoucroutePlugin5 SimpleChoucroutePlugin = (SimpleChoucroutePlugin5)pluginProxySimple.RealPluginObject;

            //Don't hesitate to put the next line in the watch and look at _impl
            //(ServiceProxyBase)host.ServiceHost.GetProxy( typeof( IChoucrouteService5Generalization ) );

            int nbSimpleCalls = SimpleChoucroutePlugin.CalledMethods.Count;
            anotherPlugin.DoSomething();
            Assert.That( nbSimpleCalls + 1 == SimpleChoucroutePlugin.CalledMethods.Count, "making sure SimpleChoucroutePlugin was called" );

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ElaborateChoucroutePlugin5" ).Start();
            IPluginProxy pluginProxyElaborate = host.FindLoadedPlugin( "Yodii.Host.Tests.ElaborateChoucroutePlugin5" );
            ElaborateChoucroutePlugin5 ElaborateChoucroutePlugin = (ElaborateChoucroutePlugin5)pluginProxyElaborate.RealPluginObject;

            int nbElaborateCalls = ElaborateChoucroutePlugin.CalledMethods.Count;
            anotherPlugin.DoSomething();
            Assert.That( nbElaborateCalls + 1 == ElaborateChoucroutePlugin.CalledMethods.Count, "making sure ElaborateChoucroutePlugin was called" );
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
