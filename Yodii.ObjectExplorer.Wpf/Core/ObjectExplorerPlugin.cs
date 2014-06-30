using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// WPF Object Explorer plugin.
    /// </summary>
    /// <remarks>
    /// If a WPF context does not exist (Application.Current), a new STA thread will be created with one in it.
    /// </remarks>
    public class ObjectExplorerPlugin : IYodiiPlugin
    {
        IYodiiEngine _activeEngine;
        ObjectExplorerWindow _window;
        ObjectExplorerWindowViewModel _vm;

        public ObjectExplorerPlugin( IYodiiEngine e )
        {
            Debug.Assert( e != null );

            _activeEngine = e;
            _activeEngine.PropertyChanged += _activeEngine_PropertyChanged;
        }

        void _activeEngine_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsRunning" )
            {
                if( !_activeEngine.IsRunning )
                {
                    Shutdown();
                }
            }
        }

        #region IYodiiPlugin Members

        bool IYodiiPlugin.Setup( PluginSetupInfo info )
        {
            return true;
        }

        void IYodiiPlugin.Start()
        {

            if( Application.Current == null )
            {
                // Create STA thread
                var thread = new Thread( CreateApp );
                thread.SetApartmentState( ApartmentState.STA );
                thread.Start();
            }
            else
            {
                Action action = delegate()
                {
                    _window = new ObjectExplorerWindow( _activeEngine );
                    _window.Show();
                };
                Application.Current.Dispatcher.Invoke( action );
            }
        }

        /// <summary>
        /// Called in a new STA thread.
        /// </summary>
        /// <param name="obj"></param>
        void CreateApp()
        {
            var app = new Application();

            Action action = delegate()
            {
                _window = new ObjectExplorerWindow( _activeEngine );
                _window.Show();

                app.Run( _window );
            };
            app.Dispatcher.Invoke( action );
        }

        void IYodiiPlugin.Stop()
        {
            Shutdown();
        }

        void Shutdown()
        {
            if( Application.Current != null )
            {
                Action action = delegate()
                {
                    if( _window != null ) _window.Close();
                };
                Application.Current.Dispatcher.Invoke( action );
            }
        }

        void IYodiiPlugin.Teardown()
        {
        }

        #endregion
    }
}
