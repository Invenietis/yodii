using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Yodii.Model;
using Yodii.Wpf.Win32;

namespace Yodii.Wpf
{
    /// <summary>
    /// Base for Yodii plugins using WPF windows.
    /// Provides support for automatic window closing when plugin starts/ends, etc.
    /// Requires either an existing Application in the current AppDomain (Application.Current), or a running Dispatcher capable of dispatching messages on a STA thread.
    /// </summary>
    public abstract class WindowPluginBase : YodiiPluginBase
    {
        private readonly IYodiiEngineProxy _engine;
        private Dispatcher _appDispatcher;

        private bool _closingWindow;
        private bool _tryingToClose;
        private bool _running;
        private bool _closeButtonIsDisabled;
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
        /// Gets a value indicating whether to show an error <see cref="MessageBox"/> when trying to close the window
        /// when the plugin should stop when the window closes (StopPluginWhenWindowCloses == true),
        /// and the plugin cannot stop at this time.
        /// </summary>
        /// <value>
        /// <c>true</c> if a <see cref="MessageBox"/> should be shown when trying to close the window
        /// when this plugin is required by configuration; otherwise, <c>false</c>: The Closing event will be silently rejected.
        /// </value>
        protected bool ShowClosingFailedMessageBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable the Close button (Top right corner of window)
        /// when the plugin should stop when the window closes (StopPluginWhenWindowCloses == true),
        /// and the plugin cannot stop at this time.
        /// </summary>
        /// <value>
        /// <c>true</c> if automatically disable the close button when the plugin is required; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When enabled, will use the Win32 API to disable and enable dynamically the Close button.
        /// If you are using your own window, you will have to bind yourself and change your own Close button using the EngineProxy.
        /// </remarks>
        protected bool AutomaticallyDisableCloseButton { get; set; }

        /// <summary>
        /// Creates the Main Window associated with the plugin.
        /// </summary>
        /// <remarks>
        /// This method will likely be called on the Application's UI thread instead of the one used to instanciate and start the Plugin.
        /// </remarks>
        /// <returns>Main window of the plugin.</returns>
        protected abstract Window CreateWindow();

        /// <summary>
        /// Gets the engine proxy given in the constructor.
        /// </summary>
        /// <value>
        /// The engine proxy.
        /// </value>
        protected IYodiiEngineProxy EngineProxy { get { return _engine; } }

        /// <summary>
        /// Gets the Window created when the plugin starts.
        /// </summary>
        protected Window Window { get { return _window; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowPluginBase"/> class with an explicit app dispatcher.
        /// </summary>
        /// <param name="engine">The injected Yodii engine proxy.</param>
        /// <param name="appDispatcher">The application dispatcher used to send UI messages. If null, will use Application.Current's dispatcher when starting the plugin.</param>
        /// <exception cref="System.ArgumentNullException">engine</exception>
        public WindowPluginBase( IYodiiEngineProxy engine, Dispatcher appDispatcher )
        {
            if( engine == null ) { throw new ArgumentNullException( "engine" ); }

            _engine = engine;
            _appDispatcher = appDispatcher;

            // Defaults
            ShowClosingFailedMessageBox = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowPluginBase"/> class using Application.Current's dispatcher to send UI messages.
        /// </summary>
        /// <param name="engine">The injected Yodii engine proxy.</param>
        public WindowPluginBase( IYodiiEngineProxy engine ) : this(engine, null)
        {
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            base.PluginPreStart( c );

            if( Application.Current == null && _appDispatcher == null )
            {
                c.Cancel( "This plugin and its window cannot start without Application.Current, or a given Dispatcher." );
            }
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            base.PluginPreStop( c );
        }

        protected override void PluginStart( IStartContext c )
        {
            if( Application.Current == null && _appDispatcher == null )
            {
                throw new InvalidOperationException( "This plugin and its window cannot start without Application.Current, or a given Dispatcher." );
            }

            if( _appDispatcher == null )
            {
                Debug.Assert( Application.Current != null );
                _appDispatcher = Application.Current.Dispatcher;
            }

            _appDispatcher.Invoke( CreateAndShowWindow );

            if( StopPluginWhenWindowCloses && AutomaticallyDisableCloseButton )
            {
                DisableCloseButtonOnLocked();
            }

            _running = true;
            base.PluginStart( c );
        }

        void CreateAndShowWindow()
        {
            // Called on the UI thread.

            _window = CreateWindow();
            _window.Closing += _window_Closing;

            ShowWindow();
        }

        protected virtual void ShowWindow() {
            _window.Show();
        }

        void DisableCloseButtonOnLocked()
        {
            _engine.IsRunningLockedChanged += DisableCloseButtonOnLockedHandler;
            UpdateCloseButton();
        }

        void DisableCloseButtonOnLockedHandler( object sender, EventArgs a )
        {
            UpdateCloseButton();
        }

        void UpdateCloseButton()
        {
            UpdateCloseButton( _engine.IsRunningLocked );
        }

        void UpdateCloseButton( bool disableButton )
        {
            if( disableButton && !_closeButtonIsDisabled )
            {
                _window.Dispatcher.Invoke( _window.DisableCloseButton );
                _closeButtonIsDisabled = true;
            }
            else if( !disableButton && _closeButtonIsDisabled )
            {
                _window.Dispatcher.Invoke( _window.EnableCloseButton );
                _closeButtonIsDisabled = false;
            }
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
                            if( ShowClosingFailedMessageBox )
                            {
                                string m = String.Format( "This window cannot be closed: its plugin is required by configuration.\nTo stop it, change the configuration of {0}.", this.GetType().FullName );
                                string title = "Cannot close window";
                                MessageBox.Show( _window,
                                    m,
                                    title,
                                    MessageBoxButton.OK, MessageBoxImage.Stop,
                                    MessageBoxResult.OK, MessageBoxOptions.None );
                            }
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
            // _tryingToClose is set when window is trying to Close despite PluginStop() not being called: it's already Closing.
            // Calling Close() during a Closing does bad things, as you can expect.
            if( _window != null )
            {
                //_tryingToClose and _closingWindow are always on the UI thread
                _window.Dispatcher.Invoke( () =>
                {
                    if( !_tryingToClose )
                    {
                        // Allow the Closing to pass.
                        _closingWindow = true;

                        _window.Close();
                    }
                } );
            }
        }
    }
}