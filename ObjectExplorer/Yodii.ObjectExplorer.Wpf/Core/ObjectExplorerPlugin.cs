#region LGPL License
/*----------------------------------------------------------------------------
* This file (ObjectExplorer\Yodii.ObjectExplorer.Wpf\Core\ObjectExplorerPlugin.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
    public class ObjectExplorerPlugin :  YodiiPluginBase
    {
        IYodiiEngineBase _activeEngine;
        ObjectExplorerWindow _window;
        bool _closing = false;
        bool _tryingToClose = false;

        public ObjectExplorerPlugin( IYodiiEngineBase e )
        {
            Debug.Assert( e != null, "A YodiiEngine must be injected." );

            _activeEngine = e;
        }

        #region IYodiiPlugin Members

        protected override void PluginPreStart( IPreStartContext c )
        {
            _closing = false;

            if( Application.Current == null )
            {
                c.Cancel("A WPF context (Application.Current) must exist for this plugin.");
            }
            base.PluginPreStart( c );
        }

        protected override void PluginStart( IStartContext c )
        {
            Debug.Assert( Application.Current != null, "A WPF context (Application.Current) must exist for this plugin." );
            Debug.Assert( _window == null, "No window exists." );

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                _window = new ObjectExplorerWindow( _activeEngine );
                _window.Closing += _window_Closing;

                _window.Show();
            } ) );

            base.PluginStart( c );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            Shutdown();
        }

        #endregion

        void _window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            // _closing is set in the Stop().
            if( !_closing )
            {
                // This didn't come from the Stop()! Cancel the Close for now, and kindly ask the engine to stop us, if at all possible.
                e.Cancel = true;

                ILivePluginInfo livePluginInfo = _activeEngine.LiveInfo.FindPlugin( this.GetType().FullName );
                if( livePluginInfo != null )
                {
                    if( livePluginInfo.Capability.CanStop )
                    {
                        // This will prevent Stop() from calling Close(), since we're, erm, already Closing.
                        _tryingToClose = true;

                        // Ask the engine to stop ourselves.
                        var r = _activeEngine.StopItem( livePluginInfo );

                        // We stopped our own plugin, without Stop() calling Close(), so the window is still there.
                        // Revoke the Cancel so it closes when exiting this method.
                        if( r.Success ) e.Cancel = false;

                        _tryingToClose = false;
                    }
                    else
                    {
                        // Can't stop? Check your configuration.
                        string m = String.Format( "The Object Explorer is required by configuration, and cannot stop itself.\nTo stop it, change the configuration of {0}.", this.GetType().FullName );
                        MessageBox.Show( _window, m, "Cannot stop",
                            MessageBoxButton.OK, MessageBoxImage.Stop,
                            MessageBoxResult.OK, MessageBoxOptions.None );
                    }
                }
            }
            else
            {
                // Engine asked us to Stop(), so off we go. Bye!
                _closing = false;
            }
        }

        void Shutdown()
        {
            Debug.Assert( Application.Current != null, "A WPF context (Application.Current) must exist for this plugin." );

            Application.Current.Dispatcher.Invoke( new Action( () =>
            {
                // _tryingToClose is set when window is trying to Close despite Stop() not being called: it's already Closing.
                // Calling Close() during a Closing does bad things, as you can expect.
                if( _window != null && !_tryingToClose )
                {
                    // Allow the Closing to pass.
                    _closing = true;

                    _window.Close();
                }

                // Clear up for next Start().
                _window = null;
            } ) );
        }
    }
}
