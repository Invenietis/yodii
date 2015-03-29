using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using NUnit.Framework;
using Yodii.Model;
using Yodii.ObjectExplorer.Tests.TestYodiiObjects;
using Yodii.ObjectExplorer.ViewModels;
using Yodii.Wpf.Tests;

namespace Yodii.ObjectExplorer.Tests
{
    [TestFixture]
    public class EngineViewModelTests
    {
        [Test]
        public void EngineViewModel_CanBeInstanciated()
        {
            EngineViewModel vm = new EngineViewModel();

            Assert.That( vm.Engine, Is.Null );

            Assert.That( vm.Services, Is.Not.Null );
            Assert.That( vm.Services, Is.AssignableTo( typeof( IObservableReadOnlyCollection<ServiceViewModel> ) ) );

            Assert.That( vm.Plugins, Is.Not.Null );
            Assert.That( vm.Plugins, Is.AssignableTo( typeof( IObservableReadOnlyCollection<PluginViewModel> ) ) );
        }

        [Test]
        public void EngineViewModel_CanLoadEngine()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                CollectionAssert.IsNotEmpty( ctx.Engine.LiveInfo.Plugins );
                CollectionAssert.IsNotEmpty( ctx.Engine.LiveInfo.Services );

                EngineViewModel vm = new EngineViewModel();

                vm.LoadEngine( ctx.GenericEngineProxy );

                Assert.That( vm.Engine, Is.Not.Null );
                CollectionAssert.IsNotEmpty( vm.Plugins );
                CollectionAssert.IsNotEmpty( vm.Services );
            }
        }

        [Test]
        public void EngineViewModel_LoadEngine_Throws_ArgumentNull()
        {
            EngineViewModel vm = new EngineViewModel();

            Assert.Throws<ArgumentNullException>( () => { vm.LoadEngine( null ); } );
        }

        [Test]
        public void EngineViewModel_Engine_CannotBeSetTwice()
        {
            using( var ctx = new YodiiRuntimeTestContext() )
            {
                EngineViewModel vm = new EngineViewModel();

                vm.LoadEngine( ctx.GenericEngineProxy );
                Assert.Throws<InvalidOperationException>( () => vm.LoadEngine( ctx.GenericEngineProxy ) );
            }
        }

        [Test]
        public void EngineViewModel_CorrectlyUpdatesViewModels_OnDiscoveredInfoChanges()
        {
            using( var ctx = YodiiRuntimeTestContext.FromEmptyDiscoveredInfo() )
            {
                CollectionAssert.IsEmpty( ctx.Engine.LiveInfo.Plugins );
                CollectionAssert.IsEmpty( ctx.Engine.LiveInfo.Services );

                EngineViewModel vm = new EngineViewModel();

                // Load with empty
                vm.LoadEngine( ctx.GenericEngineProxy );

                CollectionAssert.IsEmpty( vm.Plugins );
                CollectionAssert.IsEmpty( vm.Services );

                // Change DiscoveredInfo to add this assembly
                IYodiiEngineResult r = ctx.Engine.Configuration.SetDiscoveredInfo( TestHelper.GetDiscoveredInfoInAssembly( Assembly.GetExecutingAssembly() ) );
                Assert.That( r.Success, Is.True );
                Assert.That( ctx.Engine.LiveInfo.Plugins, Is.Not.Empty );
                Assert.That( ctx.Engine.LiveInfo.Services, Is.Not.Empty );

                Assert.That( vm.Plugins, Is.Not.Empty );
                Assert.That( vm.Services, Is.Not.Empty );

                // Remove assembly and reset empty discovery
                r = ctx.Engine.Configuration.SetDiscoveredInfo( TestHelper.GetEmptyDiscoveredInfo() );
                Assert.That( r.Success, Is.True );
                CollectionAssert.IsEmpty( ctx.Engine.LiveInfo.Plugins );
                CollectionAssert.IsEmpty( ctx.Engine.LiveInfo.Services );

                CollectionAssert.IsEmpty( vm.Plugins );
                CollectionAssert.IsEmpty( vm.Services );

            }
        }

        [Test]
        public void EngineViewModel_AndSubViewModels_HaveCorrectHierarchy()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                CollectionAssert.IsNotEmpty( ctx.Engine.LiveInfo.Plugins );
                CollectionAssert.IsNotEmpty( ctx.Engine.LiveInfo.Services );

                EngineViewModel vm = new EngineViewModel();
                vm.LoadEngine( ctx.GenericEngineProxy );

                var rootService = vm.FindService( typeof( IMyYodiiService ).FullName );
                var rootServicePlugin = vm.FindPlugin( typeof( MyYodiiPlugin ).FullName );

                var rootPlugin = vm.FindPlugin( typeof( PluginWithoutService ).FullName );

                var subService = vm.FindService( typeof( ISubService ).FullName );
                var subServicePlugin = vm.FindPlugin( typeof( SubServicePlugin ).FullName );

                Assert.That( rootService, Is.Not.Null );
                Assert.That( rootServicePlugin, Is.Not.Null );
                Assert.That( rootPlugin, Is.Not.Null );
                Assert.That( subService, Is.Not.Null );
                Assert.That( subServicePlugin, Is.Not.Null );

                Assert.That( rootService.Generalization, Is.Null );
                Assert.That( rootService.Parent, Is.Null );
                Assert.That( rootService.Children, Contains.Item( rootServicePlugin ) );
                Assert.That( rootService.Children, Contains.Item( subService ) );

                Assert.That( rootServicePlugin.Service, Is.EqualTo( rootService ) );
                Assert.That( rootServicePlugin.Parent, Is.EqualTo( rootService ) );
                Assert.That( rootServicePlugin.Children, Is.Not.Null.And.Empty );

                Assert.That( rootPlugin.Service, Is.Null );
                Assert.That( rootPlugin.Parent, Is.Null );
                Assert.That( rootPlugin.Children, Is.Not.Null.And.Empty );

                Assert.That( subService.Generalization, Is.EqualTo( rootService ) );
                Assert.That( subService.Parent, Is.EqualTo( rootService ) );
                Assert.That( subService.Children, Has.Member( subServicePlugin ) );
                Assert.That( subService.Children, Has.No.Member( rootService ) );

                Assert.That( subServicePlugin.Service, Is.EqualTo( subService ) );
                Assert.That( subServicePlugin.Parent, Is.EqualTo( subService ) );
                Assert.That( subServicePlugin.Children, Is.Not.Null.And.Empty );
            }
        }
    }
}
