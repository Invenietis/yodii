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
            var ex = Assert.Throws<ArgumentNullException>(() => new YodiiEngine(fakeMock));
            Assert.That(ex.ParamName, Is.EqualTo( "host" ));

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            Assert.That( engine.Host, Is.Not.Null );
            Assert.That( engine.IsRunning, Is.False );
            Assert.That( engine.ConfigurationManager, Is.Not.Null );
        }
    }
}
