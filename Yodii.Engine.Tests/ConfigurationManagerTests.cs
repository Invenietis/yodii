using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    public class ConfigurationManagerTests
    {
        [Test]
        public void LayerCreationTest()
        {
            ConfigurationLayer layer = new ConfigurationLayer( "TestConfig" );
            Assert.That( layer.Count == 0 );
            Assert.That( layer.ConfigurationName == "TestConfig" );

            string pluginIdentifier;
            bool result;

            // Add a random plugin GUID
            pluginIdentifier = Guid.NewGuid().ToString();
            result = layer.AddConfigurationItem( pluginIdentifier, ConfigurationStatus.Disable );
            Assert.That( result, Is.True );
            Assert.That( layer.Count == 1 );
            Assert.That( layer[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );

            // Add another random plugin GUID
            pluginIdentifier = Guid.NewGuid().ToString();
            layer.AddConfigurationItem( pluginIdentifier, ConfigurationStatus.Runnable );
            Assert.That( result, Is.True );
            Assert.That( layer.Count == 2 );
            Assert.That( layer[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );

            // Remove last plugin GUID
            result = layer.RemoveConfigurationItem( pluginIdentifier );
            Assert.That( result, Is.True );
            Assert.That( layer.Count == 1 );

            // Add some service
            pluginIdentifier = "Yodii.ManagerService";
            layer.AddConfigurationItem( pluginIdentifier, ConfigurationStatus.Runnable );
            Assert.That( result, Is.True );
            Assert.That( layer.Count == 2 );
            Assert.That( layer[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );
        }
    }
}
