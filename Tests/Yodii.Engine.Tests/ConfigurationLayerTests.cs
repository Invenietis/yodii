#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\ConfigurationLayerTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
        public void items_manipulations()
        {
            var engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            
            int countCollectionChangedEvent = 0;
            int countPropertyChangedEvent = 0;
            IConfigurationLayer layer = engine.Configuration.Layers.Create( "FirstOne" );
            layer.Items.CollectionChanged += ( s, e ) => countCollectionChangedEvent++;

            // Initialization tests
            Assert.That( layer.Items.Count, Is.EqualTo( 0 ) );
            Assert.Throws<IndexOutOfRangeException>( () => { var lambdaItem = layer.Items[42]; } );
            Assert.That( layer.Items["unk"], Is.Null );
            Assert.That( layer.Items.Contains( "unk" ), Is.False );

            // Actions without items
            layer.Items.Remove( "unk" ).CheckSuccess();
            Assert.That( countCollectionChangedEvent, Is.EqualTo( 0 ) );

            // Exception in add fonction
            Assert.Throws<ArgumentException>( () => layer.Set( null, ConfigurationStatus.Optional ) );
            Assert.Throws<ArgumentException>( () => layer.Set( "", ConfigurationStatus.Optional ) );

            // Add function tests
            layer.Set( "schmurtz", ConfigurationStatus.Optional, "Hop!" ).CheckSuccess();
            Assert.That( countCollectionChangedEvent, Is.EqualTo( 1 ) );
            Assert.That( layer.Items.Count, Is.EqualTo( 1 ) );
            Assert.That( layer.Items["schmurtz"], Is.Not.Null );
            Assert.DoesNotThrow( () => { var lambdaItem = layer.Items[0]; } );
            Assert.That( layer.Items["schmurtz"].Layer, Is.EqualTo( layer ) );
            Assert.That( layer.Items["schmurtz"].Layer.Items["schmurtz"], Is.Not.Null );
            Assert.That( layer.Items["schmurtz"].ServiceOrPluginFullName, Is.EqualTo( "schmurtz" ) );
            Assert.That( layer.Items["schmurtz"].Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( layer.Items["schmurtz"].Description, Is.EqualTo( "Hop!" ) );

            // Basic tests with item reference
            var item = layer.Items["schmurtz"];
            item.PropertyChanged += ( s, e ) => countPropertyChangedEvent++;
            item.Description = "Hoooop!";
            Assert.That( item.Description, Is.EqualTo( "Hoooop!" ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 1 ) );
            Assert.DoesNotThrow( () => item.Description = "Hop!Hop!" );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 2 ) );
            Assert.That( item.Description, Is.EqualTo( "Hop!Hop!" ) );
            Assert.DoesNotThrow( () => item.Description = null );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 3 ) );
            Assert.That( item.Description, Is.EqualTo( "" ) );

            // Set tests
            item.Set( ConfigurationStatus.Disabled ).CheckSuccess();
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Disabled ) );
            item.Set( ConfigurationStatus.Optional ).CheckSuccess();
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            item.Set( ConfigurationStatus.Runnable ).CheckSuccess();
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Runnable ) );
            item.Set( ConfigurationStatus.Running ).CheckSuccess();
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Running ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 7 ) );

            item.Set( StartDependencyImpact.FullStart ).CheckSuccess();
            Assert.That( item.Impact, Is.EqualTo( StartDependencyImpact.FullStart ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 8 ) );

            item.Set( ConfigurationStatus.Runnable,  StartDependencyImpact.IsStartRunnableOnly ).CheckSuccess();
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Runnable ) );
            Assert.That( item.Impact, Is.EqualTo( StartDependencyImpact.IsStartRunnableOnly ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 10 ) );

            item.Set( ConfigurationStatus.Optional, "Hello..." ).CheckSuccess();
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( item.Description, Is.EqualTo( "Hello..." ) );
            Assert.That( countPropertyChangedEvent, Is.EqualTo( 12 ) );

            // OnRemoved tests
            layer.Remove( "schmurtz" ).CheckSuccess();
            Assert.That( layer.Items["schmurtz"], Is.Null );
            Assert.Throws<IndexOutOfRangeException>( () => { var lambdaItem = layer.Items[0]; } );

            // Tests with item reference when item is removed
            Assert.That( item.Layer, Is.EqualTo( null ) );
            Assert.That( item.ServiceOrPluginFullName, Is.EqualTo( "schmurtz" ) );
            Assert.That( item.Status, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( item.Description, Is.Null );
            Assert.Throws<InvalidOperationException>( () => item.Set( ConfigurationStatus.Optional ) );
            Assert.Throws<InvalidOperationException>( () => item.Set( StartDependencyImpact.FullStart ) );
            Assert.Throws<InvalidOperationException>( () => item.Description = "Not settable when item is detached!" );

            // Tests with multiple add
            layer.Set( "schmurtz2", ConfigurationStatus.Optional, "desc2" ).CheckSuccess();
            layer.Set( "schmurtz1", ConfigurationStatus.Disabled, "desc1" ).CheckSuccess();
            Assert.That( layer.Items.Count, Is.EqualTo( 2 ) );

            // Sort tests
            var schmurtz1 = layer.Items["schmurtz1"];
            var schmurtz2 = layer.Items["schmurtz2"];
            Assert.That( schmurtz1, Is.EqualTo( layer.Items[0] ) );
            Assert.That( schmurtz2, Is.EqualTo( layer.Items[1] ) );
            Assert.That( schmurtz1.Description, Is.EqualTo( "desc1" ) );
            Assert.That( schmurtz2.Description, Is.EqualTo( "desc2" ) );

            layer.Set( "schmurtz0", ConfigurationStatus.Running ).CheckSuccess();
            var schmurtz0 = layer.Items["schmurtz0"];
            Assert.That( schmurtz0, Is.EqualTo( layer.Items[0] ) );
            Assert.That( schmurtz1, Is.EqualTo( layer.Items[1] ) );
            Assert.That( schmurtz2, Is.EqualTo( layer.Items[2] ) );    
        }

        [Test]
        public void clearing_layers()
        {
            var engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );

            var def = engine.Configuration.Layers.Default;
            var layer1 = engine.Configuration.Layers.Create( "Layer1" );
            var layer2 = engine.Configuration.Layers.Create( "Layer2" );
            layer1.Set( "p", ConfigurationStatus.Disabled ).CheckSuccess();
            layer2.Set( "s", ConfigurationStatus.Runnable ).CheckSuccess();
            def.Set( "o", StartDependencyImpact.IsTryStartOptionalOnly ).CheckSuccess();

            Assert.That( engine.Configuration.FinalConfiguration.Items.Count, Is.EqualTo( 3 ) );
            engine.Configuration.Layers.Clear();
            Assert.That( engine.Configuration.FinalConfiguration.Items.Count, Is.EqualTo( 0 ) );
            Assert.That( engine.Configuration.Layers.Default, Is.SameAs( def ) );
        }
    }
}
