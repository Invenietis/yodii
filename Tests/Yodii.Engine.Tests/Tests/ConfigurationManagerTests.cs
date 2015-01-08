#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Tests\ConfigurationManagerTests.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;
using Yodii.Model;
using System.Diagnostics;

namespace Yodii.Engine.Tests
{

    // ToDo test de reentrance | tests de fake interface
    [TestFixture]
    public class ConfigurationManagerTests
    {
        [Test]
        public void StartDependencyImpact_ClearTryBits_keep_others_unchanged()
        {
            Assert.That( StartDependencyImpact.IsTryStartOptionalOnly.ClearAllTryBits(), Is.EqualTo( StartDependencyImpact.Unknown ) );
            Assert.That( StartDependencyImpact.TryStartRecommended.ClearAllTryBits(), Is.EqualTo( StartDependencyImpact.Unknown ) );
            Assert.That( StartDependencyImpact.TryFullStart.ClearAllTryBits(), Is.EqualTo( StartDependencyImpact.Unknown ) );
            
            Assert.That( (StartDependencyImpact.IsTryStartOptionalOnly|StartDependencyImpact.Minimal).ClearAllTryBits(), Is.EqualTo( StartDependencyImpact.Minimal ) );
            Assert.That( (StartDependencyImpact.TryStartRecommended | StartDependencyImpact.StartRecommended | StartDependencyImpact.Minimal).ClearAllTryBits(), Is.EqualTo( StartDependencyImpact.StartRecommended | StartDependencyImpact.Minimal ) );
            Assert.That( (StartDependencyImpact.TryFullStart | StartDependencyImpact.FullStart).ClearAllTryBits(), Is.EqualTo( StartDependencyImpact.FullStart ) );
        }

        [Test]
        public void LayerCreationTest()
        {
            YodiiEngine e = new YodiiEngine( new BuggyYodiiEngineHostMock() );

            IConfigurationLayer layer = e.Configuration.Layers.Create( "TestConfig" );
            Assert.That( layer.Items.Count == 0 );
            Assert.That( layer.LayerName == "TestConfig" );

            string pluginIdentifier;
            IYodiiEngineResult result;

            // Add a random plugin GUID
            pluginIdentifier = Guid.NewGuid().ToString();
            result = layer.Items.Set( pluginIdentifier, ConfigurationStatus.Disabled );
            Assert.That( result.Success, Is.True );
            Assert.That( layer.Items.Count == 1 );
            Assert.That( layer.Items[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );

            // Add another random plugin GUID
            pluginIdentifier = Guid.NewGuid().ToString();
            result = layer.Items.Set( pluginIdentifier, ConfigurationStatus.Runnable );
            Assert.That( result.Success, Is.True );
            Assert.That( layer.Items.Count == 2 );
            Assert.That( layer.Items[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );

            // Remove last plugin GUID
            result = layer.Items.Remove( pluginIdentifier );
            Assert.That( result.Success, Is.True );
            Assert.That( layer.Items.Count == 1 );

            // Add some service
            pluginIdentifier = "Yodii.ManagerService";
            result = layer.Items.Set( pluginIdentifier, ConfigurationStatus.Runnable );
            Assert.That( result.Success, Is.True );
            Assert.That( layer.Items.Count == 2 );
            Assert.That( layer.Items[pluginIdentifier], Is.InstanceOf<ConfigurationItem>() );

        }

        [Test]
        public void LayerAddPrecedenceTest()
        {
            var engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            IConfigurationLayer layer = engine.Configuration.Layers.Create( "TestConfig" );

            IYodiiEngineResult result;

            // Precedence test for Disabled
            string pluginId = Guid.NewGuid().ToString();

            result = layer.Items.Set( pluginId, ConfigurationStatus.Disabled );
            Assert.That( result.Success, Is.True );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Disabled );

            result = layer.Items.Set( pluginId, ConfigurationStatus.Disabled );
            Assert.That( result.Success, Is.True, "Adding the same plugin twice, in the same state, is a valid operation." );
            Assert.That( layer.Items.Count == 1, "Adding the same plugin twice, in the same state, does not actually add it and increment the count." );

            result = layer.Items.Set( pluginId, ConfigurationStatus.Optional );
            Assert.That( result.Success, Is.True );
            result = layer.Items.Set( pluginId, ConfigurationStatus.Runnable );
            Assert.That( result.Success, Is.True );
            result = layer.Items.Set( pluginId, ConfigurationStatus.Running );
            Assert.That( result.Success, Is.True );

            result = layer.Items.Set( pluginId, ConfigurationStatus.Disabled );
            Assert.That( result.Success, Is.True );

            result = layer.Items.Remove( pluginId );
            Assert.That( result.Success, Is.True, "Plugin can always be removed if it exists and layer isn't bound to a parent." );

            // Precedence test for Running
            pluginId = Guid.NewGuid().ToString();

            result = layer.Items.Set( pluginId, ConfigurationStatus.Running );
            Assert.That( result.Success, Is.True );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Running );

            result = layer.Items.Set( pluginId, ConfigurationStatus.Running );
            Assert.That( result.Success, Is.True, "Adding the same plugin twice, in the same state, is a valid operation." );
            Assert.That( layer.Items.Count == 1, "Adding the same plugin twice, in the same state, does not actually add it and increment the count." );

            result = layer.Items.Set( pluginId, ConfigurationStatus.Optional );
            Assert.That( result.Success, Is.True );
            result = layer.Items.Set( pluginId, ConfigurationStatus.Runnable );
            Assert.That( result.Success, Is.True );
            result = layer.Items.Set( pluginId, ConfigurationStatus.Disabled );
            Assert.That( result.Success, Is.True );

            result = layer.Items.Remove( pluginId );
            Assert.That( result.Success, Is.True, "Plugin can always be removed if it exists and layer isn't bound to a parent." );

            // Precedence test for Optional -> Runnable -> Running
            pluginId = Guid.NewGuid().ToString();

            result = layer.Items.Set( pluginId, ConfigurationStatus.Optional );
            Assert.That( result.Success, Is.True );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Optional );

            result = layer.Items.Set( pluginId, ConfigurationStatus.Optional );
            Assert.That( result.Success, Is.True, "Adding the same plugin twice, in the same state, is a valid operation." );
            Assert.That( layer.Items.Count == 1, "Adding the same plugin twice, in the same state, does not actually add it and increment the count." );

            result = layer.Items.Set( pluginId, ConfigurationStatus.Runnable );
            Assert.That( result.Success, Is.True, "Layer override precedence: Optional -> Runnable is a valid operation." );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Runnable );

            result = layer.Items.Set( pluginId, ConfigurationStatus.Running );
            Assert.That( result.Success, Is.True, "Layer override precedence: Runnable -> Running is a valid operation." );
            Assert.That( layer.Items[pluginId].Status == ConfigurationStatus.Running );

            Assert.That( layer.Items.Count == 1, "Adding the same plugin over and over does not actually increment the count." );

            result = layer.Items.Remove( pluginId );
            Assert.That( result.Success, Is.True, "Plugin can always be removed if it exists and layer isn't bound to a parent." );
        }

        [Test]
        public void ManagerCreationTests()
        {
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            int managerChangingCount = 0;
            int managerChangedCount = 0;

            engine.Configuration.ConfigurationChanging += delegate( object sender, ConfigurationChangingEventArgs e )
            {
                Assert.That( e.IsCanceled == false, "Configuration manager does not cancel by default." );
                Assert.That( e.FinalConfiguration != null, "Proposed FinalConfiguration exists." );

                managerChangingCount++;
            };

            engine.Configuration.ConfigurationChanged += delegate( object sender, ConfigurationChangedEventArgs e )
            {
                Assert.That( e.FinalConfiguration != null, "FinalConfiguration exists." );

                managerChangedCount++;
            };

            IYodiiEngineResult result;
            IConfigurationLayer layer = engine.Configuration.Layers.Create( "BaseConfig" );

            result = layer.Items.Set( "Yodii.ManagerService", ConfigurationStatus.Runnable );
            Assert.That( result.Success, Is.True );
            result = layer.Items.Set( Guid.NewGuid().ToString(), ConfigurationStatus.Disabled );
            Assert.That( result.Success, Is.True );

            Assert.That( managerChangingCount == 2 );
            Assert.That( managerChangedCount == 2 );

            Assert.That( engine.Configuration.FinalConfiguration != null, "Non-cancelled FinalConfiguration exists." );
        }

        [Test]
        public void RemovingLayerTests()
        {
            var e = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            var layer = e.Configuration.Layers.Create( "Layer n°1" );
            layer.Items.AddSuccess( "p1", ConfigurationStatus.Disabled, ConfigurationStatus.Disabled );
            layer.Items.AddSuccess( "p2", ConfigurationStatus.Optional, ConfigurationStatus.Optional );
            layer.Items.AddSuccess( "p3", ConfigurationStatus.Runnable, ConfigurationStatus.Runnable );
            layer.Items.AddSuccess( "p4", ConfigurationStatus.Running, ConfigurationStatus.Running );

            e.Configuration.CheckFinalConfigurationItemStatus( "p1=Disabled", "p2=Optional", "p3=Runnable", "p4=Running" );

            var layer2 = e.Configuration.Layers.Create( "Layer n°2" );
            layer2.Items.AddSuccess( "p1", ConfigurationStatus.Optional, ConfigurationStatus.Disabled );
            layer2.Items.AddSuccess( "p2", ConfigurationStatus.Runnable, ConfigurationStatus.Runnable );
            layer2.Items.AddSuccess( "p2", ConfigurationStatus.Running, ConfigurationStatus.Running );
            layer2.Items.AddSuccess( "p2", ConfigurationStatus.Runnable, ConfigurationStatus.Runnable );
            layer2.Items.AddSuccess( "p3", ConfigurationStatus.Running, ConfigurationStatus.Running );
            layer2.Items.AddSuccess( "p4", ConfigurationStatus.Runnable, ConfigurationStatus.Running );

            e.Configuration.CheckFinalConfigurationItemStatus( "p1=Disabled", "p2=Runnable", "p3=Running", "p4=Running" );

            e.Configuration.Layers.Remove( layer );

            e.Configuration.CheckFinalConfigurationItemStatus( "p1=Optional", "p2=Runnable", "p3=Running", "p4=Runnable" );

        }

        [Test]
        public void ManagerTests()
        {

            //YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            //IConfigurationLayer layer = engine.ConfigurationManager.Layers.Create( "system" );
            //Assert.That( layer.Items.Add( "schmurtz1", ConfigurationStatus.Running ).Success, Is.True );
            //Assert.That( layer.Items.Add( "schmurtz2", ConfigurationStatus.Running ).Success, Is.True );
            //Assert.That( layer.Items.Add( "schmurtz3", ConfigurationStatus.Running ).Success, Is.True );
            //Assert.That( layer.Items.Add( "schmurtz4", ConfigurationStatus.Running ).Success, Is.True );
            //Assert.That( layer.Items.Add( "schmurtz5", ConfigurationStatus.Running ).Success, Is.True );
            //Assert.That( layer.Items.Add( "schmurtz6", ConfigurationStatus.Running ).Success, Is.True );

            //engine.ConfigurationManager.ConfigurationChanging += ( s, e ) => { if( e.FinalConfiguration.GetStatus( "schmurtz4" ) == ConfigurationStatus.Running )  e.CancelForExternalReason( "schmurtz!" ); };
            //Assert.That( engine.ConfigurationManager.FinalConfiguration.Items[0].ServiceOrPluginId, Is.EqualTo( "schmurtz1" ) );
            //Assert.That( engine.ConfigurationManager.FinalConfiguration.Items[1].ServiceOrPluginId, Is.EqualTo( "schmurtz2" ) );
            //Assert.That( engine.ConfigurationManager.FinalConfiguration.Items[2].ServiceOrPluginId, Is.EqualTo( "schmurtz3" ) );

            //IConfigurationLayer conflictLayer = engine.ConfigurationManager.Layers.Create( "schmurtzConflict" );
            //conflictLayer.Items.Add( "schmurtz1", ConfigurationStatus.Disabled );

            //result = engine.ConfigurationManager.Layers.Add( conflictLayer );
            //Assert.That( result.Success, Is.False );
            //Assert.That( result.ConfigurationFailureResult.FailureReasons.Count, Is.EqualTo( 2 ) );
            //Assert.That( result.ConfigurationFailureResult.FailureReasons[0], Is.StringContaining( "Running" ) );
            //Assert.That( result.ConfigurationFailureResult.FailureReasons[0], Is.StringContaining( "Disable" ) );
            //Assert.That( result.ConfigurationFailureResult.FailureReasons[0], Is.StringContaining( "schmurtz1" ) );

            //ConfigurationLayer layer2 = new ConfigurationLayer();
            //layer2.Items.Add( "schmurtz4", ConfigurationStatus.Disabled );
            //layer2.Items.Add( "schmurtz1", ConfigurationStatus.Running );

            //result = engine.ConfigurationManager.Layers.Add( layer2 );
            //Assert.That( result.Success, Is.True);
            //Assert.That( engine.ConfigurationManager.FinalConfiguration.Items[3].ServiceOrPluginId, Is.EqualTo( "schmurtz4" ) );
            //Assert.That( engine.ConfigurationManager.Layers[0].LayerName, Is.EqualTo( "" ) );

            //result = engine.ConfigurationManager.Layers.Remove( layer2 );
            //Assert.That( result.Success, Is.True );
            //Assert.Throws<IndexOutOfRangeException>( () => { FinalConfigurationItem item = engine.ConfigurationManager.FinalConfiguration.Items[3]; } );
            //Assert.That( engine.ConfigurationManager.FinalConfiguration.Items[0].ServiceOrPluginId, Is.EqualTo( "schmurtz1" ) );
            //Assert.That( engine.ConfigurationManager.Layers[0].LayerName, Is.EqualTo( "system" ) );

            //result = engine.ConfigurationManager.Layers.Add( layer );
            //Assert.That( result.Success, Is.True );

            //engine.ConfigurationManager.ConfigurationChanging += ( s, e ) => e.CancelForExternalReason( "" );
            //Assert.That( engine.ConfigurationManager.Layers.Add( layer2 ).Success, Is.False);
            //Assert.That( engine.ConfigurationManager.Layers[0].Items[0].SetStatus( ConfigurationStatus.Optional ).Success, Is.False );
            //Assert.That( engine.ConfigurationManager.Layers[0].Items.Add( "schmurtz42", ConfigurationStatus.Optional ).Success, Is.False );
            //Assert.That( engine.ConfigurationManager.Layers[0].Items.Remove( "schmurtz1" ).Success, Is.False);
            //Assert.That( engine.ConfigurationManager.Layers.Remove( layer2 ).Success, Is.False);

        }
    }
}
