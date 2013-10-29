using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Model.ConfigurationSolver;
using Yodii.Model.CoreModel;
using Yodii.Model;
using Yodii.Engine.Tests.Mocks;

namespace Yodii.Engine.Tests.ConfigurationSolverTests
{
    [TestFixture]
    class ConfigurationSolverTest
    {
        [Test]
        public void ConfigurationSolverCreation()
        {
            MockInfoFactory factory = new MockInfoFactory();
            ConfigurationManager cm = new ConfigurationManager();
          
            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginB-1", ConfigurationStatus.Runnable );

            cm.Layers.Add( cl );
            
            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, factory.Services, factory.Plugins );

            Assert.That( res.RunningPlugins.Count > 1 );
            Assert.That( res.BlockingPlugins.Count == 0 && res.BlockingServices.Count == 0 );
            Assert.That( res.ConfigurationSuccess );
          
        }
    }
}
