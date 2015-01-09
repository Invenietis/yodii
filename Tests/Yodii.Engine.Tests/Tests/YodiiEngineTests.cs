#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Tests\YodiiEngineTests.cs) is part of CiviKey. 
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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;
using Yodii.Engine.Tests.Mocks;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    class YodiiEngineTests
    {
        [Test]
        public void checking_starting_and_stopping_parameters_and_state()
        {
            Assert.That( Assert.Throws<ArgumentNullException>( () => new YodiiEngine( null ) ).ParamName, Is.EqualTo( "host" ) );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            Assert.That( engine.Host, Is.Not.Null );
            Assert.That( engine.IsRunning, Is.False );
            Assert.That( engine.Configuration, Is.Not.Null );
            Assert.That( engine.LiveInfo, Is.Not.Null );
            Assert.That( engine.LiveInfo.Plugins.Count, Is.EqualTo( 0 ) );
            Assert.That( engine.LiveInfo.Services.Count, Is.EqualTo( 0 ) );

            Assert.That( Assert.Throws<ArgumentNullException>( () => engine.SetDiscoveredInfo( null ) ).ParamName, Is.EqualTo( "info" ) );
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            engine.SetDiscoveredInfo( info ).CheckSuccess();
            Assert.That( engine.DiscoveredInfo == info );
            engine.SetDiscoveredInfo( info ).CheckSuccess();

            Assert.That( engine.LiveInfo.Plugins.Count, Is.EqualTo( 0 ) );
            Assert.That( engine.LiveInfo.Services.Count, Is.EqualTo( 0 ) );
            Assert.That( engine.IsRunning, Is.False );
            
            engine.StartEngine().CheckSuccess();
            Assert.That( engine.IsRunning );
            Assert.That( engine.LiveInfo.Plugins.Count, Is.EqualTo( 2 ) );
            Assert.That( engine.LiveInfo.Services.Count, Is.EqualTo( 1 ) );
            
            engine.StartPlugin( "PluginA-1" );
            Assert.Throws<ArgumentNullException>( () => engine.StartPlugin( null ) );
            Assert.Throws<ArgumentException>( () => engine.StartPlugin( "Unexisiting plugin name." ) );
            Assert.That( engine.IsRunning );
            Assert.Throws<InvalidOperationException>( () => engine.StartEngine() );
            engine.StopEngine();
            Assert.That( engine.IsRunning, Is.False );
            Assert.That( engine.LiveInfo.Plugins.Count, Is.EqualTo( 0 ) );
            Assert.That( engine.LiveInfo.Services.Count, Is.EqualTo( 0 ) );
            Assert.Throws<InvalidOperationException>( () => engine.StartPlugin( "PluginA-1" ) );
        }

        [Test]
        public void live_info_is_bound_to_discovered_infos()
        {
            IYodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();

            engine.SetDiscoveredInfo( info ).CheckSuccess();
            engine.StartEngine().CheckSuccess();
            
            PluginInfo pluginA1 = info.FindPlugin( "PluginA-1" );
            PluginInfo pluginA2 = info.FindPlugin( "PluginA-2" );
            ServiceInfo serviceA = info.FindService( "ServiceA" );

            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-1" ).PluginInfo, Is.SameAs( pluginA1 ) );
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-2" ).PluginInfo, Is.SameAs( pluginA2 ) );
            Assert.That( engine.LiveInfo.FindService( "ServiceA" ).ServiceInfo, Is.SameAs( serviceA ) );

            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-1" ).IsRunning, Is.False );
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-2" ).IsRunning, Is.False );
            Assert.That( engine.LiveInfo.FindService( "ServiceA" ).IsRunning, Is.False );

            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-1" ).RunningStatus, Is.EqualTo( RunningStatus.Stopped ) );
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-2" ).RunningStatus, Is.EqualTo( RunningStatus.Stopped ) );
            Assert.That( engine.LiveInfo.FindService( "ServiceA" ).RunningStatus, Is.EqualTo( RunningStatus.Stopped ) );

            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-1" ).Service, Is.SameAs( engine.LiveInfo.FindService( serviceA.ServiceFullName ) ) );
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-2" ).Service, Is.SameAs( engine.LiveInfo.FindService( serviceA.ServiceFullName ) ) );
            Assert.That( engine.LiveInfo.FindService( "ServiceA" ).Generalization, Is.Null );

            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-1" ).ConfigOriginalStatus, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-2" ).ConfigOriginalStatus, Is.EqualTo( ConfigurationStatus.Optional ) );
            Assert.That( engine.LiveInfo.FindService( "ServiceA" ).ConfigOriginalStatus, Is.EqualTo( ConfigurationStatus.Optional ) );

            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-1" ).WantedConfigSolvedStatus, Is.EqualTo( SolvedConfigurationStatus.Runnable ) );
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-2" ).WantedConfigSolvedStatus, Is.EqualTo( SolvedConfigurationStatus.Runnable ) );
            Assert.That( engine.LiveInfo.FindService( "ServiceA" ).WantedConfigSolvedStatus, Is.EqualTo( SolvedConfigurationStatus.Runnable ) );

            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-1" ).CurrentError, Is.Null );
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-2" ).CurrentError, Is.Null );
            Assert.That( engine.LiveInfo.FindService( "ServiceA" ).DisabledReason, Is.Null );


            DiscoveredInfo otherInfo = MockInfoFactory.CreateGraph003();
            PluginInfo otherPluginA1 = otherInfo.FindPlugin( "PluginA-1" );
            PluginInfo otherPluginA2 = otherInfo.FindPlugin( "PluginA-2" );
            ServiceInfo otherServiceA = otherInfo.FindService( "ServiceA" );

            engine.SetDiscoveredInfo( otherInfo ).CheckSuccess();
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-1" ).PluginInfo, Is.Not.SameAs( pluginA1 ).And.SameAs( otherPluginA1 ) );
            Assert.That( engine.LiveInfo.FindPlugin( "PluginA-2" ).PluginInfo, Is.Not.SameAs( pluginA2 ).And.SameAs( otherPluginA2 ) );
            Assert.That( engine.LiveInfo.FindService( "ServiceA" ).ServiceInfo, Is.Not.SameAs( serviceA ).And.SameAs( otherServiceA ) );

            engine.StopEngine();
        }

        [Test]
        public void post_actions_reentrancy_is_supported()
        {
            var host = new BuggyYodiiEngineHostMock();
            var engine = new YodiiEngine( host );
            var info = MockInfoFactory.CreateGraph003();
            engine.SetDiscoveredInfo( info );
            engine.StartEngine().CheckSuccess();

            var pluginA1 = engine.LiveInfo.FindPlugin( "PluginA-1" );
            var pluginA2 = engine.LiveInfo.FindPlugin( "PluginA-2" );
            var serviceA = engine.LiveInfo.FindService( "ServiceA" );
            
            int callCount = 0;
            host.PostActionToAdd = e => 
            {
                Assert.That( serviceA.IsRunning );
                ++callCount;
                if( callCount == 1 )
                {
                    Assert.That( pluginA1.IsRunning && !pluginA2.IsRunning );
                    e.StartPlugin( "PluginA-2" ).CheckSuccess();
                    Assert.That( pluginA2.IsRunning && !pluginA1.IsRunning );
                }
                else if( callCount == 2 )
                {
                    Assert.That( pluginA2.IsRunning && !pluginA1.IsRunning );
                    e.StartPlugin( "PluginA-1" ).CheckSuccess();
                    Assert.That( pluginA1.IsRunning && !pluginA2.IsRunning );
                }
                else if( callCount == 3 )
                {
                    Assert.That( pluginA1.IsRunning && !pluginA2.IsRunning );
                    e.StartPlugin( "PluginA-2" ).CheckSuccess();
                    Assert.That( pluginA2.IsRunning && !pluginA1.IsRunning );
                }
            };

            engine.StartPlugin( "PluginA-1" ).CheckSuccess();
            Assert.That( pluginA2.IsRunning && !pluginA1.IsRunning );
            Assert.That( callCount, Is.EqualTo( 4 ) );
        }
    }
}
