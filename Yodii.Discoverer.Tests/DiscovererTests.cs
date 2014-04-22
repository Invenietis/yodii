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
            //string path = Path.GetFullPath( "Yodii.Discoverer.Tests.dll" );
            //discoverer.ReadAssembly( path );
        

            //Debug.WriteLine( "Class: {0}.", typeDef.BaseType );
            //Assert.That( typeDef.Interfaces.Count == 1 );
            //Assert.That( typeDef.Methods.Count == 3 );
        }
        [Test]
        public void FileDiscoveryBis()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            Assert.That( true );
        }
    }
}
