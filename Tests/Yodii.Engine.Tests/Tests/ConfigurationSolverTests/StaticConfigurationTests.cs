#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Tests\ConfigurationSolverTests\StaticConfigurationTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
        public void when_a_service_has_no_plugins()
        {
            #region graph
            /**
             *                  +--------+
             *      +---------->|   S1   |
             *      |           |        |
             *      |           +---+----+       
             *      |               |            
             *      |               |                     
             *      |           +---+-----+              
             *  +---+-----+     |   P     |  
             *  |  S.1.1  |     |         |  
             *  | Running |     +---------+  
             *  +----+----+    
             *                 
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add( new ServiceInfo( "S1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "S1.1", d.DefaultAssembly ) );
            d.FindService( "S1.1" ).Generalization = d.FindService( "S1" );

            d.PluginInfos.Add( new PluginInfo( "P", d.DefaultAssembly ) );
            d.FindPlugin( "P" ).Service = d.FindService( "S1" );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( d );

            engine.Configuration.Layers.Default.Items.Set( "S1.1", ConfigurationStatus.Running );

            Assert.That( engine.StartEngine().Success, Is.False );
        }

        [Test]
        public void simple_graph_full_of_runnable()
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

            YodiiEngine engine = CreateSimpleGraphWithRunnables();
            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckSuccess();
                } );
        }

        internal static YodiiEngine CreateSimpleGraphWithRunnables()
        {
            DiscoveredInfo info = MockInfoFactory.SimpleGraph();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            engine.Configuration.Layers.Default.Set( "ServiceA", ConfigurationStatus.Runnable );
            engine.Configuration.Layers.Default.Set( "ServiceB", ConfigurationStatus.Runnable );
            engine.Configuration.Layers.Default.Set( "ServiceAx", ConfigurationStatus.Runnable );
            engine.Configuration.Layers.Default.Set( "PluginA-1", ConfigurationStatus.Runnable );
            engine.Configuration.Layers.Default.Set( "PluginAx-1", ConfigurationStatus.Runnable );
            engine.Configuration.Layers.Default.Set( "PluginA-2", ConfigurationStatus.Runnable );
            engine.Configuration.Layers.Default.Set( "PluginB-1", ConfigurationStatus.Runnable );
            return engine;
        }

        [Test]
        public void simple_graph_with_running_and_disable()
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

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.SimpleGraph() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Set( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Set( "ServiceAx", ConfigurationStatus.Disabled );
            cl.Items.Set( "PluginAx-1", ConfigurationStatus.Disabled );
            cl.Items.Set( "PluginA-2", ConfigurationStatus.Running );

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
        }

        [Test]
        public void simple_graph_with_running_and_disable2()
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
            
            DiscoveredInfo info = MockInfoFactory.SimpleGraph();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Default; 
            cl.Items.Set( "ServiceB", ConfigurationStatus.Disabled );
            cl.Items.Set( "PluginA-2", ConfigurationStatus.Running );

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckAllBlockingPluginsAre( "PluginA-2" );
                res.CheckNoBlockingServices();
            } );
        }

        [Test]
        public void simple_graph_with_running_and_disable3()
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

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.SimpleGraph() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create( "Other" );
            cl.Items.Set( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Set( "ServiceA", ConfigurationStatus.Disabled );

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
            } );
        }

        [Test]
        public void setting_discovered_info()
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

            DiscoveredInfo info = MockInfoFactory.SimpleGraph();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.Optional );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create( "Test" );
            cl.Items.Set( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Set( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Set( "PluginAx-1", ConfigurationStatus.Running );

            engine.StartEngine().CheckSuccess();

            info = MockInfoFactory.SimpleGraph();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.OptionalRecommended );

            engine.Configuration.SetDiscoveredInfo( info ).CheckSuccess();

            info = MockInfoFactory.SimpleGraph();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.Runnable );

            engine.Configuration.SetDiscoveredInfo( info ).CheckSuccess();

            info = MockInfoFactory.SimpleGraph();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.RunnableRecommended );

            engine.Configuration.SetDiscoveredInfo( info ).CheckSuccess();

            info = MockInfoFactory.SimpleGraph();
            info.FindPlugin( "PluginAx-1" ).AddServiceReference( info.FindService( "ServiceB" ), DependencyRequirement.Running );

            engine.Configuration.SetDiscoveredInfo( info ).CheckSuccess();
        }

        [Test]
        public void simple_graph_with_blocking_configuration()
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

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.SimpleGraph() );

            IConfigurationLayer cl = engine.Configuration.Layers.Create( "Test" );
            cl.Items.Set( "ServiceB", ConfigurationStatus.Running );
            cl.Items.Set( "PluginB-1", ConfigurationStatus.Disabled );
            cl.Items.Set( "PluginA-2", ConfigurationStatus.Running );

            engine.FullStaticResolutionOnly( res =>
                {
                    Assert.That( res.Success, Is.False );
                    res.CheckAllBlockingPluginsAre( "PluginA-2" );
                    res.CheckAllBlockingServicesAre( "ServiceB" );
                } );
        }

        [Test]
        public void simple_graph_with_blocking_configuration_without_PluginAx1()
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

            DiscoveredInfo info = MockInfoFactory.SimpleGraph();
            info.PluginInfos.Remove( info.FindPlugin( "PluginAx-1" ) );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Create( "Test" );
            cl.Items.Set( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Set( "ServiceB", ConfigurationStatus.Runnable );
            cl.Items.Set( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginB-1", ConfigurationStatus.Runnable );

            engine.FullStaticResolutionOnly( res =>
                {
                    Assert.That( res.Success, Is.False );
                    res.CheckAllBlockingServicesAre( "ServiceAx" );
                    res.CheckNoBlockingPlugins();
                    res.CheckWantedConfigSolvedStatusIs( "PluginA-2", SolvedConfigurationStatus.Runnable );
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
             *      |   |       +---------+                             +---------+
             *      |   +----+
             *      |        |
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

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.SimpleGraphWithSpecializedService() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Set( "ServiceB", ConfigurationStatus.Runnable );
            cl = engine.Configuration.Layers.Create( "Other1" );
            cl.Items.Set( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Set( "ServiceAxx", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginA-1", ConfigurationStatus.Runnable );
            cl = engine.Configuration.Layers.Create( "Other2" );
            cl.Items.Set( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginAx-1", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginAxx-1", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginB-1", ConfigurationStatus.Runnable );

            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckSuccess();
                } );
        }

        internal static YodiiEngine CreateValid002a()
        {
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.SimpleGraphWithSpecializedService() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Set( "ServiceB", ConfigurationStatus.Runnable );
            cl = engine.Configuration.Layers.Create( "Other1" ); 
            cl.Items.Set( "ServiceAx", ConfigurationStatus.Runnable );
            cl.Items.Set( "ServiceAxx", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginA-1", ConfigurationStatus.Runnable );
            cl = engine.Configuration.Layers.Create( "Other2" );
            cl.Items.Set( "PluginA-2", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginAx-1", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginAxx-1", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginB-1", ConfigurationStatus.Runnable );
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
            DiscoveredInfo info = MockInfoFactory.SimpleGraphWithSpecializedService();

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceAx", ConfigurationStatus.Running );
            cl.Items.Set( "PluginA-1", ConfigurationStatus.Disabled );
            cl.Items.Set( "PluginAx-1", ConfigurationStatus.Running );
            cl.Items.Set( "PluginAxx-1", ConfigurationStatus.Disabled );
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
                res.CheckAllPluginsRunnable( "PluginA-1, PluginA-2" );
                res.CheckAllServicesRunnable( "ServiceA" );
            } );
        }

        internal static YodiiEngine CreateValid003a()
        {
            DiscoveredInfo info = MockInfoFactory.ServiceWithTwoPlugins();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );
            engine.Configuration.Layers.Default.Items.Set( "ServiceA", ConfigurationStatus.Optional );
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
            DiscoveredInfo info = MockInfoFactory.ServiceWithTwoPlugins();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            engine.Configuration.Layers.Default.Items.Set( "ServiceA", ConfigurationStatus.Disabled );
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
            DiscoveredInfo info = MockInfoFactory.ServiceWithTwoPlugins();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            engine.Configuration.Layers.Default.Items.Set( "ServiceA", ConfigurationStatus.Runnable );
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
            DiscoveredInfo info = MockInfoFactory.ServiceWithTwoPlugins();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            engine.Configuration.Layers.Default.Items.Set( "ServiceA", ConfigurationStatus.Running );
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

            DiscoveredInfo info = MockInfoFactory.ServiceWithTwoPlugins();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceA", ConfigurationStatus.Disabled );
            cl.Items.Set( "PluginA-1", ConfigurationStatus.Running );

            engine.FullStaticResolutionOnly( res =>
                {
                    res.CheckAllBlockingPluginsAre( "PluginA-1" );
                    res.CheckWantedConfigSolvedStatusIs( "PluginA-1", SolvedConfigurationStatus.Running );
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
            DiscoveredInfo info = MockInfoFactory.ServiceWithTwoPlugins();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceA", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginA-1", ConfigurationStatus.Runnable );
            cl.Items.Set( "PluginA-2", ConfigurationStatus.Runnable );
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

            DiscoveredInfo info = MockInfoFactory.ServiceWithTwoPlugins();

            info.PluginInfos.Remove( info.FindPlugin( "PluginA-2" ) );
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceA", ConfigurationStatus.Disabled );
            cl.Items.Set( "PluginA-1", ConfigurationStatus.Running );

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

            DiscoveredInfo info = MockInfoFactory.ThreeServicesAndTwoPlugins();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceAx1", ConfigurationStatus.Running );
            cl.Items.Set( "ServiceAx2", ConfigurationStatus.Running );

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

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.ThreeServicesAndTwoPlugins() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "ServiceA", ConfigurationStatus.Running );
            cl.Items.Set( "ServiceAx1", ConfigurationStatus.Running );
            cl.Items.Set( "ServiceAx2", ConfigurationStatus.Running );

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
            DiscoveredInfo info = MockInfoFactory.MutualExclusionsViaRunningDependencies();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( info );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "Service1", ConfigurationStatus.Running );
            cl.Items.Set( "Service2", ConfigurationStatus.Running );

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
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            var disco = MockInfoFactory.SystematicMutualExclusionsViaRunningDependencies();
            var anotherBlocking = new ServiceInfo( "AnotherBlocking", disco.DefaultAssembly );
            var disabledForBlocking = new PluginInfo( "DisabledForBlocking", disco.DefaultAssembly );
            disabledForBlocking.Service = anotherBlocking;
            disco.ServiceInfos.Add( anotherBlocking );
            disco.PluginInfos.Add( disabledForBlocking );
            engine.Configuration.SetDiscoveredInfo( disco );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "Service1", ConfigurationStatus.Running );
            cl.Items.Set( "Service2", ConfigurationStatus.Running );
            cl.Items.Set( "AnotherBlocking", ConfigurationStatus.Runnable );
            cl.Items.Set( "DisabledForBlocking", ConfigurationStatus.Disabled );

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
                    res.CheckAllPluginsRunnable( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5, Plugin6, Plugin7, Plugin8" );
                } );
        }
        [Test]
        public void RunningServiceWithPlugin()
        {
            #region graph
            /**
             *                  +--------+
             *      +---------->|   S1   |
             *      |           |        |
             *      |           +---+----+       
             *      |               |            
             *      |               |                     
             *      |           +---+-----+              
             *  +---+-----+     |   P     |  
             *  |  S.1.1  |     |         |  
             *  | Running |     +---------+  
             *  +----+----+   
             *       |
             *   +---+-----+              
             *   |   P1    |  
             *   |         |  
             *   +---------+        
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("S1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("S1.1", d.DefaultAssembly));
            d.FindService("S1.1").Generalization = d.FindService("S1");

            d.PluginInfos.Add(new PluginInfo("P", d.DefaultAssembly));
            d.FindPlugin("P").Service = d.FindService("S1");
            d.PluginInfos.Add(new PluginInfo("P1", d.DefaultAssembly));
            d.FindPlugin("P1").Service = d.FindService("S1.1");

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set("S1.1", ConfigurationStatus.Running);

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("P");
            });
        }
        [Test]
        public void RunningServiceWithRunningSiblingPlugin()
        {
            #region graph
            /**
             *                  +--------+
             *      +---------->|   S1   |
             *      |           |        |
             *      |           +---+----+       
             *      |               |            
             *      |               |                     
             *      |           +---+-----+              
             *  +---+-----+     |   P     |  
             *  |  S.1.1  |     | Running |  
             *  | Running |     +---------+  
             *  +----+----+   
             *       |
             *   +---+-----+              
             *   |   P1    |  
             *   |         |  
             *   +---------+        
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("S1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("S1.1", d.DefaultAssembly));
            d.FindService("S1.1").Generalization = d.FindService("S1");

            d.PluginInfos.Add(new PluginInfo("P", d.DefaultAssembly));
            d.FindPlugin("P").Service = d.FindService("S1");
            d.PluginInfos.Add(new PluginInfo("P1", d.DefaultAssembly));
            d.FindPlugin("P1").Service = d.FindService("S1.1");

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set("S1.1", ConfigurationStatus.Running);
            cl.Items.Set("P", ConfigurationStatus.Running);

            var result = engine.StartEngine();
            Assert.That(result.Success, Is.False);
        }


        [Test]
        public void InvalidLoop()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|Service1+-------+   *----------------->|Service2|
             *      |           |Optional|       |   | Need Running     |Optional|   
             *      |           +---+----+       |   |                  +---+----+
             *      |                        +---+-----+                    |
             *      |                        |Plugin1  |                    |
             *      |                        |Optional |                    |
             *  +---+------+                 +---+-----+                    |
             *  |Service1.1|                                                |
             *  |Optional  |-----------------+                              |
             *  +----+-----+                 |                          +---+-----+
             *       |                       ---------------------------|Plugin2  |
             *       |                                  Need Running    |Optional |
             *       |                                                  +---------+
             *       |
             *       |      
             *       |      
             *  +---+-------+
             *  |Plugin1.1  |
             *  |Optional   |
             *  +-----------+
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1", d.DefaultAssembly));
            d.FindPlugin("Plugin1").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");
            d.PluginInfos.Add(new PluginInfo("Plugin2", d.DefaultAssembly));
            d.FindPlugin("Plugin2").Service = d.FindService("Service2");

            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service2"), DependencyRequirement.Running);
            d.FindPlugin("Plugin2").AddServiceReference(d.FindService("Service1.1"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("Plugin1");
                res.CheckAllPluginsRunnable("Plugin2,Plugin1.1");
                System.Diagnostics.Debug.WriteLine(res.StaticSolvedConfiguration.FindPlugin("Plugin2").FinalConfigSolvedStatus.ToString());
                res.CheckAllServicesRunnable("Service1,Service1.1,Service2");
            });
        }

        [Test]
        public void InvalidLoop2()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|Service1+-------+   *----------------->|Service2|---+
             *      |           |Optional|       |   | Need Running     |Optional|   |
             *      |           +---+----+       |   |                  +---+----+   |
             *      |               |        +---+-----+                    |        |
             *      |               |        |Plugin1  |                    |        |
             *      |               |        |Optional |                    |        |
             *  +---+------+        |        +---+-----+                    |        |
             *  |Service1.1|        |                                       |        |
             *  |Optional  |-----------------+                              |        |
             *  +----+-----+        |        |                          +---+-----+  |
             *       |              |        ---------------------------|Plugin2  |  |
             *       |              |                   Need Running    |Optional |  |
             *       |          +---+------+                            +---------+  |
             *       |          |Plugin1bis|Need Running                             |
             *       |          |Optional  |--------+                                |
             *       |          +----------+        |                   +--------+   |
             *  +---+-------+                       --------------------|Service3|   |
             *  |Plugin1.1  |                                           |Optional|   |
             *  |Optional   |                                           +---+----+   |
             *  +-----------+                                               |        |
             *                                                              |        |
             *                                                          +---+-----+  |
             *                                                          |Plugin3  |--+
             *                                                          |Optional | Need Running 
             *                                                          +---------+  
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service3", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1", d.DefaultAssembly));
            d.FindPlugin("Plugin1").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin1bis", d.DefaultAssembly));
            d.FindPlugin("Plugin1bis").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");
            d.PluginInfos.Add(new PluginInfo("Plugin2", d.DefaultAssembly));
            d.FindPlugin("Plugin2").Service = d.FindService("Service2");
            d.PluginInfos.Add(new PluginInfo("Plugin3", d.DefaultAssembly));
            d.FindPlugin("Plugin3").Service = d.FindService("Service3");

            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service2"), DependencyRequirement.Running);
            d.FindPlugin("Plugin1bis").AddServiceReference(d.FindService("Service3"), DependencyRequirement.Running);
            d.FindPlugin("Plugin2").AddServiceReference(d.FindService("Service1.1"), DependencyRequirement.Running);
            d.FindPlugin("Plugin3").AddServiceReference(d.FindService("Service2"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );



            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("Plugin1,Plugin1bis");
                res.CheckAllPluginsRunnable("Plugin2,Plugin1.1,Plugin3");
                System.Diagnostics.Debug.WriteLine(res.StaticSolvedConfiguration.FindPlugin("Plugin2").FinalConfigSolvedStatus.ToString());
                res.CheckAllServicesRunnable("Service1,Service1.1,Service2,Service3");
            });
        }

        [Test]
        public void ValidLoop1()
        {
            #region graph
            /**
             *  +--------+                              +--------+
             *  |Service1+-------+   *----------------->|Service2|---+
             *  |Optional|       |   | Need Running     |Optional|   |
             *  +---+----+       |   |                  +---+----+   |
             *      |  |     +---+-----+                    |        |
             *      |  |     |Plugin1  |                    |        |
             *      |  |     |Optional |                    |        |
             *      |  |     +---+-----+                    |        |
             *      |  |                                    |        |
             *      |  +-----+                              |        |
             *      |        |                          +---+-----+  |
             *      |        ---------------------------|Plugin2  |  |
             *      |                   Need Running    |Optional |  |
             *  +---+------+                            +---------+  |
             *  |Plugin1bis|Need Running                             |
             *  |Optional  |--------+                                |
             *  +----------+        |                   +--------+   |
             *                      --------------------|Service3|   |
             *                                          |Optional|   |
             *                                          +---+----+   |
             *                                              |        |
             *                                              |        |
             *                                          +---+-----+  |
             *                                          |Plugin3  |--+
             *                                          |Optional | Need Running 
             *                                          +---------+  
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service3", d.DefaultAssembly));

            d.PluginInfos.Add(new PluginInfo("Plugin1", d.DefaultAssembly));
            d.FindPlugin("Plugin1").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin1bis", d.DefaultAssembly));
            d.FindPlugin("Plugin1bis").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin2", d.DefaultAssembly));
            d.FindPlugin("Plugin2").Service = d.FindService("Service2");
            d.PluginInfos.Add(new PluginInfo("Plugin3", d.DefaultAssembly));
            d.FindPlugin("Plugin3").Service = d.FindService("Service3");

            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service2"), DependencyRequirement.Running);
            d.FindPlugin("Plugin1bis").AddServiceReference(d.FindService("Service3"), DependencyRequirement.Running);
            d.FindPlugin("Plugin2").AddServiceReference(d.FindService("Service1"), DependencyRequirement.Running);
            d.FindPlugin("Plugin3").AddServiceReference(d.FindService("Service2"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );



            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckAllPluginsRunnable("Plugin2,Plugin1,Plugin1bis,Plugin3");
                System.Diagnostics.Debug.WriteLine(res.StaticSolvedConfiguration.FindPlugin("Plugin2").FinalConfigSolvedStatus.ToString());
                res.CheckAllServicesRunnable("Service1,Service2,Service3");
            });
        }
        [Test]
        public void ValidInternalLoop1()
        {
            #region graph
            /**
             *                  +--------+          
             *      +---------->|Service1|
             *      |           |Optional|
             *      |           +--------+
             *      |                    
             *      |                    
             *      |                    
             *  +---+------+             
             *  |Service1.1|             
             *  |Optional  |--------+
             *  +----+-----+        |     
             *       |              |     
             *       |              |     
             *       |              |
             *       |              |
             *       |              |
             *       |              |
             *  +---+-------+       |      
             *  |Plugin1.1  |       |      
             *  |Optional   |-------+      
             *  +-----------+  Need Running           
             *                                                       
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");

            d.FindPlugin("Plugin1.1").AddServiceReference(d.FindService("Service1.1"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );



            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckAllPluginsRunnable("Plugin1.1,");
                res.CheckAllServicesRunnable("Service1,Service1.1");
            });
        }

        [Test]
        public void ValidInternalLoop2()
        {
            #region graph
            /**
             *                  +--------+          
             *      +---------->|Service1|
             *      |           |Optional|
             *      |           +---+----+
             *      |               |     
             *      |               |     
             *      |               |     
             *  +---+------+        |     
             *  |Service1.1|        |     
             *  |Optional  |        |
             *  +----+-----+        |     
             *       |              |     
             *       |              |     
             *       |              |
             *       |              |
             *       |              |
             *       |              |
             *  +---+-------+       |      
             *  |Plugin1.1  |       |      
             *  |Optional   |-------+      
             *  +-----------+  Need Running           
             *                                                       
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");

            d.FindPlugin("Plugin1.1").AddServiceReference(d.FindService("Service1"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckAllPluginsRunnable("Plugin1.1,");
                res.CheckAllServicesRunnable("Service1,Service1.1");
            });
        }

        [Test]
        public void InvalidInternalLoop1()
        {
            #region graph
            /**
             *                  +--------+           
             *      +---------->|Service1+-------+   
             *      |           |Optional|       |   
             *      |           +---+----+       |   
             *      |                        +---+-----+                  
             *      |                        |Plugin1  |                  
             *      |                        |Optional |                  
             *  +---+------+                 +---+-----+                  
             *  |Service1.1|                     | Need Running                       
             *  |Optional  |---------------------+                        
             *  +----+-----+                
             *       |         
             *       |                 
             *       |         
             *       |         
             *  +----+------+                
             *  |Plugin1.1  |                
             *  |Optional   |                
             *  +-----------+                
             *                                                            
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1", d.DefaultAssembly));
            d.FindPlugin("Plugin1").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");

            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service1.1"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("Plugin1");
                res.CheckAllPluginsRunnable("Plugin1.1");
                res.CheckAllServicesRunnable("Service1,Service1.1");
            });
        }
        [Test]
        public void InvalidInternalLoop2()
        {
            #region graph
            /**
             *                  +--------+           
             *      +---------->|Service1+---------+ 
             *      |           |Optional|         | 
             *      |           +---+----+         | 
             *      |                              | 
             *      |                              | 
             *      |                              | 
             *  +---+------+                   +---+------+
             *  |Service1.1|       +---------->|Service1.2|
             *  |Optional  +-------|-----+     |Optional  |
             *  +----+-----+       |     |     +----+-----+
             *       |             |     |          |       
             *       |             |     |          |       
             *       |             |     |      +---+------+
             *   +---+-------+     |     |      |Plugin1.2 |
             *   |Plugin1.1  |     |     +------+Optional  |
             *   |Optional   +-----+            +----------+
             *   +-----------+            Need Running                  
             *                Need Running      
             *                                                                
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.2", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");
            d.FindService("Service1.2").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1.2", d.DefaultAssembly));
            d.FindPlugin("Plugin1.2").Service = d.FindService("Service1.2");
            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");

            d.FindPlugin("Plugin1.1").AddServiceReference(d.FindService("Service1.2"), DependencyRequirement.Running);
            d.FindPlugin("Plugin1.2").AddServiceReference(d.FindService("Service1.1"), DependencyRequirement.Running);


            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("Plugin1.1,Plugin1.2");
                res.CheckAllServicesDisabled("Service1,Service1.1,Service1.2");
            });
        }
        [Test]
        public void InvalidInternalLoop2WithARunnableReference()
        {
            #region graph
            /**
             *                  +--------+           
             *      +---------->|Service1+---------+ 
             *      |           |Optional|         | 
             *      |           +---+----+         | 
             *      |                              | 
             *      |                              | 
             *      |                              | 
             *  +---+------+                   +---+------+
             *  |Service1.1|       +---------->|Service1.2|
             *  |Optional  +-------|-----+     |Optional  |
             *  +----+-----+       |     |     +----+-----+
             *       |             |     |          |       
             *       |             |     |          |       
             *       |             |     |      +---+------+
             *   +---+-------+     |     |      |Plugin1.2 |
             *   |Plugin1.1  |     |     +------+Optional  |
             *   |Optional   +-----+            +----------+
             *   +-----------+            Need Running                  
             *                Need Runnable      
             *                                                                
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.2", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");
            d.FindService("Service1.2").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1.2", d.DefaultAssembly));
            d.FindPlugin("Plugin1.2").Service = d.FindService("Service1.2");
            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");

            d.FindPlugin("Plugin1.1").AddServiceReference(d.FindService("Service1.2"), DependencyRequirement.Runnable);
            d.FindPlugin("Plugin1.2").AddServiceReference(d.FindService("Service1.1"), DependencyRequirement.Running);


            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("Plugin1.1,Plugin1.2");
                res.CheckAllServicesDisabled("Service1,Service1.1,Service1.2");
            });
        }
        internal static YodiiEngine CreateValidCommonReferences3()
        {
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.OtherMutualExclusionsViaRunningDependencies() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "Service1", ConfigurationStatus.Running );
            cl.Items.Set( "Service2", ConfigurationStatus.Running );
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
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.TwoExclusiveReferences( DependencyRequirement.Runnable ) );
            engine.Configuration.Layers.Default.Set( "Service1", ConfigurationStatus.Running );
            return engine;
        }

        [Test]
        public void ValidRunnableReferences()
        {
            var e = CreateValidRunnableReferences();
            e.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
                res.CheckAllServicesRunnable( "Service1, Service2, Service3, Service3.1, Service3.2, Service3.3, Service4, Service4.1, Service4.2" );
                res.CheckAllPluginsRunnable( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5, Plugin6, Plugin7, Plugin8, Plugin9" );
            } );
       }

        internal static YodiiEngine CreateValidRunnableReferences()
        {
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.CreateGraph006() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;

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
                res.CheckAllServicesRunnable( "Service1, Service1.1, Service1.2, Service1.3, Service2, Service2.1, Service2.2" );
                res.CheckAllPluginsRunnable( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5" );
            } );
        }

        internal static YodiiEngine CreateValidOnlyOneRunnableReference()
        {
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.CreateGraph007() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;

            return engine;
        }

        [Test]
        public void InvalidRunnableRecommendedReference()
        {
            // file://E:\Dev\Yodii\Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\InvalidRunnableRecommendedReference.png
            var e = CreateInvalidRunnableRecommendedReference();
            e.FullStaticResolutionOnly( res =>
            {
                Assert.That( res.StaticFailureResult, Is.Not.Null );
                res.CheckAllBlockingPluginsAre( "Plugin1, Plugin4, Plugin17, Plugin19, Plugin20" );
                res.CheckAllBlockingServicesAre( "Service1.2, Service2.1" );
            } );
        }

        internal static YodiiEngine CreateInvalidRunnableRecommendedReference()
        {
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.BigGraph() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "Plugin1", ConfigurationStatus.Runnable );
            cl.Items.Set( "Plugin3", ConfigurationStatus.Disabled );
            cl.Items.Set( "Plugin4", ConfigurationStatus.Running );
            cl.Items.Set( "Plugin6", ConfigurationStatus.Disabled );
            cl.Items.Set( "Plugin8", ConfigurationStatus.Runnable );
            cl.Items.Set( "Plugin9", ConfigurationStatus.Disabled );
            cl.Items.Set( "Plugin10", ConfigurationStatus.Runnable );
            cl.Items.Set( "Plugin14", ConfigurationStatus.Disabled );
            cl.Items.Set( "Plugin17", ConfigurationStatus.Running );
            cl.Items.Set( "Plugin19", ConfigurationStatus.Runnable );
            cl.Items.Set( "Plugin20", ConfigurationStatus.Runnable );
            cl.Items.Set( "Plugin24", ConfigurationStatus.Disabled );

            cl.Items.Set( "Service2", ConfigurationStatus.Disabled );
            cl.Items.Set( "Service2.1", ConfigurationStatus.Running );
            cl.Items.Set( "Service1.1", ConfigurationStatus.Running );
            cl.Items.Set( "Service1.2", ConfigurationStatus.Runnable );
            cl.Items.Set( "Service1.1.2", ConfigurationStatus.Runnable );
            cl.Items.Set( "Service1.1.3", ConfigurationStatus.Runnable );

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
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.TwoExclusiveReferences( DependencyRequirement.Optional ) );

            engine.Configuration.Layers.Default.Set( "Service1", ConfigurationStatus.Running );

            return engine;
        }

        [Test]
        public void ValidOptionalRecommendedReferences()
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

            YodiiEngine engine = CreateValidOptionalRecommendedReferences();

            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();               
            } );
        }

        internal static YodiiEngine CreateValidOptionalRecommendedReferences()
        {
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.CreateGraph005f() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "Service1", ConfigurationStatus.Running );
            cl.Items.Set( "Service2", ConfigurationStatus.Running );
            return engine;
        }        
        
        [Test]
        public void InvalidLoop3ValidWithDoubleRunnableReference()
        {
            #region graph
            /**
             *                         +--------+             
             *                         |Service2+-------------+
             *                         |Optional|             |
             *                         +---+----+             |
             *                             |                  |                +----------+
             *                             |                  +--------------->|Service2.2|
             *                             |                                   |Optional  |
             *  +---+------+               |                                   +-+------+-+
             *  |Service1  |               |                                     |      |       
             *  |Optional  |               |                     +---------------+      |       
             *  +----+-----+               |                     |                      |       
             *       |                     |                     |                      |       
             *       |                     |                     |                      |       
             *       |                 +---+------+              |                      |       
             *       |    Need Runnable|Service2.1|              |                  +---+-----+                           
             *       |    +----------->|Optional  |              |                  |Plugin2.2|             
             *       |    |            +----------+              |                  |Optional |             
             *  +----+----+-+               |                    |                  +---------+             
             *  |Plugin1    |               |                    |            
             *  |Optional   |               |                    |            
             *  +---------+-+           +---+------+             |            
             *            |             |Plugin2.1 |             |            
             *            |             |Optional  |             |            
             *            |             +----------+             |            
             *            |                                      |            
             *            |                                      |            
             *            |                                      |
             *            |Need Runnable+---+------+             |
             *            +------------>|Service3  |             |
             *                          |Optional  |             |
             *                          +----------+             |
             *                               |                   |
             *                               |                   |
             *                               |                   |
             *                           +---+------+            |
             *                           |Plugin3   |------------+   
             *                           |Optional  |  Need Running 
             *                           +----------+   
             * 
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2.2", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service3", d.DefaultAssembly));
            d.FindService("Service2.1").Generalization = d.FindService("Service2");
            d.FindService("Service2.2").Generalization = d.FindService("Service2");

            d.PluginInfos.Add(new PluginInfo("Plugin1", d.DefaultAssembly));
            d.FindPlugin("Plugin1").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin2.1", d.DefaultAssembly));
            d.FindPlugin("Plugin2.1").Service = d.FindService("Service2.1");
            d.PluginInfos.Add(new PluginInfo("Plugin2.2", d.DefaultAssembly));
            d.FindPlugin("Plugin2.2").Service = d.FindService("Service2.2");
            d.PluginInfos.Add(new PluginInfo("Plugin3", d.DefaultAssembly));
            d.FindPlugin("Plugin3").Service = d.FindService("Service3");

            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service2.1"), DependencyRequirement.Runnable);
            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service3"), DependencyRequirement.Runnable);
            d.FindPlugin("Plugin3").AddServiceReference(d.FindService("Service2.2"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckAllPluginsRunnable("Plugin1,Plugin2.1,Plugin2.2,Plugin3");
                res.CheckAllServicesRunnable("Service1,Service2,Service2.1,Service2.2,Service3");
            });
        }

        [Test]
        public void DoubleInvalidLoopWithRunningReferences()
        {
            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2.2", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service3", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service3.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service4", d.DefaultAssembly));

            d.FindService("Service2.1").Generalization = d.FindService("Service2");
            d.FindService("Service2.2").Generalization = d.FindService("Service2");
            d.FindService("Service3.1").Generalization = d.FindService("Service3");

            d.PluginInfos.Add(new PluginInfo("Plugin1", d.DefaultAssembly));
            d.FindPlugin("Plugin1").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin2", d.DefaultAssembly));
            d.FindPlugin("Plugin2").Service = d.FindService("Service2.1");
            d.PluginInfos.Add(new PluginInfo("Plugin3", d.DefaultAssembly));
            d.FindPlugin("Plugin3").Service = d.FindService("Service2.2");
            d.PluginInfos.Add(new PluginInfo("Plugin4", d.DefaultAssembly));
            d.FindPlugin("Plugin4").Service = d.FindService("Service3");

            d.PluginInfos.Add(new PluginInfo("Plugin5", d.DefaultAssembly));
            d.FindPlugin("Plugin5").Service = d.FindService("Service3.1");

            d.PluginInfos.Add(new PluginInfo("Plugin6", d.DefaultAssembly));
            d.FindPlugin("Plugin6").Service = d.FindService("Service4");

            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service2.1"), DependencyRequirement.Running);
            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service3"), DependencyRequirement.Running);
            d.FindPlugin("Plugin4").AddServiceReference(d.FindService("Service4"), DependencyRequirement.Running);
            d.FindPlugin("Plugin5").AddServiceReference(d.FindService("Service2.2"), DependencyRequirement.Running);
            d.FindPlugin("Plugin6").AddServiceReference(d.FindService("Service3.1"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new BuggyYodiiEngineHostMock());
            engine.Configuration.SetDiscoveredInfo( d );

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckAllPluginsRunnable("Plugin2,Plugin3,Plugin5,Plugin6");
                res.CheckAllServicesRunnable("Service2,Service2.1,Service2.2,Service3, Service3.1, Service4");
            });
        }

        internal static YodiiEngine CreateDynamicInvalidLoop()
        {
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.Configuration.SetDiscoveredInfo( MockInfoFactory.DynamicInvalidLoop() );

            IConfigurationLayer cl = engine.Configuration.Layers.Default;
            cl.Items.Set( "Service1", ConfigurationStatus.Running );

            return engine;
        }
    }
}
