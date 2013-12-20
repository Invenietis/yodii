using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using QuickGraph;
using Yodii.Model;
using Yodii.Lab.Mocks;
using System.IO;
using Yodii.Lab.Utils;
using System.Xml;

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
                Assert.That( vm.Graph.Vertices.Any( v => v.IsService && v.LabServiceInfo.ServiceInfo == serviceInfo ) );
                if( serviceInfo.Generalization != null )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.Specialization && e.Source.LabServiceInfo.ServiceInfo == serviceInfo && e.Target.LabServiceInfo.ServiceInfo == serviceInfo.Generalization ) );
                }
                foreach( var p in serviceInfo.Implementations )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.Implementation && e.Source.LabPluginInfo.PluginInfo == p && e.Target.LabServiceInfo.ServiceInfo == serviceInfo ) );
                }
            }

            // Check by plugin
            foreach( var pluginInfo in vm.PluginInfos )
            {
                Assert.That( vm.Graph.Vertices.Any( v => v.IsPlugin && v.LabPluginInfo.PluginInfo == pluginInfo ) );

                if( pluginInfo.Service != null )
                {
                    Assert.That( vm.Graph.Vertices.Any( v => v.IsService && v.LabServiceInfo.ServiceInfo == pluginInfo.Service ) );
                }

                foreach( var reference in pluginInfo.ServiceReferences )
                {
                    Assert.That( vm.Graph.Edges.Any( e => e.Type == YodiiGraphEdgeType.ServiceReference && e.Source.LabPluginInfo.PluginInfo == reference.Owner && e.Target.LabServiceInfo.ServiceInfo == reference.Reference && e.ReferenceRequirement == reference.Requirement ) );
                }
            }

            // Simple remove test
            vm.RemovePlugin( vm.PluginInfos.Where( i => i.PluginFullName == "Plugin.Without.Service" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 7 );
            Assert.That( vm.Graph.Edges.Count() == 6 );

            Assert.That( vm.LabState.LabPluginInfos.Count == 4 );
            Assert.That( vm.LabState.LabServiceInfos.Count == 3 );
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


            // Removing a service does not remove its implementations, but does unbind them
            vm.RemoveService( vm.ServiceInfos.Where( x => x.ServiceFullName == "ServiceAx" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 6 );
            Assert.That( vm.Graph.Edges.Count() == 4 );

            Assert.That( vm.LabState.LabPluginInfos.Count == 4 );
            Assert.That( vm.LabState.LabServiceInfos.Count == 2 );
            /**
             *                 +--------+                              +--------+
             *                 |ServiceA+-------+   *----------------->|ServiceB|
             *                 +---+----+       |   | Need Running     +---+----+
             *                     |            |   |                      |
             *                     |            |   |                      |
             *                     |            |   |                      |
             *                     |            |   |                      |
             *  +----------+   +---+-----+  +---+---*-+                +---+-----+
             *  |PluginAx-1|   |PluginA-1|  |PluginA-2|                |PluginB-1|
             *  +----------+   +---------+  +---------+                +---------+
             */
            vm.RemovePlugin( vm.PluginInfos.Where( i => i.PluginFullName == "Plugin.Ax1" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 5 );
            Assert.That( vm.Graph.Edges.Count() == 4 );
            Assert.That( vm.LabState.LabPluginInfos.Count == 3 );
            Assert.That( vm.LabState.LabServiceInfos.Count == 2 );


            // Removing a plugin also removes its references
            vm.RemovePlugin( vm.PluginInfos.Where( x => x.PluginFullName == "Plugin.A2" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 4 );
            Assert.That( vm.Graph.Edges.Count() == 2 );
            Assert.That( vm.LabState.LabPluginInfos.Count == 2 );
            Assert.That( vm.LabState.LabServiceInfos.Count == 2 );
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

            // Deleting a root service unbinds but does not destroy its tree
            vm.RemoveService( vm.ServiceInfos.Where( x => x.ServiceFullName == "ServiceA" ).First() );

            Assert.That( vm.Graph.Vertices.Count() == 7 );
            Assert.That( vm.Graph.Edges.Count() == 3 );

            // Rebind service
            vm.GetServiceInfoByName( "ServiceAx" ).Generalization = vm.GetServiceInfoByName( "ServiceB" );

            Assert.That( vm.Graph.Vertices.Count() == 7 );
            Assert.That( vm.Graph.Edges.Count() == 4 );

            // Binding a plugin adds its new relationship to the graph

            vm.GetPluginInfosByName( "Plugin.Without.Service" ).First().Service = vm.GetServiceInfoByName( "ServiceB" );

            Assert.That( vm.Graph.Vertices.Count() == 7 );
            Assert.That( vm.Graph.Edges.Count() == 5 );

            // Remove on null
            vm.GetPluginInfosByName( "Plugin.Without.Service" ).First().Service = null;

            Assert.That( vm.Graph.Vertices.Count() == 7 );
            Assert.That( vm.Graph.Edges.Count() == 4 );
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

        [Test]
        public void SaveLoadViewModelTest()
        {
            MainWindowViewModel _vm1 = CreateViewModelWithGraph001();

            string tempFilePath = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName());

            DetailedOperationResult r = _vm1.SaveState( tempFilePath );

            Assert.That( r.IsSuccessful );

            MainWindowViewModel _vm2 = new MainWindowViewModel();

            DetailedOperationResult r2 = _vm2.LoadState( tempFilePath );

            File.Delete( tempFilePath );

            Assert.That( r2.IsSuccessful );

            Assert.That( _vm1.PluginInfos.Count == _vm2.PluginInfos.Count );
            foreach( var infoB in _vm2.PluginInfos )
            {
                Assert.That( _vm1.PluginInfos.Where( x => x.PluginFullName == infoB.PluginFullName ).Count() == 1 );
                IPluginInfo infoA = _vm1.PluginInfos.Where( x => x.PluginFullName == infoB.PluginFullName ).First();

                TestExtensions.AssertPluginEquivalence( infoA, infoB, true );
            }

            Assert.That( _vm1.ServiceInfos.Count == _vm2.ServiceInfos.Count );
            foreach( var infoB in _vm2.ServiceInfos )
            {
                Assert.That( _vm1.ServiceInfos.Where( x => x.ServiceFullName == infoB.ServiceFullName ).Count() == 1 );
                var infoA = _vm1.ServiceInfos.Where( x => x.ServiceFullName == infoB.ServiceFullName ).First();

                TestExtensions.AssertServiceEquivalence( infoA, infoB, true );
            }

            TestExtensions.AssertManagerEquivalence( _vm1.LabState.Engine.Configuration, _vm2.LabState.Engine.Configuration );
        }
        
        [Test]
        public void ViewModelRegistersAllChanges()
        {
            MainWindowViewModel emptyVm = new MainWindowViewModel();
            Assert.That( emptyVm.ChangedSinceLastSave, Is.True );
            Assert.That( emptyVm.OpenedFilePath, Is.Null );

            MainWindowViewModel vm = CreateViewModelWithGraph001();
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            Assert.That( vm.OpenedFilePath, Is.Null );

            string tempFilePath = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName() );

            DetailedOperationResult r = vm.SaveState( tempFilePath );
            Assert.That( r.IsSuccessful );
            Assert.That( vm.OpenedFilePath == tempFilePath );

            Assert.That( vm.ChangedSinceLastSave, Is.False );

            MainWindowViewModel vm2 = new MainWindowViewModel();
            DetailedOperationResult r2 = vm2.LoadState( tempFilePath );
            Assert.That( r2.IsSuccessful );

            Assert.That( vm2.ChangedSinceLastSave, Is.False );

            // Monitor Configuration changes:
            // Layer created
            var layer = vm.LabState.Engine.Configuration.Layers.Create();
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Layer item added
            var yr = layer.Items.Add( "PluginA-2", ConfigurationStatus.Disabled, "test" );
            Assert.That( yr.Success );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Layer item changed
            yr = layer.Items["PluginA-2"].SetStatus( ConfigurationStatus.Running );
            Assert.That( yr.Success );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Layer item removed
            yr = layer.Items.Remove( "PluginA-2" );
            Assert.That( yr.Success );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Layer removed
            yr = vm.LabState.Engine.Configuration.Layers.Remove( layer );
            Assert.That( yr.Success );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Monitor DiscoveredInfo changes:
            // Service added
            var newService = vm.CreateNewService( "ServiceC" );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Service name changed (White box testing)
            var castNewService = (ServiceInfo)newService;
            castNewService.ServiceFullName = "ServiceD";
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Service generalization changed (White box testing)
            castNewService.Generalization = vm.LabState.ServiceInfos.First( x => x.ServiceFullName == "ServiceB" );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Plugin added
            var newPlugin = vm.CreateNewPlugin( "My New Plugin" );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Plugin name changed (White box testing)
            var castNewPlugin = (PluginInfo)newPlugin;
            castNewPlugin.PluginFullName = "My New Plugin With A Better Name";
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Plugin service changed (White box testing)
            castNewPlugin.Service = castNewService;
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Service reference added
            vm.SetPluginDependency( newPlugin, newService, DependencyRequirement.Runnable );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Reference's Dependency modified (White box testing)
            var castReference = (MockServiceReferenceInfo)castNewPlugin.ServiceReferences.First();
            castReference.Requirement = DependencyRequirement.Running;
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Reference removed (White box testing)
            vm.RemovePluginDependency( newPlugin, newService );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            // Service deleted
            vm.RemoveService( newService );
            Assert.That( vm.ChangedSinceLastSave, Is.True );
            r = vm.SaveState( tempFilePath );
            Assert.That( vm.ChangedSinceLastSave, Is.False );

            Assert.That( vm.OpenedFilePath == tempFilePath );

            File.Delete( tempFilePath );
        }

        internal static MainWindowViewModel CreateViewModelWithGraph001()
        {
            /** Imagine a graph like this:
             *                 +RUNNING-+                              +--------+
             *     +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *     |           +---+----+       |   | Need Running     +---+----+
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             * +---+-----+     +---+-----+  +RUNNING*-+                +---+-----+
             * |ServiceAx|     |PluginA-1|  |PluginA-2|                |PluginB-1|
             * +----+----+     +---------+  +---------+                +---------+
             *      |
             *      |
             * +----+-----+    +----------------------+
             * |PluginAx-1|    |Plugin.Without.Service|
             * +----------+    +----------------------+
             */
            MainWindowViewModel vm = new MainWindowViewModel();

            Assert.That( vm.LabState.Engine.IsRunning, Is.False );

            // Services
            IServiceInfo serviceA = vm.CreateNewService( "ServiceA" );

            Assert.That( vm.ServiceInfos.Contains( serviceA ) );
            Assert.That( vm.ServiceInfos.Count == 1 );
            Assert.That( vm.LabState.LabServiceInfos.Count == 1 );
            Assert.That( vm.LabState.LabServiceInfos.Where( x => x.ServiceInfo == serviceA ).Count() == 1 );

            LabServiceInfo labServiceA = vm.LabState.LabServiceInfos.Where( x => x.ServiceInfo == serviceA ).First();

            IServiceInfo serviceB = vm.CreateNewService( "ServiceB" );

            Assert.That( vm.ServiceInfos.Contains( serviceB ) );
            Assert.That( vm.ServiceInfos.Count == 2 );
            Assert.That( vm.LabState.LabServiceInfos.Count == 2 );
            Assert.That( vm.LabState.LabServiceInfos.Where( x => x.ServiceInfo == serviceB ).Count() == 1 );

            IServiceInfo serviceAx = vm.CreateNewService( "ServiceAx", serviceA );

            Assert.That( vm.ServiceInfos.Contains( serviceAx ) );
            Assert.That( vm.ServiceInfos.Count == 3 );
            Assert.That( vm.LabState.LabServiceInfos.Count == 3 );
            Assert.That( vm.LabState.LabServiceInfos.Where( x => x.ServiceInfo == serviceAx ).Count() == 1 );

            LabServiceInfo labServiceAx = vm.LabState.LabServiceInfos.Where( x => x.ServiceInfo == serviceAx ).First();

            Assert.That( labServiceAx.ServiceInfo.Generalization == labServiceA.ServiceInfo );

            Assert.That( serviceA.Generalization == null );
            Assert.That( serviceB.Generalization == null );
            Assert.That( serviceAx.Generalization == serviceA );

            // Plugins
            IPluginInfo pluginWithoutService = vm.CreateNewPlugin( "Plugin.Without.Service" );

            IPluginInfo pluginA1 = vm.CreateNewPlugin( "Plugin.A1", serviceA );

            IPluginInfo pluginA2 = vm.CreateNewPlugin( "Plugin.A2", serviceA );

            IPluginInfo pluginAx1 = vm.CreateNewPlugin( "Plugin.Ax1", serviceAx );

            IPluginInfo pluginB1 = vm.CreateNewPlugin( "Plugin.B1", serviceB );

            vm.SetPluginDependency( pluginA2, serviceB, DependencyRequirement.Running );

            Assert.That( pluginA2.ServiceReferences.Count == 1 );
            Assert.That( pluginA2.ServiceReferences[0].Reference == serviceB );
            Assert.That( pluginA2.ServiceReferences[0].Requirement == DependencyRequirement.Running );

            Assert.That( serviceA.Implementations.Count == 2 );
            Assert.That( serviceB.Implementations.Count == 1 );
            Assert.That( serviceAx.Implementations.Count == 1 );

            // Set configuration
            var cLayer1 = vm.LabState.Engine.Configuration.Layers.Create( "Test layer" );
            var result = cLayer1.Items.Add( serviceA.ServiceFullName, ConfigurationStatus.Running, "Test reason" );
            Assert.That( result.Success );

            var cLayer2 = vm.LabState.Engine.Configuration.Layers.Create( "Test layer 2" );
            result = cLayer2.Items.Add( pluginA2.PluginFullName, ConfigurationStatus.Running, "Test reason 2" );
            Assert.That( result.Success );

            // Testing tests
            foreach(var si in vm.ServiceInfos )
            {
                TestExtensions.AssertServiceEquivalence( si, si, true );
            }
            foreach( var pi in vm.PluginInfos )
            {
                TestExtensions.AssertPluginEquivalence( pi, pi, true );
            }

            TestExtensions.AssertManagerEquivalence( vm.LabState.Engine.Configuration, vm.LabState.Engine.Configuration );

            return vm;
        }
    }
}
