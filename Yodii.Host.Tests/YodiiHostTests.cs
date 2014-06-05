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

            PluginHost host = new PluginHost(); /*IYodiiEngineHost this is not enough, need access to PluginCreator & ServiceReferencesBinder*/
            YodiiEngine engine = new YodiiEngine( host);
            engine.SetDiscoveredInfo(info);

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.Host.Tests.ChoucroutePlugin", ConfigurationStatus.Optional );
                         
            var result = engine.Start();
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin" ).Start();
            IPluginProxy pluginProxy = host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin", false );
            ChoucroutePlugin choucroute = (ChoucroutePlugin) pluginProxy.RealPluginObject;
            Assert.That( choucroute.CalledMethods.Count == 3 );
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin", false ).Status == InternalRunningStatus.Started );

            Assert.That(engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin" ).Capability.CanStop==true);
            engine.LiveInfo.FindPlugin( "Yodii.Host.Tests.ChoucroutePlugin" ).Stop();
            Assert.That( host.FindLoadedPlugin( "Yodii.Host.Tests.ChoucroutePlugin", false ).Status == InternalRunningStatus.Stopped );
            Assert.That( choucroute.CalledMethods.Count == 5 );//plugin is stopped but we can still directly access it ?

            IChoucrouteService service= (IChoucrouteService)host.ServiceHost.GetProxy(typeof( IChoucrouteService) );
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
        //TODO (short term): see what happens if te plugin does not work properly
        //Tester exception balancé au start
        //Tester exception balancé au stop
    }
}
