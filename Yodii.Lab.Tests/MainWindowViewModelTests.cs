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
    [TestFixture( Category = "Yodii.Lab" )]
    public class MainWindowViewModelTests
    {
        [Test]
        public void AddServicesPluginsTest()
        {
            CreateViewModelWithGraph001();
        }

        [Test]
        public void GraphIntegrityTest()
        {
            var vm = CreateViewModelWithGraph001();

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
                Assert.That( vm.Graph.Vertices.Any( v => v.IsService && v.LiveServiceInfo.ServiceInfo == serviceInfo ) );
                if( serviceInfo.Generalization != null )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.Specialization && e.Source.LiveServiceInfo.ServiceInfo == serviceInfo && e.Target.LiveServiceInfo.ServiceInfo == serviceInfo.Generalization ) );
                }
                foreach( var p in serviceInfo.Implementations )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.Implementation && e.Source.LivePluginInfo.PluginInfo == p && e.Target.LiveServiceInfo.ServiceInfo == serviceInfo ) );
                }
            }

            // Check by plugin
            foreach( var pluginInfo in vm.PluginInfos )
            {
                Assert.That( vm.Graph.Vertices.Any( v => v.IsPlugin && v.LivePluginInfo.PluginInfo == pluginInfo ) );

                if( pluginInfo.Service != null )
                {
                    Assert.That( vm.Graph.Vertices.Any( v => v.IsService && v.LiveServiceInfo.ServiceInfo == pluginInfo.Service ) );
                }

                foreach( var reference in pluginInfo.ServiceReferences )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.ServiceReference && e.Source.LivePluginInfo.PluginInfo == reference.Owner && e.Target.LiveServiceInfo.ServiceInfo == reference.Reference && e.ReferenceRequirement == reference.Requirement ) );
                }
            }

            // Simple remove test
            vm.RemovePlugin( vm.PluginInfos.Where( i => i.PluginFullName == "Plugin.Without.Service" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 7 );
            Assert.That( vm.Graph.Edges.Count() == 6 );

            Assert.That( vm.LivePluginInfos.Count == 4 );
            Assert.That( vm.LiveServiceInfos.Count == 3 );
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

            Assert.That( vm.LivePluginInfos.Count == 3 );
            Assert.That( vm.LiveServiceInfos.Count == 2 );
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
            vm = CreateViewModelWithGraph001();

            // Deleting a root service entirely destroys its tree
            vm.RemoveService( vm.ServiceInfos.Where( x => x.ServiceFullName == "ServiceA" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 3 );
            Assert.That( vm.Graph.Edges.Count() == 1 );
        }

        [Test]
        public void VertexSelection()
        {
            MainWindowViewModel vm = CreateViewModelWithGraph001();

            foreach( YodiiGraphVertex v in vm.Graph.Vertices )
            {
                vm.SelectedVertex = v;
                Assert.That( vm.SelectedVertex == v );
                Assert.That( vm.HasSelection );
            }

            vm.SelectedVertex = null;
            Assert.That( vm.SelectedVertex == null );
            Assert.That( !vm.HasSelection );
        }

        internal static MainWindowViewModel CreateViewModelWithGraph001()
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
            Assert.That( vm.LiveServiceInfos.Count == 1 );
            Assert.That( vm.LiveServiceInfos.Where( x => x.ServiceInfo == serviceA ).Count() == 1 );

            ILiveServiceInfo liveServiceA = vm.LiveServiceInfos.Where( x => x.ServiceInfo == serviceA ).First();

            IServiceInfo serviceB = vm.CreateNewService( "ServiceB" );

            Assert.That( vm.ServiceInfos.Contains( serviceB ) );
            Assert.That( vm.ServiceInfos.Count == 2 );
            Assert.That( vm.LiveServiceInfos.Count == 2 );
            Assert.That( vm.LiveServiceInfos.Where( x => x.ServiceInfo == serviceB ).Count() == 1 );

            IServiceInfo serviceAx = vm.CreateNewService( "ServiceAx", serviceA );

            Assert.That( vm.ServiceInfos.Contains( serviceAx ) );
            Assert.That( vm.ServiceInfos.Count == 3 );
            Assert.That( vm.LiveServiceInfos.Count == 3 );
            Assert.That( vm.LiveServiceInfos.Where( x => x.ServiceInfo == serviceAx ).Count() == 1 );

            ILiveServiceInfo liveServiceAx = vm.LiveServiceInfos.Where( x => x.ServiceInfo == serviceAx ).First();

            Assert.That( liveServiceAx.Generalization == liveServiceA );

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
