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

        public ObjectExplorerPlugin( IYodiiEngine e )
        {
            Debug.Assert( e != null, "A YodiiEngine must be injected." );

            _activeEngine = e;
        }

        #region IYodiiPlugin Members

        bool IYodiiPlugin.Setup( PluginSetupInfo info )
        {
            if( Application.Current == null )
            {
                info.FailedUserMessage = "A WPF context (Application.Current) must exist for this plugin.";
                info.FailedDetailedMessage = "A WPF context (Application.Current) must exist for this plugin.";
                return false;
            }
            else
            {
                return true;
            }
        }

        void IYodiiPlugin.Start()
        {
            Debug.Assert( Application.Current != null, "A WPF context (Application.Current) must exist for this plugin." );

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                _window = new ObjectExplorerWindow( _activeEngine );

                _window.Show();
            } ) );
        }

        void IYodiiPlugin.Stop()
        {
            Shutdown();
        }

        void IYodiiPlugin.Teardown()
        {
        }

        #endregion

        void Shutdown()
        {
            Debug.Assert( Application.Current != null, "A WPF context (Application.Current) must exist for this plugin." );

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                if( _window != null )
                {
                    _window.AllowClose = true;
                    _window.Close();
                }
            } ) );
        }
    }
}
