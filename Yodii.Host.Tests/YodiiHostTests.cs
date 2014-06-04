using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Yodii.Model;
using Yodii.Engine;
using Yodii.Discoverer;
using CK.Core;
using System.IO;

namespace Yodii.Host.Tests
{
    [TestFixture]
    public class YodiiHostTests
    {
        [Test]
        public void ToSeeWhatHappensChoucrouteTest1()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost(); /*IYodiiEngineHost this is not enough, need access to PluginCreator & ServiceReferencesBinder*/
            YodiiEngine engine = new YodiiEngine( host);
            engine.SetDiscoveredInfo(info);

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.Host.Tests.ChoucroutePlugin", ConfigurationStatus.Running );
                         
            var result = engine.Start();

        }
    }
}
