using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NUnit.Framework;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.Wpf.Tests
{
    [TestFixture]
    public class WpfTests
    {
        Thread _appThread;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // Run an empty WPF context if necessary.
            if( Application.Current == null )
            {
                CreateApplicationInNewThread();
            }
        }

        void CreateApplicationInNewThread()
        {
            Assert.That( Application.Current, Is.Null );

            ManualResetEventSlim ev = new ManualResetEventSlim( false );

            _appThread = new Thread( () =>
            {
                Application a = new Application();
                a.ShutdownMode = ShutdownMode.OnExplicitShutdown; // Don't close when a plugin window closes!
                a.Startup += ( s, e ) => { ev.Set(); };
                a.Run(); // Blocks forever
            } );
            _appThread.SetApartmentState( ApartmentState.STA );
            _appThread.Start();

            ev.Wait();
            Assert.That( Application.Current, Is.Not.Null );
        }

        void StopApplicationAndThread()
        {
            if( _appThread != null )
            {
                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Application.Current.Shutdown();
                } ) );

                _appThread.Join( TimeSpan.FromMilliseconds( 100 ) );
                _appThread = null;
            }
        }



        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            StopApplicationAndThread();
        }

        [Test]
        public void WindowPluginBase_CanBeStarted_AndStopped()
        {
            using( var ctx = new YodiiRuntimeTestContext().StartPlugin<TestWindowPlugin>() )
            {
                ILivePluginInfo pluginLive = ctx.FindLivePlugin<TestWindowPlugin>();

                Assert.That( pluginLive, Is.Not.Null );
                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Running ) );

                // Check for a WPF context
                Assert.That( Application.Current, Is.Not.Null );
            }
        }

        [Test]
        public void WindowPluginBase_StaysRunning_WhenWindowIsClosed_Without_StopPluginWhenWindowCloses()
        {
            // Set config
            TestWindowPlugin.StopPluginWhenWindowClosesConfig = false;

            using( var ctx = new YodiiRuntimeTestContext().StartPlugin<TestWindowPlugin>() )
            {
                ManualResetEventSlim ev = new ManualResetEventSlim();
                ILivePluginInfo pluginLive = ctx.FindLivePlugin<TestWindowPlugin>();

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Window w = Application.Current.Windows.Cast<Window>().Single();
                    w.Closed += ( s, e ) => ev.Set();
                    w.Close();
                } ) );

                ev.Wait();

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    CollectionAssert.IsEmpty( Application.Current.Windows, "Window has been closed, no other Windows remain" );
                } ) );

                // Window closed, but still running
                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Running ) );
            }
        }

        [Test]
        public void WindowPluginBase_Stops_WhenWindowIsClosed_With_StopPluginWhenWindowCloses()
        {
            // Set config
            TestWindowPlugin.StopPluginWhenWindowClosesConfig = true;

            using( var ctx = new YodiiRuntimeTestContext().StartPlugin<TestWindowPlugin>() )
            {
                ManualResetEventSlim ev = new ManualResetEventSlim();
                ILivePluginInfo pluginLive = ctx.FindLivePlugin<TestWindowPlugin>();

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Window w = Application.Current.Windows.Cast<Window>().Single();
                    w.Closed += ( s, e ) => ev.Set();
                    w.Close();
                } ) );

                ev.Wait();

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    CollectionAssert.IsEmpty( Application.Current.Windows, "Window has been closed, no other Windows remain" );
                } ) );

                // Window closed, but still running
                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Stopped ) );
            }
        }

        [Test]
        public void WindowPluginBase_DoesNotAllowClosing_WhenWindowIsClosed_With_PluginRequired_and_StopPluginWhenWindowClosesConfig()
        {
            TestWindowPlugin.StopPluginWhenWindowClosesConfig = true;

            using( var ctx = new YodiiRuntimeTestContext() )
            {
                var result = ctx.Engine.Configuration.Layers.Default.Set( typeof( TestWindowPlugin ).FullName, ConfigurationStatus.Running );
                Assert.That( result.Success, Is.True, "TestWindowPlugin should be able to be set as Running" );

                ILivePluginInfo pluginLive = ctx.FindLivePlugin<TestWindowPlugin>();
                Assert.That( pluginLive, Is.Not.Null, "Plugin should have been set up when config changed" );
                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.RunningLocked ), "Plugin should have been started when config changed" );

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Window w = Application.Current.Windows.Cast<Window>().Single();
                    w.Close(); // Calls closing...
                } ) );

                // Wait until Close propagates
                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Window w = Application.Current.Windows.Cast<Window>().Single();

                    Assert.That( w.IsLoaded, Is.True, "Window should not have been closed since plugin is required" );
                } ) );
            }
        }

        [Test]
        public void WindowPluginBase_AllowsClosing_WhenWindowIsClosed_With_PluginRequired_Without_StopPluginWhenWindowClosesConfig()
        {
            TestWindowPlugin.StopPluginWhenWindowClosesConfig = false;

            using( var ctx = new YodiiRuntimeTestContext() )
            {
                var result = ctx.Engine.Configuration.Layers.Default.Set( typeof( TestWindowPlugin ).FullName, ConfigurationStatus.Running );
                Assert.That( result.Success, Is.True, "TestWindowPlugin should be able to be set as Running" );

                ILivePluginInfo pluginLive = ctx.FindLivePlugin<TestWindowPlugin>();
                Assert.That( pluginLive, Is.Not.Null, "Plugin should have been set up when config changed" );
                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.RunningLocked ), "Plugin should have been started when config changed" );

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Window w = Application.Current.Windows.Cast<Window>().Single();
                    w.Close(); // Calls closing...

                    CollectionAssert.IsEmpty( Application.Current.Windows, "Window has been closed, no other Windows remain" );
                } ) );
            }
        }
    }
}
