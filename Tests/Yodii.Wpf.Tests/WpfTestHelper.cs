using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NUnit.Framework;

namespace Yodii.Wpf.Tests
{
    [SetUpFixture]
    public class WpfTestHelper
    {
        public static Thread ApplicationThread { get; private set; }
        public static Dispatcher Dispatcher { get; private set; }

        [SetUp]
        public void SetUp()
        {
            if( Dispatcher == null )
            {
                CreateDispatcherInNewThread();
            }
        }

        private void CreateDispatcherInNewThread()
        {
            Assert.That( Dispatcher, Is.Null );
            Assert.That( ApplicationThread, Is.Null );

            ManualResetEventSlim ev = new ManualResetEventSlim( false );

            ApplicationThread = new Thread( () =>
            {
                Dispatcher = Dispatcher.CurrentDispatcher;

                ev.Set();

                Dispatcher.Run(); // Blocks forever

            } );
            ApplicationThread.IsBackground = true;
            ApplicationThread.SetApartmentState( ApartmentState.STA );
            ApplicationThread.Start();

            ev.Wait();

            Assert.That( Dispatcher, Is.Not.Null );
        }

        [TearDown]
        public void TearDown()
        {
            ShutdownDispatcher();
        }

        static void ShutdownDispatcher()
        {
            if( ApplicationThread != null )
            {
                Dispatcher.Invoke( new Action( () =>
                {
                    Dispatcher.ExitAllFrames();
                    Dispatcher = null;
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
