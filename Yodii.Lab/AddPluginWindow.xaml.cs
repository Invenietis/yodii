#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\AddPluginWindow.xaml.cs) is part of CiviKey. 
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
using System.Windows;
using CK.Core;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for AddPluginWindow.xaml
    /// </summary>
    internal partial class AddPluginWindow : Window, INotifyPropertyChanged
    {
        public event EventHandler<NewPluginEventArgs> NewPluginCreated;
        readonly Dictionary<IServiceInfo,DependencyRequirement> _serviceReferences; // TODO

        string _pluginName;
        IServiceInfo _service;

        public AddPluginWindow( ICKObservableReadOnlyCollection<IServiceInfo> availableServices, IServiceInfo selectedService = null )
        {
            _serviceReferences = new Dictionary<IServiceInfo, DependencyRequirement>();
            AvailableServices = availableServices;

            if( selectedService != null && availableServices.Count > 0 )
            {
                SelectedService = selectedService;
            }

            this.DataContext = this;

            InitializeComponent();

            this.NewPluginNameTextBox.Focus();
        }

        #region Properties

        public ICKObservableReadOnlyCollection<IServiceInfo> AvailableServices
        {
            get;
            private set;
        }

        public bool HasService
        {
            get { return SelectedService != null; }
        }

        public string NewPluginName
        {
            get { return _pluginName; }
            set
            {
                if( value != _pluginName )
                {
                    _pluginName = value;
                    RaisePropertyChanged( "NewPluginName" );
                }
            }
        }

        public IServiceInfo SelectedService
        {
            get { return _service; }
            set
            {
                if( value != _service )
                {
                    _service = value;
                    RaisePropertyChanged( "SelectedService" );
                    RaisePropertyChanged( "HasService" );
                }
            }
        }

        #endregion Properties

        private void CreateButton_Click( object sender, RoutedEventArgs e )
        {
            IServiceInfo service = SelectedService;
            if( !HasService ) service = null;

            string newPluginName = NewPluginName;
            if( newPluginName == null ) newPluginName = String.Empty;

            if( RaiseNewPlugin( newPluginName, service, _serviceReferences ) )
            {
                this.Close();
            }
        }

        private bool RaiseNewPlugin( string newPluginName, IServiceInfo service, Dictionary<IServiceInfo, DependencyRequirement> serviceReferences )
        {
            if( NewPluginCreated != null )
            {
                NewPluginEventArgs args = new NewPluginEventArgs()
                {
                    PluginName = newPluginName,
                    Service = service,
                    ServiceReferences = serviceReferences
                };

                // Raise event
                NewPluginCreated( this, args );

                if( args.CancelReason != null )
                {
                    MessageBox.Show( args.CancelReason, "Couldn't create plugin", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK );
                    return false;
                }
            }

            return true;
        }

        private void CancelButton_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ClearServiceButton_Click( object sender, RoutedEventArgs e )
        {
            SelectedService = null;
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
        protected void RaisePropertyChanged( string caller )
        {
            Debug.Assert( caller != null );
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        #endregion INotifyPropertyChanged utilities

    }

    internal class NewPluginEventArgs : EventArgs
    {
        public string PluginName { get; internal set; }
        public IServiceInfo Service { get; internal set; }

        public Dictionary<IServiceInfo, DependencyRequirement> ServiceReferences { get; internal set; }

        public string CancelReason { get; set; }
    }
}
