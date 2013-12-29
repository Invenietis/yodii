using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Model;
using CK.Core;
using Yodii.Engine.Tests.Mocks;

namespace Yodii.Engine.Tests.ConfigurationSolverTests
{
    [TestFixture]
    class StaticConfigurationTests
    {
        [Test]
        public void Valid001a()
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

            YodiiEngine engine = CreateValid001a();
            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckSuccess();
                } );
        }

        internal static YodiiEngine CreateValid001a()
        {
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
            return engine;
        }

        [Test]
        public void Valid001b()
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

            YodiiEngine engine = CreateValid001b();
            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
        }

        internal static YodiiEngine CreateValid001b()
        {
            DiscoveredInfo info = MockInfoFactory.CreateGraph001();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Disabled );
            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Disabled );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Running );
            return engine;
        }

        [Test]
        public void Invalid001c()
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

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckAllBlockingPluginsAre( "PluginA-2" );
                res.CheckNoBlockingServices();
            } );
        }

        [Test]
        public void Valid001d()
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

            YodiiEngine engine = CreateValid001d();

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
        }

        internal static YodiiEngine CreateValid001d()
        {
            DiscoveredInfo info = MockInfoFactory.CreateGraph001();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );
            return engine;
        }

        [Test]
        public void SetDiscoverdInfo()
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
        public void Invalid001g()
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

            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckAllBlockingPluginsAre( "PluginA-2" );
                    res.CheckAllBlockingServicesAre( "ServiceB" );
                } );
        }

        [Test]
        public void Invalid001MinusPluginAx()
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

            engine.FullStaticResolutionOnly( res =>
                {

                    res.CheckAllBlockingServicesAre( "ServiceAx" );
                    res.CheckNoBlockingPlugins();
                    res.CheckWantedConfigSolvedStatusIs( "PluginA-2", ConfigurationStatus.Runnable );
                } );
        }

        [Test]
        public void Valid002a()
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

            YodiiEngine engine = CreateValid002a();
            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckSuccess();
                } );
        }

        internal static YodiiEngine CreateValid002a()
        {
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
            return engine;
        }

        [Test]
        public void Valid002b()
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

            YodiiEngine engine = CreateValid002b();

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
        }

        internal static YodiiEngine CreateValid002b()
        {
            DiscoveredInfo info = MockInfoFactory.CreateGraph002();

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceAx", ConfigurationStatus.Running );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Disabled );
            cl.Items.Add( "PluginAx-1", ConfigurationStatus.Running );
            cl.Items.Add( "PluginAxx-1", ConfigurationStatus.Disabled );
            return engine;
        }

        [Test]
        public void Valid003a()
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
            YodiiEngine engine = CreateValid003a();

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
                res.CheckAllPluginsOptional( "PluginA-1, PluginA-2" );
                res.CheckAllServicesOptional( "ServiceA" );
            } );
        }

        internal static YodiiEngine CreateValid003a()
        {
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Optional );
            return engine;
        }

        [Test]
        public void Valid003b()
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

            YodiiEngine engine = CreateValid003b();
            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
        }

        internal static YodiiEngine CreateValid003b()
        {
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Disabled );
            return engine;
        }

        [Test]
        public void Valid003c()
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

            YodiiEngine engine = CreateValid003c();
            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
        }

        internal static YodiiEngine CreateValid003c()
        {
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            return engine;
        }

        [Test]
        public void Valid003d()
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
            YodiiEngine engine = CreateValid003d();

            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckSuccess();
                } );
        }

        internal static YodiiEngine CreateValid003d()
        {
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Running );
            return engine;
        }

        [Test]
        public void Invalid003e()
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

            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckAllBlockingPluginsAre( "PluginA-1" );
                    res.CheckWantedConfigSolvedStatusIs( "PluginA-1", ConfigurationStatus.Running );
                } );
        }

        [Test]
        public void Valid003f()
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

            YodiiEngine engine = CreateValid003f();

            engine.FullStaticResolutionOnly( res =>
               {
                   res.CheckSuccess();
               } );
        }

        internal static YodiiEngine CreateValid003f()
        {
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Add( "PluginA-2", ConfigurationStatus.Runnable );
            return engine;
        }

        [Test]
        public void Invalid003MinusPluginA2()
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

            engine.FullStaticResolutionOnly( res =>
               {
                   res.CheckAllBlockingPluginsAre( "PluginA-1" );
               } );
        }

        [Test]
        public void Invalid004a()
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

            engine.FullStaticResolutionOnly( res =>
               {
                   res.CheckAllBlockingServicesAre( "ServiceAx1,ServiceAx2" );
               } );
        }

        [Test]
        public void Invalid004b()
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

            engine.FullStaticResolutionOnly( res =>
               {
                   res.CheckAllBlockingServicesAre( "ServiceA,ServiceAx1,ServiceAx2" );
               } );
        }

        [Test]
        public void InvalidCommonReferences1()
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

            engine.FullStaticResolutionOnly( res =>
               {
                   res.CheckAllBlockingServicesAre( "Service1 | Service2" );
                   res.CheckNoBlockingPlugins();
               } );
        }

        [Test]
        public void InvalidCommonReferences2()
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
            cl.Items.Add( "DisabledForBlocking", ConfigurationStatus.Disabled );

            engine.FullStaticResolutionOnly( res =>
               {
                   res.CheckAllBlockingServicesAre( "Service1 | Service2, AnotherBlocking" );
                   res.CheckNoBlockingPlugins();
               } );
        }

        [Test]
        public void ValidCommonReferences3()
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
             *          +-----------+   |          +-----+-----+          |    |Optional   |             |Optional   |<--+       
             *              |           |                |                |    +-----------+             +-----+-----+     
             *              |           |                |                |        |                           |           
             *          +---+-------+   +--------->+-----+-----+          |        |                           |
             *          |Service3.3 |              |Service3.4 |          | +---+-------+              +----+------+  
             *          |Optional   |              |Optional   |          +>|Service4.3 |              |Service4.4 |  
             *          +--+--------+              +-----------+            |Optional   |              |Optional   |  
             *             |                            |                   +--+--------+              +-----------+ 
             *             |                            |                      |                            |
             *             |                            |                      |                            |
             *          +--+-----+                  +---+----+                 |                            |
             *          |Plugin5 |                  |Plugin6 |              +--+-----+                  +---+----+
             *          |Optional|                  |Optional|              |Plugin7 |                  |Plugin8 |
             *          +--------+                  +--------+              |Optional|                  |Optional|
             *                                                              +--------+                  +--------+
            */
            #endregion

            YodiiEngine engine = CreateValidCommonReferences3();

            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckSuccess();
                } );
        }

        internal static YodiiEngine CreateValidCommonReferences3()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph005c() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );
            cl.Items.Add( "Service2", ConfigurationStatus.Running );
            return engine;
        }

        [Test]
        public void ValidCommonReferences4()
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
            
            var e = CreateValidCommonReferences4();
            e.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
       }

        internal static YodiiEngine CreateValidCommonReferences4()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph005d() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );

            return engine;
        }

        [Test]
        public void ValidRunnableReferences()
        {
            // file://E:\Dev\Yodii\Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\ValidRunnableReferences.png

            var e = CreateValidRunnableReferences();
            e.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
                res.CheckAllServicesOptional( "Service1, Service2, Service3, Service3.1, Service3.2, Service3.3, Service4, Service4.1, Service4.2" );
                res.CheckAllPluginsOptional( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5, Plugin6, Plugin7, Plugin8, Plugin9" );
            } );
       }

        internal static YodiiEngine CreateValidRunnableReferences()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph006() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();

            return engine;
        }

        [Test]
        public void ValidOnlyOneRunnableReference()
        {
            // file://E:\Dev\Yodii\Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\ValidOnlyOneRunnableReference.png

            var e = CreateValidOnlyOneRunnableReference();
            e.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
                res.CheckAllServicesOptional( "Service1, Service1.1, Service1.2, Service1.3, Service2, Service2.1, Service2.2" );
                res.CheckAllPluginsOptional( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5" );
            } );
        }

        internal static YodiiEngine CreateValidOnlyOneRunnableReference()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph007() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();

            return engine;
        }

        [Test]
        public void InvalidRunnableTryStartReference()
        {
            // file://E:\Dev\Yodii\Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\InvalidRunnableTryStartReference.png
            var e = CreateInvalidRunnableTryStartReference();
            e.FullStaticResolutionOnly( res =>
            {
                Assert.That( res.StaticFailureResult, Is.Not.Null );
                //res.CheckAllBlockingPluginsAre("Plugin1, Plugin17, Plugin19, Plugin20, Plugin8");
                //res.CheckAllBlockingServicesAre( "Service1.2" );
            } );
        }

        internal static YodiiEngine CreateInvalidRunnableTryStartReference()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph008() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Plugin1", ConfigurationStatus.Runnable );
            cl.Items.Add( "Plugin3", ConfigurationStatus.Disabled );
            cl.Items.Add( "Plugin4", ConfigurationStatus.Running );
            cl.Items.Add( "Plugin6", ConfigurationStatus.Disabled );
            cl.Items.Add( "Plugin8", ConfigurationStatus.Runnable );
            cl.Items.Add( "Plugin9", ConfigurationStatus.Disabled );
            cl.Items.Add( "Plugin10", ConfigurationStatus.Runnable );
            cl.Items.Add( "Plugin14", ConfigurationStatus.Disabled );
            cl.Items.Add( "Plugin17", ConfigurationStatus.Running );
            cl.Items.Add( "Plugin19", ConfigurationStatus.Runnable );
            cl.Items.Add( "Plugin20", ConfigurationStatus.Runnable );
            cl.Items.Add( "Plugin24", ConfigurationStatus.Disabled );

            cl.Items.Add( "Service2", ConfigurationStatus.Disabled );
            cl.Items.Add( "Service2.1", ConfigurationStatus.Running );
            cl.Items.Add( "Service1.1", ConfigurationStatus.Running );
            cl.Items.Add( "Service1.2", ConfigurationStatus.Runnable );
            cl.Items.Add( "Service1.1.2", ConfigurationStatus.Runnable );
            cl.Items.Add( "Service1.1.3", ConfigurationStatus.Runnable );

            return engine;
        }

        [Test]
        public void ValidOptionalReferences()
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
            *       |Optional                                    |
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

            var e = CreateValidOptionalReferences();
            e.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
        }

        internal static YodiiEngine CreateValidOptionalReferences()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph005e() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );

            return engine;
        }

        [Test]
        public void ValidOptionalTryStartReferences()
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
             *          +-----------+   |          +-----+-----+          |    |Optional   |             |Optional   |<--+       
             *              |           |                |                |    +-----------+             +-----+-----+     
             *              |           |                |                |        |                           |           
             *          +---+-------+   +--------->+-----+-----+          |        |                           |
             *          |Service3.3 |              |Service3.4 |          | +---+-------+              +----+------+  
             *          |Optional   |              |Optional   |          +>|Service4.3 |              |Service4.4 |  
             *          +--+--------+              +-----------+            |Optional   |              |Optional   |  
             *             |                            |                   +--+--------+              +-----------+ 
             *             |                            |                      |                            |
             *             |                            |                      |                            |
             *          +--+-----+                  +---+----+                 |                            |
             *          |Plugin5 |                  |Plugin6 |              +--+-----+                  +---+----+
             *          |Optional|                  |Optional|              |Plugin7 |                  |Plugin8 |
             *          +--------+                  +--------+              |Optional|                  |Optional|
             *                                                              +--------+                  +--------+
            */
            #endregion

            YodiiEngine engine = CreateValidOptionalTryStartReferences();

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();               
            } );
        }

        internal static YodiiEngine CreateValidOptionalTryStartReferences()
        {
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraph005f() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );
            cl.Items.Add( "Service2", ConfigurationStatus.Running );
            return engine;
        }
    }
}
