using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Model;
using Yodii.ObjectExplorer.ViewModels;
using Yodii.Wpf.Tests;

namespace Yodii.ObjectExplorer.Tests
{
    [TestFixture]
    public class ServiceViewModelTests
    {
        [Test]
        public void ServiceViewModel_CanBeInstanciated()
        {
            ServiceViewModel vm = new ServiceViewModel();

            Assert.That( vm.Service, Is.Null );
        }

        [Test]
        public void ServiceViewModel_CanLoadService()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                CollectionAssert.IsNotEmpty( ctx.Engine.LiveInfo.Services );
                ILiveServiceInfo s = ctx.Engine.LiveInfo.Services.First();

                EngineViewModel evm = new EngineViewModel();
                evm.LoadEngine( ctx.GenericEngineProxy );

                ServiceViewModel vm = new ServiceViewModel();
                vm.LoadLiveItem( evm, s );
                Assert.That( vm.Service, Is.Not.Null );
            }
        }

        [Test]
        public void ServiceViewModel_CannotLoadServiceTwice()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                CollectionAssert.IsNotEmpty( ctx.Engine.LiveInfo.Services );
                ILiveServiceInfo s = ctx.Engine.LiveInfo.Services.First();

                EngineViewModel evm = new EngineViewModel();
                evm.LoadEngine( ctx.GenericEngineProxy );

                ServiceViewModel vm = new ServiceViewModel();
                vm.LoadLiveItem( evm, s );
                Assert.Throws<InvalidOperationException>( () => vm.LoadLiveItem( evm, s ) );
            }
        }

        [Test]
        public void ServiceViewModel_CorrectlyLoads_DisplayAttributeData()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                ILiveServiceInfo s = ctx.Engine.LiveInfo.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IServiceWithDisplayAttribute" );
                Assert.That( s, Is.Not.Null );

                EngineViewModel evm = new EngineViewModel();
                evm.LoadEngine( ctx.GenericEngineProxy );

                ServiceViewModel vm = evm.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IServiceWithDisplayAttribute" );

                //Assert.That( vm.DisplayName, Is.EqualTo( "My service (with display attribute)" ), "DisplayName should be retrieved from Display attribute's Name property" );
                Assert.That( vm.DisplayName, Is.EqualTo( "IServiceWithDisplayAttribute" ), "DisplayName is not supported by interfaces and cannot be set for a service." );
                Assert.That( vm.Description, Is.EqualTo( "A service with a display attribute." ), "Description should be retrieved from Display attribute" );
                Assert.That( vm.FullName, Is.EqualTo( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IServiceWithDisplayAttribute" ), "FullName is equal to the service's FullName" );
            }
        }
        [Test]
        public void ServiceViewModel_CorrectlyLoadsDefaultProperties_WithoutDisplayAttributeData()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                ILiveServiceInfo s = ctx.Engine.LiveInfo.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IMyYodiiService" );
                Assert.That( s, Is.Not.Null );

                EngineViewModel evm = new EngineViewModel();
                evm.LoadEngine( ctx.GenericEngineProxy );

                ServiceViewModel vm = evm.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IMyYodiiService" );

                Assert.That( vm.DisplayName, Is.EqualTo( "IMyYodiiService" ), "DisplayName should be the interface name without namespace when Display attribute's Name property is not used" );
                Assert.That( vm.Description, Is.EqualTo( String.Empty ), "Description should be empty when Display's Description is unused" );
                Assert.That( vm.FullName, Is.EqualTo( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IMyYodiiService" ), "FullName is equal to the service's FullName" );
            }
        }
        [Test]
        public void ServiceViewModel_CorrectlyLoads_LiveItem_And_AssemblyInfo()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                ILiveServiceInfo s = ctx.Engine.LiveInfo.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IMyYodiiService" );
                Assert.That( s, Is.Not.Null );

                EngineViewModel evm = new EngineViewModel();
                evm.LoadEngine( ctx.GenericEngineProxy );

                ServiceViewModel vm = evm.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IMyYodiiService" );

                Assert.That( vm.LiveItem, Is.EqualTo( s ), "Live item should be the loaded live info" );
                Assert.That( vm.AssemblyInfo, Is.EqualTo( s.ServiceInfo.AssemblyInfo ), "AssemblyInfo should be the live items's assembly info" );
            }
        }
        [Test]
        public void EngineViewModel_CorrectlyChanges_SelectedItem()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                ILiveServiceInfo s = ctx.Engine.LiveInfo.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IMyYodiiService" );
                Assert.That( s, Is.Not.Null );

                ILivePluginInfo p = ctx.Engine.LiveInfo.FindPlugin( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.PluginWithDisplayAttribute" );
                Assert.That( p, Is.Not.Null );

                EngineViewModel evm = new EngineViewModel();
                evm.LoadEngine( ctx.GenericEngineProxy );

                ServiceViewModel svm = evm.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IMyYodiiService" );
                PluginViewModel pvm = evm.FindPlugin( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.PluginWithDisplayAttribute" );

                // Test SelectedItem

                Assert.That( evm.SelectedItem, Is.Null, "SelectedItem starts null" );
                Assert.That( evm.SelectedPlugin, Is.Null, "SelectedPlugin starts null" );
                Assert.That( evm.SelectedService, Is.Null, "SelectedService starts null" );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.False );

                evm.SelectedItem = pvm;
                Assert.That( evm.SelectedItem, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedPlugin, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.True );

                evm.SelectedItem = svm;
                Assert.That( evm.SelectedItem, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedService, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.True );
                Assert.That( pvm.IsSelected, Is.False );

                evm.SelectedItem = null;
                Assert.That( evm.SelectedItem, Is.Null );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.False );

                pvm.IsSelected = true;
                Assert.That( evm.SelectedItem, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedPlugin, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.True );

                pvm.IsSelected = false;
                Assert.That( evm.SelectedItem, Is.Null );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.False );

                pvm.IsSelected = true;
                svm.IsSelected = true;
                pvm.IsSelected = false;
                Assert.That( evm.SelectedItem, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedService, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.True );
                Assert.That( pvm.IsSelected, Is.False );

                svm.IsSelected = false;
                Assert.That( evm.SelectedItem, Is.Null );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.False );

                svm.IsSelected = true;
                pvm.IsSelected = true;
                svm.IsSelected = false;
                Assert.That( evm.SelectedItem, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedPlugin, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.True );

                // Test SelectedPlugin and SelectedService
                // Plugin is currently selected
                evm.SelectedService = null; // No change (service wasn't selected anyway)
                Assert.That( evm.SelectedItem, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedPlugin, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.True );

                evm.SelectedPlugin = null;
                Assert.That( evm.SelectedItem, Is.Null );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.False );

                evm.SelectedPlugin = pvm;
                Assert.That( evm.SelectedItem, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedPlugin, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.True );

                evm.SelectedService = svm;
                Assert.That( evm.SelectedItem, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedService, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.True );
                Assert.That( pvm.IsSelected, Is.False );

                evm.SelectedPlugin = pvm;
                Assert.That( evm.SelectedItem, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedPlugin, Is.EqualTo( pvm ) );
                Assert.That( evm.SelectedService, Is.Null );
                Assert.That( svm.IsSelected, Is.False );
                Assert.That( pvm.IsSelected, Is.True );

                evm.SelectedService = svm;
                Assert.That( evm.SelectedItem, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedService, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.True );
                Assert.That( pvm.IsSelected, Is.False );

                evm.SelectedPlugin = null; // No change (plugin wasn't selected)
                Assert.That( evm.SelectedItem, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedService, Is.EqualTo( svm ) );
                Assert.That( evm.SelectedPlugin, Is.Null );
                Assert.That( svm.IsSelected, Is.True );
                Assert.That( pvm.IsSelected, Is.False );


            }
        }
    }
}
