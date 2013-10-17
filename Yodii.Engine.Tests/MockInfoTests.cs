using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Engine.Tests.Mocks;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    public class MockInfoTests
    {
        [Test]
        public void MockFactoryGenerationTest()
        {
            MockInfoFactory factory = new MockInfoFactory();

            IReadOnlyList<IPluginInfo> plugins = factory.Plugins;
            IReadOnlyList<IServiceInfo> services = factory.Services;

            foreach( IServiceInfo s in services )
            {
                Assert.That( s.Implementations.Count > 0 );
                foreach( IPluginInfo p in s.Implementations )
                {
                    Assert.That( p.Service, Is.SameAs( s ) );
                    Assert.That( plugins, Contains.Item( p ) );
                }
            }

            foreach( IPluginInfo p in plugins )
            {
                if( p.Service != null )
                {
                    Assert.That( p.Service.Implementations, Contains.Item( p ) );
                }

                foreach( IServiceReferenceInfo reference in p.ServiceReferences )
                {
                    Assert.That( reference.Owner, Is.SameAs( p ) );
                    Assert.That( services, Contains.Item( reference.Reference ) );
                }
            }
        }
    }
}
