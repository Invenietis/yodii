using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Yodii.Model;

namespace Yodii.Wpf
{
    /// <summary>
    /// Base for Yodii plugins using WPF windows.
    /// Provides support for automatic window closing when plugin starts/ends, etc.
    /// Requires an existing Application in the current AppDomain (Application.Current).
    /// </summary>
    public abstract class WindowPluginBase : YodiiPluginBase
    {
        private readonly IYodiiEngineBase _engine;

        private bool _closingWindow;
        private bool _tryingToClose;
        private bool _running;
        private Window _window;

        private bool _stopPluginWhenWindowCloses;

        /// <summary>
        /// Whether to stop the plugin when the main window closes.
        /// If false, the plugin will stay started when the window closes,
        /// and you will have to call Window.Show() by yourself when required.
        /// </summary>
        protected bool StopPluginWhenWindowCloses
        {
            get
            {
                return _stopPluginWhenWindowCloses;
            }
            set
            {
                if( _running ) { throw new InvalidOperationException( "StopPluginWhenWindowCloses cannot be set while plugin is running." ); }
                _stopPluginWhenWindowCloses = value;
            }
        }

        /// <summary>
        /// Creates the Main Window associated with the plugin.
        /// </summary>
        /// <remarks>
        /// This method will likely be called on the Application's UI thread instead of the one used to instanciate and start the Plugin.
        /// </remarks>
        /// <returns>Main window of the plugin.</returns>
        protected abstract Window CreateWindow();

        public WindowPluginBase( IYodiiEngineBase engine )
        {
            if( engine == null ) { throw new ArgumentNullException( "engine" ); }

            _engine = engine;
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            base.PluginPreStart( c );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            base.PluginPreStop( c );
        }

        protected override void PluginStart( IStartContext c )
        {
            if( Application.Current == null )
            {
                throw new InvalidOperationException( "Cannot start this plugin when no WPF Application exists." );
            }

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                ShowWindow();
            } ) );

            _running = true;
            base.PluginStart( c );
        }

        private void ShowWindow()
        {
            // Called on the UI thread.
            _window = CreateWindow();

            _window.Show();
        }

        protected override void PluginStop( IStopContext c )
        {
            CloseWindowOnPluginStop();

            _running = false;
            base.PluginStop( c );
        }

        protected ILivePluginInfo GetLivePluginInfo()
        {
            return _engine.LiveInfo.FindPlugin( this.GetType().FullName );
        }

        private void _window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            // _closingWindow is set in PluginStop() => CloseWindow().

            if( !_closingWindow )
            {
                // This didn't come from PluginStop()! robably somebody trying the Close button/Alt-F4/Ctrl-W/etc.

                if( StopPluginWhenWindowCloses )
                {
                    // Intercept and cancel the Close for now. We'll ask the engine to Close us instead (and set _closingWindow).
                    e.Cancel = true;

                    ILivePluginInfo livePluginInfo = GetLivePluginInfo();
                    if( livePluginInfo != null )
                    {
                        if( livePluginInfo.Capability.CanStop )
                        {
                            // This will prevent PluginStop/CloseWindow() from calling Close(), since we're, erm, already Closing.
                            _tryingToClose = true;

                            // Ask the engine to stop ourselves.
                            var r = _engine.StopItem( livePluginInfo );
                            // This called PluginStop()/CloseWindow().
                            // The Plugin probably DID Stop, but the Window will still be there.
                            // If the stop worked, revoke the Cancel so that the window closes when exiting this method.

                            if( r.Success ) e.Cancel = false;

                            _tryingToClose = false;
                        }
                        else
                        {
                            // Can't stop? Check your configuration.
                            string m = String.Format( "This plugin is required by configuration, and cannot stop itself.\nTo stop it, change the configuration of {0}.", this.GetType().FullName );
                            MessageBox.Show( _window, m, "Cannot stop",
                                MessageBoxButton.OK, MessageBoxImage.Stop,
                                MessageBoxResult.OK, MessageBoxOptions.None );
                        }
                    }
                }
            }
            else
            {
                // Engine asked us to PluginStop(), so off we go. Bye!
                _closingWindow = false;
            }
        }

        private void CloseWindowOnPluginStop()
        {
            Debug.Assert( Application.Current != null, "A WPF context (Application.Current) must exist for this plugin." );

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                // _tryingToClose is set when window is trying to Close despite PluginStop() not being called: it's already Closing.
                // Calling Close() during a Closing does bad things, as you can expect.
                if( _window != null && !_tryingToClose )
                {
                    // Allow the Closing to pass.
                    _closingWindow = true;

                    _window.Close();
                }
            } ) );
        }
    }
}