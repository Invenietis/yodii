using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Yodii.Model;
using Yodii.Discoverer;
//using Mono.Cecil;
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
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Yodii.Discoverer.Tests.ChoucroutePlugin" ).Service == info.ServiceInfos.First( s => s.ServiceFullName == "Yodii.Discoverer.Tests.IChoucrouteService" ) );
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Yodii.Discoverer.Tests.ChoucroutePlugin" ).ServiceReferences.FirstOrDefault().Reference == info.ServiceInfos.First( s => s.ServiceFullName == "Yodii.Discoverer.Tests.IAnotherService" ) );
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Yodii.Discoverer.Tests.ChoucroutePlugin" ).ServiceReferences.FirstOrDefault().Requirement == DependencyRequirement.Optional );
        }

        [Test]
        public void ChoucrouteTest2()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Yodii.Discoverer.Tests.Plugin1" ).Service == info.ServiceInfos.First( s => s.ServiceFullName == "Yodii.Discoverer.Tests.IService2" ) );
            Assert.That( !info.PluginInfos.Any( p => p.PluginFullName == "Yodii.Discoverer.Tests.UntaggedPlugin" ) );
        }

        [Test]
        public void ChoucrouteTest3()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();
            Assert.That( info.PluginInfos.Any( p => p.PluginFullName == "Yodii.Discoverer.Tests.DerivedClass" ) );
        }
    }
}
