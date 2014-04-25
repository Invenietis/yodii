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
        public void FileDiscovery()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            discoverer.Discover();
            Assert.That( discoverer.FindService( "Service1" ).Generalization.ServiceFullName == "Service2" );
        }
    }
}
