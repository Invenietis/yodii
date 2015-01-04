using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Yodii.Model;
using Yodii.Engine;
using Yodii.Discoverer;
using CK.Core;
using System.IO;

namespace Yodii.Host.Tests
{

    [TestFixture]
    public class SpecializationTests
    {
        [Test]
        public void simple_root_plugin_start_stop()
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( "simple_root_plugin_start_stop" ) )
            {
                var c = new ServiceSpecializationContext();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginRoot." );
                c.Engine.Start( c.PluginRoot.Live ).CheckSuccess();
                c.PluginRoot.CheckState( PluginStatus.Started );
                c.ServiceRoot.CheckState( ServiceStatus.Started );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.Host.GetTrackedEntries();

                TestHelper.ConsoleMonitor.Trace().Send( "Stopping ServiceRoot." );
                c.Engine.Stop( c.ServiceRoot.Live ).CheckSuccess();
                c.PluginRoot.CheckState( PluginStatus.Stopped );
                c.ServiceRoot.CheckState( ServiceStatus.Stopped );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.Stopping, ServiceStatus.Stopped );
                c.Host.GetTrackedEntries();
            }
        }

        [Test]
        public void simple_root_plugin_swap()
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( "simple_root_plugin_swap" ) )
            {
                var c = new ServiceSpecializationContext();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginRoot." );
                c.Engine.Start( c.PluginRoot.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginRoot );
                c.ServiceRoot.CheckState( ServiceStatus.Started );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.Stopped );
                c.ServiceSubBSub.CheckState( ServiceStatus.Stopped );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.Host.GetTrackedEntries();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting AltPluginRoot." );
                c.Engine.Start( c.AltPluginRoot.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.AltPluginRoot );
                c.ServiceRoot.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.Stopped );
                c.ServiceSubBSub.CheckState( ServiceStatus.Stopped );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.Host.GetTrackedEntries();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginRoot." );
                c.Engine.Start( c.PluginRoot.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginRoot );
                c.ServiceRoot.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.Stopped );
                c.ServiceSubBSub.CheckState( ServiceStatus.Stopped );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.Host.GetTrackedEntries();
            }
        }

        [Test]
        public void swap_to_more_specialized_implementations()
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( "swap_to_more_specialized_implementations" ) )
            {
                var c = new ServiceSpecializationContext();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginRoot." );
                c.Engine.Start( c.PluginRoot.Live ).CheckSuccess();
                c.ServiceRoot.CheckState( ServiceStatus.Started );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.Stopped );
                c.ServiceSubBSub.CheckState( ServiceStatus.Stopped );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.ServiceSubA.CheckEventsAndClear();
                c.ServiceSubB.CheckEventsAndClear();
                c.ServiceSubBSub.CheckEventsAndClear();
                c.Host.GetTrackedEntries();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginSubB." );
                c.Engine.Start( c.PluginSubB.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginSubB );
                c.ServiceRoot.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.Started );
                c.ServiceSubBSub.CheckState( ServiceStatus.Stopped );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckEventsAndClear();
                c.ServiceSubB.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.ServiceSubBSub.CheckEventsAndClear();
                c.Host.GetTrackedEntries();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginSubBSub." );
                c.Engine.Start( c.PluginSubBSub.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginSubBSub );
                c.ServiceRoot.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubBSub.CheckState( ServiceStatus.Started );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckEventsAndClear();
                c.ServiceSubB.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.ServiceSubBSub.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.Host.GetTrackedEntries();
            }
        }

        [Test]
        public void swap_to_generalization_implementations()
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( "swap_to_generalization_implementations" ) )
            {
                var c = new ServiceSpecializationContext();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginSubBSub." );
                c.Engine.Start( c.PluginSubBSub.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginSubBSub );
                c.ServiceRoot.CheckState( ServiceStatus.Started );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.Started );
                c.ServiceSubBSub.CheckState( ServiceStatus.Started );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.ServiceSubA.CheckEventsAndClear();
                c.ServiceSubB.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.ServiceSubBSub.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.Host.GetTrackedEntries();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginSubB." );
                c.Engine.Start( c.PluginSubB.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginSubB );
                c.ServiceRoot.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubBSub.CheckState( ServiceStatus.Stopped );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckEventsAndClear();
                c.ServiceSubB.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.ServiceSubBSub.CheckEventsAndClear( ServiceStatus.Stopping, ServiceStatus.Stopped );
                c.Host.GetTrackedEntries();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginRoot." );
                c.Engine.Start( c.PluginRoot.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginRoot );
                c.ServiceRoot.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.Stopped );
                c.ServiceSubBSub.CheckState( ServiceStatus.Stopped );

                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckEventsAndClear();
                c.ServiceSubB.CheckEventsAndClear( ServiceStatus.Stopping, ServiceStatus.Stopped );
                c.ServiceSubBSub.CheckEventsAndClear();
                c.Host.GetTrackedEntries();

            }
        }

        [Test]
        public void swap_to_other_branch()
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( "swap_to_other_branch" ) )
            {
                var c = new ServiceSpecializationContext();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginSubBSub." );
                c.Engine.Start( c.PluginSubBSub.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginSubBSub );
                c.ServiceRoot.CheckState( ServiceStatus.Started );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.Started );
                c.ServiceSubBSub.CheckState( ServiceStatus.Started );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.ServiceSubA.CheckEventsAndClear();
                c.ServiceSubB.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.ServiceSubBSub.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.Host.GetTrackedEntries();

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginSubA." );
                c.Engine.Start( c.PluginSubA.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginSubA );
                c.ServiceRoot.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckState( ServiceStatus.Started );
                c.ServiceSubB.CheckState( ServiceStatus.Stopped );
                c.ServiceSubBSub.CheckState( ServiceStatus.Stopped );
                c.ServiceRoot.CheckEventsAndClear( ServiceStatus.StoppingSwapped, ServiceStatus.StartingSwapped, ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckEventsAndClear( ServiceStatus.Starting, ServiceStatus.Started );
                c.ServiceSubB.CheckEventsAndClear( ServiceStatus.Stopping, ServiceStatus.Stopped );
                c.ServiceSubBSub.CheckEventsAndClear( ServiceStatus.Stopping, ServiceStatus.Stopped );
                c.Host.GetTrackedEntries();
            }
        }

        [Test]
        public void disabling_plugin()
        {
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( "disabling_plugin" ) )
            {
                var c = new ServiceSpecializationContext();
                Assert.That( c.AllPlugins.All( p => p.CheckState( PluginStatus.Null ) ), "Since they never ran, they are Null." );

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginSubBSub." );
                c.Engine.Start( c.PluginSubBSub.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.PluginSubBSub );
                
                TestHelper.ConsoleMonitor.Trace().Send( "Disabling PluginSubBSub by configuration." );
                c.Engine.Configuration.Layers.Default.Items.Set( c.PluginSubBSub.Live.PluginInfo.PluginFullName, ConfigurationStatus.Disabled ).CheckSuccess();
                Assert.That( c.AllPlugins.All( p => (p == c.PluginSubBSub && p.CheckState( PluginStatus.Stopped )) || p.CheckState( PluginStatus.Null ) ), "All are null except PluginSubBSub that has been started." );
                Assert.That( c.AllServices.All( s => s.CheckState( ServiceStatus.Stopped ) ) );

                TestHelper.ConsoleMonitor.Trace().Send( "Starting PluginSubBSubAlternate (IDisposable plugin)." );
                c.Engine.Start( c.AltPluginSubBSub.Live ).CheckSuccess();
                c.CheckOnePluginStarted( c.AltPluginSubBSub );
                c.ServiceRoot.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubA.CheckState( ServiceStatus.Stopped );
                c.ServiceSubB.CheckState( ServiceStatus.StartedSwapped );
                c.ServiceSubBSub.CheckState( ServiceStatus.StartedSwapped );

                TestHelper.ConsoleMonitor.Trace().Send( "Disabling AltPluginSubBSub by configuration." );
                c.Engine.Configuration.Layers.Default.Items.Set( c.AltPluginSubBSub.Live.PluginInfo.PluginFullName, ConfigurationStatus.Disabled ).CheckSuccess();

                Assert.That( c.AllPlugins.All( p => (p == c.PluginSubBSub || p == c.PluginSubBSub && p.CheckState( PluginStatus.Stopped )) || p.CheckState( PluginStatus.Null ) ), 
                                "All are null except PluginSubBSub that has been started previously and AltPluginSubBSub is null: since it is IDisposable it has been released." );
                Assert.That( c.AllServices.All( s => s.CheckState( ServiceStatus.Stopped ) ) );

            }
        }
    }
}
