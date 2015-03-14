using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.ObjectExplorer.ViewModels;
using Yodii.Wpf.Tests;

namespace Yodii.ObjectExplorer.Tests
{
    [TestFixture]
    public class ObjectExplorerViewModelTests
    {
        [Test]
        public void ObjectExplorerViewModel_CanBeInstanciated()
        {
            ObjectExplorerViewModel vm = new ObjectExplorerViewModel();

            Assert.That( vm.Engine, Is.Null );
            Assert.That( vm.EngineViewModel, Is.Null );
        }

        [Test]
        public void ObjectExplorerViewModel_CanLoadEngine()
        {
            // Also tests PropertyChanged, which we'll only test once because IN FODY WE TRUST.
            using( var ctx = new YodiiRuntimeTestContext() )
            {
                bool firedCorrectPropertyChanged = false;
                ObjectExplorerViewModel vm = new ObjectExplorerViewModel();
                vm.PropertyChanged += ( s, e ) => { if( e.PropertyName == "Engine" ) firedCorrectPropertyChanged = true; };

                vm.LoadEngine( ctx.GenericEngineProxy );

                Assert.That( firedCorrectPropertyChanged, Is.True );
                Assert.That( vm.Engine, Is.Not.Null );
                Assert.That( vm.EngineViewModel, Is.Not.Null );
                Assert.That( vm.Engine.ExternalEngine, Is.SameAs( ctx.Engine ) );
            }

        }

        [Test]
        public void ObjectExplorerViewModel_LoadEngine_ThrowsArgumentNullException_WithNullEngine()
        {
            ObjectExplorerViewModel vm = new ObjectExplorerViewModel();

            Assert.Throws<ArgumentNullException>( () => vm.LoadEngine( null ) );
        }

        [Test]
        public void ObjectExplorerViewModel_Engine_CannotBeSetTwice()
        {
            using( var ctx = new YodiiRuntimeTestContext() )
            {
                ObjectExplorerViewModel vm = new ObjectExplorerViewModel();

                vm.LoadEngine( ctx.GenericEngineProxy );
                Assert.Throws<InvalidOperationException>( () => vm.LoadEngine( ctx.GenericEngineProxy ) );
            }
        }
    }
}
