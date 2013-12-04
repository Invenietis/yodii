using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    class YodiiEngineTests
    {
        [Test]
        public void EngineCreationTest()
        {
            YodiiEngineHostMock fakeMock = null;
            var ex = Assert.Throws<Exception>(() => new YodiiEngine(fakeMock));
            Assert.That(ex.Message, Is.EqualTo("host"));

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            Assert.That( engine.Host, Is.Not.Null );
            Assert.That( engine.ConfigurationManager, Is.Not.Null );
        }
        [Test]
        public void EngineStartTest()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "service1", ConfigurationStatus.Optional );
            cl.Items.Add( "plugin1", ConfigurationStatus.Optional );
            cl.Items.Add( "plugin2", ConfigurationStatus.Optional );

            Assert.That(engine.DiscoveredInfo, Is.Empty);

            IYodiiEngineResult result = engine.Start();
            Assert.That( engine.IsRunning, Is.True );
            Assert.That( result.Success, Is.True );
        }
    }
}
