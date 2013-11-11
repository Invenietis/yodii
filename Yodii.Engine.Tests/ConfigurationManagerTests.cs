using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
            Assert.That( layer.LayerName == "TestConfig" );

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
            Assert.That( result, Is.True );
            result = layer.Items.Add( pluginId, ConfigurationStatus.Runnable );
            Assert.That( result, Is.True );
            result = layer.Items.Add( pluginId, ConfigurationStatus.Running );
            Assert.That( result, Is.True );

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
            Assert.That( result, Is.True );
            result = layer.Items.Add( pluginId, ConfigurationStatus.Runnable );
            Assert.That( result, Is.True );
            result = layer.Items.Add( pluginId, ConfigurationStatus.Disable );
            Assert.That( result, Is.True );

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
            Assert.That( result, Is.True, "Layer override precedence: Optional -> Runnable is a valid operation." );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Runnable );

            result = layer.Items.Add( pluginId, ConfigurationStatus.Running );
            Assert.That( result, Is.True, "Layer override precedence: Runnable -> Running is a valid operation." );
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

        [Test]
        public void ManagerTests()
        {
            ConfigurationLayer layer = new ConfigurationLayer( "system" );
            layer.Items.Add( "schmurtz1", ConfigurationStatus.Running );
            layer.Items.Add( "schmurtz2", ConfigurationStatus.Running );
            layer.Items.Add( "schmurtz3", ConfigurationStatus.Running );
            layer.Items.Add( "schmurtz4", ConfigurationStatus.Running );
            layer.Items.Add( "schmurtz5", ConfigurationStatus.Running );
            layer.Items.Add( "schmurtz6", ConfigurationStatus.Running );

            ConfigurationManager manager = new ConfigurationManager();
            manager.ConfigurationChanging += ( s, e ) => { if( e.FinalConfiguration.GetStatus( "schmurtz4" ) == ConfigurationStatus.Running )  e.Cancel( "schmurtz!" ); };
            ConfigurationResult result = manager.Layers.Add( layer );
            Assert.That( result == true );
            Assert.That( manager.FinalConfiguration.Items[0].ServiceOrPluginId, Is.EqualTo( "schmurtz1" ) );
            Assert.That( manager.FinalConfiguration.Items[1].ServiceOrPluginId, Is.EqualTo( "schmurtz2" ) );
            Assert.That( manager.FinalConfiguration.Items[2].ServiceOrPluginId, Is.EqualTo( "schmurtz3" ) );

            ConfigurationLayer conflictLayer = new ConfigurationLayer( "schmurtzConflict" );
            conflictLayer.Items.Add( "schmurtz1", ConfigurationStatus.Disable );

            result = manager.Layers.Add( conflictLayer );
            Assert.That( result == false );
            Assert.That( result.FailureCauses.Count, Is.EqualTo( 2 ) );
            Assert.That( result.FailureCauses[0], Is.StringContaining( "Running" ) );
            Assert.That( result.FailureCauses[0], Is.StringContaining( "Disable" ) );
            Assert.That( result.FailureCauses[0], Is.StringContaining( "schmurtz1" ) );

            ConfigurationLayer layer2 = new ConfigurationLayer();
            layer2.Items.Add( "schmurtz4", ConfigurationStatus.Disable );
            layer2.Items.Add( "schmurtz1", ConfigurationStatus.Running );

            result = manager.Layers.Add( layer2 );
            Assert.That( result == true );
            Assert.That( manager.FinalConfiguration.Items[3].ServiceOrPluginId, Is.EqualTo( "schmurtz4" ) );
            Assert.That( manager.Layers[0].LayerName, Is.EqualTo( "" ) );

            result = manager.Layers.Remove( layer2 );
            Assert.That( result == true );
            Assert.Throws<IndexOutOfRangeException>( () => { FinalConfigurationItem item = manager.FinalConfiguration.Items[3]; } );
            Assert.That( manager.FinalConfiguration.Items[0].ServiceOrPluginId, Is.EqualTo( "schmurtz1" ) );
            Assert.That( manager.Layers[0].LayerName, Is.EqualTo( "system" ) );

            result = manager.Layers.Add( layer );
            Assert.That( result == true );

            manager.ConfigurationChanging += ( s, e ) => e.Cancel( "" );
            Assert.That( manager.Layers.Add( layer2 ) == false );
            Assert.That( manager.Layers[0].Items[0].SetStatus( ConfigurationStatus.Optional ) == false );
            Assert.That( manager.Layers[0].Items.Add( "schmurtz42", ConfigurationStatus.Optional ) == false );
            Assert.That( manager.Layers[0].Items.Remove( "schmurtz1" ) == false );
            Assert.That( manager.Layers.Remove( layer2 ) == false );



            // XML serialization test

            XmlWriterSettings ws = new XmlWriterSettings();
            ws.NewLineHandling = NewLineHandling.None;

            XmlReaderSettings rs = new XmlReaderSettings();

            ConfigurationManager deserializedManager;

            using( MemoryStream ms = new MemoryStream() )
            {
                using( XmlWriter xw = XmlWriter.Create( ms, ws ) )
                {
                    ConfigurationManagerXmlSerializer.SerializeConfigurationManager( manager, xw );
                }

                ms.Seek( 0, System.IO.SeekOrigin.Begin );

                // Debug string
                //using( StreamReader sr = new StreamReader( ms ) )
                //{
                //    string s = sr.ReadToEnd();
                //}

                using( XmlReader r = XmlReader.Create( ms, rs ) )
                {
                    deserializedManager = ConfigurationManagerXmlSerializer.DeserializeConfigurationManager( r );
                }
            }

            AssertManagerEquivalence( manager, deserializedManager );

        }

        private void AssertManagerEquivalence( ConfigurationManager a, ConfigurationManager b )
        {
            if( a == null && b == null ) return;

            Assert.That( a != null && b != null );

            Assert.That( a.Layers.Count == b.Layers.Count );

            for( int i = 0; i < a.Layers.Count; i++ )
            {
                // Consider equivalent if they're in the exact same order?
                var layerA = a.Layers[i];
                var layerB = b.Layers[i];

                foreach( var item in layerA.Items )
                {
                    Assert.That( layerB.Items.Any( x => x.ServiceOrPluginId == item.ServiceOrPluginId && x.Status == item.Status ) );
                }
            }

            if( a.FinalConfiguration != null )
            {
                Assert.That( b.FinalConfiguration != null );
                Assert.That( a.FinalConfiguration.Items.Count == b.FinalConfiguration.Items.Count );
                foreach( var item in a.FinalConfiguration.Items )
                {
                    Assert.That( b.FinalConfiguration.Items.Any( x => x.ServiceOrPluginId == item.ServiceOrPluginId && x.Status == item.Status ) );
                }
            }
        }


    }
}
