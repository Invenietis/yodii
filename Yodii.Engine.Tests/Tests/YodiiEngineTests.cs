using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;
using Yodii.Engine.Tests.Mocks;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    class YodiiEngineTests
    {
        [Test]
        public void EngineCreationTest()
        {
            YodiiEngineHostMock fakeMock = null;
            var ex = Assert.Throws<ArgumentNullException>(() => new YodiiEngine(fakeMock));
            Assert.That(ex.ParamName, Is.EqualTo( "host" ));

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            Assert.That( engine.Host, Is.Not.Null );
            Assert.That( engine.IsRunning, Is.False );
            Assert.That( engine.Configuration, Is.Not.Null );
        }

        [Test]
        public void EngineUseTest()
        {
            IYodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            Assert.Throws<ArgumentNullException>( () => engine.SetDiscoveredInfo( null ) );

            DiscoveredInfo discoveredInfo = MockInfoFactory.CreateGraph003();

            IYodiiEngineResult result = engine.SetDiscoveredInfo( discoveredInfo );
            Assert.That( result.Success, Is.True );
            Assert.That( engine.DiscoveredInfo == discoveredInfo );

            PluginInfo pluginA1 = discoveredInfo.FindPlugin( "PluginA-1" );
            PluginInfo pluginA2 = discoveredInfo.FindPlugin( "PluginA-2" );
            ServiceInfo serviceA = discoveredInfo.FindService( "ServiceA" );



            result = engine.Start();
            Assert.That( result.Success, Is.True );
            Assert.That( engine.LiveInfo, Is.Not.Null );
            Assert.That( engine.LiveInfo.FindPlugin( pluginA1.PluginFullName ).PluginInfo, Is.EqualTo( pluginA1 ) );
            Assert.That( engine.LiveInfo.FindPlugin( pluginA2.PluginFullName ).PluginInfo, Is.EqualTo( pluginA2 ) );
            Assert.That( engine.LiveInfo.FindService( serviceA.ServiceFullName ).ServiceInfo, Is.EqualTo( serviceA ) );

            Assert.That( engine.LiveInfo.FindPlugin( pluginA1.PluginFullName ).IsRunning, Is.False );
            Assert.That( engine.LiveInfo.FindPlugin( pluginA2.PluginFullName ).IsRunning, Is.False );
            Assert.That( engine.LiveInfo.FindService( serviceA.ServiceFullName ).IsRunning, Is.False );

            Assert.That( engine.LiveInfo.FindPlugin( pluginA1.PluginFullName ).RunningStatus, Is.EqualTo( RunningStatus.Stopped ) );
            Assert.That( engine.LiveInfo.FindPlugin( pluginA2.PluginFullName ).RunningStatus, Is.EqualTo( RunningStatus.Stopped ) );
            Assert.That( engine.LiveInfo.FindService( serviceA.ServiceFullName ).RunningStatus, Is.EqualTo( RunningStatus.Stopped ) );

            Assert.That( engine.LiveInfo.FindPlugin( pluginA1.PluginFullName ).Service, Is.EqualTo( engine.LiveInfo.FindService( serviceA.ServiceFullName ) ) );
            Assert.That( engine.LiveInfo.FindPlugin( pluginA2.PluginFullName ).Service, Is.EqualTo( engine.LiveInfo.FindService( serviceA.ServiceFullName ) ) );
            Assert.That( engine.LiveInfo.FindService( serviceA.ServiceFullName ).Generalization, Is.Null );

            Assert.That( engine.LiveInfo.FindPlugin( pluginA1.PluginFullName ).ConfigOriginalStatus, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( engine.LiveInfo.FindPlugin( pluginA2.PluginFullName ).ConfigOriginalStatus, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( engine.LiveInfo.FindService( serviceA.ServiceFullName ).ConfigOriginalStatus, Is.EqualTo( ConfigurationStatus.Optional ) );

            Assert.That( engine.LiveInfo.FindPlugin( pluginA1.PluginFullName ).ConfigSolvedStatus, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( engine.LiveInfo.FindPlugin( pluginA2.PluginFullName ).ConfigSolvedStatus, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( engine.LiveInfo.FindService( serviceA.ServiceFullName ).ConfigSolvedStatus, Is.EqualTo( ConfigurationStatus.Optional ) );

            Assert.That( engine.LiveInfo.FindPlugin( pluginA1.PluginFullName ).CurrentError, Is.Null );
            Assert.That( engine.LiveInfo.FindPlugin( pluginA2.PluginFullName ).CurrentError, Is.Null );
            Assert.That( engine.LiveInfo.FindService( serviceA.ServiceFullName ).DisabledReason, Is.EqualTo( "None" ) );

            engine.Stop();

            //il se passe quoi si on fait cela ?
            //discoveredInfo = MockInfoFactory.CreateGraph005();

        }
    }
}
