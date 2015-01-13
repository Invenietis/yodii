#region LGPL License
/*----------------------------------------------------------------------------
* This file (ObjectExplorer\Yodii.ObjectExplorer.WpfHostDemo\MainWindow.xaml.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.ObjectExplorer.WpfHostDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        IYodiiEngineExternal _engine;
        ILivePluginInfo _objectExplorer;
        ObjectExplorerManager _manager;

        public MainWindow()
        {
            _manager = new ObjectExplorerManager();
            _engine = _manager.Engine;

            _manager.SetDiscoveredInfo();

            // ObjectExplorer doesn't exist while Engine is dead, so we watch the plugins until it does.
            _engine.LiveInfo.Plugins.CollectionChanged += Plugins_CollectionChanged;

            this.DataContext = this;

            InitializeComponent();
            Engine.StartEngine();
        }

        void Plugins_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            ObjectExplorerPlugin = _engine.LiveInfo.FindPlugin( "Yodii.ObjectExplorer.Wpf.ObjectExplorerPlugin" );
        }

        public IYodiiEngineExternal Engine { get { return _engine; } }

        public ILivePluginInfo ObjectExplorerPlugin
        {
            get { return _objectExplorer; }
            private set
            {
                _objectExplorer = value;
                RaisePropertyChanged();
            }
        }

        private void Start_Click( object sender, RoutedEventArgs e )
        {
            Engine.StartEngine();
        }

        private void Stop_Click( object sender, RoutedEventArgs e )
        {
            Engine.StopEngine();
        }

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            Engine.StopEngine();
        }

        private void StartOE_Click( object sender, RoutedEventArgs e )
        {
            if( ObjectExplorerPlugin.Capability.CanStart )
            {
                Engine.StartItem( ObjectExplorerPlugin, StartDependencyImpact.Unknown );
            }
            else
            {
                MessageBox.Show( "The Object Explorer is disabled by configuration and cannot be started.", "Cannot start" );
            }
        }
        
        private void StopOE_Click( object sender, RoutedEventArgs e )
        {
            if( ObjectExplorerPlugin.Capability.CanStop )
            {
                Engine.StopItem( ObjectExplorerPlugin );
            }
            else
            {
                MessageBox.Show( "The Object Explorer is required by configuration and cannot be stopped.", "Cannot stop" );
            }
        }
        private void ResetConfig_Click( object sender, RoutedEventArgs e )
        {
            _manager.ResetConfiguration();
        }


        #region INotifyPropertyChanged utilities

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Throw new PropertyChanged.
        /// </summary>
        /// <param name="caller">Fill with Member name, when called from a property.</param>
        protected void RaisePropertyChanged( [CallerMemberName] string caller = null )
        {
            Debug.Assert( caller != null );
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        #endregion INotifyPropertyChanged utilities
    }
}
