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
    public class ConfigurationLayerTests
    {
        [Test]
        public void ConfigurationLayerWithoutManagerTests()
        {
            int countCollectionChangedEvent = 0;
            int countPropertyChangedEvent = 0;
            ConfigurationLayer layer = new ConfigurationLayer();
            layer.Items.CollectionChanged += ( s, e ) => countCollectionChangedEvent++;

            //initialization tests
            Assert.That( layer.Items.Count, Is.EqualTo( 0 ) );
            Assert.Throws<IndexOutOfRangeException>( () => { ConfigurationItem lambdaItem = layer.Items[42]; } );
            Assert.That( layer.Items["schmurtz"], Is.Null );
            Assert.That( layer.Items.Contains( "schmurtz" ), Is.False );

            //actions without items
            ConfigurationResult result = layer.Items.Remove( "schmurtz" );
            Assert.That( result == false ); //use a implicit cast
            Assert.That( result.IsSuccessful, Is.False );
            Assert.That( result.FailureCauses.Count, Is.EqualTo( 1 ) );
            Assert.That( result.FailureCauses.Contains("Item not found"), Is.True );

            Assert.That( countCollectionChangedEvent, Is.EqualTo( 0 ) );

            //exception in add fonction
            Assert.Throws<ArgumentException>( () => layer.Items.Add( null, ConfigurationStatus.Optional ) );
            Assert.Throws<ArgumentException>( () => layer.Items.Add( "", ConfigurationStatus.Optional ) );

            //add function tests
            result = layer.Items.Add( "schmurtz", ConfigurationStatus.Optional );
            Assert.That( result == true );
            Assert.That( result.IsSuccessful, Is.True );
            Assert.That( result.FailureCauses, Is.Not.Null );
            Assert.That( result.FailureCauses.Count, Is.EqualTo( 0 ) );
            Assert.That( countCollectionChangedEvent, Is.EqualTo( 1 ) );
            Assert.That( layer.Items.Count, Is.EqualTo( 1 ) );
            Assert.That( layer.Items["schmurtz"], Is.Not.Null );
            Assert.DoesNotThrow( () => { ConfigurationItem lambdaItem = layer.Items[0]; } );
            Assert.That( layer.Items["schmurtz"].Layer, Is.EqualTo( layer ) );
            Assert.That( layer.Items["schmurtz"].Layer.Items["schmurtz"], Is.Not.Null );
            Assert.That( layer.Items["schmurtz"].ServiceOrPluginId, Is.EqualTo( "schmurtz" ) );
            Assert.That( layer.Items["schmurtz"].Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( layer.Items["schmurtz"].StatusReason, Is.EqualTo( "" ) );

            //basic tests with item reference
            ConfigurationItem item = layer.Items["schmurtz"];
            item.PropertyChanged += ( s, e ) => countPropertyChangedEvent++;
            item.StatusReason = null;
            Assert.That( item.StatusReason, Is.EqualTo( "" ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => item.StatusReason = "schmurtz" );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 2 ) );
            Assert.DoesNotThrow( () => item.StatusReason = "" );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 3 ) );

            //setstatus tests
            Assert.That( item.SetStatus( ConfigurationStatus.Disable ).IsSuccessful, Is.True );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Disable ) );
            Assert.That( item.SetStatus( ConfigurationStatus.Optional ).IsSuccessful, Is.True );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( item.SetStatus( ConfigurationStatus.Runnable ).IsSuccessful, Is.True );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Runnable ) );
            Assert.That( item.SetStatus( ConfigurationStatus.Running ).IsSuccessful, Is.True );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Running ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 7 ) );

            Assert.DoesNotThrow( () => item.SetStatus( ConfigurationStatus.Optional, null ) );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( item.StatusReason, Is.EqualTo( "" ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 9 ) );

            //add function tests when the item already exist
            result = layer.Items.Add( "schmurtz", ConfigurationStatus.Disable );
            Assert.That( result == true );
            Assert.That( layer.Items.Count, Is.EqualTo( 1 ) );
            Assert.That( layer.Items["schmurtz"].Status, Is.EqualTo( ConfigurationStatus.Disable ) );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Disable ) );
            result = layer.Items.Add( "schmurtz", ConfigurationStatus.Optional );
            Assert.That( result == true );
            Assert.That( layer.Items.Count, Is.EqualTo( 1 ) );
            Assert.That( layer.Items["schmurtz"].Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            result = layer.Items.Add( "schmurtz", ConfigurationStatus.Runnable );
            Assert.That( result == true );
            Assert.That( layer.Items.Count, Is.EqualTo( 1 ) );
            Assert.That( layer.Items["schmurtz"].Status, Is.EqualTo( ConfigurationStatus.Runnable ) );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Runnable ) );
            result = layer.Items.Add( "schmurtz", ConfigurationStatus.Running );
            Assert.That( result == true );
            Assert.That( layer.Items.Count, Is.EqualTo( 1 ) );
            Assert.That( layer.Items["schmurtz"].Status, Is.EqualTo( ConfigurationStatus.Running ) );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Running ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 13 ) );

            //OnRemoved tests
            layer.Items.Remove( "schmurtz" );
            Assert.That( result == true );
            Assert.That( result.IsSuccessful, Is.True);
            Assert.That( result.FailureCauses, Is.Not.Null );
            Assert.That( result.FailureCauses.Count, Is.EqualTo( 0 ) );
            Assert.That( countCollectionChangedEvent, Is.EqualTo( 2 ) );
            Assert.That( layer.Items["schmurtz"], Is.Null );
            Assert.Throws < IndexOutOfRangeException>( () => { ConfigurationItem lambdaItem = layer.Items[0]; } );

            //tests with item reference when item is removed
            Assert.That( item.Layer, Is.EqualTo( null ) );
            Assert.That( item.ServiceOrPluginId, Is.EqualTo( "schmurtz" ) );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( item.StatusReason, Is.EqualTo( null ) );

            //tests with multiple add
            result = layer.Items.Add( "schmurtz2", ConfigurationStatus.Optional, "schmurtz?" );
            Assert.That( result == true );
            result = layer.Items.Add( "schmurtz1", ConfigurationStatus.Disable, "schmurtz?" );
            Assert.That( result == true );
            Assert.That( layer.Items.Count, Is.EqualTo( 2 ) );

            //sort tests
            ConfigurationItem schmurtz1 = layer.Items["schmurtz1"];
            ConfigurationItem schmurtz2 = layer.Items["schmurtz2"];
            Assert.That( schmurtz1, Is.EqualTo( layer.Items[0] ) );
            Assert.That( schmurtz2, Is.EqualTo( layer.Items[1] ) );
            Assert.That( schmurtz1.StatusReason, Is.EqualTo( "schmurtz?" ) );
            Assert.That( schmurtz2.StatusReason, Is.EqualTo( "schmurtz?" ) );

            result = layer.Items.Add( "schmurtz0", ConfigurationStatus.Running );
            ConfigurationItem schmurtz0 = layer.Items["schmurtz0"];
            Assert.That( schmurtz0, Is.EqualTo( layer.Items[0] ) );
            Assert.That( schmurtz1, Is.EqualTo( layer.Items[1] ) );
            Assert.That( schmurtz2, Is.EqualTo( layer.Items[2] ) );
            
        }
    }
}
