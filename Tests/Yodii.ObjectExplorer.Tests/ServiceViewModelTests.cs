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
    }
}
