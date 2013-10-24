using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using QuickGraph;
using Yodii.Model.CoreModel;

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

            Assert.That( vm.Graph.Vertices.Count() == 7 );
            Assert.That( vm.Graph.Edges.Count() == 6 );

            /** TODO **/
            //Assert.That( vm.Graph.Vertices.Where( v => v.Type == YodiiVertexType.Service ).Count() == 3 );
            //Assert.That( vm.Graph.Vertices.Where( v => v.Type == YodiiVertexType.Plugin ).Count() == 4 );

            //Assert.That( vm.Graph.Edges.Where( e => e.Type == YodiiEdgeType.Implementation ).Count() == 4 );
            //Assert.That( vm.Graph.Edges.Where( e => e.Type == YodiiEdgeType.Specialization ).Count() == 1 );
            //Assert.That( vm.Graph.Edges.Where( e => e.Type == YodiiEdgeType.Requirement ).Count() == 1 );

            //// Check integrity, by service
            //foreach( var serviceInfo in vm.ServiceInfos )
            //{
            //    Assert.That( vm.Graph.Vertices.Any( v => v.Type == YodiiVertexType.Service && v.ServiceInfo == serviceInfo ) );
            //    if( serviceInfo.Generalization != null )
            //    {
            //        Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiEdgeType.Specialization && e.Source.ServiceInfo == serviceInfo && e.Target.ServiceInfo == serviceInfo.Generalization ) );
            //    }
            //    foreach( var p in serviceInfo.Implementations )
            //    {
            //        Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiEdgeType.Implementation && e.Source.PluginInfo == p && e.Target.ServiceInfo == serviceInfo ) );
            //    }
            //}

            //// Check by plugin
            //foreach( var pluginInfo in vm.PluginInfos )
            //{
            //    Assert.That( vm.Graph.Vertices.Any( v => v.Type == YodiiVertexType.Plugin && v.PluginInfo == pluginInfo ) );

            //    if( pluginInfo.Service != null )
            //    {
            //        Assert.That( vm.Graph.Vertices.Any( v => v.Type == YodiiVertexType.Service && v.ServiceInfo == pluginInfo.Service ) );
            //    }

            //    foreach( var reference in pluginInfo.ServiceReferences )
            //    {
            //        Assert.That( vm.Graph.Vertices.Any( v => v.Type == YodiiVertexType.Requirement && v.Source.PluginInfo == reference.Owner && v.Target.ServiceInfo == reference.Reference && v.ReferenceRequirement == reference.Requirement ) );
            //    }
            //}
            
        }

        internal MainWindowViewModel CreateBasePlugins()
        {
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
