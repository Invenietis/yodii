using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NUnit.Framework;

namespace Yodii.Wpf.Tests
{
    [SetUpFixture]
    public class WpfTestHelper
    {
        public static Thread ApplicationThread { get; private set; }
        public static Application Application { get; private set; }

        [SetUp]
        public void SetUp()
        {
            if( Application.Current == null )
            {
                CreateApplicationInNewThread();
            }
        }

        [TearDown]
        public void TearDown()
        {
            //ShutdownApplication();
        }

        static void CreateApplicationInNewThread()
        {
            Assert.That( Application.Current, Is.Null );
            Assert.That( ApplicationThread, Is.Null );
            Assert.That( Application.Current, Is.Null );

            ManualResetEventSlim ev = new ManualResetEventSlim( false );

            ApplicationThread = new Thread( () =>
            {
                //Application a = Application;

                Application = new Application();
                Application.ShutdownMode = ShutdownMode.OnExplicitShutdown; // Don't close when a plugin window closes!
                Application.Startup += ( s, e ) => { ev.Set(); };

                Application.Run(); // Blocks forever
            } );
            ApplicationThread.IsBackground = true;
            ApplicationThread.SetApartmentState( ApartmentState.STA );
            ApplicationThread.Start();

            ev.Wait();
            Assert.That( Application.Current, Is.Not.Null );
        }

        static void ShutdownApplication()
        {
            if( ApplicationThread != null )
            {
                Application.Dispatcher.Invoke( new Action( () =>
                {
                    Application.Shutdown();
                } ) );

                ApplicationThread.Join( TimeSpan.FromMilliseconds( 100 ) );
                ApplicationThread = null;
            }
        }

    }

    public static class TestsWindowExtensions
    {
        public static void CloseAndWaitForClosed( this Window w )
        {
            ManualResetEventSlim ev = new ManualResetEventSlim();

            w.Closed += ( s, e ) => ev.Set();
            w.Close();

            ev.Wait( TimeSpan.FromSeconds( 1 ) );
        }
    }
}
