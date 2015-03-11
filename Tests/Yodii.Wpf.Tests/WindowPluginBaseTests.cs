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
using Yodii.Wpf.Win32;

namespace Yodii.Wpf.Tests
{
    [TestFixture]
    public class WindowPluginBaseTests
    {
        [Test]
        public void WindowPluginBase_ClosesWindow_WhenStopping()
        {
            using( var ctx = new YodiiRuntimeTestContext().StartPlugin<TestWindowPlugin>() )
            {
                ILivePluginInfo pluginLive = ctx.FindLivePlugin<TestWindowPlugin>();

                Assert.That( pluginLive, Is.Not.Null );
                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Running ) );

                Assert.That( Application.Current, Is.Not.Null );

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    CollectionAssert.IsNotEmpty( Application.Current.Windows, "Window has been created and windows exist in this Application" );
                } ) );

                IYodiiEngineResult result = ctx.Engine.StopPlugin( typeof( TestWindowPlugin ).FullName );
                Assert.That( result.Success, Is.True );

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    CollectionAssert.IsEmpty( Application.Current.Windows, "Window has been removed and no windows remain in this Application" );
                } ) );
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
            TestWindowPlugin.AutomaticallyDisableCloseButtonConfig = true;

            using( var ctx = new YodiiRuntimeTestContext() )
            {
                IYodiiEngineResult result = ctx.Engine.Configuration.Layers.Default.Set( typeof( TestWindowPlugin ).FullName, ConfigurationStatus.Running );
                Assert.That( result.Success, Is.True, "TestWindowPlugin should be able to be set as Running" );

                ILivePluginInfo pluginLive = ctx.FindLivePlugin<TestWindowPlugin>();
                Assert.That( pluginLive, Is.Not.Null, "Plugin should have been set up when config changed" );
                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.RunningLocked ), "Plugin should have been started when config changed" );

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Window w = Application.Current.Windows.Cast<Window>().Single();
                    Assert.That( w.IsCloseButtonDisabled(), Is.True );
                    w.Close(); // Calls closing...
                } ) );

                // Wait until Close propagates
                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Window w = Application.Current.Windows.Cast<Window>().Single();

                    Assert.That( w.IsLoaded, Is.True, "Window should not have been closed since plugin is required" );
                } ) );

                // Change config to set it as not-required
                result = ctx.Engine.Configuration.Layers.Default.Set( typeof( TestWindowPlugin ).FullName, ConfigurationStatus.Runnable );
                Assert.That( result.Success, Is.True, "TestWindowPlugin should be able to be set as Runnable" );

                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Stopped ), "Plugin went from RunningLocked to Stopped after changing config from Running to Runnable" );
                result = ctx.Engine.StartPlugin( typeof( TestWindowPlugin ).FullName );
                Assert.That( result.Success, Is.True, "TestWindowPlugin could be started after being set to Runnable" );

                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Window w = Application.Current.Windows.Cast<Window>().Single();
                    Assert.That( w.IsCloseButtonDisabled(), Is.False );
                } ) );
            }
        }

        [Test]
        public void WindowPluginBase_AllowsClosing_WhenWindowIsClosed_With_PluginRequired_Without_StopPluginWhenWindowClosesConfig()
        {
            TestWindowPlugin.StopPluginWhenWindowClosesConfig = false;
            TestWindowPlugin.AutomaticallyDisableCloseButtonConfig = true;

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
                    Assert.That( w.IsCloseButtonDisabled(), Is.False );
                    w.Close(); // Calls closing...

                    CollectionAssert.IsEmpty( Application.Current.Windows, "Window has been closed, no other Windows remain" );
                } ) );

                Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.RunningLocked ), "Plugin is still alive" );
            }
        }
    }
}
