using NUnit.Framework;
using System;
using System.IO;
using Yodii.DemoApp;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Model;

namespace Yodii.DemoApp.Tests
{
    [TestFixture]
    public class Tryout
    {
        [Test]
        public void Whatever()
        {
            var d = new PluginDiscoverer();
            d.ReadAssembly( Path.GetFullPath( "Yodii.DemoApp.dll" ) );
            d.Discover();


        }
    }
}
