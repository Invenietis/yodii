using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Engine.Tests.ConfigurationSolverTests;
using Yodii.Engine.Tests.Mocks;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    class XmlDeserializationTests
    {
        [Test]
        public void TestXmlDeserialization()
        {
            YodiiEngine engineA = MockXmlUtils.CreateEngineFromXmlResource( "Valid001a" );
            YodiiEngine engineB = StaticConfigurationTests.CreateValid001a();

            EquivalenceExtensions.AssertEngineInfoEquivalence( engineA, engineB );

            engineA = MockXmlUtils.CreateEngineFromXmlResource( "Graph005" );
            var info = MockInfoFactory.CreateGraph005();

            EquivalenceExtensions.AssertDiscoveredInfoEquivalence( engineA.DiscoveredInfo, info );
        }
    }
}
