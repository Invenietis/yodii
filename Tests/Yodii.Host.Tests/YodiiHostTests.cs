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
        public void ToSeeWhatHappensChoucrouteTest1()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.Host.Tests.ChoucroutePlugin", ConfigurationStatus.Optional );

            var result = engine.Start();
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin" ).Start();
            IPluginProxy pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin", false );
            ChoucroutePlugin choucroute = (ChoucroutePlugin)pluginProxy.RealPluginObject;
            Assert.That( choucroute.CalledMethods.Count == 3 );
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin", false ).Status == InternalRunningStatus.Started );

            Assert.That( engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin" ).Capability.CanStop == true );
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin" ).Stop();
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin", false ).Status == InternalRunningStatus.Stopped );
            Assert.That( choucroute.CalledMethods.Count == 5 );//plugin is stopped but we can still directly access it since assembly is still there.

            IChoucrouteService service= (IChoucrouteService)host.ServiceHost.GetProxy( typeof( IChoucrouteService ) );
            Assert.Throws<Yodii.Model.ServiceStoppedException>( (delegate() { int i =service.CalledMethods.Count; }) );

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin" ).Start();
            Assert.That( service.CalledMethods.Count == 7 );

            IDiscoveredInfo info2 = discoverer.GetDiscoveredInfo();
            engine.SetDiscoveredInfo( info );
            //test that the pluginproxy hasn't changed after a getDiscoveredInfo
            IPluginProxy pluginProxy2 = host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin", false );
            ChoucroutePlugin choucroute2 = (ChoucroutePlugin)pluginProxy.RealPluginObject;

            Assert.That( pluginProxy == pluginProxy2 );
            Assert.That( choucroute == choucroute2 );
        }
        /// <summary>
        /// When the first setup fails, the proxy isn't even made.
        /// </summary>
        [Test]
        public void IfSetupFailsChoucrouteTest2()
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
            IPluginProxy proxy=host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin2", false );
            Assert.IsNull( proxy );
        }
        /// <summary>
        /// The fact a stop fails is pretty much ignored.
        /// </summary>
        [Test]
        public void IfStopFailsChoucrouteTest3()
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
            IPluginProxy pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin3", false );
            ChoucroutePlugin3 choucroute = (ChoucroutePlugin3)pluginProxy.RealPluginObject;
            Assert.That( choucroute.CalledMethods.Count == 3 );//cstor setup start
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin3", false ).Status == InternalRunningStatus.Started );

            Assert.That( engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Capability.CanStop == true );
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Stop();
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin3", false ).Status == InternalRunningStatus.Stopped );
            Assert.That( choucroute.CalledMethods.Count == 5 );//+stop teardown

            IChoucrouteService3 service= (IChoucrouteService3)host.ServiceHost.GetProxy( typeof( IChoucrouteService3 ) );
            Assert.Throws<Yodii.Model.ServiceStoppedException>( (delegate() { int i =service.CalledMethods.Count; }) );

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin3" ).Start();
            Assert.That( service.CalledMethods.Count == 7 );//+setup start
        }
        /// <summary>
        /// If a start fails, the stop&teardown methods are called.
        /// </summary>
        [Test]
        public void IfSartFailsChoucrouteTest4()
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
            IPluginProxy pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin4", false );
            ChoucroutePlugin4 choucroute = (ChoucroutePlugin4)pluginProxy.RealPluginObject;
            Assert.That( choucroute.CalledMethods.Count == 5 );//ctor, setup, start (execption), stop, teardown.
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin4", false ).Status == InternalRunningStatus.Stopped );
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin4" ).Stop(); //we can call stop on an allready stopped plugin.
            Assert.That( choucroute.CalledMethods.Count == 5 ); //but this call for stop does not get to the plugin.

            IChoucrouteService4 service= (IChoucrouteService4)host.ServiceHost.GetProxy( typeof( IChoucrouteService4 ) );
            engine.CheckStopped( "Yodii.Host.Tests.IChoucrouteService4" );

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin4" ).Start();
            Assert.That( choucroute.CalledMethods.Count == 9 );//+setup, start (execption), stop, teardown.
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin4", false ).Status == InternalRunningStatus.Stopped );
        }
        /// <summary>
        /// Making sure the _impl of the generalized service updates itself.
        /// </summary>
        [Test]
        public void ServiceGeneralizationChoucrouteTest5()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );

            var result = engine.Start();

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.AnotherPlugin5" ).Start();
            IPluginProxy pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.AnotherPlugin5", false );
            AnotherPlugin5 anotherPlugin = (AnotherPlugin5)pluginProxy.RealPluginObject;

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.SimpleChoucroutePlugin5" ).Start();
            IPluginProxy pluginProxySimple = host.FindLoadedPlugin( "Yodii.Host.Tests.SimpleChoucroutePlugin5", false );
            SimpleChoucroutePlugin5 SimpleChoucroutePlugin = (SimpleChoucroutePlugin5)pluginProxySimple.RealPluginObject;

            //Don't hesitate to put the next line in the watch and look at _impl
            //(ServiceProxyBase)host.ServiceHost.GetProxy( typeof( IChoucrouteService5Generalization ) );

            int nbSimpleCalls = SimpleChoucroutePlugin.CalledMethods.Count;
            anotherPlugin.DoSomething();
            Assert.That( nbSimpleCalls + 1 == SimpleChoucroutePlugin.CalledMethods.Count, "making sure SimpleChoucroutePlugin was called" );

            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ElaborateChoucroutePlugin5" ).Start();
            IPluginProxy pluginProxyElaborate = host.FindLoadedPlugin( "Yodii.Host.Tests.ElaborateChoucroutePlugin5", false );
            ElaborateChoucroutePlugin5 ElaborateChoucroutePlugin = (ElaborateChoucroutePlugin5)pluginProxyElaborate.RealPluginObject;

            int nbElaborateCalls = ElaborateChoucroutePlugin.CalledMethods.Count;
            anotherPlugin.DoSomething();
            Assert.That( nbElaborateCalls + 1 == ElaborateChoucroutePlugin.CalledMethods.Count, "making sure ElaborateChoucroutePlugin was called" );
        }



        [Test]
        public void Host_ThrowsException_OnUnknownPluginCtorParameters()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );
            IYodiiEngineResult result = engine.Start();

            var testPlugin = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.PluginParameterTests.ParameterTestPlugin" );

            Assert.That( testPlugin, Is.Not.Null );

            // Try to start when MyCustomClass cannot be resolved
            testPlugin.Start();

            Assert.That( testPlugin.IsRunning, Is.False );
            Assert.That( testPlugin.CurrentError, Is.InstanceOf( typeof( InvalidPluginDefinitionException ) ) );

            engine.Stop();

            // Set a PluginCreator able to resolve MyCustomClass
            host.PluginCreator = CustomPluginCreator;

            // Try to start when MyCustomClass can be resolved
            result = engine.Start();

            testPlugin = engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.PluginParameterTests.ParameterTestPlugin" );

            Assert.That( testPlugin, Is.Not.Null );

            testPlugin.Start();

            Assert.That( testPlugin.IsRunning, Is.True );
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
