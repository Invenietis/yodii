using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Yodii.Model;
using Yodii.Discoverer;
using Mono.Cecil;
using CK.Core;

namespace Yodii.Discoverer.Tests
{
    [TestFixture]
    class DiscovererTests
    {
        [Test]
        public void ChoucrouteTest1()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            discoverer.GetDiscoveredInfo();
            //Assert.That( discoverer.FindPlugin( "ChoucroutePlugin" ).Service == discoverer.FindService( "IChoucrouteService" ) );
            //Assert.That( discoverer.FindPlugin( "ChoucroutePlugin" ).ServiceReferences[0].Reference == discoverer.FindService( "IChoucrouteServiceRef" ) );
            //Assert.That( discoverer.FindPlugin( "ChoucroutePlugin" ).ServiceReferences[0].Requirement == DependencyRequirement.Running );
            //Assert.That( discoverer.FindService( "IChoucrouteService" ).Implementations.Contains( discoverer.FindPlugin( "ChoucroutePlugin" ) ) );
        }

        [Test]
        public void ChoucrouteTest2()
        {
            //StandardDiscoverer discoverer = new StandardDiscoverer();
            //discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            //discoverer.Discover();
            //Assert.That( discoverer.FindPlugin( "Plugin1" ).Service == discoverer.FindService( "Service2" ) );
            //Assert.That( discoverer.FindService( "Service2" ).Implementations.Contains( discoverer.FindPlugin( "Plugin1" ) ) );
            //Assert.That( discoverer.FindPlugin( "Plugin1" ).ErrorMessage == "A plugin cannot have more than 2 services" );
            //Assert.That( discoverer.FindPlugin( "UntaggedPlugin" ) == null );
        }
        [Test]
        public void ChoucrouteTest3()
        {
            //StandardDiscoverer discoverer = new StandardDiscoverer();
            //discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            //discoverer.Discover();
            //Assert.That( discoverer.FindService( "IService2" ).Generalization.ServiceFullName == "IService1" );
        }
    }
}
