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
            Assert.That( layer.Items.Count == 0 );
            Assert.That( layer.ConfigurationName == "TestConfig" );

            string pluginIdentifier;
            bool result;

            // Add a random plugin GUID
            pluginIdentifier = Guid.NewGuid().ToString();
            result = layer.Items.Add( pluginIdentifier, ConfigurationStatus.Disable );
            Assert.That( result, Is.True );
            Assert.That( layer.Items.Count == 1 );
            Assert.That( layer.Items[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );

            // Add another random plugin GUID
            pluginIdentifier = Guid.NewGuid().ToString();
            result = layer.Items.Add( pluginIdentifier, ConfigurationStatus.Runnable );
            Assert.That( result, Is.True );
            Assert.That( layer.Items.Count == 2 );
            Assert.That( layer.Items[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );

            // Remove last plugin GUID
            result = layer.Items.Remove( pluginIdentifier );
            Assert.That( result, Is.True );
            Assert.That( layer.Items.Count == 1 );

            // Add some service
            pluginIdentifier = "Yodii.ManagerService";
            result = layer.Items.Add( pluginIdentifier, ConfigurationStatus.Runnable );
            Assert.That( result, Is.True );
            Assert.That( layer.Items.Count == 2 );
            Assert.That( layer.Items[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );
        }

        [Test]
        public void LayerAddPrecedenceTest()
        {
            ConfigurationLayer layer = new ConfigurationLayer( "TestConfig" );

            bool result;

            // Precedence test for Disabled
            string pluginId = Guid.NewGuid().ToString();

            result = layer.Items.Add( pluginId, ConfigurationStatus.Disable );
            Assert.That( result, Is.True );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Disable );

            result = layer.Items.Add( pluginId, ConfigurationStatus.Disable );
            Assert.That( result, Is.True, "Adding the same plugin twice, in the same state, is a valid operation." );
            Assert.That( layer.Items.Count == 1, "Adding the same plugin twice, in the same state, does not actually add it and increment the count." );

            result = layer.Items.Add( pluginId, ConfigurationStatus.Optional );
            Assert.That( result, Is.False, "Layer override precedence: changing Disabled status is not valid." );
            result = layer.Items.Add( pluginId, ConfigurationStatus.Runnable );
            Assert.That( result, Is.False, "Layer override precedence: changing Disabled status is not valid." );
            result = layer.Items.Add( pluginId, ConfigurationStatus.Running );
            Assert.That( result, Is.False, "Layer override precedence: changing Disabled status is not valid." );

            result = layer.Items.Add( pluginId, ConfigurationStatus.Disable );
            Assert.That( result, Is.True );

            result = layer.Items.Remove( pluginId );
            Assert.That( result, Is.True, "Plugin can always be removed if it exists and layer isn't bound to a parent." );

            // Precedence test for Running
            pluginId = Guid.NewGuid().ToString();

            result = layer.Items.Add( pluginId, ConfigurationStatus.Running );
            Assert.That( result, Is.True );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Running );

            result = layer.Items.Add( pluginId, ConfigurationStatus.Running );
            Assert.That( result, Is.True, "Adding the same plugin twice, in the same state, is a valid operation." );
            Assert.That( layer.Items.Count == 1, "Adding the same plugin twice, in the same state, does not actually add it and increment the count." );

            result = layer.Items.Add( pluginId, ConfigurationStatus.Optional );
            Assert.That( result, Is.False, "Layer override precedence: changing Running status is not valid." );
            result = layer.Items.Add( pluginId, ConfigurationStatus.Runnable );
            Assert.That( result, Is.False, "Layer override precedence: changing Running status is not valid." );
            result = layer.Items.Add( pluginId, ConfigurationStatus.Disable );
            Assert.That( result, Is.False, "Layer override precedence: changing Running status is not valid." );

            result = layer.Items.Remove( pluginId );
            Assert.That( result, Is.True, "Plugin can always be removed if it exists and layer isn't bound to a parent." );

            // Precedence test for Optional -> Runnable -> Running
            pluginId = Guid.NewGuid().ToString();

            result = layer.Items.Add( pluginId, ConfigurationStatus.Optional );
            Assert.That( result, Is.True );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Optional );

            result = layer.Items.Add( pluginId, ConfigurationStatus.Optional );
            Assert.That( result, Is.True, "Adding the same plugin twice, in the same state, is a valid operation." );
            Assert.That( layer.Items.Count == 1, "Adding the same plugin twice, in the same state, does not actually add it and increment the count." );

            result = layer.Items.Add( pluginId, ConfigurationStatus.Runnable );
            Assert.That( result, Is.False, "Layer override precedence: Optional -> Runnable is a valid operation." );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Runnable );
            
            result = layer.Items.Add( pluginId, ConfigurationStatus.Running );
            Assert.That( result, Is.False, "Layer override precedence: Runnable -> Running is a valid operation." );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Running );

            Assert.That( layer.Items.Count == 1, "Adding the same plugin over and over does not actually increment the count." );

            result = layer.Items.Remove( pluginId );
            Assert.That( result, Is.True, "Plugin can always be removed if it exists and layer isn't bound to a parent." );
        }

        [Test]
        public void ManagerCreationTests()
        {
            ConfigurationManager cm = new ConfigurationManager();
            int managerChangingCount = 0;
            int managerChangedCount = 0;

            Assert.That( cm.FinalConfiguration == null, "Initial FinalConfiguration is null." );

            cm.ConfigurationChanging += delegate( object sender, ConfigurationChangingEventArgs e )
            {
                Assert.That( e.IsCanceled == false, "Configuration manager does not cancel by default." );
                Assert.That( e.FinalConfiguration != null, "Proposed FinalConfiguration exists." );

                managerChangingCount++;
            };

            cm.ConfigurationChanged += delegate( object sender, ConfigurationChangedEventArgs e )
            {
                Assert.That( e.FinalConfiguration != null, "FinalConfiguration exists." );

                managerChangedCount++;
            };

            bool result;
            ConfigurationLayer layer = new ConfigurationLayer( "BaseConfig" );

            layer.Items.Add( "Yodii.ManagerService", ConfigurationStatus.Runnable );
            layer.Items.Add( Guid.NewGuid().ToString(), ConfigurationStatus.Disable );

            result = cm.Layers.Add( layer ); // Fires Changing => Changed once.
            Assert.That( result, Is.True );

            Assert.That( managerChangingCount == 1 );
            Assert.That( managerChangedCount == 1 );

            Assert.That( cm.FinalConfiguration != null, "Non-cancelled FinalConfiguration exists." );
        }
    }
}
