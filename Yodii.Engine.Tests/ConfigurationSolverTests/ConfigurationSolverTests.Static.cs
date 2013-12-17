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
    partial class ConfigurationSolverTest
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
            engine.SetDiscoveredInfo( info );
          
            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );

            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginB-1", ConfigurationStatus.Runnable );

            engine.FullStart( res =>
                {
                    res.CheckSuccess();
                } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Disabled);

            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Disabled );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Running );

            engine.FullStart( res =>
            {
                res.CheckSuccess();
            } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create(); 
            cl.Items.Add( "ServiceB", ConfigurationStatus.Disabled );

            cl.Items.Add( "PluginA-2", ConfigurationStatus.Running );

            engine.FullStart( res =>
            {
                res.CheckBlockingPluginsAre( "PluginA-2" );
                res.CheckNoBlockingServices();
            } );
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

            DiscoveredInfo info = MockInfoFactory.CreateGraph001();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create(); 
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );

            engine.FullStart( res =>
            {
                res.CheckSuccess();
            } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );

            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Running );

            IYodiiEngineResult res = engine.Start();

            res.CheckSuccess();

            info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.OptionalTryStart );

            res = engine.SetDiscoveredInfo( info );

            res.CheckSuccess();

            info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.Runnable );

            res = engine.SetDiscoveredInfo( info );

            res.CheckSuccess();

            info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.RunnableTryStart );

            res = engine.SetDiscoveredInfo( info );

            res.CheckSuccess();

            info = MockInfoFactory.CreateGraph001();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.Running );

            res = engine.SetDiscoveredInfo( info );

            res.CheckSuccess();
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );

            cl.Items.Add( "PluginB-1", ConfigurationStatus.Disabled );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Running );

            engine.FullStart( res =>
                {
                    res.CheckBlockingPluginsAre( "PluginA-2" );
                    res.CheckBlockingServicesAre( "ServiceB" );
                } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );

            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginB-1", ConfigurationStatus.Runnable );

            engine.FullStart( res =>
                {

                    res.CheckBlockingServicesAre( "ServiceAx" );
                    res.CheckNoBlockingPlugins();
                    res.CheckWantedConfigSolvedStatusIs( "PluginA-2", ConfigurationStatus.Runnable );
                } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Add( "ServiceAxx", ConfigurationStatus.Runnable );

            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginAxx-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginB-1", ConfigurationStatus.Runnable );

            engine.FullStart( res =>
                {
                    res.CheckSuccess();
                } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Running );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Disabled );
            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Running );
            cl.Items.Add( "PluginAxx-1", ConfigurationStatus.Disabled );

            engine.FullStart( res =>
            {
                res.CheckSuccess();
            } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Optional );

            engine.FullStart( res =>
            {
                res.CheckSuccess();
            } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );

            engine.FullStart( res =>
            {
                res.CheckSuccess();
            } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );

            engine.FullStart( res =>
            {
                res.CheckSuccess();
            } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );

            engine.FullStart( res =>
                {
                    res.CheckSuccess();
                } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Running );

            engine.FullStart( res =>
                {
                    res.CheckBlockingPluginsAre( "PluginA-1" );
                    res.CheckWantedConfigSolvedStatusIs( "PluginA-1", ConfigurationStatus.Running );
                } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable);
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );

            engine.FullStart( res =>
               {
                   Assert.That( res.Success, Is.True );
               } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Running );

            engine.FullStart( res =>
               {
                   res.CheckBlockingPluginsAre( "PluginA-1" );
               } );
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
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceAx1", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceAx2", ConfigurationStatus.Running );

            engine.FullStart( res =>
               {
                   res.CheckBlockingServicesAre( "ServiceAx1|ServiceAx2" );
               } );
        }

        [Test]
        public void ConfigurationSolverCreation004b()
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

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph004() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceAx1", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceAx2", ConfigurationStatus.Running );

            engine.FullStart( res =>
               {
                   res.CheckBlockingServicesAre( "ServiceAx1|ServiceAx2" );
               } );
        }

        [Test]
        public void ConfigurationSolverCommonReferencesWork()
        {
            #region graph
                /*
                *                  +--------+                            +--------+
                *      +-----------|Service1+                            |Service2|---------------+
                *      |           |Running |                            |Running |               |
                *      |           +---+----+                            +---+----+               |
                *      |               |                                      |                   |
                *      |               |                                      |                   |
                *      |               |                                      |                   |
                *  +---+-----+         |                                      |                   |
                *  |Plugin1  |     +---+-----+                            +---+-----+         +---+-----+
                *  |Optional |     |Plugin2  |                            |Plugin3  |         |Plugin4  |
                *  +----+----+     |Optional |                            |Optional |         |Optional |
                *       |          +---------+                            +---------+         +-----+---+
                *       |                   |                                 |                     |
                *       |                   |                                 |                     |
                *       |                   |                                 |                     |
                *       |                   |                                 |                     |
                *       |                   |                                 |                     |
                 *      |                   |                                 |                     |
                *       |                   |                                 |                     |
                *       |                   |           +--------+            |                     |
                *       |                   |           |Service3+            |                     |
                *       |       +-----------|-----------|Optional|------------|------+--------------+-----------+
                *       |       |           |           +---+----+            |      |              |           |                
                *       |       |           |               |                 |      |              |           |                
                *       |       |           |               |                 |      |              |           |                
                *       |   +---+-------+   +-------->+-----+-----+           |  +---+-------+      |       +---+-------+        
                *       |   |Service3.1 |             |Service3.2 |           |  |Service3.3 |      |       |Service3.4 |        
                *       +-->|Optional   |             |Optional   |           +->|Optional   |<-----+       |Optional   |        
                 *          +-----------+             +-----------+              +-----------+              +-----------+        
                 *          |           |             |           |              |           |              |           |        
                 *          |           |             |           |              |           |              |           |        
                 *          |           |             |           |              |           |              |           |        
                 *      +---+-----+ +---+-----+   +---+-----+ +---+-----+    +---+-----+ +---+-----+    +---+-----+ +---+-----+  
                 *      |Plugin5  | |Plugin6  |   |Plugin7  | |Plugin8  |    |Plugin9  | |Plugin10 |    |Plugin11 | |Plugin12 |  
                 *      |Optional | |Optional |   |Optional | |Optional |    |Optional | |Optional |    |Optional | |Optional |  
                 *      +---------+ +---------+   +---------+ +---------+    +---------+ +---------+    +---------+ +---------+  
                 * 
                */
            #endregion
            DiscoveredInfo info = MockInfoFactory.CreateGraph005();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );
            cl.Items.Add( "Service2", ConfigurationStatus.Running );

            engine.FullStart( res =>
               {
                   Assert.That( res.StaticFailureResult.BlockingServices.Count == 1 );
               } );
        }

        [Test]
        public void ConfigurationSolverCommonReferencesWork2()
        {
            #region graph
            /*
            *                  +--------+                            +--------+
            *      +-----------|Service1+                            |Service2|---------------+                  +---------------+ 
            *      |           |Running |                            |Running |               |                  |AnotherBlocking+ 
            *      |           +---+----+                            +---+----+               |                  |    Runnable   | 
            *      |               |                                      |                   |                  +-------+-------+ 
            *      |               |                                      |                   |                          |       
            *      |               |                                      |                   |                          |       
            *  +---+-----+         |                                      |                   |                          |       
            *  |Plugin1  |     +---+-----+                            +---+-----+         +---+-----+                    |       
            *  |Optional |     |Plugin2  |                            |Plugin3  |         |Plugin4  |            +-------+-------------+ 
            *  +----+----+     |Optional |                            |Optional |         |Optional |            |DisabledForBlocking  | 
            *       |          +---------+                            +---------+         +-----+---+            |     Disabled        | 
            *       |                   |                                 |                     |                +---------------------+ 
            *       |                   |                                 |                     |
            *       |                   |                                 |                     |
            *       |                   |                                 |                     |
            *       |                   |                                 |                     |
             *      |                   |                                 |                     |
            *       |                   |                                 |                     |
            *       |                   |           +--------+            |                     |
            *       |                   |           |Service3+            |                     |
            *       |       +-----------|-----------|Optional|------------|------+--------------+-----------+
            *       |       |           |           +---+----+            |      |              |           |                
            *       |       |           |               |                 |      |              |           |                
            *       |       |           |               |                 |      |              |           |                
            *       |   +---+-------+   +-------->+-----+-----+           |  +---+-------+      |       +---+-------+        
            *       |   |Service3.1 |             |Service3.2 |           |  |Service3.3 |      |       |Service3.4 |        
            *       +-->|Optional   |             |Optional   |           +->|Optional   |      +------>|Optional   |        
            *           +-----------+             +-----------+              +-----------+              +-----------+        
            *           |           |             |           |              |           |              |           |        
            *           |           |             |           |              |           |              |           |        
            *           |           |             |           |              |           |              |           |        
            *       +---+-----+ +---+-----+   +---+-----+ +---+-----+    +---+-----+ +---+-----+    +---+-----+ +---+-----+  
            *       |Plugin5  | |Plugin6  |   |Plugin7  | |Plugin8  |    |Plugin9  | |Plugin10 |    |Plugin11 | |Plugin12 |  
            *       |Optional | |Optional |   |Optional | |Optional |    |Optional | |Optional |    |Optional | |Optional |  
            *       +---------+ +---------+   +---------+ +---------+    +---------+ +---------+    +---------+ +---------+  
            *  
            */
            #endregion
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            var disco = MockInfoFactory.CreateGraph005b();
            var anotherBlocking = new ServiceInfo( "AnotherBlocking", disco.DefaultAssembly );
            var disabledForBlocking = new PluginInfo( "DisabledForBlocking", disco.DefaultAssembly );
            disabledForBlocking.Service = anotherBlocking;
            disco.ServiceInfos.Add( anotherBlocking );
            disco.PluginInfos.Add( disabledForBlocking );
            engine.SetDiscoveredInfo( disco );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );
            cl.Items.Add( "Service2", ConfigurationStatus.Running );
            cl.Items.Add( "AnotherBlocking", ConfigurationStatus.Runnable );
            cl.Items.Add( disabledForBlocking.PluginId.ToString(), ConfigurationStatus.Disabled );

            engine.FullStart( res =>
               {
                   res.CheckBlockingServicesAre( "Service1 | Service2, AnotherBlocking" );
                   res.CheckNoBlockingPlugins();
               } );
        }

        [Test]
        public void ConfigurationSolverCommonReferencesWork3()
        {
            #region graph
            /*
            *                  +--------+                            +--------+
            *      +-----------|Service1+                            |Service2|---------------+
            *      |           |Running |                            |Running |               |      
            *      |           +---+----+                            +----+---+               |      
            *      |               |                                      |                   |      
            *      |               |                                      |                   |      
            *      |               |                                      |                   |      
            *  +---+-----+         |                                      |                   |      
            *  |Plugin1  |     +---+-----+                            +---+-----+         +---+-----+
            *  |Optional |     |Plugin2  |                            |Plugin3  |         |Plugin4  +--------------------+
            *  +----+----+     |Optional |------------------------+   |Optional |         |Optional |                    | 
            *       |          +---------+                        |   +---------+         +---------+                    | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
             *      |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |           +--------+    |       |                                              |          
            *       |                   |           |Service3+    |       |                   +--------+                 |          
            *       |       +-----------|-----------|Optional|    |       |                   |Service4+                 |          
            *       |       |           |           +---+----+    |       |       +-----------|Optional|-------+         |            
            *       |       |           |               |         |       |       |           +---+----+       |         |               
            *       |       |           |               |         |       |       |                            |         |           
            *       |   +---+-------+   |          +----+------+  |       |       |                            |         |           
            *       |   |Service3.1 |   |          |Service3.2 |  |       |    +--+--------+             +-----+-----+   |       
            *       +-->|Optional   |   |          |Optional   |  +-------|--->|Service4.1 |             |Service4.2 |   |       
            *           +-----------+   |          +-----+-----+          |    |Optional   |             |Optional   |<--+       
            *               |           |                |                |    +-----------+             +-----+-----+     
            *               |           |                |                |        |                           |           
            *           +---+-------+   +--------->+-----+-----+          |        |                           |
            *           |Service3.3 |              |Service3.4 |          | +---+-------+              +----+------+  
            *           |Optional   |              |Optional   |          +>|Service4.3 |              |Service4.4 |  
            *           +--+--------+              +-----------+            |Optional   |              |Optional   |  
            *              |                            |                   +--+--------+              +-----------+ 
            *              |                            |                      |                            |
            *              |                            |                      |                            |
            *           +--+-----+                  +---+----+                 |                            |
            *           |Plugin6 |                  |Plugin7 |              +--+-----+                  +---+----+
            *           |Optional|                  |Optional|              |Plugin8 |                  |Plugin9 |
            *           +--------+                  +--------+              |Optional|                  |Optional|
            *                                                               +--------+                  +--------+
            */
            #endregion

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph005c() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );
            cl.Items.Add( "Service2", ConfigurationStatus.Running );

            engine.FullStart( res =>
                {
                    res.CheckSuccess();
                } );
        }

        [Test]
        public void ConfigurationSolverCommonReferencesWork4()
        {
            #region graph
            /*
            *                  +--------+                            
            *      +-----------|Service1+                            
            *      |           |Running |                            
            *      |           +---+----+                            
            *      |               |                                 
            *      |               |                                 
            *      |               |                                 
            *  +---+-----+         |                                 
            *  |Plugin1  |     +---+-----+                           
            *  |Optional |     |Plugin2  |                           
            *  +----+----+     |Optional |-----------------------+ 
            *       |          +---------+                       |
            *       |                                            |
            *       |                                            |
            *       |                                            |
            *       |                                            |
            *       |Runnable                                    |
             *      |                                            |
            *       |                                            |
             *      |                                            |
            *       |                              +---------+   |          
            *       |                              |Service2 |<--+         
            *       |       +----------------------|Optional |             
            *       |       |                       +---+----+               
            *       |       |                          |                       
            *       |       |                          |                   
            *       |   +---+-------+             +----+------+            
            *       |   |Service2.1 |             |Service2.2 |        
            *       +-->|Optional   |             |Optional   |        
            *           +-----------+             +-----+-----+            
            *               |                           |            
            *               |                           |            
            *               |                         +--+-----+
            *               |                         |Plugin4 |
            *            +--+-----+                   |Optional|
            *            |Plugin3 |                   +--------+
            *            |Optional|          
            *            +--------+          
            *                           
            *                           
            *                                                        
            */
            #endregion
            SuccessfulConfigurationSolverCommonReferencesWork4();
        }

        static YodiiEngine SuccessfulConfigurationSolverCommonReferencesWork4()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph005d() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );

            engine.FullStart( res =>
            {
                res.CheckSuccess();
            } );

            return engine;
        }
    }
}
