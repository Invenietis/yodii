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

                ServiceViewModel vm = new ServiceViewModel();
                vm.LoadLiveService( s );
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

                ServiceViewModel vm = new ServiceViewModel();
                vm.LoadLiveService( s );
                Assert.Throws<InvalidOperationException>( () => vm.LoadLiveService( s ) );
            }
        }

        [Test]
        public void ServiceViewModel_CorrectlyLoads_DisplayAttributeData()
        {
            using( var ctx = new YodiiRuntimeTestContext( Assembly.GetExecutingAssembly() ) )
            {
                ILiveServiceInfo s = ctx.Engine.LiveInfo.FindService( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IServiceWithDisplayAttribute" );
                Assert.That( s, Is.Not.Null );

                ServiceViewModel vm = new ServiceViewModel();
                vm.LoadLiveService( s );

                Assert.That( vm.DisplayName, Is.EqualTo( "My service (with display attribute)" ), "DisplayName should be retrieved from Display attribute's Name property" );
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

                ServiceViewModel vm = new ServiceViewModel();
                vm.LoadLiveService( s );

                Assert.That( vm.DisplayName, Is.EqualTo( "IMyYodiiService" ), "DisplayName should be the interface name without namespace when Display attribute's Name property is not used" );
                Assert.That( vm.Description, Is.EqualTo( String.Empty ), "Description should be empty when Display's Description is unused" );
                Assert.That( vm.FullName, Is.EqualTo( "Yodii.ObjectExplorer.Tests.TestYodiiObjects.IMyYodiiService" ), "FullName is equal to the service's FullName" );
            }
        }
    }
}
