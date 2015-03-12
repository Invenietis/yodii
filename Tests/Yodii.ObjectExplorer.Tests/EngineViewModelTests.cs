using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using NUnit.Framework;
using Yodii.Model;
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

                vm.LoadEngine( ctx.Engine );

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

                vm.LoadEngine( ctx.Engine );
                Assert.Throws<InvalidOperationException>( () => vm.LoadEngine( ctx.Engine ) );
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
                vm.LoadEngine( ctx.Engine );

                CollectionAssert.IsEmpty( vm.Plugins );
                CollectionAssert.IsEmpty( vm.Services );

                // Change DiscoveredInfo to add this assembly (1 service, 1 plugin)
                IYodiiEngineResult r = ctx.Engine.Configuration.SetDiscoveredInfo( TestHelper.GetDiscoveredInfoInAssembly( Assembly.GetExecutingAssembly() ) );
                Assert.That( r.Success, Is.True );
                Assert.That( ctx.Engine.LiveInfo.Plugins, Has.Count.EqualTo( 1 ) );
                Assert.That( ctx.Engine.LiveInfo.Services, Has.Count.EqualTo( 1 ) );

                Assert.That( vm.Plugins, Has.Count.EqualTo( 1 ) );
                Assert.That( vm.Services, Has.Count.EqualTo( 1 ) );

                // Remove assembly and reset empty discovery
                r = ctx.Engine.Configuration.SetDiscoveredInfo( TestHelper.GetEmptyDiscoveredInfo() );
                Assert.That( r.Success, Is.True );
                CollectionAssert.IsEmpty( ctx.Engine.LiveInfo.Plugins );
                CollectionAssert.IsEmpty( ctx.Engine.LiveInfo.Services );

                CollectionAssert.IsEmpty( vm.Plugins );
                CollectionAssert.IsEmpty( vm.Services );

            }
        }
    }
}
