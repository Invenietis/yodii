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
        public void ConfigurationSolverCreation001a()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Runnable|       |   | Need Running     |Runnable|   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |Runnable |     |PluginA-1|  |Runnable |                |PluginB-1|
             *  +----+----+     |Runnable |  +---------+                |Runnable |
             *       |          +---------+                             +---------+
             *       |
             *  +----+-----+
             *  |PluginAx-1|
             *  |Runnable  |
             *  +----------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();

            ConfigurationManager cm = new ConfigurationManager();
          
            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginA-1").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginAx-1").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginA-2").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginB-1").PluginId.ToString(), ConfigurationStatus.Runnable );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.False );
            Assert.That( res.RunningPlugins, Is.Null );
            Assert.That( res.BlockingPlugins.Count, Is.EqualTo( 2 ) );
            Assert.That( res.BlockingServices, Is.Empty);
            CollectionAssert.AreEquivalent( new[] { "PluginA-2", "PluginA-1" }, res.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName ) );
        }

        [Test]
        public void ConfigurationSolverCreation001b()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Running |       |   | Need Running     |Running |   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |Disable  |     |PluginA-1|  |Running  |                |PluginB-1|
             *  +----+----+     |Optional |  +---------+                |Optional |
             *       |          +---------+                             +---------+
             *       |
             *  +----+-----+
             *  |PluginAx-1|
             *  |Disable   |
             *  +----------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();

            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Disable);
            cl.Items.Add( info.FindPlugin("PluginAx-1").PluginId.ToString(), ConfigurationStatus.Disable );
            cl.Items.Add( info.FindPlugin( "PluginA-2" ).PluginId.ToString(), ConfigurationStatus.Running );

            cm.Layers.Add( cl );


            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.True );
            Assert.That( res.RunningPlugins, Is.Not.Null );
            Assert.That( res.BlockingServices, Is.Null );
            Assert.That( res.BlockingPlugins, Is.Null );
        }

        [Test]
        public void ConfigurationSolverCreation001c()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Optional|       |   | Need Running     |Disable |   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |Optional |     |PluginA-1|  |Running  |                |PluginB-1|
             *  +----+----+     |Optional |  +---------+                |Optional |
             *       |          +---------+                             +---------+
             *       |
             *  +----+-----+
             *  |PluginAx-1|
             *  |Optional  |
             *  +----------+
             */
            #endregion
            
            DiscoveredInfo info = MockInfoFactory.CreateGraph001();

            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceB", ConfigurationStatus.Disable );
            cl.Items.Add( info.FindPlugin( "PluginA-2" ).PluginId.ToString(), ConfigurationStatus.Running );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.False );
            Assert.That( res.RunningPlugins, Is.Null );
            Assert.That( res.BlockingServices, Is.Empty );
            Assert.That( res.BlockingPlugins.Count, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => res.BlockingPlugins.Single( p => p.PluginInfo.PluginFullName == "PluginA-2" ) );
        }

        [Test]
        public void ConfigurationSolverCreation001d()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Disable |       |   | Need Running     |Running |   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |Optional |     |PluginA-1|  |Optional |                |PluginB-1|
             *  +----+----+     |Optional |  +---------+                |Optional |
             *       |          +---------+                             +---------+
             *       |
             *  +----+-----+
             *  |PluginAx-1|
             *  |Optional  |
             *  +----------+
             */
            #endregion
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disable );

            cm.Layers.Add( cl );

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();


            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.True );
            Assert.That( res.RunningPlugins, Is.Not.Null );
            Assert.That( res.BlockingServices, Is.Null );
            Assert.That( res.BlockingPlugins, Is.Null );
        }

        [Test]
        public void ConfigurationSolverCreation001MinusPluginAx()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Runnable|       |   | Need Running     |Runnable|   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |Runnable |     |PluginA-1|  |Runnable |                |PluginB-1|
             *  +----+----+     |Runnable |  +---------+                |Runnable |
             *                  +---------+                             +---------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();
            info.PluginInfos.Remove( info.FindPlugin( "PluginAx-1" ) );

            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginA-1").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginA-2").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginB-1").PluginId.ToString(), ConfigurationStatus.Runnable );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.False );
            Assert.That( res.RunningPlugins, Is.Null );
            Assert.That( res.BlockingServices.Count, Is.EqualTo( 2 ) );
            Assert.That( res.BlockingPlugins.Count, Is.EqualTo( 2 ) );

            CollectionAssert.AreEquivalent( new[] { "ServiceA", "ServiceAx" }, res.BlockingServices.Select( s => s.ServiceInfo.ServiceFullName ) );
        }

        [Test]
        public void ConfigurationSolverCreation002a()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Runnable|       |   | Need Running     |Runnable|   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |Runnable |     |PluginA-1|  |Runnable |                |PluginB-1|
             *  +----+----+     |Runnable |  +---------+                |Runnable |
             *      |           +---------+                             +---------+
             *      |
             *      |--------|
             *      |   +----+-----+
             *      |   |PluginAx-1|
             *      |   +----------+
             *      |         
             *  +---+------+  
             *  |ServiceAxx|
             *  |Runnable  |
             *  +----+-----+  
             *       |      
             *       |      
             *       |      
             *  +---+-------+
             *  |PluginAxx-1|
             *  |Runnable   |
             *  +-----------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph002();
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAxx", ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginA-1" ).PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginA-2" ).PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginAx-1" ).PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginAxx-1" ).PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginB-1").PluginId.ToString(), ConfigurationStatus.Runnable );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.False );
            Assert.That( res.RunningPlugins, Is.Null );
            Assert.That( res.BlockingServices, Is.Empty );
            Assert.That( res.BlockingPlugins.Count, Is.EqualTo( 3 ) );
            CollectionAssert.AreEquivalent( new[] { "PluginA-2", "PluginA-1", "PluginAx-1" }, res.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName ) );
        }

        [Test]
        public void ConfigurationSolverCreation002b()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Optional|       |   | Need Running     |Optional|   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |Running  |     |PluginA-1|  |Optional |                |PluginB-1|
             *  +----+----+     |Disable  |  +---------+                |Optional |
             *      |           +---------+                             +---------+
             *      |
             *      |--------|
             *      |   +----+-----+
             *      |   |PluginAx-1|
             *      |   |Running   |
             *      |   +----------+
             *      |         
             *  +---+------+  
             *  |ServiceAxx|
             *  |Optional  |
             *  +----+-----+  
             *       |      
             *       |      
             *       |      
             *  +---+-------+
             *  |PluginAxx-1|
             *  |Disable    |
             *  +-----------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph002();

            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Running );
            cl.Items.Add( info.FindPlugin( "PluginA-1" ).PluginId.ToString(), ConfigurationStatus.Disable );
            cl.Items.Add( info.FindPlugin( "PluginAx-1" ).PluginId.ToString(), ConfigurationStatus.Running );
            cl.Items.Add( info.FindPlugin( "PluginAxx-1" ).PluginId.ToString(), ConfigurationStatus.Disable );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.True );
            Assert.That( res.BlockingServices, Is.Null );
            Assert.That( res.BlockingPlugins, Is.Null );
            Assert.That( res.RunningPlugins.Count, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => res.RunningPlugins.Single( p => p.PluginInfo.PluginFullName == "PluginAx-1" ) );
        }

        [Test]
        public void ConfigurationSolverCreation003a()
        {
            #region graph
            /**
             *  +--------+
             *  |ServiceA+ ------+
             *  |Optional|       |
             *  +---+----+       |
             *      |            |
             *      |            |
             *      |            |
             *      |        +---+---*-+
             *  +---+-----+  |PluginA-2|
             *  |PluginA-1|  |Optional |
             *  |Optional |  +---------+
             *  +---------+
             */
            #endregion
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Optional );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.True );
            Assert.That( res.RunningPlugins, Is.Empty );
            Assert.That( res.BlockingServices, Is.Null);
        }

        [Test]
        public void ConfigurationSolverCreation003b()
        {
            #region graph
            /**
             *  +--------+
             *  |ServiceA+ ------+
             *  |Disable |       |
             *  +---+----+       |
             *      |            |
             *      |            |
             *      |            |
             *      |        +---+-----+
             *  +---+-----+  |PluginA-2|
             *  |PluginA-1|  |Optional |
             *  |Optional |  +---------+
             *  +---------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disable );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.True );
            Assert.That( res.RunningPlugins, Is.Empty );
            Assert.That( res.BlockingServices, Is.Null );
        }

        [Test]
        public void ConfigurationSolverCreation003c()
        {
            #region graph
            /**
             *  +--------+
             *  |ServiceA+ ------+
             *  |Runnable|       |
             *  +---+----+       |
             *      |            |
             *      |            |
             *      |            |
             *      |        +---+---*-+
             *  +---+-----+  |PluginA-2|
             *  |PluginA-1|  |Optional |
             *  |Optional |  +---------+
             *  +---------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.True );
            Assert.That( res.RunningPlugins, Is.Empty );
            Assert.That( res.BlockingServices, Is.Null );
        }

        [Test]
        public void ConfigurationSolverCreation003d()
        {
            #region graph
            /**
             *  +--------+
             *  |ServiceA+ ------+
             *  |Running |       |
             *  +---+----+       |
             *      |            |
             *      |            |
             *      |            |
             *      |        +---+---*-+
             *  +---+-----+  |PluginA-2|
             *  |PluginA-1|  |Optional |
             *  |Optional |  +---------+
             *  +---------+
             */
            #endregion
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.True );
            Assert.That( res.RunningPlugins, Is.Empty );
            Assert.That( res.BlockingServices, Is.Null );
        }

        [Test]
        public void ConfigurationSolverCreation003e()
        {
            #region graph
            /**
             *  +--------+
             *  |ServiceA+ ------+
             *  |Disable |       |
             *  +---+----+       |
             *      |            |
             *      |            |
             *      |            |
             *      |        +---+---*-+
             *  +---+-----+  |PluginA-2|
             *  |PluginA-1|  |Optional |
             *  |Running  |  +---------+
             *  +---------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            ConfigurationManager cm = new ConfigurationManager();

            ConfigurationLayer cl = new ConfigurationLayer();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disable );
            cl.Items.Add( info.FindPlugin("PluginA-1").PluginId.ToString(), ConfigurationStatus.Running );

            cm.Layers.Add( cl );

            ConfigurationSolver cs = new ConfigurationSolver();
            IConfigurationSolverResult res = cs.Initialize( cm.FinalConfiguration, info );

            Assert.That( res.ConfigurationSuccess, Is.False );
            Assert.That( res.RunningPlugins, Is.Null );
            Assert.That( res.BlockingPlugins.Count, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => res.BlockingPlugins.Single( p => p.PluginInfo.PluginFullName == "PluginA-1" ) );
        }
    }
}
