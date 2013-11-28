﻿using System;
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
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
          
            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginA-1").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginAx-1").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginA-2").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginB-1").PluginId.ToString(), ConfigurationStatus.Runnable );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count, Is.EqualTo( 2 ) );
            Assert.That( res.StaticFailureResult.BlockingServices, Is.Empty);
            CollectionAssert.AreEquivalent( new[] { "PluginA-2", "PluginA-1" }, res.StaticFailureResult.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName ) );
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

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Disabled);
            cl.Items.Add( info.FindPlugin("PluginAx-1").PluginId.ToString(), ConfigurationStatus.Disabled );
            cl.Items.Add( info.FindPlugin( "PluginA-2" ).PluginId.ToString(), ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );
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

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create(); 
            cl.Items.Add( "ServiceB", ConfigurationStatus.Disabled );
            cl.Items.Add( info.FindPlugin( "PluginA-2" ).PluginId.ToString(), ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            Assert.That( res.StaticFailureResult.BlockingServices, Is.Empty );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => res.StaticFailureResult.BlockingPlugins.Single( p => p.PluginInfo.PluginFullName == "PluginA-2" ) );
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
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create(); 
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();


            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );
        }

        [Test]
        public void ConfigurationSolverCreation001e()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Running |       |   | Need ?           |Running |   
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
             *  |Running   |
             *  +----------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.Optional );

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Add( info.FindPlugin("PluginAx-1").PluginId.ToString(), ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );

            info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.OptionalTryStart );

            res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );

            info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.Runnable );

            res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );

            info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.RunnableTryStart );

            res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );

            info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.Running );

            res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );
        }

        [Test]
        public void ConfigurationSolverCreation001g()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |Optional|       |   | Need Running     |Running |   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |Optional |     |PluginA-1|  |Running  |                |PluginB-1|
             *  +----+----+     |Optional |  +---------+                |Disable  |
             *       |          +---------+                             +---------+
             *       |
             *  +----+-----+
             *  |PluginAx-1|
             *  |Optional  |
             *  +----------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( info.FindPlugin( "PluginB-1" ).PluginId.ToString(), ConfigurationStatus.Disabled );
            cl.Items.Add( info.FindPlugin( "PluginA-2" ).PluginId.ToString(), ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            //Assert.That( res.StaticFailureResult.StaticSolvedConfiguration.Plugins.FirstOrDefault( plugin => plugin.WantedConfigSolvedStatus == SolvedConfigurationStatus.Running ), Is.Null );
            Assert.That( res.StaticFailureResult.BlockingServices.Count, Is.EqualTo( 1 ) );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => res.StaticFailureResult.BlockingPlugins.Single( p => p.PluginInfo.PluginFullName == "PluginA-2" ) );
            Assert.DoesNotThrow( () => res.StaticFailureResult.BlockingServices.Single( s => s.ServiceInfo.ServiceFullName == "ServiceB" ) );
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

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginA-1").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginA-2").PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginB-1").PluginId.ToString(), ConfigurationStatus.Runnable );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );

            Assert.That( res.StaticFailureResult.StaticSolvedConfiguration.Plugins.FirstOrDefault( plugin => plugin.WantedConfigSolvedStatus == SolvedConfigurationStatus.Running ), Is.Null );
            Assert.That( res.StaticFailureResult.BlockingServices.Count, Is.EqualTo( 2 ) );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count, Is.EqualTo( 2 ) );

            CollectionAssert.AreEquivalent( new[] { "ServiceA", "ServiceAx" }, res.StaticFailureResult.BlockingServices.Select( s => s.ServiceInfo.ServiceFullName ) );
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
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAxx", ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginA-1" ).PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginA-2" ).PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginAx-1" ).PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginAxx-1" ).PluginId.ToString(), ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin("PluginB-1").PluginId.ToString(), ConfigurationStatus.Runnable );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            Assert.That( res.StaticFailureResult.StaticSolvedConfiguration.Plugins.FirstOrDefault( plugin => plugin.WantedConfigSolvedStatus == SolvedConfigurationStatus.Running ), Is.Null );
            Assert.That( res.StaticFailureResult.BlockingServices, Is.Empty );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count, Is.EqualTo( 3 ) );
            CollectionAssert.AreEquivalent( new[] { "PluginA-2", "PluginA-1", "PluginAx-1" }, res.StaticFailureResult.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName ) );
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

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Running );
            cl.Items.Add( info.FindPlugin( "PluginA-1" ).PluginId.ToString(), ConfigurationStatus.Disabled );
            cl.Items.Add( info.FindPlugin( "PluginAx-1" ).PluginId.ToString(), ConfigurationStatus.Running );
            cl.Items.Add( info.FindPlugin( "PluginAxx-1" ).PluginId.ToString(), ConfigurationStatus.Disabled );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );
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

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Optional );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );
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

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );
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

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );
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
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.True );
            Assert.That( res.StaticFailureResult, Is.Null );
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
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );
            cl.Items.Add( info.FindPlugin("PluginA-1").PluginId.ToString(), ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            Assert.That( res.StaticFailureResult.StaticSolvedConfiguration.Plugins.FirstOrDefault( plugin => plugin.WantedConfigSolvedStatus == SolvedConfigurationStatus.Running ), Is.Null );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => res.StaticFailureResult.BlockingPlugins.Single( p => p.PluginInfo.PluginFullName == "PluginA-1" ) );
        }

        [Test]
        public void ConfigurationSolverCreation003f()
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
             *  |PluginA-1|  |Runnable |
             *  |Runnable |  +---------+
             *  +---------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( info.FindPlugin( "PluginA-1" ).PluginId.ToString(), ConfigurationStatus.Runnable);
            cl.Items.Add( info.FindPlugin( "PluginA-2" ).PluginId.ToString(), ConfigurationStatus.Runnable );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            Assert.That( res.StaticFailureResult.StaticSolvedConfiguration.Plugins.FirstOrDefault( plugin => plugin.WantedConfigSolvedStatus == SolvedConfigurationStatus.Running ), Is.Null );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => res.StaticFailureResult.BlockingPlugins.Single( p => p.PluginInfo.PluginFullName == "PluginA-2" ) );
        }

        [Test]
        public void ConfigurationSolverCreation003MinusPluginA2()
        {
            #region graph
            /**
             *  +--------+
             *  |ServiceA| 
             *  |Disabled| 
             *  +---+----+ 
             *      |      
             *      |      
             *      |      
             *      |      
             *  +---+-----+
             *  |PluginA-1|
             *  |Running  |
             *  +---------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph003();

            info.PluginInfos.Remove( info.FindPlugin( "PluginA-2" ) );
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );
            cl.Items.Add( info.FindPlugin( "PluginA-1" ).PluginId.ToString(), ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => res.StaticFailureResult.BlockingPlugins.Single( p => p.PluginInfo.PluginFullName == "PluginA-1" ) );
        }

        [Test]
        public void ConfigurationSolverCreation004a()
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
             *      |         +---+------+
             *  +---+------+  |ServiceAx2|
             *  |ServiceAx1|  |Running   |
             *  |Running   |  +----------+
             *  +----------+      |       
             *      |             |       
             *      |             |       
             *      |             |       
             *      |          +---+-------+
             *  +---+-------+  |PluginAx2-1|
             *  |PluginAx1-1|  |Optional   |
             *  |Optional   |  +-----------+
             *  +-----------+
             */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph004();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "ServiceAx1", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceAx2", ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            Assert.That( res.StaticFailureResult.StaticSolvedConfiguration.Plugins.FirstOrDefault( plugin => plugin.WantedConfigSolvedStatus == SolvedConfigurationStatus.Running ), Is.Null );
            Assert.That( res.StaticFailureResult.BlockingServices.Count, Is.EqualTo( 2 ) );
            CollectionAssert.AreEquivalent( new[] { "ServiceAx1", "ServiceAx2" }, res.StaticFailureResult.BlockingServices.Select( s => s.ServiceInfo.ServiceFullName ) );
        }
    }
}
