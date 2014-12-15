using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Engine.Tests.Mocks;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    public class LiveInfoNotificationsTests
    {
        [Test]
        public void ConfigChanged()
        {
            /**
             *  +--------+
             *  |ServiceA+ ------+
             *  |        |       |
             *  +---+----+       |
             *      |            |
             *      |            |
             *      |            |
             *      |        +---+---*-+
             *  +---+-----+  |PluginA-2|
             *  |PluginA-1|  |         |
             *  |         |  +---------+
             *  +---------+
             */
            DiscoveredInfo info = MockInfoFactory.CreateGraph003();
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.SetDiscoveredInfo( info );
            engine.Start();
            ILiveServiceInfo sA = engine.LiveInfo.FindService( "ServiceA" );
            ILivePluginInfo p1 = engine.LiveInfo.FindPlugin( "PluginA-1" );
            ILivePluginInfo p2 = engine.LiveInfo.FindPlugin( "PluginA-2" );

            Assert.That( sA != null && p1 != null && p2 != null );

            Assert.That( p1.Capability.CanStart && p2.Capability.CanStart && sA.Capability.CanStart, Is.True );
            Assert.That( p1.Capability.CanStartWithFullStart && p2.Capability.CanStartWithFullStart && sA.Capability.CanStartWithFullStart, Is.True );
            Assert.That( p1.Capability.CanStartWithStartRecommended && p2.Capability.CanStartWithStartRecommended && sA.Capability.CanStartWithStartRecommended, Is.True );
            Assert.That( p1.Capability.CanStartWithStopOptionalAndRunnable && p2.Capability.CanStartWithStopOptionalAndRunnable && sA.Capability.CanStartWithStopOptionalAndRunnable, Is.True );
            Assert.That( p1.Capability.CanStartWithFullStop && p2.Capability.CanStartWithFullStop && sA.Capability.CanStartWithFullStop, Is.True );

            HashSet<string> propertyChanged = new HashSet<string>();
            p1.PropertyChanged += ( s, e ) => 
                                    { 
                                        Assert.That( s, Is.SameAs( p1 ) ); 
                                        Assert.That( propertyChanged.Add( "p1."+e.PropertyName ) );
                                    };
            p1.Capability.PropertyChanged += ( s, e ) => 
                                                { 
                                                    Assert.That( s, Is.SameAs( p1.Capability ) ); 
                                                    Assert.That( propertyChanged.Add( "p1.Capablity."+e.PropertyName ) );
                                                };
            p2.PropertyChanged += ( s, e ) => 
                                    { 
                                        Assert.That( s, Is.SameAs( p2 ) ); 
                                        Assert.That( propertyChanged.Add( "p2."+e.PropertyName ) );
                                    };
            p2.Capability.PropertyChanged += ( s, e ) => 
                                                { 
                                                    Assert.That( s, Is.SameAs( p2.Capability ) ); 
                                                    Assert.That( propertyChanged.Add( "p2.Capablity."+e.PropertyName ) );
                                                };
            sA.PropertyChanged += ( s, e ) => 
                                    { 
                                        Assert.That( s, Is.SameAs( sA ) ); 
                                        Assert.That( propertyChanged.Add( "sA."+e.PropertyName ) );
                                    };
            sA.Capability.PropertyChanged += ( s, e ) => 
                                                { 
                                                    Assert.That( s, Is.SameAs( sA.Capability ) ); 
                                                    Assert.That( propertyChanged.Add( "sA.Capablity."+e.PropertyName ) );
                                                };

            IConfigurationLayer config = engine.Configuration.Layers.Create( "Default" );
            config.Items.Add( p1.FullName, ConfigurationStatus.Disabled );
            
            Assert.That( p1.Capability.CanStart && p1.Capability.CanStartWithFullStart 
                        && p1.Capability.CanStartWithStartRecommended && p1.Capability.CanStartWithStopOptionalAndRunnable 
                        && p1.Capability.CanStartWithFullStop, Is.False );

            Assert.That( p2.Capability.CanStart && sA.Capability.CanStart, Is.True );
            Assert.That( p2.Capability.CanStartWithFullStart && sA.Capability.CanStartWithFullStart, Is.True );
            Assert.That( p2.Capability.CanStartWithStartRecommended && sA.Capability.CanStartWithStartRecommended, Is.True );
            Assert.That( p2.Capability.CanStartWithStopOptionalAndRunnable && sA.Capability.CanStartWithStopOptionalAndRunnable, Is.True );
            Assert.That( p2.Capability.CanStartWithFullStop && sA.Capability.CanStartWithFullStop, Is.True );


            CollectionAssert.AreEquivalent( new string[]{
                "p1.Capablity.CanStart", 
                "p1.Capablity.CanStartWithFullStart", 
                "p1.Capablity.CanStartWithStartRecommended", 
                "p1.Capablity.CanStartWithStopOptionalAndRunnable", 
                "p1.Capablity.CanStartWithFullStop", 
                "p1.DisabledReason", 
                "p1.RunningStatus", 
                "p1.ConfigOriginalStatus", 
                "p1.WantedConfigSolvedStatus",
                "p1.FinalConfigSolvedStatus" }, propertyChanged );
            propertyChanged.Clear();

            config.Items.Add( p1.FullName, ConfigurationStatus.Optional );

            CollectionAssert.AreEquivalent( new string[]{
                "p1.Capablity.CanStart", 
                "p1.Capablity.CanStartWithFullStart", 
                "p1.Capablity.CanStartWithStartRecommended", 
                "p1.Capablity.CanStartWithStopOptionalAndRunnable", 
                "p1.Capablity.CanStartWithFullStop", 
                "p1.DisabledReason", 
                "p1.RunningStatus", 
                "p1.ConfigOriginalStatus", 
                "p1.WantedConfigSolvedStatus",
                "p1.FinalConfigSolvedStatus" }, propertyChanged );
        }

    }
}
