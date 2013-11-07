using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
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
            ConfigurationManager cm = new ConfigurationManager();
          
            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            //cl.Items.Add( "ServiceAxx", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Runnable );
            //cl.Items.Add( "PluginAxx-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginB-1", ConfigurationStatus.Runnable );

            cm.Layers.Add( cl );

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();


            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.False );
            Assert.That( res.RunningPlugins, Is.Null );
            Assert.That( res.BlockingServices.Count == 0 );
            CollectionAssert.AreEquivalent( new[] { "PluginA-2", "PluginA-1" }, res.BlockingPlugins.Select( p => p.PluginFullName ) );
        }

        [Test]
        public void ConfigurationSolverCreation002()
        {
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAxx", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginB-1", ConfigurationStatus.Runnable );

            cm.Layers.Add( cl );

            DiscoveredInfo info = MockInfoFactory.CreateGraph002();


            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.False );
            Assert.That( res.RunningPlugins, Is.Null );
            Assert.That( res.BlockingServices.Count == 1 );
            CollectionAssert.AreEquivalent( new[] { "PluginA-2", "PluginA-1" }, res.BlockingPlugins.Select( p => p.PluginFullName ) );
            CollectionAssert.AreEquivalent( new[] { "ServiceAxx"}, res.BlockingServices.Select( s => s.ServiceFullName ) );
        }

        [Test]
        public void ConfigurationSolverCreation001MinusAx()
        {
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginB-1", ConfigurationStatus.Runnable );

            cm.Layers.Add( cl );

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();
            info.PluginInfos.Remove( info.FindPlugin( "PluginAx-1" ) );


            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.False );
            Assert.That( res.RunningPlugins, Is.Null );
            Assert.That( res.BlockingServices.Count == 1 );
            Assert.That( res.BlockingPlugins.Count == 1 );

            
            Assert.DoesNotThrow( () => res.BlockingServices.Single( s => s.ServiceFullName == "ServiceAx" ) );
        }
    }
}
