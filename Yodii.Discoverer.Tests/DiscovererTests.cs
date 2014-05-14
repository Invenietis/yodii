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
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();
            //Assert.That( info.PluginInfos.First( p => p.PluginFullName == "ChoucroutePlugin" ).Service == info.ServiceInfos.First( s => s.ServiceFullName == "IChoucrouteService" ) );
            //Assert.That( info.PluginInfos.First( p => p.PluginFullName == "ChoucroutePlugin" ).ServiceReferences[0].Reference == info.ServiceInfos.First( s => s.ServiceFullName == "IChoucrouteServiceRef" ) );
            //Assert.That( info.PluginInfos.First( p => p.PluginFullName == "ChoucroutePlugin" ).ServiceReferences[0].Requirement == DependencyRequirement.Running );
            //Assert.That( info.ServiceInfos.First( s => s.ServiceFullName == "IChoucrouteService" ).Implementations.Contains( info.PluginInfos.First( p => p.PluginFullName == "ChoucroutePlugin" ) ) );
        }

        [Test]
        public void ChoucrouteTest2()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Plugin1" ).Service == info.ServiceInfos.First( s => s.ServiceFullName == "Service2" ) );
            Assert.That( info.ServiceInfos.First( s => s.ServiceFullName == "Service2" ).Implementations.Contains( info.PluginInfos.First( p => p.PluginFullName == "Plugin1" ) ) );
            Assert.That( info.PluginInfos.FirstOrDefault( p => p.PluginFullName == "UntaggedPlugin" ) == null );
        }
    }
}
