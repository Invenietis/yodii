using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using QuickGraph;
using Yodii.Model;
using Yodii.Lab.Mocks;

namespace Yodii.Lab.Tests
{
    [TestFixture(Category="Yodii.Lab")]
    public class MainWindowViewModelTests
    {
        [Test]
        public void AddServicesPluginsTest()
        {
            CreateBasePlugins();
        }

        [Test]
        public void GraphIntegrityTest()
        {
            var vm = CreateBasePlugins();

            Assert.That( vm.Graph.Vertices.Count() == 8 );
            Assert.That( vm.Graph.Edges.Count() == 6 );

            Assert.That( vm.Graph.Vertices.Where( v => v.IsService ).Count() == 3 );
            Assert.That( vm.Graph.Vertices.Where( v => v.IsPlugin ).Count() == 5 );

            Assert.That( vm.Graph.Edges.Where( e => e.Type == YodiiGraphEdgeType.Implementation ).Count() == 4 );
            Assert.That( vm.Graph.Edges.Where( e => e.Type == YodiiGraphEdgeType.Specialization ).Count() == 1 );
            Assert.That( vm.Graph.Edges.Where( e => e.Type == YodiiGraphEdgeType.ServiceReference ).Count() == 1 );

            // Check integrity, by service
            foreach( var serviceInfo in vm.ServiceInfos )
            {
                Assert.That( vm.Graph.Vertices.Any( v => v.IsService && v.ServiceInfo == serviceInfo ) );
                if( serviceInfo.Generalization != null )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.Specialization && e.Source.ServiceInfo == serviceInfo && e.Target.ServiceInfo == serviceInfo.Generalization ) );
                }
                foreach( var p in serviceInfo.Implementations )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.Implementation && e.Source.PluginInfo == p && e.Target.ServiceInfo == serviceInfo ) );
                }
            }

            // Check by plugin
            foreach( var pluginInfo in vm.PluginInfos )
            {
                Assert.That( vm.Graph.Vertices.Any( v => v.IsPlugin && v.PluginInfo == pluginInfo ) );

                if( pluginInfo.Service != null )
                {
                    Assert.That( vm.Graph.Vertices.Any( v => v.IsService && v.ServiceInfo == pluginInfo.Service ) );
                }

                foreach( var reference in pluginInfo.ServiceReferences )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.ServiceReference && e.Source.PluginInfo == reference.Owner && e.Target.ServiceInfo == reference.Reference && e.ReferenceRequirement == reference.Requirement ) );
                }
            }

            // Simple remove test
            vm.RemovePlugin( vm.PluginInfos.Where( i => i.PluginFullName == "Plugin.Without.Service" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 7 );
            Assert.That( vm.Graph.Edges.Count() == 6 );
            /**
             *                 +--------+                              +--------+
             *     +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *     |           +---+----+       |   | Need Running     +---+----+
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             * +---+-----+     +---+-----+  +---+---*-+                +---+-----+
             * |ServiceAx|     |PluginA-1|  |PluginA-2|                |PluginB-1|
             * +----+----+     +---------+  +---------+                +---------+
             *      |
             *      |
             * +----+-----+
             * |PluginAx-1|
             * +----------+
             */


            // Removing a service also removes its implementations
            vm.RemoveService( vm.ServiceInfos.Where( x => x.ServiceFullName == "ServiceAx" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 5 );
            Assert.That( vm.Graph.Edges.Count() == 4 );
            /**
             *                 +--------+                              +--------+
             *                 |ServiceA+-------+   *----------------->|ServiceB|
             *                 +---+----+       |   | Need Running     +---+----+
             *                     |            |   |                      |
             *                     |            |   |                      |
             *                     |            |   |                      |
             *                     |            |   |                      |
             *                 +---+-----+  +---+---*-+                +---+-----+
             *                 |PluginA-1|  |PluginA-2|                |PluginB-1|
             *                 +---------+  +---------+                +---------+
             */


            // Removing a plugin also removes its references
            vm.RemovePlugin( vm.PluginInfos.Where( x => x.PluginFullName == "Plugin.A2" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 4 );
            Assert.That( vm.Graph.Edges.Count() == 2 );
            /**
             * +--------+     +--------+
             * |ServiceA|     |ServiceB|
             * +---+----+     +---+----+
             *     |              |
             *     |              |
             *     |              |
             *     |              |
             * +---+-----+    +---+-----+
             * |PluginA-1|    |PluginB-1|
             * +---------+    +---------+
             */


            // Reset
            vm = CreateBasePlugins();

            // Deleting a root service entirely destroys its tree
            vm.RemoveService( vm.ServiceInfos.Where( x => x.ServiceFullName == "ServiceA" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 3 );
            Assert.That( vm.Graph.Edges.Count() == 1 );
        }

        internal MainWindowViewModel CreateBasePlugins()
        {
            /** Imagine a graph like this:
             *                 +--------+                              +--------+
             *     +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *     |           +---+----+       |   | Need Running     +---+----+
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             * +---+-----+     +---+-----+  +---+---*-+                +---+-----+
             * |ServiceAx|     |PluginA-1|  |PluginA-2|                |PluginB-1|
             * +----+----+     +---------+  +---------+                +---------+
             *      |
             *      |
             * +----+-----+    +----------------------+
             * |PluginAx-1|    |Plugin.Without.Service|
             * +----------+    +----------------------+
             */
            MainWindowViewModel vm = new MainWindowViewModel();

            Assert.That( vm.IsLive, Is.False );

            // Services
            IServiceInfo serviceA = vm.CreateNewService( "ServiceA" );

            Assert.That( vm.ServiceInfos.Contains( serviceA ) );
            Assert.That( vm.ServiceInfos.Count == 1 );

            IServiceInfo serviceB = vm.CreateNewService( "ServiceB" );

            Assert.That( vm.ServiceInfos.Contains( serviceB ) );
            Assert.That( vm.ServiceInfos.Count == 2 );

            IServiceInfo serviceAx = vm.CreateNewService( "ServiceAx", serviceA );

            Assert.That( vm.ServiceInfos.Contains( serviceAx ) );
            Assert.That( vm.ServiceInfos.Count == 3 );

            Assert.That( serviceA.Generalization == null );
            Assert.That( serviceB.Generalization == null );
            Assert.That( serviceAx.Generalization == serviceA );

            // Plugins
            IPluginInfo pluginWithoutService = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.Without.Service" );

            IPluginInfo pluginA1 = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.A1", serviceA );

            IPluginInfo pluginA2 = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.A2", serviceA );

            IPluginInfo pluginAx1 = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.Ax1", serviceAx );

            IPluginInfo pluginB1 = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.B1", serviceB );

            vm.SetPluginDependency( pluginA2, serviceB, RunningRequirement.Running );

            Assert.That( pluginA2.ServiceReferences.Count == 1 );
            Assert.That( pluginA2.ServiceReferences[0].Reference == serviceB );
            Assert.That( pluginA2.ServiceReferences[0].Requirement == RunningRequirement.Running );

            Assert.That( serviceA.Implementations.Count == 2 );
            Assert.That( serviceB.Implementations.Count == 1 );
            Assert.That( serviceAx.Implementations.Count == 1 );

            return vm;
        }
    }
}
