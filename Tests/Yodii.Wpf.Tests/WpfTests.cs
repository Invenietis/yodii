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
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if( _appThread != null )
            {
                Application.Current.Dispatcher.Invoke( new Action( () =>
                {
                    Application.Current.Shutdown();
                    _appThread.Join( TimeSpan.FromSeconds( 1 ) );
                } ) );
            }
        }

        [Test]
        public void WpfBaseTest()
        {
            YodiiHost host = new YodiiHost();
            YodiiEngine engine = new YodiiEngine( host );

            engine.Configuration.SetDiscoveredInfo( TestHelper.GetDiscoveredInfoInCallingAssembly() );

            IYodiiEngineResult result = engine.StartEngine();
            Assert.That( result.Success );

            string pluginName = typeof( TestWindowPlugin ).FullName;

            var pluginLive = engine.LiveInfo.FindPlugin( pluginName );
            var pluginProxy = host.FindLoadedPlugin( pluginName );

            Assert.That( pluginLive, Is.Not.Null );
            Assert.That( pluginLive.Capability.CanStart );
            Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Stopped ) );

            // Check window creation

            IYodiiEngineResult r = engine.StartPlugin( pluginName );

            Assert.That( r.Success, Is.True );
            Assert.That( Application.Current, Is.Not.Null );
            Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Running ) );

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                Assert.That( Application.Current.Windows.Count, Is.EqualTo( 1 ) );

                Window w = Application.Current.Windows.Cast<Window>().Single();

                w.Close();
            } ) );

            Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Running ) );

            r = engine.StopPlugin( pluginName );
            Assert.That( r.Success, Is.True );

            // Do it again!

            r = engine.StartPlugin( pluginName );

            Assert.That( r.Success, Is.True );
            Assert.That( Application.Current, Is.Not.Null );
            Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Running ) );

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                Assert.That( Application.Current.Windows.Count, Is.EqualTo( 1 ) );

                Window w = Application.Current.Windows.Cast<Window>().Single();

                w.Close();
            } ) );

            Assert.That( pluginLive.RunningStatus, Is.EqualTo( RunningStatus.Running ) );

            r = engine.StopPlugin( pluginName );
            Assert.That( r.Success, Is.True );
        }
    }
}
