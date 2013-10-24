using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using Yodii.Model.CoreModel;

namespace Yodii.Lab.Tests
{
    [TestFixture(Category="Yodii.Lab")]
    public class MainWindowViewModelTests
    {
        [Test]
        public void AddServicesPluginsTest()
        {
            MainWindowViewModel vm = new MainWindowViewModel();

            Assert.That( vm.IsLive, Is.False );

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


            IPluginInfo pluginWithoutService = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.Without.Service" );

            IPluginInfo pluginA1 = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.A1", serviceA );

            IPluginInfo pluginA2 = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.A2", serviceA );

            IPluginInfo pluginAx1 = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.Ax1", serviceAx );

            IPluginInfo pluginB1 = vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.B1", serviceB );

            vm.SetPluginDependency( pluginA2, serviceB, RunningRequirement.Running );
        }
    }
}
